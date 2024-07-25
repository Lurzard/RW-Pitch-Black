using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Linq;
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
    public int MaxTimeUntilRevive => (8 - PBOptions.pursuerAgro.Value) * (diedToSporeCloud ? 6 : 9999); //ACTUALLY.. ONLY RESPAWN FROM SPORES. THE OTHER TRACKER WILL HANDLE RESPAWNS
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

public static class NightTerrorHooks
{
    public static ConditionalWeakTable<Centipede, NightTerrorData> NightTerrorInfo = new();
    public static ConditionalWeakTable<AbstractCreature, NightTerrorAbstractData> NTAbstractCWT = new();
    public static ConditionalWeakTable<AbstractCreature, StrongBox<int>> KILLIT = new();

    public static void NightTerrorReleasePlayersInGrasp(this Creature self) {
        if (self.abstractCreature.creatureTemplate.IsNightTerror()) {
            for (int i = 0; i < self.grasps.Length; i++) {
                if (self.grasps[i]?.grabbed is Player) {
                    self.ReleaseGrasp(i);
                }
            }
        }

    }

    internal static void Apply()
    {
        new Hook(typeof(Centipede).GetMethod("get_Red", Public | NonPublic | Instance), (Func<Centipede, bool> orig, Centipede self) => self.Template.IsNightTerror() || orig(self));

        On.AbstractCreature.Update += AbstractCreature_Update;
        On.Creature.CanBeGrabbed += Creature_CanBeGrabbed;
        On.SporeCloud.Update += SporeCloud_Update;

        //spinch: moved FlareBomb.Update hook to the one in Plugin.cs, so there's no longer a double hook
        
        On.Centipede.ctor += Centipede_ctor;
        On.CentipedeGraphics.ctor += CentipedeGraphics_ctor;
        On.Centipede.Violence += Centipede_Violence;
        On.CentipedeAI.DoIWantToShockCreature += CentipedeAI_DoIWantToShockCreature;
        On.Centipede.Shock += Centipede_Shock;
        On.Centipede.ShortCutColor += Centipede_ShortCutColor;
        On.CentipedeAI.ctor += CentipedeAICTOR;
    }

