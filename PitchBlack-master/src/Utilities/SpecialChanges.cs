using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PitchBlack;

public class SpecialChanges
{
    public static void Apply()
    {
        //Gold
        On.KarmaFlower.ApplyPalette += KarmaFlower_ApplyPalette;

        //Rose
        On.VoidSpawnGraphics.ApplyPalette += VoidSpawnGraphics_ApplyPalette;
        //On.VoidSpawnEgg.DrawSprites += VoidSpawnEgg_DrawSprites;

    }

    public static Color RoseRGB = new Color(0.529f, 0.184f, 0.360f);
    public static Color DesaturatedGold = new Color(0.5294117647f, 0.47843137254f, 0.42352941176f);
    public static Color SaturatedRose = RoseRGB * 2f;

    private static void VoidSpawnEgg_DrawSprites(On.VoidSpawnEgg.orig_DrawSprites orig, VoidSpawnEgg self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (rCam.room.game.IsStorySession && rCam.room.game.GetStorySession.saveStateNumber == Plugin.BeaconName)
        {
            //sLeaser.sprites[0].color = RoseRGB;
        }
    }

    private static void VoidSpawnGraphics_ApplyPalette(On.VoidSpawnGraphics.orig_ApplyPalette orig, VoidSpawnGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (rCam.room.game.IsStorySession && rCam.room.game.GetStorySession.saveStateNumber == Plugin.BeaconName)
        {
            if (self.dayLightMode)
            {
                sLeaser.sprites[self.GlowSprite].color = SaturatedRose;
                return;
            }
            sLeaser.sprites[self.GlowSprite].color = Color.Lerp(SaturatedRose, RoseRGB, Mathf.InverseLerp(0.3f, 0.9f, self.darkness));
        }
    }

    private static void KarmaFlower_ApplyPalette(On.KarmaFlower.orig_ApplyPalette orig, KarmaFlower self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (rCam.room.game.IsStorySession && rCam.room.game.GetStorySession.saveStateNumber == Plugin.BeaconName)
        {
            self.color = DesaturatedGold;
        }
    }
}
