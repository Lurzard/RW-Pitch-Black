using System.Runtime.CompilerServices;
using MoreSlugcats;
using RWCustom;
using ScavengerCosmetic;
using Unity.Mathematics;
using UnityEngine;

namespace PitchBlack;

public class ScholarScavHooks
{

    public static void Apply()
    {
        On.ScavengerGraphics.DrawSprites += ScavengerGraphics_DrawSprites;
        On.ScavengerGraphics.InitiateSprites += ScavengerGraphics_InitiateSprites;
        On.ScavengerGraphics.ctor += ScavengerGraphics_ctor;
        On.MoreSlugcats.VultureMaskGraphics.GenerateColor += VultureMaskGraphics_GenerateColor;
        On.ScavengerGraphics.AddToContainer += ScavengerGraphics_AddToContainer;
        On.Scavenger.SetUpCombatSkills += Scavenger_SetUpCombatSkills;
        On.ScavengerGraphics.IndividualVariations.ctor += IndividualVariations_ctor;
        On.ScavengerGraphics.GenerateColors += ScavengerGraphics_GenerateColors;
        On.Scavenger.Throw += Scavenger_Throw;
        On.Scavenger.GrabbedObjectSnatched += Scavenger_GrabbedObjectSnatched;
        On.Scavenger.ctor += Scavenger_ctor;
    }

    //public static readonly VultureMask.MaskType UMBRA = new VultureMask.MaskType("UMBRA", true);

    private static void Scavenger_GrabbedObjectSnatched(On.Scavenger.orig_GrabbedObjectSnatched orig, Scavenger self, PhysicalObject grabbedObject, Creature thief)
    {
        orig(self, grabbedObject, thief);
        if (self.Template.type == CreatureTemplateType.UmbraScav)
        {
            self.AI.agitation = 1f;
        }
    }

    private static void Scavenger_Throw(On.Scavenger.orig_Throw orig, Scavenger self, Vector2 throwDir)
    {
        orig(self, throwDir);
        if (self.Template.type == CreatureTemplateType.UmbraScav)
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
        if (self.scavenger.Template.type == CreatureTemplateType.UmbraScav)
        {
            FContainer fcontainer = rCam.ReturnFContainer("Foreground"); //this fixes the shader issue, proper alpha implemented!
            for (int k = self.TotalSprites - 2; k < self.TotalSprites; k++)
            {
                fcontainer.AddChild(sLeaser.sprites[k]);
            }
        }
    }

    private static void VultureMaskGraphics_GenerateColor(On.MoreSlugcats.VultureMaskGraphics.orig_GenerateColor orig, VultureMaskGraphics self, int colorSeed)
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
        if (self.scavenger.Template.type == CreatureTemplateType.UmbraScav)
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
            if (self.scavenger.Template.type == CreatureTemplateType.UmbraScav)
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

