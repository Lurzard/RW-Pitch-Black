using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using static System.Reflection.BindingFlags;

namespace PitchBlack;

public class NightTerrorData
{
    public int fleeing;
    public Vector2 fleeTo = Vector2.zero;
}

public class NightTerrorAbstractData
{
    public WeakReference<AbstractCreature> abstrNightterrorRef;

    public NightTerrorAbstractData(AbstractCreature centi)
    {
        abstrNightterrorRef = new(centi);
        MaxHP = (centi.state as HealthState).health;
    }

    private bool _justRevived;

    public int timeUntilRevive;
    public bool diedToSporeCloud;
    public readonly float MaxHP;

    //public int MaxTimeUntilRevive => !diedToSporeCloud ? 15 : 3;
    //public int MaxTimeUntilRevive => PBOptions.pursuerAgro2.Value * (!diedToSporeCloud ? 60 : 6);
    public int MaxTimeUntilRevive => (8 - PBOptions.pursuerAgro.Value) * (!diedToSporeCloud ? 9999 : 6); //ACTUALLY.. ONLY RESPAWN FROM SPORES. THE OTHER TRACKER WILL HANDLE RESPAWNS
    //spinch: thrown PuffBalls makes NT revive faster because that looks cool

    public void DecreaseReviveTimer(int timeDecreasedBy = 2)
    {
        timeUntilRevive = Mathf.Max(0, timeUntilRevive - timeDecreasedBy);

        // Debug.Log($"Pitch Black: NightTerror got hit! {nameof(timeUntilRevive)} = {timeUntilRevive}");
    }

    public void TryRevive()
    {
        if (!abstrNightterrorRef.TryGetTarget(out var centi) || !PBOptions.pursuer.Value) return;

        if (centi.state.alive || centi.realizedCreature != null && !centi.realizedCreature.dead) return;

        timeUntilRevive++;

        if (timeUntilRevive >= MaxTimeUntilRevive)
        {
            Revive();
        }
    }
    public void Revive()
    {
        //yoinked from BigSpider.Revive
        if (!abstrNightterrorRef.TryGetTarget(out var centi)) return;

        timeUntilRevive = 0;
        _justRevived = true;

        SetRealizedCreatureDataOnRevive();

        (centi.state as HealthState).health = MaxHP;
        centi.abstractAI.SetDestination(centi.pos);

        for (int i = centi.stuckObjects.Count - 1; i >= 0; i--)
        {
            if (centi.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearStick
                && centi.realizedCreature.abstractCreature.stuckObjects[i].A.type == AbstractPhysicalObject.AbstractObjectType.Spear
                && centi.stuckObjects[i].A.realizedObject is Spear spear)
            {
                spear.ChangeMode(Weapon.Mode.Free);
            }
        }
        if (ModManager.MMF)
        {
            centi.LoseAllStuckObjects();
        }

        Debug.Log("Pitch Black: NightTerror revived!");
    }
    public void SetRealizedCreatureDataOnRevive()
    {
        if (!abstrNightterrorRef.TryGetTarget(out AbstractCreature centi)) return;

        if (centi.realizedCreature != null && _justRevived)
        {
            _justRevived = false;
            centi.realizedCreature.dead = false;
            centi.realizedCreature.killTag = null;
            centi.realizedCreature.killTagCounter = 0;
        }
    }
}

public class ChillTheFUCKOut
{
    // Since this is referencing the creature that the Night Terror is murdering and not the NT itself I can't really compress it any - Niko
    public int timesZapped = 0;
}

public static class NightTerrorHooks
{
    public static ConditionalWeakTable<Centipede, NightTerrorData> NightTerrorInfo = new();
    public static ConditionalWeakTable<AbstractCreature, NightTerrorAbstractData> NTAbstractCWT = new();
    public static ConditionalWeakTable<AbstractCreature, ChillTheFUCKOut> KILLIT = new();

    public static void NightTerrorReleasePlayersInGrasp(this Centipede self)
    {
        if (self.abstractCreature.creatureTemplate.type != CreatureTemplateType.NightTerror)
            return;

        for (int i = 0; i < self.grasps.Length; i++)
        {
            if (self.grasps[i]?.grabbed is Player)
            {
                self.ReleaseGrasp(i);
            }
        }
    }
    public static bool TryGetNightTerror(this Centipede centi, out NightTerrorData NTData)
    {
        if (centi.abstractCreature.creatureTemplate.type == CreatureTemplateType.NightTerror)
        {
            NTData = NightTerrorInfo.GetValue(centi, _ => new());
            return true;
        }
        NTData = null;
        return false;
    }
    public static bool TryGetAbstractNightTerror(this AbstractCreature centi, out NightTerrorAbstractData NTData)
    {
        if (centi.creatureTemplate.type == CreatureTemplateType.NightTerror)
        {
            NTData = NTAbstractCWT.GetValue(centi, _ => new(centi));
            return true;
        }
        NTData = null;
        return false;
    }
    public static ChillTheFUCKOut GetZapVictim(this AbstractCreature abstrCrit) => KILLIT.GetValue(abstrCrit, _ => new());

