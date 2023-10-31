using MonoMod.RuntimeDetour;
using PitchBlack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using static System.Reflection.BindingFlags;

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
    //public int MaxTimeUntilRevive => PBOptions.pursuerAgro.Value * (!diedToSporeCloud ? 60 : 6);
    public int MaxTimeUntilRevive => PBOptions.pursuerAgro.Value * (!diedToSporeCloud ? 9999 : 6); //ACTUALLY.. ONLY RESPAWN FROM SPORES. THE OTHER TRACKER WILL HANDLE RESPAWNS
    //spinch: thrown PuffBalls makes NT revive faster because that looks cool

    public void DecreaseReviveTimer(int timeDecreasedBy = 2)
    {
        timeUntilRevive = Mathf.Max(0, timeUntilRevive - timeDecreasedBy);

#if false
        Debug.Log($"Pitch Black: NightTerror got hit! {nameof(timeUntilRevive)} = {timeUntilRevive}");
#endif
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
        if (!abstrNightterrorRef.TryGetTarget(out var centi)) return;

        if (centi.realizedCreature != null && /*timeUntilRevive == 0 &&*/ _justRevived)
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

// Moon - This makes this file cursed why is it like this, remind me to never touch it again
namespace PitchBlack
{
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
            new Hook(typeof(Centipede).GetMethod("get_Red", Public | NonPublic | Instance), (System.Func<Centipede, bool> orig, Centipede self) => self.Template.type == CreatureTemplateType.NightTerror || orig(self));

            //DISABLING THESE IN FAVOR OF THE NEW RESPAWN TIMERS
            //On.Spear.HitSomething += Spear_HitSomething;
            //On.Rock.HitSomething += Rock_HitSomething;
            //On.FirecrackerPlant.HitSomething += FirecrackerPlant_HitSomething;

            On.Centipede.Update += Centipede_Update;
            On.AbstractCreature.Update += AbstractCreature_Update;
            On.SporeCloud.Update += SporeCloud_Update;

            On.WormGrass.WormGrassPatch.InteractWithCreature += WormGrassPatch_InteractWithCreature;
            On.WormGrass.WormGrassPatch.Update += WormGrassPatch_Update;
            On.WormGrass.WormGrassPatch.AlreadyTrackingCreature += WormGrassPatch_AlreadyTrackingCreature;

            //spinch: moved FlareBomb.Update hook to the one in Plugin.cs, so there's no longer a double hook
            
            On.Centipede.ctor += Centipede_ctor;
            On.CentipedeGraphics.ctor += CentipedeGraphics_ctor;
            //On.CentipedeAI.Update += CentipedeAI_Update;
            On.Centipede.Violence += Centipede_Violence;
            On.CentipedeAI.DoIWantToShockCreature += CentipedeAI_DoIWantToShockCreature;
            On.Centipede.Shock += Centipede_Shock;
            On.Centipede.ShortCutColor += Centipede_ShortCutColor;
            On.CentipedeAI.ctor += CentipedeAICTOR;
            //On.AbstractCreatureAI.ctor += AbstractCreatureAI_ctor;

            //On.RoomRealizer.PutOutARoom += RoomRealizer_PutOutARoom; Does't seem like this is needed. Creatures can move from unloaded rooms fine -WW
        }

        #region weapon.HitSomething hooks
        private static bool Spear_HitSomething(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
        {
            bool val = orig(self, result, eu);
            if (result.obj is Centipede centi && centi.abstractCreature.creatureTemplate.type == CreatureTemplateType.NightTerror)
            {
                centi.NightTerrorReleasePlayersInGrasp();
                if (centi.abstractCreature.TryGetAbstractNightTerror(out var nt))
                {
                    nt.DecreaseReviveTimer();
                }
            }
            return val;
        }
        private static bool Rock_HitSomething(On.Rock.orig_HitSomething orig, Rock self, SharedPhysics.CollisionResult result, bool eu)
        {
            bool val = orig(self, result, eu);
            if (result.obj is Centipede centi && centi.abstractCreature.creatureTemplate.type == CreatureTemplateType.NightTerror)
            {
                centi.NightTerrorReleasePlayersInGrasp();
                if (centi.abstractCreature.TryGetAbstractNightTerror(out var nt))
                {
                    nt.DecreaseReviveTimer();
                }
            }
            return val;
        }
        private static bool FirecrackerPlant_HitSomething(On.FirecrackerPlant.orig_HitSomething orig, FirecrackerPlant self, SharedPhysics.CollisionResult result, bool eu)
        {
            bool val = orig(self, result, eu);
            if (result.obj is Centipede centi && centi.abstractCreature.creatureTemplate.type == CreatureTemplateType.NightTerror)
            {
                centi.NightTerrorReleasePlayersInGrasp();
                if (centi.abstractCreature.TryGetAbstractNightTerror(out var nt))
                {
                    nt.DecreaseReviveTimer();
                }
            }
            return val;
        }
        #endregion

        private static AbstractRoom RoomRealizer_PutOutARoom(On.RoomRealizer.orig_PutOutARoom orig, RoomRealizer self, ref List<RoomRealizer.RealizedRoomTracker> candidates)
        {
            List<RoomRealizer.RealizedRoomTracker> newCandidates = new List<RoomRealizer.RealizedRoomTracker>();
            foreach (RoomRealizer.RealizedRoomTracker roomTracker in candidates) {
                foreach (AbstractCreature abcrit in roomTracker.room.creatures) {
                    if (abcrit.creatureTemplate.type != CreatureTemplateType.NightTerror) {
                        newCandidates.Add(roomTracker);
                    }
                }
            }
            candidates = newCandidates;
            return orig(self, ref candidates);
        }

        #region update method hooks
        private static void AbstractCreature_Update(On.AbstractCreature.orig_Update orig, AbstractCreature self, int time)
        {
            orig(self, time);
            if (self.TryGetAbstractNightTerror(out var nt))
            {
                nt.TryRevive();
#if false
                Debug.Log($"Pitch Black NightTerror Revive Status:" +
                    $"\n\tIs dead? = {!self.state.alive || self.realizedCreature != null && self.realizedCreature.dead} | {nameof(nt.timeUntilRevive)} = {nt.timeUntilRevive}");
#endif
            }
        }

        private static void Centipede_Update(On.Centipede.orig_Update orig, Centipede self, bool eu)
        {
            orig(self, eu);
            if (self.abstractCreature.creatureTemplate.type == CreatureTemplateType.NightTerror && self.grabbedBy != null && (!self.dead || self.stun <= 0))
            {
                //spinch: nothing can grab the night terror if its not dead or stunned
                //spinch: thats right miros birds. suck it
                for (int j = 0; j < self.grabbedBy.Count; j++)
                {
                    if (self.grabbedBy[j] == null
                        || self.grabbedBy[j].grabber == null
                        || self.grabbedBy[j].grabber.grasps == null)
                    {
                        continue;
                    }

                    for (int i = 0; i < self.grabbedBy[j].grabber.grasps.Length; i++)
                    {
                        if (self.grabbedBy[j].grabber.grasps[i]?.grabbed == self)
                            self.grabbedBy[j].grabber.ReleaseGrasp(i);
                    }
                }

                if (self.abstractCreature.TryGetAbstractNightTerror(out var nt))
                {
                    nt.SetRealizedCreatureDataOnRevive();
                    //spinch: there's checks in that method to make sure it was just revived
                }
            }
        }

        private static void SporeCloud_Update(On.SporeCloud.orig_Update orig, SporeCloud self, bool eu)
        {
            orig(self, eu);

            if (!self.nonToxic)
            {
                foreach (var abstrCrit in self.room.abstractRoom.creatures)
                {
                    if (abstrCrit.realizedCreature is InsectoidCreature
                        && abstrCrit.creatureTemplate.type == CreatureTemplateType.NightTerror
                        && abstrCrit.TryGetAbstractNightTerror(out var nt))
                    {
                        nt.diedToSporeCloud = true;
                    }
                }
            }
        }
        #endregion

        #region wormgrass immunity (stop getting pulled)
        private static void WormGrassPatch_InteractWithCreature(On.WormGrass.WormGrassPatch.orig_InteractWithCreature orig, WormGrass.WormGrassPatch self, WormGrass.WormGrassPatch.CreatureAndPull creatureAndPull)
        {
            if (creatureAndPull.creature.abstractCreature.creatureTemplate.type != CreatureTemplateType.NightTerror)
                orig(self, creatureAndPull);
        }
        public static void WormGrassPatch_Update(On.WormGrass.WormGrassPatch.orig_Update orig, WormGrass.WormGrassPatch self)
        {
            orig(self);
            self.trackedCreatures.RemoveAll(critAndPull => critAndPull.creature.abstractCreature.creatureTemplate.type == CreatureTemplateType.NightTerror);
        }
        public static bool WormGrassPatch_AlreadyTrackingCreature(On.WormGrass.WormGrassPatch.orig_AlreadyTrackingCreature orig, WormGrass.WormGrassPatch self, Creature creature)
        {
            bool val = orig(self, creature);
            if (creature.abstractCreature.creatureTemplate.type == CreatureTemplateType.NightTerror && self.trackedCreatures.Any(creatureAndPull => creatureAndPull.creature == creature))
                return true;
            return val;
        }
        #endregion

        /// <summary>
        /// Adds a Nightterror tracker upon creation of its abstractcreatureAI
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        /// <param name="world"></param>
        /// <param name="parent"></param>
        private static void AbstractCreatureAI_ctor(On.AbstractCreatureAI.orig_ctor orig, AbstractCreatureAI self, World world, AbstractCreature parent)
        {
            orig(self, world, parent);
            if (parent.creatureTemplate.type == CreatureTemplateType.NightTerror)
            {
                //parent.NTT(world); //WW- GONNA TRY REMOVING THIS IN FAVOR OF THE NEW VERSION
            }
        }

        /// <summary>
        /// Make the nightterror AI behave more like a red centipede
        /// </summary>
        private static void CentipedeAICTOR(On.CentipedeAI.orig_ctor orig, CentipedeAI self, AbstractCreature creature, World world)
        {
            orig(self, creature, world);
            if (creature.creatureTemplate.type == CreatureTemplateType.NightTerror)
            {
                self.pathFinder.stepsPerFrame = 15;
#if (true)
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
#endif
            }
        }

        private static Color Centipede_ShortCutColor(On.Centipede.orig_ShortCutColor orig, Centipede self)
        {
            if (self.abstractCreature.creatureTemplate.type == CreatureTemplateType.NightTerror)
            {
                return new Color(0.286f, 0.286f, 0.952f);
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

        private static void CentipedeAI_Update(On.CentipedeAI.orig_Update orig, CentipedeAI self)
        {
            orig(self);
            if (self.centipede.abstractCreature.creatureTemplate.type == CreatureTemplateType.NightTerror)
            {
                self.run = 500;
                #region
                //self.behavior = CentipedeAI.Behavior.Hunt;
                //self.centipede.bodyDirection = true;

                // This seems to cause an index error when the Nightterror is in the same room as a player. Leave this off unless you know how to fix it.

                // Didn't get any errors when I tried it? If you get it again use the .pdb to get a line number
                // Disabled though because it made it less scary - Niko
                /*if (NightTerrorInfo.TryGetValue(self.centipede, out var NTInfo))
                {

                    if (NTInfo.fleeing == 0)
                    {
                        for (int i = 0; i < self.centipede.room.game.Players.Count; i++)
                        {
                            if (!(self.centipede.room.game.Players[i].realizedCreature as Player).dead)
                            {
                                self.tracker.SeeCreature(self.centipede.room.game.Players[i]);
                                self.creature.abstractAI.RealAI.SetDestination(self.centipede.room.game.Players[i].pos);
                                self.creature.abstractAI.destination = self.centipede.room.game.Players[i].pos;
                                self.creature.abstractAI.migrationDestination = new WorldCoordinate?(self.centipede.room.game.Players[i].pos);
                                self.creature.abstractAI.InternalSetDestination(self.centipede.room.game.Players[i].pos);
                                self.behavior = CentipedeAI.Behavior.Hunt;
                                self.run = 1000;
                                break;
                            }
                        }
                    }

                    // turning off the fear code for now
                    /*if (NTInfo.fleeing > 0)  
                    {
                        NTInfo.fleeing--;

                        self.SetDestination(new WorldCoordinate(self.centipede.abstractCreature.pos.room, (int)NTInfo.fleeTo.x, (int)NTInfo.fleeTo.y, self.centipede.abstractCreature.pos.abstractNode));
                    }/
                }*/
                #endregion
            }
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
                self.abstractCreature.personality.nervous = 0.4577313f;
                self.abstractCreature.personality.sympathy = 0.4754336f;

                abstractCreature.ignoreCycle = true;

                if (!NightTerrorInfo.TryGetValue(self, out _))
                    NightTerrorInfo.Add(self, new NightTerrorData());
                
                self.bodyChunks = new BodyChunk[19];
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
}