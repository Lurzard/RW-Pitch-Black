using UnityEngine;
using Color = UnityEngine.Color;

namespace PitchBlack;

internal class EchoGraphics
{
    public static void Apply()
    {
        On.Ghost.ctor += Ghost_ctor;
        On.Ghost.InitiateSprites += Ghost_InitiateSprites;
        On.GoldFlakes.GoldFlake.DrawSprites += GoldFlake_DrawSprites;
    }


    public static void Undo()
    {
        On.Ghost.ctor -= Ghost_ctor;
        On.Ghost.InitiateSprites -= Ghost_InitiateSprites;
        On.GoldFlakes.GoldFlake.DrawSprites -= GoldFlake_DrawSprites;
    }

    private static void Ghost_ctor(On.Ghost.orig_ctor orig, Ghost self, Room room, PlacedObject placedObject, GhostWorldPresence worldGhost)
    {
        orig(self, room, placedObject, worldGhost);
        if (MiscUtils.IsBeaconOrPhoto(room.game.session))
        {
            self.goldColor = new Color(0.380f, 0.216f, 0.984f);
        }
    }

    private static void Ghost_InitiateSprites(On.Ghost.orig_InitiateSprites orig, Ghost self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (MiscUtils.IsBeaconOrPhoto(rCam.room.game.session))
        {
            sLeaser.sprites[self.HeadMeshSprite].shader = rCam.game.rainWorld.Shaders["PurpleEchoSkin"];
            for (int i = 0; i < self.legs.GetLength(0); i++)
            {
                sLeaser.sprites[self.ThightSprite(i)].shader = rCam.game.rainWorld.Shaders["PurpleEchoSkin"];
                sLeaser.sprites[self.LowerLegSprite(i)].shader = rCam.game.rainWorld.Shaders["PurpleEchoSkin"];
            }
        }
    }

    private static void GoldFlake_DrawSprites(On.GoldFlakes.GoldFlake.orig_DrawSprites orig, GoldFlakes.GoldFlake self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (MiscUtils.IsBeaconOrPhoto(rCam.room.game.session))
        {
            float f = Mathf.InverseLerp(-1f, 1f, Vector2.Dot(RWCustom.Custom.DegToVec(45f), RWCustom.Custom.DegToVec(Mathf.Lerp(self.lastYRot, self.yRot, timeStacker) * 57.29578f + Mathf.Lerp(self.lastRot, self.rot, timeStacker))));
            float ghostMode = rCam.ghostMode;
            Color c = RWCustom.Custom.HSL2RGB(0.70252777f, 0.97f, Mathf.Lerp(0.65f, 0f, ghostMode));
            Color d = RWCustom.Custom.HSL2RGB(0.70252777f, Mathf.Lerp(1f, 0.97f, ghostMode), Mathf.Lerp(1f, 0.65f, ghostMode));
            sLeaser.sprites[0].color = Color.Lerp(c, d, f);
        }
    }
}