    internal static void Apply()
    {
        new Hook(typeof(Centipede).GetMethod("get_Red", Public | NonPublic | Instance), (Func<Centipede, bool> orig, Centipede self) => self.Template.type == CreatureTemplateType.NightTerror || orig(self));

        On.AbstractCreature.Update += AbstractCreature_Update;
        On.Creature.CanBeGrabbed += Creature_CanBeGrabbed;
        On.SporeCloud.Update += SporeCloud_Update;

        IL.WormGrass.WormGrassPatch.Update += IL_WormGrass_WormGrassPatch_Update;
        // On.WormGrass.WormGrassPatch.InteractWithCreature += WormGrassPatch_InteractWithCreature;
        // On.WormGrass.WormGrassPatch.Update += WormGrassPatch_Update;
        // On.WormGrass.WormGrassPatch.AlreadyTrackingCreature += WormGrassPatch_AlreadyTrackingCreature;

        //spinch: moved FlareBomb.Update hook to the one in Plugin.cs, so there's no longer a double hook
        
        On.Centipede.ctor += Centipede_ctor;
        On.CentipedeGraphics.ctor += CentipedeGraphics_ctor;
        On.Centipede.Violence += Centipede_Violence;
        On.CentipedeAI.DoIWantToShockCreature += CentipedeAI_DoIWantToShockCreature;
        On.Centipede.Shock += Centipede_Shock;
        On.Centipede.ShortCutColor += Centipede_ShortCutColor;
        On.CentipedeAI.ctor += CentipedeAICTOR;
    }
    // UPDATED
    // Prevent the Night Terror from being grabbed if it's not dead or stunned
    private static bool Creature_CanBeGrabbed(On.Creature.orig_CanBeGrabbed orig, Creature self, Creature grabber)
    {
        bool result = orig(self, grabber);
        if (self.Template.type == CreatureTemplateType.NightTerror) {
            if (self.dead || self.stun > 0) {
                return true;
            }
            return false;
        }
        return result;
    }

    #region update method hooks
    private static void AbstractCreature_Update(On.AbstractCreature.orig_Update orig, AbstractCreature self, int time)
    {
        orig(self, time);
        if (self.TryGetAbstractNightTerror(out NightTerrorAbstractData nt))
        {
            nt.TryRevive();
            // Debug.Log($"Pitch Black NightTerror Revive Status:\n\tIs dead? = {!self.state.alive || self.realizedCreature != null && self.realizedCreature.dead} | {nameof(nt.timeUntilRevive)} = {nt.timeUntilRevive}");
        }
    }
    private static void SporeCloud_Update(On.SporeCloud.orig_Update orig, SporeCloud self, bool eu)
    {
        orig(self, eu);

        if (!self.nonToxic)
        {
            foreach (AbstractCreature abstrCrit in self.room.abstractRoom.creatures)
            {
                if (abstrCrit.creatureTemplate.type == CreatureTemplateType.NightTerror && abstrCrit.TryGetAbstractNightTerror(out var nt))
                {
                    nt.diedToSporeCloud = true;
                }
            }
        }
    }
    #endregion

