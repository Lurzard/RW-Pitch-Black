using System;
using System.Runtime.CompilerServices;
using MonoMod.RuntimeDetour;
using UnityEngine;
using static System.Reflection.BindingFlags;

namespace PitchBlack;

public static class NTHooks
{
    #region Not Hooks
    
    public static void NightTerrorReleasePlayersInGrasp(this Creature self) {
        if (self.abstractCreature.creatureTemplate.IsNightTerror()) {
            for (int i = 0; i < self.grasps.Length; i++) {
                if (self.grasps[i]?.grabbed is Player) {
                    self.ReleaseGrasp(i);
                }
            }
        }
    }
    
    #endregion
    
    public static void Apply()
    {
        new Hook(typeof(Centipede).GetMethod("get_Red", Public | NonPublic | Instance), (Func<Centipede, bool> orig, Centipede self) => self.Template.IsNightTerror() || orig(self));
        On.AbstractCreature.Update += AbstractCreature_Update;
        On.AbstractCreature.ctor += AbstractCreature_ctor; 
        On.Creature.CanBeGrabbed += Creature_CanBeGrabbed;
        On.SporeCloud.Update += SporeCloud_Update;
        On.Centipede.ctor += Centipede_ctor;
        On.CentipedeGraphics.ctor += CentipedeGraphics_ctor;
        On.Centipede.Violence += Centipede_Violence;
        On.CentipedeAI.DoIWantToShockCreature += CentipedeAI_DoIWantToShockCreature;
        On.Centipede.Shock += Centipede_Shock;
        On.Centipede.ShortCutColor += Centipede_ShortCutColor;
        On.CentipedeAI.ctor += CentipedeAI_ctor;
        On.PreyTracker.AddPrey += PreyTracker_AddPrey;
        On.PreyTracker.ForgetPrey += PreyTracker_ForgetPrey;
        On.PreyTracker.Update += PreyTracker_Update;
        On.AbstractCreatureAI.CanRoamThroughRoom += AbstractCreatureAI_CanRoamThroughRoom;
    }
    
    private static bool AbstractCreatureAI_CanRoamThroughRoom(On.AbstractCreatureAI.orig_CanRoamThroughRoom orig, AbstractCreatureAI self, int room)
    {
        bool result = orig(self, room);
        if (self.parent.creatureTemplate.IsNightTerror()) {
            return true;
        }
        return result;
    }

    private static void PreyTracker_Update(On.PreyTracker.orig_Update orig, PreyTracker self)
    {
        orig(self);
        if (self.AI.creature.creatureTemplate.IsNightTerror()) {
            self.prey.RemoveAll(x => x.critRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat && x.critRep.representedCreature.Room.realizedRoom?.ValidTrackRoom() == false);
        }
    }

    private static void PreyTracker_ForgetPrey(On.PreyTracker.orig_ForgetPrey orig, PreyTracker self, AbstractCreature crit)
    {
        if (self.AI.creature.creatureTemplate.IsNightTerror() && crit.creatureTemplate.type == CreatureTemplate.Type.Slugcat) {
            bool? validRoom = crit.Room.realizedRoom?.ValidTrackRoom();
            if (validRoom == null || validRoom == true) {
                return;
            }
        }
        orig(self, crit);
    }

    private static void PreyTracker_AddPrey(On.PreyTracker.orig_AddPrey orig, PreyTracker self, Tracker.CreatureRepresentation creature)
    {
        if (self.AI.creature.creatureTemplate.IsNightTerror() && creature.representedCreature.creatureTemplate.type != CreatureTemplate.Type.Slugcat) {
            return;
        }
        orig(self, creature);
    }

