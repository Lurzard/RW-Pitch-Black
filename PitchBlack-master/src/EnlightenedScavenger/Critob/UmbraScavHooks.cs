using MoreSlugcats;
using Unity.Mathematics;
using UnityEngine;

namespace PitchBlack;

public class UmbraScavHooks
{

    public static void Apply()
    {
        On.ScavengerGraphics.DrawSprites += ScavengerGraphics_DrawSprites;
        On.ScavengerGraphics.InitiateSprites += ScavengerGraphics_InitiateSprites;
        On.ScavengerGraphics.ctor += ScavengerGraphics_ctor;
        On.MoreSlugcats.VultureMaskGraphics.GenerateColor += VultureMaskGraphics_GenerateColor;
        //On.Scavenger.ctor += Scavenger_ctor;

    }

    private static void VultureMaskGraphics_GenerateColor(On.MoreSlugcats.VultureMaskGraphics.orig_GenerateColor orig, VultureMaskGraphics self, int colorSeed)
    {
        if (self.maskType == UmbraMask.UMBRA)
        {
            self.ColorA = new HSLColor(1f, 1f, 1f);
            self.ColorB = new HSLColor(1f, 1f, 1f);
            return;
        }
    }

    private static void ScavengerGraphics_ctor(On.ScavengerGraphics.orig_ctor orig, ScavengerGraphics self, PhysicalObject ow)
    {
        orig(self,ow);
        if (self.scavenger.Template.type == CreatureTemplateType.UmbraScav)
        {
            self.maskGfx = new VultureMaskGraphics(self.scavenger, UmbraMask.UMBRA, self.MaskSprite, "UmbraMask");
            self.maskGfx.GenerateColor(self.scavenger.abstractCreature.ID.RandomSeed);
        }
    }

    private static void Scavenger_ctor(On.Scavenger.orig_ctor orig, Scavenger self, AbstractCreature abstractCreature, World world)
    {
        orig(self,abstractCreature,world);
        if (self.Template.type == CreatureTemplateType.UmbraScav)
        {
            //self.abstractCreature.ID == 
        }
    }

    private static void ScavengerGraphics_InitiateSprites(On.ScavengerGraphics.orig_InitiateSprites orig, ScavengerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (self.scavenger.Template.type == CreatureTemplateType.UmbraScav)
        {
            sLeaser.sprites[self.TotalSprites - 1] = new FSprite("pixel", true);
            sLeaser.sprites[self.TotalSprites - 1].scale = 5f;
            sLeaser.sprites[self.TotalSprites - 2] = new FSprite("Futile_White", true);
            sLeaser.sprites[self.TotalSprites - 2].shader = rCam.game.rainWorld.Shaders["FlatLight"];
        }
    }

    private static void ScavengerGraphics_DrawSprites(On.ScavengerGraphics.orig_DrawSprites orig, ScavengerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPosV2)
    {
        float2 float2 = math.lerp(self.drawPositions[self.headDrawPos, 1], self.drawPositions[self.headDrawPos, 0], timeStacker);
        float2 float3 = math.lerp(self.drawPositions[self.chestDrawPos, 1], self.drawPositions[self.chestDrawPos, 0], timeStacker);
        float2 float4 = math.lerp(self.drawPositions[self.hipsDrawPos, 1], self.drawPositions[self.hipsDrawPos, 0], timeStacker);
        float2 float5 = (float3 + float4) / 2f;
        float2 @float = camPosV2.ToF2();

        orig(self, sLeaser, rCam ,timeStacker, camPosV2);
        if (self.scavenger.Template.type == CreatureTemplateType.UmbraScav)
        {
                sLeaser.sprites[self.TotalSprites - 1].x = float2.x - @float.x;
                sLeaser.sprites[self.TotalSprites - 1].y = float2.y - @float.y + 32f;
                sLeaser.sprites[self.TotalSprites - 1].alpha = Mathf.Lerp(self.lastMarkAlpha, self.markAlpha, timeStacker);
                sLeaser.sprites[self.TotalSprites - 2].x = float2.x - @float.x;
                sLeaser.sprites[self.TotalSprites - 2].y = float2.y - @float.y + 32f;
                sLeaser.sprites[self.TotalSprites - 2].alpha = 0.2f * Mathf.Lerp(self.lastMarkAlpha, self.markAlpha, timeStacker);
                sLeaser.sprites[self.TotalSprites - 2].scale = 1f + Mathf.Lerp(self.lastMarkAlpha, self.markAlpha, timeStacker);
        }
    }
}