    private static void ScavengerGraphics_DrawSprites(On.ScavengerGraphics.orig_DrawSprites orig, ScavengerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPosV2)
    {
        orig(self, sLeaser, rCam, timeStacker, camPosV2);
        float2 float2 = math.lerp(self.drawPositions[self.headDrawPos, 1], self.drawPositions[self.headDrawPos, 0], timeStacker);
        float2 float3 = math.lerp(self.drawPositions[self.chestDrawPos, 1], self.drawPositions[self.chestDrawPos, 0], timeStacker);
        float2 float4 = math.lerp(self.drawPositions[self.hipsDrawPos, 1], self.drawPositions[self.hipsDrawPos, 0], timeStacker);
        float2 floatTheSequel = camPosV2.ToF2(); //@float in ScavengerGraphics.DrawSPrites
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            if (self.scavenger.Template.type == CreatureTemplateType.UmbraScav)
            {
                //Mark
                sLeaser.sprites[self.TotalSprites - 1].x = float2.x - floatTheSequel.x;
                sLeaser.sprites[self.TotalSprites - 1].y = float2.y - floatTheSequel.y + 32f;
                sLeaser.sprites[self.TotalSprites - 1].alpha = Mathf.Lerp(self.lastMarkAlpha, self.markAlpha, timeStacker);
                sLeaser.sprites[self.TotalSprites - 1].scale = 5f;
                sLeaser.sprites[self.TotalSprites - 1].color = Color.white;
                sLeaser.sprites[self.TotalSprites - 1].element = Futile.atlasManager.GetElementWithName("pixel");
                sLeaser.sprites[self.TotalSprites - 1].isVisible = true;
                sLeaser.sprites[self.TotalSprites - 1].rotation = 0f;

                //Mark Glow
                sLeaser.sprites[self.TotalSprites - 2].x = float2.x - floatTheSequel.x;
                sLeaser.sprites[self.TotalSprites - 2].y = float2.y - floatTheSequel.y + 32f;
                sLeaser.sprites[self.TotalSprites - 2].alpha = 0.2f * Mathf.Lerp(self.lastMarkAlpha, self.markAlpha, timeStacker);
                sLeaser.sprites[self.TotalSprites - 2].scale = 2f + Mathf.Lerp(self.lastMarkAlpha, self.markAlpha, timeStacker);
                sLeaser.sprites[self.TotalSprites - 2].element = Futile.atlasManager.GetElementWithName("Futile_White");
                sLeaser.sprites[self.TotalSprites - 2].shader = rCam.game.rainWorld.Shaders["FlatLight"];
                sLeaser.sprites[self.TotalSprites - 2].color = Color.white;
                sLeaser.sprites[self.TotalSprites - 2].isVisible = true;
                sLeaser.sprites[self.totalSprites - 2].rotation = 0f;

                //glow inherits MASK POSITION???
                //sprites work as intended without a mask

                //sLeaser.sprites[self.TotalSprites - 1].SetPosition(200f, 250f); //pixel debugging
                //sLeaser.sprites[self.TotalSprites - 2].SetPosition(250f, 250f); //glow debugging
            }
        }
    }

    public static bool CheckIfOnScreen(Vector2 position, Room room)
    {
        return room.ViewedByAnyCamera(position, 0f);
    }

    private static void Scavenger_ctor(On.Scavenger.orig_ctor orig, Scavenger self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (self.Template.type == CreatureTemplateType.UmbraScav)
        {
            self.abstractCreature.personality.aggression = 0.4f;
            self.abstractCreature.personality.bravery = 0.6f;
            self.abstractCreature.personality.dominance = 0.6f;
            self.abstractCreature.personality.energy = 0.8f;
            self.abstractCreature.personality.nervous = 0.2f;
            self.abstractCreature.personality.sympathy = 0.7f;
        }
    }

    private static void ScavengerGraphics_GenerateColors(On.ScavengerGraphics.orig_GenerateColors orig, ScavengerGraphics self)
    {
        orig(self);
        if (self.scavenger.Template.type == CreatureTemplateType.UmbraScav)
        {
            self.bodyColor = new HSLColor(0.08184808f, 0.06207584f, 0.8753151f);
            self.headColor = new HSLColor(0.08184808f, 0.06207584f, 0.8753151f);
            self.decorationColor = new HSLColor(0.6535784f, 0.1437009f, 0.3652394f);
            self.eyeColor = new HSLColor(0.6535784f, 0.7f, 0.1f);
            self.bellyColor = new HSLColor(0.08184808f, 0.06207584f, 0.8753151f);
        }
    }

    private static void IndividualVariations_ctor(On.ScavengerGraphics.IndividualVariations.orig_ctor orig, ref ScavengerGraphics.IndividualVariations self, Scavenger scavenger)
    {
        orig(ref self, scavenger);
        if (scavenger.Template.type == CreatureTemplateType.UmbraScav)
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
        if (self.Template.type == CreatureTemplateType.UmbraScav)
        {
            self.dodgeSkill = 0.7692045f;
            self.midRangeSkill = 0.3360332f;
            self.blockingSkill = 0.4186646f;
            self.reactionSkill = 0.8545572f;
        }
    }
}