using UnityEngine;

namespace PitchBlack;

// Did not test this because it's so shrimple it should work and if it doesn't I really messed up - Moon
public class EchoHooks
{
    public static void Apply()
    {
        On.GhostWorldPresence.ctor_World_GhostID_int += GhostWorldPresence_ctor_World_GhostID_int;
        On.Ghost.ctor += Ghost_ctor;
        On.Ghost.InitiateSprites += Ghost_InitiateSprites;
        On.GoldFlakes.GoldFlake.DrawSprites += GoldFlake_DrawSprites;
    }
    private static void Ghost_ctor(On.Ghost.orig_ctor orig, Ghost self, Room room, PlacedObject placedObject, GhostWorldPresence worldGhost)
    {
        orig(self, room, placedObject, worldGhost);
        if (MiscUtils.IsBeaconOrPhoto(room.game.session))
        {
            self.goldColor = Plugin.SaturatedAntiGold;
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
    private static void GhostWorldPresence_ctor_World_GhostID_int(On.GhostWorldPresence.orig_ctor_World_GhostID_int orig, GhostWorldPresence self, World world, GhostWorldPresence.GhostID ghostID, int spinningTopSpawnId)
    {
        orig(self, world, ghostID, spinningTopSpawnId);

        // This is also used to determine the room Echoes are placed in, so we can easily move them into VV

        if (MiscUtils.IsBeaconOrPhoto(world.game.session))
        {
            if (ghostID == GhostWorldPresence.GhostID.CC)
            {
                // Else1
                self.songName = "ELSE_LXIX";
            }
            else if (ghostID == GhostWorldPresence.GhostID.SI)
            {
                //Else7
                self.songName = "ELSELXIX";
            }
            else if (ghostID == GhostWorldPresence.GhostID.LF)
            {
                //Else5
                self.songName = "ELSELXIX";
            }
            else if (ghostID == GhostWorldPresence.GhostID.SH)
            {
                //Else3
                self.songName = "ELSE_LXIX";
            }
            else if (ghostID == GhostWorldPresence.GhostID.UW)
            {
                //Else4
                self.songName = "ELSE_LXIX";
            }
            else if (ghostID == GhostWorldPresence.GhostID.SB)
            {
                //Else2
                self.songName = "ELSE_LXIX";
            }
        }
    }
}