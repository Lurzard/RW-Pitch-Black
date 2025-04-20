using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using UnityEngine;
using static System.Reflection.BindingFlags;
using static Mono.Cecil.Cil.OpCodes;
using System.Linq;
using System.Collections.Generic;
using MoreSlugcats;
using Random = UnityEngine.Random;
using RWCustom;

namespace PitchBlack;

static class LMLLHooks
{
    internal static void Apply()
    {
        new Hook(typeof(DaddyLongLegs).GetMethod("get_SizeClass", Public | NonPublic | Instance), AdjustSizeClass);
        On.MoreSlugcats.StowawayBugAI.WantToEat += (On.MoreSlugcats.StowawayBugAI.orig_WantToEat orig, StowawayBugAI self, CreatureTemplate.Type input) => input != PBCreatureTemplateType.LMiniLongLegs && orig(self, input);
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
    }
    static void IL_Player_CaneatMeat(ILContext il)
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
    static void DaddyGraphics_DrawSprites(On.DaddyGraphics.orig_DrawSprites orig, DaddyGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.daddy.Template.type == PBCreatureTemplateType.LMiniLongLegs) {
            for (int i = 0; i < self.daddy.bodyChunks.Length; i++) {
                sLeaser.sprites[self.BodySprite(i)].scale = (self.owner.bodyChunks[i].rad * 1.1f + 2f) / 8f;
            }
        }
    }
    static void On_DaddyLongLegs_Act(On.DaddyLongLegs.orig_Act orig, DaddyLongLegs self, int legsGrabbing)
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
    static bool Player_IsObjectThrowable(On.Player.orig_IsObjectThrowable orig, Player self, PhysicalObject obj)
    {
        bool res = orig(self, obj);
        if (obj is LittleLongLegs) return true;
        return res;
    }
    static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        Player.ObjectGrabability res = orig(self, obj);
        if (obj is LittleLongLegs lmll) {
            if (lmll.State.alive) {
                if (lmll.FoodPoints >= LittleLongLegs.TooMuchFoodToBeCarried) {
                    return Player.ObjectGrabability.CantGrab;
                }
                return Player.ObjectGrabability.TwoHands;
            }
            else
                return Player.ObjectGrabability.BigOneHand;
        }
        return res;
    }
    static bool AdjustSizeClass(Func<DaddyLongLegs, bool> orig, DaddyLongLegs self) {
        return self.Template.type != PBCreatureTemplateType.LMiniLongLegs && orig(self);
    }
    static CreatureTemplate.Relationship DaddyAI_IUseARelationshipTracker_UpdateDynamicRelationship(On.DaddyAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, DaddyAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        CreatureTemplate.Relationship res = orig(self, dRelation);
        if (self.creature?.creatureTemplate.type is CreatureTemplate.Type tp && tp != CreatureTemplate.Type.BrotherLongLegs && tp != PBCreatureTemplateType.LMiniLongLegs && dRelation.trackerRep?.representedCreature is AbstractCreature c && c.creatureTemplate.type == PBCreatureTemplateType.LMiniLongLegs)
        {
            bool flag = self.daddy is DaddyLongLegs d && c.realizedCreature is DaddyLongLegs d2 && d.eyeColor == d2.eyeColor && d.effectColor == d2.effectColor;
            res.type = flag ? CreatureTemplate.Relationship.Type.Ignores : CreatureTemplate.Relationship.Type.Eats;
            res.intensity = flag ? 0f : 1f;
        }
        if (self.creature?.realizedCreature is LittleLongLegs lmll && dRelation.trackerRep?.representedCreature?.realizedCreature is Creature crit && crit.Template.type != PBCreatureTemplateType.LMiniLongLegs && lmll.mainBodyChunk.rad > crit.mainBodyChunk.rad*1.1f) {
            res.type = CreatureTemplate.Relationship.Type.Eats;
            res.intensity = 1f;
        }
        return res;
    }
    static void On_DaddyLongLegs_ctor(On.DaddyLongLegs.orig_ctor orig, DaddyLongLegs self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (abstractCreature.creatureTemplate.type == PBCreatureTemplateType.LMiniLongLegs)
        {
            if (world.region?.regionParams is Region.RegionParams r)
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
    static void IL_DaddyLongLegs_ctor(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(x => x.MatchNewarr<BodyChunk>()))
        {
            c.Emit(Ldarg_0);
            c.EmitDelegate((int length, DaddyLongLegs self) => {
                if (self.Template.type == PBCreatureTemplateType.LMiniLongLegs) {
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
                if (self.Template.type == PBCreatureTemplateType.LMiniLongLegs) {
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
            c.EmitDelegate((int length, DaddyLongLegs self) => self.Template.type == PBCreatureTemplateType.LMiniLongLegs ? Random.Range(3, 5) : length);
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
                if (self.Template.type == PBCreatureTemplateType.LMiniLongLegs)
                {
                    for (var i = 0; i < sz.Count; i++)
                        sz[i] *= .4f;
                }
            });
        }
        else
            Plugin.logger.LogError("Couldn't ILHook DaddyLongLegs.ctor (part 4)!");
    }
    static void DaddyLongLegs_Update(On.DaddyLongLegs.orig_Update orig, DaddyLongLegs self, bool eu)
    {
        orig(self, eu);
        if (self.Template.type == PBCreatureTemplateType.LMiniLongLegs) {
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
    static void IL_DaddyLongLegs_Act(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After, x => x.MatchLdcR4(0.6f)))
        {
            c.Emit(Ldarg_0);
            c.EmitDelegate((float num, DaddyLongLegs self) => self.safariControlled && self.Template.type == PBCreatureTemplateType.LMiniLongLegs ? 0.45f : num);
        }
        else
            Plugin.logger.LogError("Couldn't ILHook DaddyLongLegs.Act!");
    }
    static void SLOracleBehaviorHasMark_CreatureJokeDialog(On.SLOracleBehaviorHasMark.orig_CreatureJokeDialog orig,  SLOracleBehaviorHasMark self) {
        orig(self);
        if (self.CheckStrayCreatureInRoom() == PBCreatureTemplateType.LMiniLongLegs) {
            self.dialogBox.NewMessage(self.Translate("Oh no."), 10);
        }
    }
    static void SSOracleBehavior_CreatureJokeDialog(On.SSOracleBehavior.orig_CreatureJokeDialog orig, SSOracleBehavior self) {
        orig(self);
        if (self.CheckStrayCreatureInRoom() == PBCreatureTemplateType.LMiniLongLegs) {
            self.dialogBox.NewMessage(self.Translate("Take your friend with you. Please. I beg you.."), 10);
        }
    }
    static float OverseerAbstractAI_HowInterestingIsCreature(On.OverseerAbstractAI.orig_HowInterestingIsCreature orig, OverseerAbstractAI self, AbstractCreature testCrit) {
        if (testCrit?.creatureTemplate.type == PBCreatureTemplateType.LMiniLongLegs)
        {
            var num = .2f;
            if (testCrit.state.dead)
                num /= 10f;
            num *= testCrit.Room.AttractionValueForCreature(self.parent.creatureTemplate.type);
            return num * Mathf.Lerp(.5f, 1.5f, self.world.game.SeededRandom(self.parent.ID.RandomSeed + testCrit.ID.RandomSeed));
        }
        return orig(self, testCrit);
    }
}