    private static void AbstractCreature_ctor(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
    {
        orig(self, world, creatureTemplate, realizedCreature, pos, ID);
        Plugin.KILLIT.Add(self, new StrongBox<int>(0));
        if (creatureTemplate.IsNightTerror()) {
            if (Plugin.pursuerTracker.TryGetValue(world.game, out var trackers)) {
                trackers.Add(new NTTracker(world.game){pursuer = self});
            }
            self.lavaImmune = true;
            self.voidCreature = true;
            self.ignoreCycle = true;
            self.HypothermiaImmune = true;
        }
    }
    
    // Prevent the Night Terror from being grabbed if it's not dead or stunned
    private static bool Creature_CanBeGrabbed(On.Creature.orig_CanBeGrabbed orig, Creature self, Creature grabber)
    {
        bool result = orig(self, grabber);
        if (self.Template.IsNightTerror()) {
            if (self.dead || self.stun > 0) {
                return true;
            }
            Debug.Log("Removed Grasp");
            return false;
        }
        return result;
    }

    #region update method hooks

    private static void AbstractCreature_Update(On.AbstractCreature.orig_Update orig, AbstractCreature self, int time)
    {
        orig(self, time);
        if (Plugin.NTAbstractCWT.TryGetValue(self, out NightTerror nt)) {
            nt.TryRevive();
            //Debug.Log($"Pitch Black NightTerror Revive Status:\n\tIs dead? = {!self.state.alive || self.realizedCreature != null && self.realizedCreature.dead} | {nameof(nt.timeUntilRevive)} = {nt.timeUntilRevive}");
        }
    }

    private static void SporeCloud_Update(On.SporeCloud.orig_Update orig, SporeCloud self, bool eu)
    {
        orig(self, eu);

        if (!self.nonToxic) {
            foreach (AbstractCreature abstrCrit in self.room.abstractRoom.creatures) {
                if (Plugin.NTAbstractCWT.TryGetValue(abstrCrit, out NightTerror nt)) {
                    nt.diedToSporeCloud = true;
                }
            }
        }
    }

    #endregion
    
    // Make the nightterror AI behave more like a red centipede
    private static void CentipedeAI_ctor(On.CentipedeAI.orig_ctor orig, CentipedeAI self, AbstractCreature creature, World world)
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

            // Remove unneeded Trackers
            self.modules.RemoveAll(x => x is ThreatTracker || x is RainTracker || x is DenFinder || x is InjuryTracker);
            self.utilityComparer.uTrackers.RemoveAll(x => x.module is ThreatTracker || x.module is RainTracker || x.module is DenFinder || x.module is InjuryTracker);

            // 22 makes it 1.5 times as fast as a red centipede
            self.pathFinder.stepsPerFrame = 15;
        }
    }
    
    #region Shocking Things

    private static void Centipede_Shock(On.Centipede.orig_Shock orig, Centipede self, PhysicalObject shockObj)
    {
        orig(self, shockObj);
        if (self.abstractCreature.creatureTemplate.IsNightTerror() && shockObj?.abstractPhysicalObject is AbstractCreature abstrCrit && Plugin.KILLIT.TryGetValue(abstrCrit, out StrongBox<int> timesZapped)) {
            timesZapped.Value++;
        }
    }

    private static bool CentipedeAI_DoIWantToShockCreature(On.CentipedeAI.orig_DoIWantToShockCreature orig, CentipedeAI self, AbstractCreature critter)
    {
        bool result = orig(self, critter);
        if (self.centipede.abstractCreature.creatureTemplate.IsNightTerror() && critter.realizedCreature is Player && Plugin.KILLIT.TryGetValue(critter, out StrongBox<int> timesZapped) && timesZapped.Value < 6)
        {
            return true;
        }
        return result;
    }

    #endregion
    
    // Alters the damage done to Night Terror, currently it halfs all damage
    private static void Centipede_Violence(On.Centipede.orig_Violence orig, Centipede self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus) {
        if (self.abstractCreature.creatureTemplate.IsNightTerror()) {
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
            // Change int to modify chunk length
            self.bodyChunks = new BodyChunk[20];
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
    
    private static Color Centipede_ShortCutColor(On.Centipede.orig_ShortCutColor orig, Centipede self)
    {
        Color result = orig(self);
        if (self.abstractCreature.creatureTemplate.IsNightTerror())
        {
            return Colors.NightmareColor;
        }
        return result;
    }
    
    // Sets the Night Terror highlight color
    private static void CentipedeGraphics_ctor(On.CentipedeGraphics.orig_ctor orig, CentipedeGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
        if (self.centipede.abstractCreature.creatureTemplate.IsNightTerror())
        {
            self.hue = 0.9f;
            self.saturation = 0.96f;
        }
    }
}