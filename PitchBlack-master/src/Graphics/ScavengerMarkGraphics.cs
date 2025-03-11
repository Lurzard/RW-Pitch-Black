using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MoreSlugcats;
using RWCustom;
using Unity.Mathematics;
using UnityEngine;

namespace PitchBlack;

public class ScavengerMarkGraphics
{
    public static void Apply()
    {
        On.ScavengerGraphics.DrawSprites += ScavengerGraphics_DrawSprites;
        On.ScavengerGraphics.InitiateSprites += ScavengerGraphics_InitiateSprites;


    }

    private static void ScavengerGraphics_InitiateSprites(On.ScavengerGraphics.orig_InitiateSprites orig, ScavengerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        if (!MiscUtils.IsBeaconOrPhoto(rCam.room.game.session))
        {
            return;
        }
        else
        {
            orig(self, sLeaser, rCam);
            if (self.scavenger.Elite) //placeholder
            {
                sLeaser.sprites[self.TotalSprites - 1] = new FSprite("pixel", true);
                sLeaser.sprites[self.TotalSprites - 1].scale = 5f;
                sLeaser.sprites[self.TotalSprites - 2] = new FSprite("Futile_White", true);
                sLeaser.sprites[self.TotalSprites - 2].shader = rCam.game.rainWorld.Shaders["FlatLight"];
            }
        }
    }

    private static void ScavengerGraphics_DrawSprites(On.ScavengerGraphics.orig_DrawSprites orig, ScavengerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPosV2)
    {
        orig(self, sLeaser, rCam, timeStacker, camPosV2);
        if (!MiscUtils.IsBeaconOrPhoto(rCam.room.game.session)) { return; }

        if (self.scavenger.Elite) //placeholder
        {
            float2 float2 = math.lerp(self.drawPositions[self.headDrawPos, 1], self.drawPositions[self.headDrawPos, 0], timeStacker);
            float2 float3 = math.lerp(self.drawPositions[self.chestDrawPos, 1], self.drawPositions[self.chestDrawPos, 0], timeStacker);
            float2 float4 = math.lerp(self.drawPositions[self.hipsDrawPos, 1], self.drawPositions[self.hipsDrawPos, 0], timeStacker);
            float2 float5 = (float3 + float4) / 2f;
            float2 @float = camPosV2.ToF2();

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
