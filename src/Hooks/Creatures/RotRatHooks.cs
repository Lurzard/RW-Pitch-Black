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

public static class RotRatHooks
{
    private static readonly CreatureTemplate.Type rotrat = Enums.CreatureTemplateType.Rotrat;
    
    public static void Apply()
    {
        On.MouseAI.ctor += MouseAI_ctor;
        On.MouseAI.Update += MouseAI_Update;
        On.MouseAI.IUseARelationshipTracker_ModuleToTrackRelationship += Preyrelationshipfix;
        On.LanternMouse.ctor += LanternMouse_ctor;
        On.LanternMouse.InitiateGraphicsModule += LanternMouse_InitiateGraphicsModule;
        On.LanternMouse.Update += LanternMouse_Update;
        On.LanternMouse.CarryObject += LanternMouse_CarryObject;
    }
    
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
}