using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using UnityEngine;
using static System.Reflection.BindingFlags;
using static Mono.Cecil.Cil.OpCodes;
using System.Linq;
using System.Collections.Generic;
using MoreSlugcats;

namespace PitchBlack;

static class LMLLHooks
{
    internal static void Apply()
    {
        new Hook(typeof(DaddyLongLegs).GetMethod("get_SizeClass", Public | NonPublic | Instance), AdjustSizeClass);
        On.MoreSlugcats.StowawayBugAI.WantToEat += (On.MoreSlugcats.StowawayBugAI.orig_WantToEat orig, StowawayBugAI self, CreatureTemplate.Type input) => input != CreatureTemplateType.LMiniLongLegs && orig(self, input);
        On.SLOracleBehaviorHasMark.CreatureJokeDialog += SLOracleBehaviorHasMark_CreatureJokeDialog;
        On.SSOracleBehavior.CreatureJokeDialog += SSOracleBehavior_CreatureJokeDialog;
        On.OverseerAbstractAI.HowInterestingIsCreature += OverseerAbstractAI_HowInterestingIsCreature;
        On.DaddyLongLegs.Update += DaddyLongLegs_Update;
        On.DaddyLongLegs.ctor += On_DaddyLongLegs_ctor;
        On.DaddyAI.IUseARelationshipTracker_UpdateDynamicRelationship += DaddyAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        IL.DaddyLongLegs.ctor += IL_DaddyLongLegs_ctor;
        IL.DaddyLongLegs.Act += DaddyLongLegs_Act;
    }
    static bool AdjustSizeClass(Func<DaddyLongLegs, bool> orig, DaddyLongLegs self) {
        return self.Template.type != CreatureTemplateType.LMiniLongLegs && orig(self);
    }
    static CreatureTemplate.Relationship DaddyAI_IUseARelationshipTracker_UpdateDynamicRelationship(On.DaddyAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, DaddyAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        CreatureTemplate.Relationship res = orig(self, dRelation);
        if (self.creature?.creatureTemplate.type is CreatureTemplate.Type tp && tp != CreatureTemplate.Type.BrotherLongLegs && tp != CreatureTemplateType.LMiniLongLegs && dRelation.trackerRep?.representedCreature is AbstractCreature c && c.creatureTemplate.type == CreatureTemplateType.LMiniLongLegs)
        {
            bool flag = self.daddy is DaddyLongLegs d && c.realizedCreature is DaddyLongLegs d2 && d.eyeColor == d2.eyeColor && d.effectColor == d2.effectColor;
            res.type = flag ? CreatureTemplate.Relationship.Type.Ignores : CreatureTemplate.Relationship.Type.Eats;
            res.intensity = flag ? 0f : 1f;
        }
        return res;
    }
    static void On_DaddyLongLegs_ctor(On.DaddyLongLegs.orig_ctor orig, DaddyLongLegs self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (abstractCreature.creatureTemplate.type == CreatureTemplateType.LMiniLongLegs)
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
            c.EmitDelegate((int length, DaddyLongLegs self) => self.Template.type == CreatureTemplateType.LMiniLongLegs ? 2 : length);
        }
        else
            Plugin.logger.LogError("Couldn't ILHook DaddyLongLegs.ctor (part 1)!");
        if (c.TryGotoNext(MoveType.After, x => x.MatchNewobj<BodyChunk>()))
        {
            c.Emit(Ldarg_0);
            c.EmitDelegate((BodyChunk chunk, DaddyLongLegs self) =>
            {
                if (self.Template.type == CreatureTemplateType.LMiniLongLegs)
                    chunk.rad *= .5f;
                return chunk;
            });
        }
        else
            Plugin.logger.LogError("Couldn't ILHook DaddyLongLegs.ctor (part 2)!");
        if (c.TryGotoNext(MoveType.After, x => x.MatchNewarr<DaddyTentacle>()) && c.TryGotoNext(x => x.MatchNewarr<DaddyTentacle>()))
        {
            c.Emit(Ldarg_0);
            c.EmitDelegate((int length, DaddyLongLegs self) => self.Template.type == CreatureTemplateType.LMiniLongLegs ? 2 : length);
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
                if (self.Template.type == CreatureTemplateType.LMiniLongLegs)
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
        if (self.Consious && self.moving)
        {
            for (var i = 0; i < self.bodyChunks.Length; i++)
                self.bodyChunks[i].vel.x += .1f * Math.Sign(self.bodyChunks[i].vel.x);
        }
    }
    static void DaddyLongLegs_Act(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After, x => x.MatchLdcR4(0.6f)))
        {
            c.Emit(Ldarg_0);
            c.EmitDelegate((float num, DaddyLongLegs self) => self.safariControlled && self.Template.type == CreatureTemplateType.LMiniLongLegs ? 0.45f : num);
        }
        else
            Plugin.logger.LogError("Couldn't ILHook DaddyLongLegs.Act!");
    }
    static void SLOracleBehaviorHasMark_CreatureJokeDialog(On.SLOracleBehaviorHasMark.orig_CreatureJokeDialog orig,  SLOracleBehaviorHasMark self) {
        orig(self);
        if (self.CheckStrayCreatureInRoom() == CreatureTemplateType.LMiniLongLegs) {
            self.dialogBox.NewMessage(self.Translate("Oh no."), 10);
        }
    }
    static void SSOracleBehavior_CreatureJokeDialog(On.SSOracleBehavior.orig_CreatureJokeDialog orig, SSOracleBehavior self) {
        orig(self);
        if (self.CheckStrayCreatureInRoom() == CreatureTemplateType.LMiniLongLegs) {
            self.dialogBox.NewMessage(self.Translate("Take your friend with you. Please. I beg you.."), 10);
        }
    }
    static float OverseerAbstractAI_HowInterestingIsCreature(On.OverseerAbstractAI.orig_HowInterestingIsCreature orig, OverseerAbstractAI self, AbstractCreature testCrit) {
        if (testCrit?.creatureTemplate.type == CreatureTemplateType.LMiniLongLegs)
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