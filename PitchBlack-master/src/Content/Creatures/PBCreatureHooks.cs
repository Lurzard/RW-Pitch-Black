using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using On.MoreSlugcats;
using RWCustom;
using UnityEngine;
using static System.Reflection.BindingFlags;
using static Mono.Cecil.Cil.OpCodes;
using Random = UnityEngine.Random;
using VultureMaskGraphics = MoreSlugcats.VultureMaskGraphics;

namespace PitchBlack;

public static class PBCreatureHooks
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
    
    private static readonly CreatureTemplate.Type citizen = PBEnums.CreatureTemplateType.Citizen;
    private static readonly CreatureTemplate.Type littlelonglegs = PBEnums.CreatureTemplateType.LMiniLongLegs;
    // MiscUtils has a check for Night Terror
    private static readonly CreatureTemplate.Type rotrat = PBEnums.CreatureTemplateType.Rotrat;
    private static readonly CreatureTemplate.Type umbra = PBEnums.CreatureTemplateType.UmbraScav;
    
    /// <summary>
    /// Merged hooks from all the different fisobs creature hooks classes into one class.
    /// </summary>
    public static void Apply()
    {
        // Lantern Mouse (Rot Rat)
        On.MouseAI.ctor += MouseAI_ctor;
        On.MouseAI.Update += MouseAI_Update;
        On.MouseAI.IUseARelationshipTracker_ModuleToTrackRelationship += Preyrelationshipfix;
        On.LanternMouse.ctor += LanternMouse_ctor;
        On.LanternMouse.InitiateGraphicsModule += LanternMouse_InitiateGraphicsModule;
        On.LanternMouse.Update += LanternMouse_Update;
        On.LanternMouse.CarryObject += LanternMouse_CarryObject;
        
        // Centipede (Night Terror)
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
        
        // DaddyLongLegs (LMLL)
        new Hook(typeof(DaddyLongLegs).GetMethod("get_SizeClass", Public | NonPublic | Instance), AdjustSizeClass);
        StowawayBugAI.WantToEat += (orig, self, input) => input != PBEnums.CreatureTemplateType.LMiniLongLegs && orig(self, input);
        On.SLOracleBehaviorHasMark.CreatureJokeDialog += SLOracleBehaviorHasMark_CreatureJokeDialog;
        On.SSOracleBehavior.CreatureJokeDialog += SSOracleBehavior_CreatureJokeDialog;
        On.OverseerAbstractAI.HowInterestingIsCreature += OverseerAbstractAI_HowInterestingIsCreature;
        On.DaddyLongLegs.Update += DaddyLongLegs_Update;
        On.DaddyLongLegs.ctor += On_DaddyLongLegs_ctor;
        On.DaddyAI.IUseARelationshipTracker_UpdateDynamicRelationship += DaddyAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        On.DaddyLongLegs.Act += On_DaddyLongLegs_Act;
        On.DaddyGraphics.DrawSprites += DaddyGraphics_DrawSprites;
        On.Player.Grabability += Player_Grabability;
        On.Player.IsObjectThrowable += Player_IsObjectThrowable;
        IL.DaddyLongLegs.ctor += IL_DaddyLongLegs_ctor;
        IL.DaddyLongLegs.Act += IL_DaddyLongLegs_Act;
        IL.Player.CanEatMeat += IL_Player_CaneatMeat;
        
        // Some hooks in CreatureEdits are for UmbraScav and Citizen
        
        // Scavenger (Citizen)
        On.Scavenger.Update += Scavenger_Update;
        On.Scavenger.Grab += Scavenger_Grab;
        On.Scavenger.Collide += Scavenger_Collide;
        On.ScavengerAbstractAI.InitGearUp += ScavengerAbstractAI_InitGearUp;
        
        // Scavenger (UmbraScav)
        //On.ScavengerGraphics.InitiateSprites += ScavengerGraphics_InitiateSprites;
        //On.ScavengerGraphics.ctor += ScavengerGraphics_ctor;
        //On.MoreSlugcats.VultureMaskGraphics.GenerateColor += VultureMaskGraphics_GenerateColor;
        //On.ScavengerGraphics.AddToContainer += ScavengerGraphics_AddToContainer;
        //On.Scavenger.SetUpCombatSkills += Scavenger_SetUpCombatSkills;
        //On.ScavengerGraphics.IndividualVariations.ctor += IndividualVariations_ctor;
        //On.Scavenger.Throw += Scavenger_Throw;
        //On.Scavenger.GrabbedObjectSnatched += Scavenger_GrabbedObjectSnatched;
    }
    
    #region Rot Rat

    private static void LanternMouse_CarryObject(On.LanternMouse.orig_CarryObject orig, LanternMouse self)
    {
        if (!self.safariControlled && self.grasps[0].grabbed is Creature && self.AI.DynamicRelationship((self.grasps[0].grabbed as Creature).abstractCreature).type != CreatureTemplate.Relationship.Type.Eats) 
        {
            self.AI.preyTracker.ForgetPrey((self.grasps[0].grabbed as Creature).abstractCreature);
            self.LoseAllGrasps();
            return;   
        }
        PhysicalObject grabbed = self.grasps[0].grabbed;
        float num = self.mainBodyChunk.rad + self.grasps[0].grabbed.bodyChunks[self.grasps[0].chunkGrabbed].rad;
        Vector2 a = -Custom.DirVec(self.mainBodyChunk.pos, grabbed.bodyChunks[self.grasps[0].chunkGrabbed].pos) * (num - Vector2.Distance(self.mainBodyChunk.pos, grabbed.bodyChunks[self.grasps[0].chunkGrabbed].pos));
        float num2 = grabbed.bodyChunks[self.grasps[0].chunkGrabbed].mass / (self.mainBodyChunk.mass + grabbed.bodyChunks[self.grasps[0].chunkGrabbed].mass);
        num2 *= 0.2f * (1f - self.AI.stuckTracker.Utility());
        self.mainBodyChunk.pos += a * num2;
        self.mainBodyChunk.vel += a * num2;
        grabbed.bodyChunks[self.grasps[0].chunkGrabbed].pos -= a * (1f - num2);
        grabbed.bodyChunks[self.grasps[0].chunkGrabbed].vel -= a * (1f - num2);
        Vector2 vector = self.mainBodyChunk.pos + Custom.DirVec(self.bodyChunks[1].pos, self.mainBodyChunk.pos) * num;
        Vector2 vector2 = grabbed.bodyChunks[self.grasps[0].chunkGrabbed].vel - self.mainBodyChunk.vel;
        grabbed.bodyChunks[self.grasps[0].chunkGrabbed].vel = self.mainBodyChunk.vel;
        if (self.enteringShortCut == null && (vector2.magnitude * grabbed.bodyChunks[self.grasps[0].chunkGrabbed].mass > 30f || !Custom.DistLess(vector, grabbed.bodyChunks[self.grasps[0].chunkGrabbed].pos, 70f + grabbed.bodyChunks[self.grasps[0].chunkGrabbed].rad)))
        {
            self.LoseAllGrasps();
        }
        else
        {
            grabbed.bodyChunks[self.grasps[0].chunkGrabbed].MoveFromOutsideMyUpdate(self.abstractCreature.world.game.evenUpdate, vector);
        }
        if (self.grasps[0] != null)
        {
            for (int i = 0; i < 2; i++)
            {
                self.grasps[0].grabbed.PushOutOf(self.bodyChunks[i].pos, self.bodyChunks[i].rad, self.grasps[0].chunkGrabbed);
            }
        }
    }

    private static AIModule Preyrelationshipfix(On.MouseAI.orig_IUseARelationshipTracker_ModuleToTrackRelationship orig, MouseAI self, CreatureTemplate.Relationship relationship)
    {
        if(relationship.type == CreatureTemplate.Relationship.Type.Eats)
        {
            return self.preyTracker;
        }
        return orig(self, relationship);
    }

    private static void LanternMouse_ctor(On.LanternMouse.orig_ctor orig, LanternMouse self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if(self.Template.type == rotrat)
        {
            Random.State state = Random.state;
            Random.InitState(self.abstractCreature.ID.RandomSeed);
            float hue;
            if (Random.value < 0.01f) {
                hue = 0.8532407407407407f;
                Debug.Log("the mouse behind the slaughter....");
                // hehe purple mouse.
            }
            else if (Random.value < 0.05f) {
                hue = Mathf.Lerp(0.444f, 0.527f, Random.value);
                //shock cyans?
            }
            else if (Random.value < 0.2f) {
                hue = Mathf.Lerp(0f, 0.05f, Random.value);
                //shock reds?
            }
            else {
                hue = Mathf.Lerp(0.055f, 0.125f, Random.value);
                //shock oranges + yellows?
            }
            HSLColor color = new HSLColor(hue, 1f, Random.Range(0.4f,0.8f));
            float value = Random.value;
            self.iVars = new LanternMouse.IndividualVariations(value, color);
            Random.state = state;
        }
    }

    private static void LanternMouse_InitiateGraphicsModule(On.LanternMouse.orig_InitiateGraphicsModule orig, LanternMouse self)
    {
        if (self.Template.type == rotrat) {
            self.graphicsModule = new RotratGraphics(self);
            self.graphicsModule.Reset();
        }
        else {
            orig(self);
        }
    }

    private static void LanternMouse_Update(On.LanternMouse.orig_Update orig, LanternMouse self, bool eu)
    {
        orig(self, eu);
        if(self.Template.type == rotrat)
        {
            if(self.grasps[0] != null)
            {
                self.CarryObject();
            }
            if (self.AI.behavior == MouseAI.Behavior.Hunt)
            {
                if (self.AI.preyTracker.MostAttractivePrey != null)
                {
                    Tracker.CreatureRepresentation prey = self.AI.preyTracker.MostAttractivePrey;
                    Creature realprey = prey.representedCreature.realizedCreature;
                    if (Custom.DistLess(prey.representedCreature.pos, self.abstractCreature.pos, 4f))
                    {
                        self.Squeak(1f);
                        if (self.grasps[0] == null && (realprey.dead || realprey.Stunned))
                        {
                            self.Grab(prey.representedCreature.realizedObject, 0, 0, Creature.Grasp.Shareability.CanNotShare, 0.5f, false, true);
                            self.AI.behavior = MouseAI.Behavior.ReturnPrey;
                        }
                        else
                        {
                            if(realprey.TotalMass < self.TotalMass*1.5)
                            {
                                realprey.Violence(self.mainBodyChunk, Custom.DirVec(self.mainBodyChunk.pos, realprey.mainBodyChunk.pos), realprey.mainBodyChunk, null, Creature.DamageType.Bite, Random.Range(0.6f, 1.4f), Random.Range(0.2f, 1.2f));
                                self.Grab(prey.representedCreature.realizedObject, 0, 0, Creature.Grasp.Shareability.CanNotShare, Random.Range(0.3f, 0.7f), true, true);
                                
                            }
                            else
                            {
                                if(Random.Range(0f,100f) < 20f)
                                {

                                    realprey.Violence(self.mainBodyChunk, Custom.DirVec(self.mainBodyChunk.pos, realprey.mainBodyChunk.pos), realprey.mainBodyChunk, null, Creature.DamageType.Bite, Random.Range(0.6f, 1.4f), Random.Range(0.2f, 1.2f));

                                }
                                else
                                {
                                    realprey.Stun(realprey.stun);
                                }
                                self.Grab(prey.representedCreature.realizedObject, 0, 0, Creature.Grasp.Shareability.CanNotShare, Random.Range(0.3f, 0.7f), true, false);
                            }
                            
                        }
                    }
                }
            }
        }
    }

    private static void MouseAI_Update(On.MouseAI.orig_Update orig, MouseAI self)
    {
        if(self.mouse.Template.type == rotrat)
        {
            self.preyTracker.Update();
            self.stuckTracker.Update();
            orig(self);
            AIModule aimoduule = self.utilityComparer.HighestUtilityModule();
            if (aimoduule != null && aimoduule is PreyTracker)
            {
                self.behavior = MouseAI.Behavior.Hunt;
            }
            if (self.behavior == MouseAI.Behavior.Hunt)
            {
                if (self.mouse.grasps[0] != null && self.mouse.grasps[0].grabbed is Creature && self.StaticRelationship((self.mouse.grasps[0].grabbed as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Eats)
                {
                    self.behavior = MouseAI.Behavior.ReturnPrey;
                }
                else if (self.preyTracker.MostAttractivePrey != null && !self.mouse.safariControlled)
                {
                    self.creature.abstractAI.SetDestination(self.preyTracker.MostAttractivePrey.BestGuessForPosition());
                    self.mouse.runSpeed = Mathf.Lerp(self.mouse.runSpeed, 1f, 0.08f);
                }
            }
            if (self.behavior == MouseAI.Behavior.ReturnPrey)
            {
                if (self.denFinder.GetDenPosition() != null)
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    self.creature.abstractAI.SetDestination(self.denFinder.GetDenPosition().Value);
                    Debug.Log($"rorat number {self.mouse.abstractCreature.ID.number.ToString()}: YIPPE! i found a den!");
                }
                else
                {
                    Debug.Log($"rorat number {self.mouse.abstractCreature.ID.number.ToString()}: FUCK! no den found :[");
                }
            }
            if (Input.GetKey(KeyCode.T) && Input.GetKey(KeyCode.M))
            {
                self.behavior = MouseAI.Behavior.Hunt;
                Debug.Log("RRs have been forced to hunt.");
            }
        }
        else
        {
            orig(self);
        }
    }

    private static void MouseAI_ctor(On.MouseAI.orig_ctor orig, MouseAI self, AbstractCreature creature, World world)
    {
        orig(self, creature, world);
        if(self.mouse.Template.type == rotrat)
        {
            self.AddModule(new PreyTracker(self, 3, 2f, 10f, 70f, 0.5f));
            self.utilityComparer.AddComparedModule(self.preyTracker, null, 1f, 1.5f);
            self.AddModule(new StuckTracker(self,true,false));
        }
    }

    #endregion
    
    #region Night Terror

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
            return Plugin.NightmareColor;
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

    #endregion
    
    #region LMLL

    private static void IL_Player_CaneatMeat(ILContext il)
    {
        var cursor = new ILCursor(il);
        if (cursor.TryGotoNext(MoveType.After, i => i.MatchIsinst(out _))) {
            cursor.EmitDelegate((object obj) => {
                bool flag = obj != null && obj is not LittleLongLegs;
                flag = flag || (obj is LittleLongLegs lmll && lmll.dead && lmll.FoodPoints < LittleLongLegs.TooMuchFoodToBeCarried);
                Debug.Log($"Object is: {obj?.GetType()}, return will be {flag}, foodpoints are {(obj as IPlayerEdible)?.FoodPoints}");
                return flag;
            });
        }
        else {
            Plugin.logger.LogDebug($"IL error with {nameof(IL_Player_CaneatMeat)}");
        }
    }

    private static void DaddyGraphics_DrawSprites(On.DaddyGraphics.orig_DrawSprites orig, DaddyGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.daddy.Template.type == littlelonglegs) {
            for (int i = 0; i < self.daddy.bodyChunks.Length; i++) {
                sLeaser.sprites[self.BodySprite(i)].scale = (self.owner.bodyChunks[i].rad * 1.1f + 2f) / 8f;
            }
        }
    }

    private static void On_DaddyLongLegs_Act(On.DaddyLongLegs.orig_Act orig, DaddyLongLegs self, int legsGrabbing)
    {
        orig(self, legsGrabbing);
        for (int i = 0; i < self.eatObjects.Count; i++) {
            if (self is LittleLongLegs lmll && self.eatObjects[i].progression > 1f && self.eatObjects[i].chunk.owner is Creature crit) {
                int increase;
                if (crit is IPlayerEdible playerEdible) {
                    lmll.FoodPoints += increase = playerEdible.FoodPoints;
                }
                else {
                    lmll.FoodPoints += increase = crit.Template.meatPoints;
                }
                self.State.meatLeft += increase;
                lmll.LittleLongLegsSizeChange(increase);
            }
        }
    }

    private static bool Player_IsObjectThrowable(On.Player.orig_IsObjectThrowable orig, Player self, PhysicalObject obj)
    {
        bool res = orig(self, obj);
        if (obj is LittleLongLegs) return true;
        return res;
    }

    private static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        Player.ObjectGrabability res = orig(self, obj);
        if (obj is LittleLongLegs lmll)
        {
            if (lmll.State.alive) {
                if (lmll.FoodPoints >= LittleLongLegs.TooMuchFoodToBeCarried) {
                    return Player.ObjectGrabability.CantGrab;
                }
                return Player.ObjectGrabability.TwoHands;
            }

            return Player.ObjectGrabability.BigOneHand;
        }
        return res;
    }

    private static bool AdjustSizeClass(Func<DaddyLongLegs, bool> orig, DaddyLongLegs self) {
        return self.Template.type != littlelonglegs && orig(self);
    }

    private static CreatureTemplate.Relationship DaddyAI_IUseARelationshipTracker_UpdateDynamicRelationship(On.DaddyAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, DaddyAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        CreatureTemplate.Relationship res = orig(self, dRelation);
        if (self.creature?.creatureTemplate.type is { } tp 
            && tp != CreatureTemplate.Type.BrotherLongLegs 
            && tp != littlelonglegs 
            && dRelation.trackerRep?.representedCreature is { } c 
            && c.creatureTemplate.type == littlelonglegs) {
            bool flag = self.daddy is { } d && c.realizedCreature is DaddyLongLegs d2 && d.eyeColor == d2.eyeColor && d.effectColor == d2.effectColor;
            res.type = flag ? CreatureTemplate.Relationship.Type.Ignores : CreatureTemplate.Relationship.Type.Eats;
            res.intensity = flag ? 0f : 1f;
        }
        if (self.creature?.realizedCreature is LittleLongLegs lmll 
            && dRelation.trackerRep?.representedCreature?.realizedCreature is { } crit 
            && crit.Template.type != PBEnums.CreatureTemplateType.LMiniLongLegs 
            && lmll.mainBodyChunk.rad > crit.mainBodyChunk.rad*1.1f) {
            res.type = CreatureTemplate.Relationship.Type.Eats;
            res.intensity = 1f;
        }
        return res;
    }

    private static void On_DaddyLongLegs_ctor(On.DaddyLongLegs.orig_ctor orig, DaddyLongLegs self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (abstractCreature.creatureTemplate.type == littlelonglegs)
        {
            if (world.region?.regionParams is { } r)
            {
                self.effectColor = r.corruptionEffectColor;
                self.eyeColor = r.corruptionEyeColor;
                self.colorClass = r.corruptionEyeColor == r.corruptionEffectColor;
            }
            else
            {
                self.effectColor = Color.Lerp(new(.7f, .7f, .4f), Color.gray, .5f);
                self.eyeColor = Color.Lerp(new(.5f, .3f, 0f), Color.gray, .5f);
                self.colorClass = false;
            }
        }
    }

    private static void IL_DaddyLongLegs_ctor(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(x => x.MatchNewarr<BodyChunk>()))
        {
            c.Emit(Ldarg_0);
            c.EmitDelegate((int length, DaddyLongLegs self) => {
                if (self.Template.type == littlelonglegs) {
                    if (Random.value >= 0.94f) {
                        return 3;
                    }
                    return 2;
                }
                return length;
            });
        }
        else
            Plugin.logger.LogError("Couldn't ILHook DaddyLongLegs.ctor (part 1)!");
        if (c.TryGotoNext(MoveType.After, x => x.MatchNewobj<BodyChunk>()))
        {
            c.Emit(Ldarg_0);
            c.EmitDelegate((BodyChunk chunk, DaddyLongLegs self) =>
            {
                if (self.Template.type == littlelonglegs) {
                    chunk.rad *= .5f;
                    chunk.mass *= 0.08f;
                }
                return chunk;
            });
        }
        else
            Plugin.logger.LogError("Couldn't ILHook DaddyLongLegs.ctor (part 2)!");
        if (c.TryGotoNext(MoveType.After, x => x.MatchNewarr<DaddyTentacle>()) && c.TryGotoNext(x => x.MatchNewarr<DaddyTentacle>()))
        {
            c.Emit(Ldarg_0);
            c.EmitDelegate((int length, DaddyLongLegs self) => self.Template.type == littlelonglegs ? Random.Range(3, 5) : length);
        }
        else
            Plugin.logger.LogError("Couldn't ILHook DaddyLongLegs.ctor (part 3)!");
        if (c.TryGotoNext(MoveType.After, x => x.MatchStfld<PhysicalObject>("appendages")))
        {
            c.Emit(Ldarg_0);
            c.Emit(Ldloc_S, il.Body.Variables.First(x =>
            {
                var nm = x.VariableType.FullName;
                return nm.Contains("List") && nm.Contains("Single");
            }));
            c.EmitDelegate((DaddyLongLegs self, List<float> sz) =>
            {
                if (self.Template.type == littlelonglegs)
                {
                    for (var i = 0; i < sz.Count; i++)
                        sz[i] *= .4f;
                }
            });
        }
        else
            Plugin.logger.LogError("Couldn't ILHook DaddyLongLegs.ctor (part 4)!");
    }

    private static void DaddyLongLegs_Update(On.DaddyLongLegs.orig_Update orig, DaddyLongLegs self, bool eu)
    {
        orig(self, eu);
        if (self.Template.type == littlelonglegs) {
            if (self.Consious && self.moving)
            {
                for (var i = 0; i < self.bodyChunks.Length; i++)
                    self.bodyChunks[i].vel.x += .1f * Math.Sign(self.bodyChunks[i].vel.x);
            }
            foreach (var grasp in self.grabbedBy) {
                if (grasp.grabber is Player) {
                    grasp.pacifying = true;
                }
            }
            if (self is LittleLongLegs lmll && lmll.FoodPoints > 12) {
                lmll.splitCounter++;
                foreach(BodyChunk chunk in self.bodyChunks) {
                    chunk.pos += Random.Range(5f, 7f)*Custom.RNV();
                }
                if (Random.value >= 0.8f) {
                    self.Stun(30);
                }
                if (lmll.splitCounter >= 100) {
                    if (Random.value <= 0.95f) {
                        lmll.LittleLongLegsSplit();
                        self.Stun(55);
                    }
                    else {
                        AbstractCreature abstractCreature = new AbstractCreature(self.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BrotherLongLegs), null, self.room.GetWorldCoordinate(self.mainBodyChunk.pos), self.room.game.GetNewID());
                        self.room.abstractRoom.AddEntity(abstractCreature);
                        abstractCreature.RealizeInRoom();
                        self.room.RemoveObject(self);
                        self.Destroy();
                        self.abstractCreature.Room.RemoveEntity(self.abstractCreature);
                        self.abstractCreature.Destroy();
                    }
                }
            }
        }
    }

    private static void IL_DaddyLongLegs_Act(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After, x => x.MatchLdcR4(0.6f)))
        {
            c.Emit(Ldarg_0);
            c.EmitDelegate((float num, DaddyLongLegs self) => self.safariControlled && self.Template.type == littlelonglegs ? 0.45f : num);
        }
        else
            Plugin.logger.LogError("Couldn't ILHook DaddyLongLegs.Act!");
    }

    private static void SLOracleBehaviorHasMark_CreatureJokeDialog(On.SLOracleBehaviorHasMark.orig_CreatureJokeDialog orig,  SLOracleBehaviorHasMark self) {
        orig(self);
        if (self.CheckStrayCreatureInRoom() == littlelonglegs) {
            self.dialogBox.NewMessage(self.Translate("Oh no."), 10);
        }
    }

    private static void SSOracleBehavior_CreatureJokeDialog(On.SSOracleBehavior.orig_CreatureJokeDialog orig, SSOracleBehavior self) {
        orig(self);
        if (self.CheckStrayCreatureInRoom() == littlelonglegs) {
            self.dialogBox.NewMessage(self.Translate("Take your friend with you. Please. I beg you.."), 10);
        }
    }

    private static float OverseerAbstractAI_HowInterestingIsCreature(On.OverseerAbstractAI.orig_HowInterestingIsCreature orig, OverseerAbstractAI self, AbstractCreature testCrit) {
        if (testCrit?.creatureTemplate.type == littlelonglegs)
        {
            var num = .2f;
            if (testCrit.state.dead)
                num /= 10f;
            num *= testCrit.Room.AttractionValueForCreature(self.parent.creatureTemplate.type);
            return num * Mathf.Lerp(.5f, 1.5f, self.world.game.SeededRandom(self.parent.ID.RandomSeed + testCrit.ID.RandomSeed));
        }
        return orig(self, testCrit);
    }

    #endregion
    
    #region Citizen

    private static void ScavengerAbstractAI_InitGearUp(On.ScavengerAbstractAI.orig_InitGearUp orig, ScavengerAbstractAI self)
    {
        if (self.parent.creatureTemplate.type == citizen)
        {
            return;
        }

        orig(self);
    }

    private static void Scavenger_Collide(On.Scavenger.orig_Collide orig, Scavenger self, PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        if (self.Template.type == citizen)
        {
            return;
        }

        orig(self, otherObject, myChunk, otherChunk);
    }

    private static bool Scavenger_Grab(On.Scavenger.orig_Grab orig, Scavenger self, PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
    {
        if (self.Template.type == citizen)
        {
            return false;
        }

        return orig(self, obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);
    }

    private static void Scavenger_Update(On.Scavenger.orig_Update orig, Scavenger self, bool eu)
    {
        // Doing this twice before and after orig, to guarantee intended collision. An IL hook would probably be about as effective.
        if (self.Template.type == citizen)
        {
            self.CollideWithObjects = false;
        }

        orig(self, eu);
        if (self.Template.type == citizen)
        {
            self.CollideWithObjects = false;
        }
    }

    #endregion
    
    #region UmbraScav
    
    private static void Scavenger_GrabbedObjectSnatched(On.Scavenger.orig_GrabbedObjectSnatched orig, Scavenger self, PhysicalObject grabbedObject, Creature thief)
    {
        orig(self, grabbedObject, thief);
        if (self.Template.type == umbra)
        {
            self.AI.agitation = 1f;
        }
    }

    private static void Scavenger_Throw(On.Scavenger.orig_Throw orig, Scavenger self, Vector2 throwDir)
    {
        orig(self, throwDir);
        if (self.Template.type == umbra)
        {
            if (self.grasps[0].grabbed is Spear)
            {
                self.grasps[0].grabbed.firstChunk.vel = throwDir * Mathf.Max(20f, (self.grasps[0].grabbed as Weapon).exitThrownModeSpeed + 5f);
            }
        }
    }

    private static void ScavengerGraphics_AddToContainer(On.ScavengerGraphics.orig_AddToContainer orig, ScavengerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        orig(self, sLeaser, rCam, newContatiner);
        if (self.scavenger.Template.type == umbra)
        {
            FContainer fcontainer = rCam.ReturnFContainer("Foreground"); //this fixes the shader issue, proper alpha implemented!
            for (int k = self.TotalSprites - 2; k < self.TotalSprites; k++)
            {
                fcontainer.AddChild(sLeaser.sprites[k]);
            }
        }
    }

    private static void VultureMaskGraphics_GenerateColor(On.MoreSlugcats.VultureMaskGraphics.orig_GenerateColor orig, MoreSlugcats.VultureMaskGraphics self, int colorSeed)
    {
        orig(self, colorSeed);
        if (self.maskType == UmbraMask.UMBRA)
        {
            self.ColorA = new HSLColor(1f, 1f, 1f);
            self.ColorB = new HSLColor(1f, 1f, 1f);
            return;
        }
    }

    private static void ScavengerGraphics_ctor(On.ScavengerGraphics.orig_ctor orig, ScavengerGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
        if (self.scavenger.Template.type == umbra)
        {
            self.maskGfx = new VultureMaskGraphics(self.scavenger, UmbraMask.UMBRA, self.MaskSprite, "UmbraMask");
            self.maskGfx.GenerateColor(self.scavenger.abstractCreature.ID.RandomSeed);
        }
    }

    private static void ScavengerGraphics_InitiateSprites(On.ScavengerGraphics.orig_InitiateSprites orig, ScavengerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            if (self.scavenger.Template.type == umbra)
            {
                sLeaser.sprites[self.TotalSprites - 1] = new FSprite("pixel", true);
                sLeaser.sprites[self.TotalSprites - 1].scale = 5f;
                sLeaser.sprites[self.TotalSprites - 1].rotation = 0f;
                sLeaser.sprites[self.TotalSprites - 1].isVisible = true;

                sLeaser.sprites[self.TotalSprites - 2] = new FSprite("Futile_White", true);
                sLeaser.sprites[self.TotalSprites - 2].shader = rCam.game.rainWorld.Shaders["FlatLight"];
                sLeaser.sprites[self.totalSprites - 2].rotation = 0f;
                sLeaser.sprites[self.TotalSprites - 2].isVisible = true;
            }
            self.AddToContainer(sLeaser, rCam, null);
        }
    }

    private static void IndividualVariations_ctor(On.ScavengerGraphics.IndividualVariations.orig_ctor orig, ref ScavengerGraphics.IndividualVariations self, Scavenger scavenger)
    {
        orig(ref self, scavenger);
        if (scavenger.Template.type == umbra)
        {
            self.generalMelanin = 0.25f;
            self.headSize = 0.4048982f;
            self.eartlerWidth = 0.5190374f;
            self.eyeSize = 0.8917776f;
            self.eyesAngle = 0.6871811f;
            self.fatness = 0.7519351f;
            self.narrowWaist = 0.2204362f;
            self.neckThickness = 0.3437042f;
            self.pupilSize = 0f;
            self.deepPupils = false;
            self.coloredPupils = 1;
            self.handsHeadColor = 0f;
            self.legsSize = 0.21172f;
            self.armThickness = 0.627722f;
            self.coloredEartlerTips = true;
            self.wideTeeth = 0.9969704f;
            self.tailSegs = 5;
        }
    }

    private static void Scavenger_SetUpCombatSkills(On.Scavenger.orig_SetUpCombatSkills orig, Scavenger self)
    {
        orig(self);
        if (self.Template.type == umbra)
        {
            self.dodgeSkill = 0.7692045f;
            self.midRangeSkill = 0.3360332f;
            self.blockingSkill = 0.4186646f;
            self.reactionSkill = 0.8545572f;
        }
    }
    
    #endregion
}