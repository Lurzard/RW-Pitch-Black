using System;
using System.Numerics;
using JetBrains.Annotations;
using RWCustom;
using UnityEngine;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

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
        On.Ghost.ApplyPalette += Ghost_ApplyPalette;
        On.Ghost.Rags.DrawSprites += Rags_DrawSprites;
        On.Ghost.Chains.DrawSprites += Chains_DrawSprites;
        On.Ghost.Rags.InitiateSprites += Rags_InitiateSprites;
    }

    private static void Rags_InitiateSprites(On.Ghost.Rags.orig_InitiateSprites orig, Ghost.Rags self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (MiscUtils.IsBeaconOrPhoto(rCam.room.game.session))
        {
            for (int i = 0; i < self.segments.Length; i++)
            {
                sLeaser.sprites[self.firstSprite + i].shader = rCam.room.game.rainWorld.Shaders["DreamerRag"];
            }
        }
    }

    public static Color white = Color.white;

    // NOTES -Lur
    // All campaign checks will be moved to Dreamer checks once done, for the most part
    // Attempting to make the echo colored White instead of black (Albino) and RippleGold instead of Gold
    // The shaders used for the echoes may be coloring the vertices black by intention, we have to change that to white

    private static void Chains_DrawSprites(On.Ghost.Chains.orig_DrawSprites orig, Ghost.Chains self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (MiscUtils.IsBeaconOrPhoto(rCam.room.game.session))
        {
            for (int i = 0; i < self.segments.Length; i++)
            {
                UnityEngine.Vector2 vector = self.AttachPos(i, timeStacker);
                for (int j = 0; j < self.segments[i].GetLength(0); j++)
                {
                    UnityEngine.Vector2 vector2 = UnityEngine.Vector2.Lerp(self.segments[i][j, 1], self.segments[i][j, 0], timeStacker);
                    if (self.segments[i][j, 4].y == 0.2f)
                    {
                        sLeaser.sprites[self.firstSprite + self.firstSpriteOfChains[i] + j * 2].color = Color.Lerp(white, self.ghost.goldColor, 0.65f);
                    }
                    else
                    {
                        float ang = Mathf.Sin(Mathf.Lerp(self.segments[i][j, 5].y, self.segments[i][j, 5].x, timeStacker)) * 360f / 3.1415927f;
                        float num = Mathf.Abs(UnityEngine.Vector2.Dot(Custom.DegToVec(ang), Custom.DirVec(vector, vector2)));
                        num = Custom.BackwardsSCurve(num, 0.3f);
                        sLeaser.sprites[self.firstSprite + self.firstSpriteOfChains[i] + j * 2].color = Color.Lerp(white, self.ghost.goldColor, 0.65f + 0.1f * Mathf.Sin(num * 3.1415927f * 2f));
                        sLeaser.sprites[self.firstSprite + self.firstSpriteOfChains[i] + j * 2 + 1].color = Color.Lerp(white, self.ghost.goldColor, 0.1f + 0.9f * num);
                    }
                }
            }
        }
    }

    private static void Rags_DrawSprites(On.Ghost.Rags.orig_DrawSprites orig, Ghost.Rags self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (MiscUtils.IsBeaconOrPhoto(rCam.room.game.session))
        {
            for (int i = 0; i < self.segments.Length; i++)
            {
                UnityEngine.Vector2 a = self.AttachPos(i, timeStacker);
                float num2 = 0f;
                for (int j = 0; j < self.segments[i].GetLength(0); j++)
                {
                    UnityEngine.Vector2 vector = UnityEngine.Vector2.Lerp(self.segments[i][j, 1], self.segments[i][j, 0], timeStacker);
                    UnityEngine.Vector2 normalized = (a - vector).normalized;
                    float num4 = 0.35f + 0.65f * Custom.BackwardsSCurve(Mathf.Pow(Mathf.Abs(UnityEngine.Vector2.Dot(UnityEngine.Vector3.Slerp(self.segments[i][j, 5], self.segments[i][j, 4], timeStacker), Custom.DegToVec(45f + Custom.VecToDeg(normalized)))), 2f), 0.5f);
                    (sLeaser.sprites[self.firstSprite + i] as TriangleMesh).verticeColors[j * 4] = Color.Lerp(white, self.ghost.goldColor, (num4 + num2) / 2f);
                    (sLeaser.sprites[self.firstSprite + i] as TriangleMesh).verticeColors[j * 4 + 1] = Color.Lerp(white, self.ghost.goldColor, (num4 + num2) / 2f);
                    (sLeaser.sprites[self.firstSprite + i] as TriangleMesh).verticeColors[j * 4 + 2] = Color.Lerp(white, self.ghost.goldColor, num4);
                    (sLeaser.sprites[self.firstSprite + i] as TriangleMesh).verticeColors[j * 4 + 3] = Color.Lerp(white, self.ghost.goldColor, num4);
                }
            }
        }
    }

    private static void Ghost_ApplyPalette(On.Ghost.orig_ApplyPalette orig, Ghost self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (MiscUtils.IsBeaconOrPhoto(rCam.room.game.session))
        {
            self.blackColor = white;
            sLeaser.sprites[self.NeckConnectorSprite].color = self.blackColor;
            sLeaser.sprites[self.ButtockSprite(0)].color = self.blackColor;
            sLeaser.sprites[self.ButtockSprite(1)].color = self.blackColor;
            for (int i = 0; i < (sLeaser.sprites[self.BodyMeshSprite] as TriangleMesh).verticeColors.Length; i++)
            {
                (sLeaser.sprites[self.BodyMeshSprite] as TriangleMesh).verticeColors[i] = self.blackColor;
            }
            for (int j = 0; j < self.legs.GetLength(0); j++)
            {
                for (int k = 0; k < (sLeaser.sprites[self.ThightSprite(j)] as TriangleMesh).verticeColors.Length; k++)
                {
                    (sLeaser.sprites[self.ThightSprite(j)] as TriangleMesh).verticeColors[k] = self.blackColor;
                }
            }
        }
    }

    private static void Ghost_ctor(On.Ghost.orig_ctor orig, Ghost self, Room room, PlacedObject placedObject, GhostWorldPresence worldGhost)
    {
        orig(self, room, placedObject, worldGhost);
        if (MiscUtils.IsBeaconOrPhoto(room.game.session))
        {
            self.goldColor = RainWorld.RippleGold;
        }
    }
    private static void Ghost_InitiateSprites(On.Ghost.orig_InitiateSprites orig, Ghost self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (MiscUtils.IsBeaconOrPhoto(rCam.room.game.session))
        {
            sLeaser.sprites[self.HeadMeshSprite].shader = rCam.game.rainWorld.Shaders["DreamerSkin"];
            for (int i = 0; i < self.legs.GetLength(0); i++)
            {
                sLeaser.sprites[self.ThightSprite(i)].shader = rCam.game.rainWorld.Shaders["DreamerSkin"];
                sLeaser.sprites[self.LowerLegSprite(i)].shader = rCam.game.rainWorld.Shaders["DreamerSkin"];
            }
        }
    }
    private static void GoldFlake_DrawSprites(On.GoldFlakes.GoldFlake.orig_DrawSprites orig, GoldFlakes.GoldFlake self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (MiscUtils.IsBeaconOrPhoto(rCam.room.game.session))
        {
            float f = Mathf.InverseLerp(-1f, 1f, UnityEngine.Vector2.Dot(RWCustom.Custom.DegToVec(45f), RWCustom.Custom.DegToVec(Mathf.Lerp(self.lastYRot, self.yRot, timeStacker) * 57.29578f + Mathf.Lerp(self.lastRot, self.rot, timeStacker))));
            float ghostMode = rCam.ghostMode;
            Color c = RWCustom.Custom.HSL2RGB(0.68f, 0.97f, Mathf.Lerp(0.65f, 0f, ghostMode));
            Color d = RWCustom.Custom.HSL2RGB(0.68f, Mathf.Lerp(1f, 0.97f, ghostMode), Mathf.Lerp(1f, 0.65f, ghostMode));
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