    #region wormgrass immunity (stop getting pulled)
    static void IL_WormGrass_WormGrassPatch_Update(ILContext il) {
        var cursor = new ILCursor(il);
        var label = cursor.DefineLabel();

        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchStloc(out _))) {
            return;
        }
        cursor.Emit(OpCodes.Ldloc, 0);
        cursor.EmitDelegate((Creature crit) => {
            if (crit != null && crit.Template.type == CreatureTemplateType.NightTerror) {
                return true;
            }
            return false;
        });
        cursor.Emit(OpCodes.Brtrue, label);
        
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchNewobj(out _), i => i.MatchCallOrCallvirt(out _))) {
            return;
        }
        cursor.MarkLabel(label);
    }
    #endregion

    /// <summary>
    /// Make the nightterror AI behave more like a red centipede
    /// </summary>
    private static void CentipedeAICTOR(On.CentipedeAI.orig_ctor orig, CentipedeAI self, AbstractCreature creature, World world)
    {
        orig(self, creature, world);
        if (creature.creatureTemplate.type == CreatureTemplateType.NightTerror)
        {
            self.pathFinder.stepsPerFrame = 15;
            for (int i = 0; i < self.modules.Count; i++)
            {
                if (self.modules[i] is PreyTracker)
                {
                    self.modules.RemoveAt(i);
                    self.AddModule(new PreyTracker(self, 5, 1f, 100f, 150f, 0.05f));
                    self.utilityComparer.uTrackers.RemoveAt(1);
                    self.utilityComparer.AddComparedModule(self.preyTracker, null, 0.9f, 1.1f);
                }
            }
        }
    }

    private static Color Centipede_ShortCutColor(On.Centipede.orig_ShortCutColor orig, Centipede self)
    {
        if (self.abstractCreature.creatureTemplate.type == CreatureTemplateType.NightTerror)
        {
            return new Color(0.2f, 0f, 1f);
        }
        return orig(self);
    }

    private static void Centipede_Shock(On.Centipede.orig_Shock orig, Centipede self, PhysicalObject shockObj)
    {
        orig(self, shockObj);
        if (self.abstractCreature.creatureTemplate.type == CreatureTemplateType.NightTerror)
        {
            if (shockObj?.abstractPhysicalObject is AbstractCreature abstrCrit)
            {
                abstrCrit.GetZapVictim().timesZapped++;
            }
        }
    }

    private static bool CentipedeAI_DoIWantToShockCreature(On.CentipedeAI.orig_DoIWantToShockCreature orig, CentipedeAI self, AbstractCreature critter)
    {
        if (self.centipede.abstractCreature.creatureTemplate.type == CreatureTemplateType.NightTerror)
        {
            if (critter.realizedCreature is Player && critter.GetZapVictim().timesZapped < 6)
            {
                return true;
            }
        }
        return orig(self, critter);
    }

    private static void Centipede_Violence(On.Centipede.orig_Violence orig, Centipede self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
    {
        if (self.abstractCreature.creatureTemplate.type == CreatureTemplateType.NightTerror)
        {
            //damage = 0f;
            //spinch: i think it'd be cooler if it was damage /= 2 so nt can get hit and maybe die
            damage /= 2;
        }
        orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
    }

    private static void Centipede_ctor(On.Centipede.orig_ctor orig, Centipede self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (self.abstractCreature.creatureTemplate.type == CreatureTemplateType.NightTerror)
        {
            //spinch: personality stats are from ID 3560. the energy stat makes it go fucking zoom
            self.abstractCreature.personality.aggression = 0.216056f;
            self.abstractCreature.personality.bravery = 0.8485757f;
            self.abstractCreature.personality.dominance = 0.7141814f;
            self.abstractCreature.personality.energy = 0.9055567f;
            self.abstractCreature.personality.nervous = 0.2577313f;
            self.abstractCreature.personality.sympathy = 0.2754336f;

            abstractCreature.ignoreCycle = true;

            if (!NightTerrorInfo.TryGetValue(self, out _))
                NightTerrorInfo.Add(self, new NightTerrorData());
            
            self.bodyChunks = new BodyChunk[21];
            for (int i = 0; i < self.bodyChunks.Length; i++)
            {
                float num = i / (float)(self.bodyChunks.Length - 1);
                float num2 = Mathf.Lerp(Mathf.Lerp(2f, 3.5f, self.size), Mathf.Lerp(4f, 6.5f, self.size), Mathf.Pow(Mathf.Clamp(Mathf.Sin(3.1415927f * num), 0f, 1f), Mathf.Lerp(0.7f, 0.3f, self.size)));
                num2 += 1.5f;

                self.bodyChunks[i] = new(self, i, new Vector2(0f, 0f), num2, Mathf.Lerp(0.042857144f, 0.32352942f, Mathf.Pow(self.size, 1.4f)))
                {
                    loudness = 0f
                };
                self.bodyChunks[i].mass += 0.02f + 0.08f * Mathf.Clamp01(Mathf.Sin(Mathf.InverseLerp(0f, self.bodyChunks.Length - 1, i) * 3.1415927f));
            }

            //spinch: i put the stuff in the for loop below into the one above
            //for (int j = 0; j < self.bodyChunks.Length; j++)
            //{
            //    self.bodyChunks[j].mass += 0.02f + 0.08f * Mathf.Clamp01(Mathf.Sin(Mathf.InverseLerp(0f, self.bodyChunks.Length - 1, j) * 3.1415927f));
            //}

            self.mainBodyChunkIndex = self.bodyChunks.Length / 2;
            if (!self.Small && (self.CentiState.shells == null || self.CentiState.shells.Length != self.bodyChunks.Length))
            {
                self.CentiState.shells = new bool[self.bodyChunks.Length];
                for (int k = 0; k < self.CentiState.shells.Length; k++)
                {
                    self.CentiState.shells[k] = false;
                }
            }

            self.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[self.bodyChunks.Length * (self.bodyChunks.Length - 1) / 2];
            int num3 = 0;
            for (int l = 0; l < self.bodyChunks.Length; l++)
            {
                for (int m = l + 1; m < self.bodyChunks.Length; m++)
                {
                    float num4 = 0f;
                    self.bodyChunkConnections[num3] = new PhysicalObject.BodyChunkConnection(self.bodyChunks[l], self.bodyChunks[m], self.bodyChunks[l].rad + self.bodyChunks[m].rad, PhysicalObject.BodyChunkConnection.Type.Push, 1f - num4, -1f);
                    num3++;
                }
            }
        }
    }

    private static void CentipedeGraphics_ctor(On.CentipedeGraphics.orig_ctor orig, CentipedeGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
        if ((ow as Centipede).abstractCreature.creatureTemplate.type == CreatureTemplateType.NightTerror)
        {
            self.hue = 0.702f;
            self.saturation = 0.96f;
        }
    }
}