    /// <summary>
    /// Prevent the Night Terror from being grabbed if it's not dead or stunned
    /// </summary>
    private static bool Creature_CanBeGrabbed(On.Creature.orig_CanBeGrabbed orig, Creature self, Creature grabber)
    {
        bool result = orig(self, grabber);
        if (self.Template.IsNightTerror()) {
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
        if (NTAbstractCWT.TryGetValue(self, out NightTerrorAbstractData nt)) {
            nt.TryRevive();
            // Debug.Log($"Pitch Black NightTerror Revive Status:\n\tIs dead? = {!self.state.alive || self.realizedCreature != null && self.realizedCreature.dead} | {nameof(nt.timeUntilRevive)} = {nt.timeUntilRevive}");
        }
    }
    private static void SporeCloud_Update(On.SporeCloud.orig_Update orig, SporeCloud self, bool eu)
    {
        orig(self, eu);

        if (!self.nonToxic) {
            foreach (AbstractCreature abstrCrit in self.room.abstractRoom.creatures) {
                if (NTAbstractCWT.TryGetValue(abstrCrit, out NightTerrorAbstractData nt)) {
                    nt.diedToSporeCloud = true;
                }
            }
        }
    }
    #endregion

    /// <summary>
    /// Make the nightterror AI behave more like a red centipede
    /// </summary>
    private static void CentipedeAICTOR(On.CentipedeAI.orig_ctor orig, CentipedeAI self, AbstractCreature creature, World world)
    {
        orig(self, creature, world);
        if (creature.creatureTemplate.IsNightTerror())
        {
            // Convert this Linq to normal for loops if it turns out to be too laggy
            // Remove the prey tracker, then add it back but modified
            self.modules.RemoveAll(x => x is PreyTracker);
            self.AddModule(new PreyTracker(self, 5, 1f, 100f, 150f, 0.05f));
            self.utilityComparer.uTrackers.RemoveAll(x => x.module is PreyTracker);
            self.utilityComparer.AddComparedModule(self.preyTracker, null, 0.9f, 1.1f);

            // Remove the Threat Tracker
            self.modules.RemoveAll(x => x is ThreatTracker);
            self.utilityComparer.uTrackers.RemoveAll(x => x.module is ThreatTracker);

            // This makes it 1.5 times as fast as a red centipede
            self.pathFinder.stepsPerFrame = 22;
        }
    }

    private static Color Centipede_ShortCutColor(On.Centipede.orig_ShortCutColor orig, Centipede self)
    {
        Color result = orig(self);
        if (self.abstractCreature.creatureTemplate.IsNightTerror())
        {
            return new Color(0.2f, 0f, 1f);
        }
        return result;
    }

    #region Shocking Things
    private static void Centipede_Shock(On.Centipede.orig_Shock orig, Centipede self, PhysicalObject shockObj)
    {
        orig(self, shockObj);
        if (self.abstractCreature.creatureTemplate.IsNightTerror() && shockObj?.abstractPhysicalObject is AbstractCreature abstrCrit && KILLIT.TryGetValue(abstrCrit, out StrongBox<int> timesZapped)) {
            timesZapped.Value++;
        }
    }

    private static bool CentipedeAI_DoIWantToShockCreature(On.CentipedeAI.orig_DoIWantToShockCreature orig, CentipedeAI self, AbstractCreature critter)
    {
        bool result = orig(self, critter);
        if (self.centipede.abstractCreature.creatureTemplate.IsNightTerror() && critter.realizedCreature is Player && KILLIT.TryGetValue(critter, out StrongBox<int> timesZapped) && timesZapped.Value < 6)
        {
            return true;
        }
        return result;
    }
    #endregion

    /// <summary>
    /// Alters the damage done to Night Terror, currently it halfs all damage
    /// </summary>
    private static void Centipede_Violence(On.Centipede.orig_Violence orig, Centipede self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus) {
        if (self.abstractCreature.creatureTemplate.IsNightTerror()) {
            //damage = 0f;
            //spinch: i think it'd be cooler if it was damage /= 2 so nt can get hit and maybe die
            damage *= 0.5f;
        }
        orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
    }

    private static void Centipede_ctor(On.Centipede.orig_ctor orig, Centipede self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (self.abstractCreature.creatureTemplate.IsNightTerror())
        {
            self.abstractCreature.personality.aggression = 1;
            self.abstractCreature.personality.bravery = 1;
            self.abstractCreature.personality.dominance = 1;
            self.abstractCreature.personality.energy = 1;
            self.abstractCreature.personality.nervous = 0;
            self.abstractCreature.personality.sympathy = 0;

            abstractCreature.ignoreCycle = true;

            if (!NightTerrorInfo.TryGetValue(self, out _)) {
                NightTerrorInfo.Add(self, new NightTerrorData());
            }
            
            self.bodyChunks = new BodyChunk[21];
            for (int i = 0; i < self.bodyChunks.Length; i++)
            {
                float chunkRad = 1.5f + Mathf.Lerp(Mathf.Lerp(2f, 3.5f, self.size), Mathf.Lerp(4f, 6.5f, self.size), Mathf.Pow(Mathf.Clamp(Mathf.Sin(Mathf.PI * (i / (float)(self.bodyChunks.Length - 1))), 0f, Mathf.PI), Mathf.Lerp(0.7f, 0.3f, self.size)));

                self.bodyChunks[i] = new(self, i, new Vector2(0f, 0f), chunkRad, 0.3f)
                {
                    loudness = 0f,
                    mass = 0.02f + 0.08f * Mathf.Clamp01(Mathf.Sin(Mathf.InverseLerp(0f, self.bodyChunks.Length - 1, i) * 3.1415927f))
                };
            }

            self.mainBodyChunkIndex = self.bodyChunks.Length / 2;
            self.CentiState.shells = new bool[self.bodyChunks.Length];

            self.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[self.bodyChunks.Length * (self.bodyChunks.Length - 1) / 2];
            int num3 = 0;
            for (int l = 0; l < self.bodyChunks.Length; l++)
            {
                for (int m = l + 1; m < self.bodyChunks.Length; m++)
                {
                    self.bodyChunkConnections[num3] = new PhysicalObject.BodyChunkConnection(self.bodyChunks[l], self.bodyChunks[m], self.bodyChunks[l].rad + self.bodyChunks[m].rad, PhysicalObject.BodyChunkConnection.Type.Push, 1f, 0);
                    num3++;
                }
            }
        }
    }

    /// <summary>
    /// Sets the Night Terror body color
    /// </summary>
    private static void CentipedeGraphics_ctor(On.CentipedeGraphics.orig_ctor orig, CentipedeGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
        if (self.centipede.abstractCreature.creatureTemplate.IsNightTerror())
        {
            self.hue = 0.702f;
            self.saturation = 0.96f;
        }
    }
}