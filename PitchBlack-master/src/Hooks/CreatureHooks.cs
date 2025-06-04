using UnityEngine;
using System;
using RWCustom;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Collections.Generic;
using static PitchBlack.PBEnums;
using Unity.Mathematics;

namespace PitchBlack;

internal class CreatureHooks
{
    class CicadaCWT
    {
        public int glowSprite1;
        public int glowSprite2;
        // Seems unused -Lur
        //public int lightBulbSprite;
    }
    //CWTs
    static readonly ConditionalWeakTable<CicadaGraphics, CicadaCWT> cicadaCWT = new ConditionalWeakTable<CicadaGraphics, CicadaCWT>();

    // Used to make Dreamer albino, where it is applicable
    public static Color white = Plugin.BeaconEyeColor;

    public static void Apply()
    {
        // Cicadas
        On.CicadaGraphics.InitiateSprites += CicadaGraphics_InitiateSprites;
        On.CicadaGraphics.DrawSprites += CicadaGraphics_DrawSprites;
        On.CicadaGraphics.AddToContainer += CicadaGraphics_AddToContainer;
        On.CicadaGraphics.ctor += CicadaGraphics_ctor;

        // Guardians (Temp)
        //On.TempleGuardGraphics.InitiateSprites += TempleGuardGraphics_InitiateSprites;
        //On.TempleGuardGraphics.DrawSprites += TempleGuardGraphics_DrawSprites;
        //On.TempleGuardGraphics.Arm.ApplyPalette += Arm_ApplyPalette;
        //On.TempleGuardGraphics.Halo.InitiateSprites += Halo_InitiateSprites;
        //On.TempleGuardGraphics.Halo.GlyphSwapper.InitiateSprites += GlyphSwapper_InitiateSprites;

        // VoidSpawn (DreamSpawn + StarSpawn family)
        On.VoidSpawn.GenerateBody += VoidSpawn_GenerateBody;
        On.VoidSpawnGraphics.ctor += VoidSpawnGraphics_ctor;
        On.VoidSpawnGraphics.InitiateSprites += VoidSpawnGraphics_InitiateSprites;
        On.VoidSpawnGraphics.DrawSprites += VoidSpawnGraphics_DrawSprites;
        //On.VoidSpawnGraphics.UpdateGlowSpriteColor += VoidSpawnGraphics_UpdateGlowSpriteColor;
        On.VoidSpawnGraphics.Antenna.InitiateSprites += Antenna_InitiateSprites;
        On.VoidSpawnGraphics.Antenna.DrawSprites += Antenna_DrawSprites;

        // Echo (Dreamer ID)
        On.GhostWorldPresence.ctor_World_GhostID_int += GhostWorldPresence_ctor_World_GhostID_int;
        On.Ghost.ctor += Ghost_ctor;
        On.Ghost.InitiateSprites += Ghost_InitiateSprites;
        On.GoldFlakes.GoldFlake.DrawSprites += GoldFlake_DrawSprites;
        // These are mainly for switching out blackColor
        On.Ghost.ApplyPalette += Ghost_ApplyPalette;
        On.Ghost.Chains.DrawSprites += Chains_DrawSprites;
        On.Ghost.Rags.DrawSprites += Rags_DrawSprites;
        On.Ghost.Rags.InitiateSprites += Rags_InitiateSprites;

        On.Scavenger.ctor += Scavenger_ctor;
        On.ScavengerGraphics.DrawSprites += ScavengerGraphics_DrawSprites;
        On.ScavengerGraphics.GenerateColors += ScavengerGraphics_GenerateColors;
        On.ScavengerGraphics.AddToContainer += ScavengerGraphics_AddToContainer;
    }

    #region Dreamer
    // NOTES -Lur
    // Attempting to make the echo colored White instead of black (Albino) and RippleGold instead of Gold. Worked out, had to use new edited shaders:
    // DreamerSkin is an edited version of GhostSkin to change palette blackColor to white and hardcoded gold color to ripplegold
    // DreamerRag is an edited version of TentaclePlant to change palette blackColor to white (ripplegold lerping is added in-code)
    private static void Rags_InitiateSprites(On.Ghost.Rags.orig_InitiateSprites orig, Ghost.Rags self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (MiscUtils.IsBeaconOrPhoto(rCam.room.game.session)) //(MiscUtils.Dreamer(self.ghost))
        {
            for (int i = 0; i < self.segments.Length; i++)
            {
                sLeaser.sprites[self.firstSprite + i].shader = rCam.room.game.rainWorld.Shaders["DreamerRag"];
            }
        }
    }

    private static void Rags_DrawSprites(On.Ghost.Rags.orig_DrawSprites orig, Ghost.Rags self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        // If Dreamer, replace color lerping
        if (MiscUtils.IsBeaconOrPhoto(rCam.room.game.session)) //(MiscUtils.Dreamer(self.ghost))
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

    private static void Chains_DrawSprites(On.Ghost.Chains.orig_DrawSprites orig, Ghost.Chains self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        // If Dreamer, replace color lerping
        if (MiscUtils.IsBeaconOrPhoto(rCam.room.game.session)) //(MiscUtils.Dreamer(self.ghost))
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

    private static void Ghost_ApplyPalette(On.Ghost.orig_ApplyPalette orig, Ghost self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        // If Dreamer, Color the normally palette blackColor sprites to white
        if (MiscUtils.IsBeaconOrPhoto(rCam.room.game.session)) //(MiscUtils.Dreamer(self))
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
        // Changes the gold to purple for all cosmetics if the ID is Dreamer
        if (MiscUtils.IsBeaconOrPhoto(room.game.session)) //(MiscUtils.Dreamer(self))
        {
            self.goldColor = Plugin.Rose;
        }
    }
    private static void Ghost_InitiateSprites(On.Ghost.orig_InitiateSprites orig, Ghost self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (MiscUtils.IsBeaconOrPhoto(rCam.room.game.session)) //(MiscUtils.Dreamer(self))
        {
            sLeaser.sprites[self.HeadMeshSprite].shader = rCam.game.rainWorld.Shaders["DreamerSkin"];
            sLeaser.sprites[self.DistortionSprite].shader = rCam.game.rainWorld.Shaders["DreamerDistortion"];
            sLeaser.sprites[self.LightSprite].color = new Color(0.25882354f, 0.79607843137f, 0.50980392156f);
            for (int i = 0; i < self.legs.GetLength(0); i++)
            {
                sLeaser.sprites[self.ThightSprite(i)].shader = rCam.game.rainWorld.Shaders["DreamerSkin"];
                sLeaser.sprites[self.LowerLegSprite(i)].shader = rCam.game.rainWorld.Shaders["DreamerSkin"];
            }
        }
    }
    private static void GoldFlake_DrawSprites(On.GoldFlakes.GoldFlake.orig_DrawSprites orig, GoldFlakes.GoldFlake self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        // Checks campaign instead, Changes the hardcoded color for this effect
        if (MiscUtils.IsBeaconOrPhoto(rCam.room.game.session))
        {
            float f = Mathf.InverseLerp(-1f, 1f, Vector2.Dot(Custom.DegToVec(45f), Custom.DegToVec(Mathf.Lerp(self.lastYRot, self.yRot, timeStacker) * 57.29578f + Mathf.Lerp(self.lastRot, self.rot, timeStacker))));
            float ghostMode = rCam.ghostMode;
            Color c = Custom.HSL2RGB(0.9f, 0.97f, Mathf.Lerp(0.65f, 0f, ghostMode));
            Color d = Custom.HSL2RGB(0.9f, Mathf.Lerp(1f, 0.97f, ghostMode), Mathf.Lerp(1f, 0.65f, ghostMode));
            sLeaser.sprites[0].color = Color.Lerp(c, d, f);
        }
    }
    private static void GhostWorldPresence_ctor_World_GhostID_int(On.GhostWorldPresence.orig_ctor_World_GhostID_int orig, GhostWorldPresence self, World world, GhostWorldPresence.GhostID ghostID, int spinningTopSpawnId)
    {
        orig(self, world, ghostID, spinningTopSpawnId);
        if (ghostID == GhostID.Dreamer)
        {
            // Placeholder
            self.songName = "ELSE_LXIX";
        }
    }
    #endregion
    #region VoidSpawn
    private static void Antenna_DrawSprites(On.VoidSpawnGraphics.Antenna.orig_DrawSprites orig, VoidSpawnGraphics.Antenna self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (MiscUtils.IsDreamSpawn(self.vsGraphics.spawn))
        {
            sLeaser.sprites[self.firstSprite].shader = rCam.game.rainWorld.Shaders["DreamSpawnBody"];
        }
        if (self.vsGraphics.spawn.variant == PBEnums.VoidSpawn.SpawnType.StarSpawn)
        {
            sLeaser.sprites[self.firstSprite].shader = rCam.game.rainWorld.Shaders["StarSpawnBody"];
        }
    }

    private static void Antenna_InitiateSprites(On.VoidSpawnGraphics.Antenna.orig_InitiateSprites orig, VoidSpawnGraphics.Antenna self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (MiscUtils.IsDreamSpawn(self.vsGraphics.spawn))
        {
            sLeaser.sprites[self.firstSprite].shader = rCam.game.rainWorld.Shaders["DreamSpawnBody"];
        }
        if (self.vsGraphics.spawn.variant == PBEnums.VoidSpawn.SpawnType.StarSpawn)
        {
            sLeaser.sprites[self.firstSprite].shader = rCam.game.rainWorld.Shaders["StarSpawnBody"];
        }
    }

    private static void VoidSpawnGraphics_UpdateGlowSpriteColor(On.VoidSpawnGraphics.orig_UpdateGlowSpriteColor orig, VoidSpawnGraphics self, RoomCamera.SpriteLeaser sLeaser)
    {
        if (MiscUtils.IsDreamSpawn(self.spawn))
        {
            if (self.dayLightMode)
            {
                sLeaser.sprites[self.GlowSprite].color = Plugin.SaturatedRose;
                return;
            }
            sLeaser.sprites[self.GlowSprite].color = Color.Lerp(Plugin.SaturatedRose, Plugin.Rose, Mathf.InverseLerp(0.3f, 0.9f, self.darkness));
        }
        else orig(self, sLeaser);
    }

    private static void VoidSpawnGraphics_DrawSprites(On.VoidSpawnGraphics.orig_DrawSprites orig, VoidSpawnGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (!self.spawn.culled)
        {
            if (MiscUtils.IsDreamSpawn(self.spawn))
            {
                sLeaser.sprites[self.BodyMeshSprite].shader = rCam.game.rainWorld.Shaders["DreamSpawnBody"];
                //sLeaser.sprites[self.GlowSprite].color = Plugin.SaturatedRose;
                if (self.hasOwnGoldEffect)
                {
                    sLeaser.sprites[self.EffectSprite].shader = rCam.game.rainWorld.Shaders["RoseGlow"];
                }
            }
            if (self.spawn.variant == PBEnums.VoidSpawn.SpawnType.StarSpawn)
            {
                sLeaser.sprites[self.BodyMeshSprite].shader = rCam.game.rainWorld.Shaders["StarSpawnBody"];
                if (self.hasOwnGoldEffect)
                {
                    sLeaser.sprites[self.EffectSprite].shader = rCam.game.rainWorld.Shaders["GreenGlow"];
                }
            }
        }
    }

    private static void VoidSpawnGraphics_InitiateSprites(On.VoidSpawnGraphics.orig_InitiateSprites orig, VoidSpawnGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (MiscUtils.IsDreamSpawn(self.spawn))
        {
            sLeaser.sprites[self.BodyMeshSprite].shader = rCam.game.rainWorld.Shaders["DreamSpawnBody"];
            sLeaser.sprites[self.GlowSprite].shader = rCam.game.rainWorld.Shaders["FlatWaterLightBothSides"];
            if (self.hasOwnGoldEffect)
            {
                sLeaser.sprites[self.EffectSprite].shader = rCam.game.rainWorld.Shaders["RoseGlow"];
            }
        }
        if (self.spawn.variant == PBEnums.VoidSpawn.SpawnType.StarSpawn)
        {
            sLeaser.sprites[self.BodyMeshSprite].shader = rCam.game.rainWorld.Shaders["StarSpawnBody"];
            sLeaser.sprites[self.GlowSprite].shader = rCam.game.rainWorld.Shaders["FlatWaterLightBothSides"];
            if (self.hasOwnGoldEffect)
            {
                sLeaser.sprites[self.EffectSprite].shader = rCam.game.rainWorld.Shaders["GreenGlow"];
            }
        }
        //self.AddToContainer(sLeaser, rCam, null);
    }

    // Copied from decompiled code, an IL hook would be preferred -Lur
    // This doesn't call orig. Can't inject my own code without duplicating adding connections, can't use my own local variables and call orig, my solution for now is to replace orig
    private static void VoidSpawn_GenerateBody(On.VoidSpawn.orig_GenerateBody orig, VoidSpawn self)
    {
        int segments = UnityEngine.Random.Range(3, UnityEngine.Random.Range(3, 16));
        int index = 0;
        List<BodyChunk> list = new List<BodyChunk>();
        List<PhysicalObject.BodyChunkConnection> list2 = new List<PhysicalObject.BodyChunkConnection>();
        float sizeMult = 1f;
        if (self.variant == VoidSpawn.SpawnType.RippleAmoeba
            || self.variant == PBEnums.VoidSpawn.SpawnType.DreamAmoeba)
        {
            sizeMult = 2f;
            segments = UnityEngine.Random.Range(5, 8);
        }
        else if (self.variant == VoidSpawn.SpawnType.RippleJelly
            || self.variant == PBEnums.VoidSpawn.SpawnType.DreamJelly)
        {
            segments = UnityEngine.Random.Range(3, 4);
        }
        else if (self.variant == VoidSpawn.SpawnType.RippleNoodle
            || self.variant == PBEnums.VoidSpawn.SpawnType.DreamNoodle)
        {
            sizeMult = 0.5f;
            segments = UnityEngine.Random.Range(6, 10);
        }
        float length = Mathf.Lerp(3f, 8f, UnityEngine.Random.value);
        if (self.variant == VoidSpawn.SpawnType.RippleAmoeba)
        {
            length = Mathf.Lerp(8f, 12f, UnityEngine.Random.value);
        }
        else if (self.variant == VoidSpawn.SpawnType.RippleNoodle)
        {
            length = Mathf.Lerp(1f, 6f, UnityEngine.Random.value);
        }
        else if (self.variant == PBEnums.VoidSpawn.SpawnType.DreamBiter)
        {
            sizeMult = UnityEngine.Random.Range(0.25f, 0.75f);
            length = Mathf.Lerp(1f, 6f, UnityEngine.Random.value);
            segments = UnityEngine.Random.Range(5, 9);
        }
        float num5 = Mathf.Lerp(Mathf.Lerp(0.5f, 4f, UnityEngine.Random.value), length / 2f, UnityEngine.Random.value);
        float p = Mathf.Lerp(0.1f, 0.7f, UnityEngine.Random.value);
        self.sizeFac = Mathf.Lerp(0.5f, 1.2f, UnityEngine.Random.value) * sizeMult;
        self.swimSpeed = Mathf.Lerp(0.5f, 1f, UnityEngine.Random.value);
        self.dominance = Mathf.InverseLerp(0f, 2.4f, self.sizeFac);
        self.dominance *= Mathf.InverseLerp(3f, 8f, (float)segments);
        for (int i = 0; i < segments; i++)
        {
            float num6 = (float)i / (float)(segments - 1);
            float num7 = Mathf.Lerp(Mathf.Lerp(length, num5, num6), Mathf.Lerp(num5, length, Mathf.Sin(Mathf.Pow(num6, p) * 3.1415927f)), 0.5f) * self.sizeFac;
            list.Add(new BodyChunk(self, index, default(Vector2), num7, num7 * 0.1f));
            if (i > 0)
            {
                list2.Add(new PhysicalObject.BodyChunkConnection(list[i - 1], list[i], Mathf.Lerp((list[i - 1].rad + list[i].rad) * 1.25f, Mathf.Max(list[i - 1].rad, list[i].rad), 0.5f), PhysicalObject.BodyChunkConnection.Type.Normal, 1f, -1f));
            }
            index++;
        }
        self.mainBody = list.ToArray();
        self.bodyChunks = list.ToArray();
        self.bodyChunkConnections = list2.ToArray();
    }

    private static void VoidSpawnGraphics_ctor(On.VoidSpawnGraphics.orig_ctor orig, VoidSpawnGraphics self, PhysicalObject owner)
    {
        orig(self, owner);
        float thickness = Mathf.Lerp(self.spawn.sizeFac, 0.5f + 0.5f * UnityEngine.Random.value, UnityEngine.Random.value);
        int segments = UnityEngine.Random.Range(4, 8);
        if (self.spawn.variant == PBEnums.VoidSpawn.SpawnType.DreamAmoeba)
        {
            // Taken from RippleAmoeba
            self.antennae.Add(new VoidSpawnGraphics.TailAntenna(self, self.totalSprites, segments, 12f * thickness, self.spawn.mainBody[self.spawn.mainBody.Length - 1].rad, 0f, 0.1f * thickness, 2, 2.2f));
            self.AddSubModule(self.antennae[self.antennae.Count - 1]);
        }
    }
    #endregion
    #region TempleGuard
    static Color ripple = RainWorld.RippleColor;

    private static void GlyphSwapper_InitiateSprites(On.TempleGuardGraphics.Halo.GlyphSwapper.orig_InitiateSprites orig, TempleGuardGraphics.Halo.GlyphSwapper self, int frst, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, frst, sLeaser, rCam);
        if (MiscUtils.IsBeaconOrPhoto(rCam.game.session))
        {
            sLeaser.sprites[frst + 2].color = ripple;
            for (int i = 0; i < 2; i++)
            {
                sLeaser.sprites[frst + i].color = ripple;
            }
        }
    }

    private static void Halo_InitiateSprites(On.TempleGuardGraphics.Halo.orig_InitiateSprites orig, TempleGuardGraphics.Halo self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (MiscUtils.IsBeaconOrPhoto(rCam.game.session))
        {
            for (int i = 0; i < self.circles; i++)
            {
                sLeaser.sprites[self.firstSprite + i].color = ripple;
            }
            int circles = self.circles;
            for (int j = 0; j < self.glyphs.Length; j++)
            {
                for (int k = 0; k < self.glyphs[j].Length; k++)
                {
                    sLeaser.sprites[self.firstSprite + circles].color = ripple;
                    circles++;
                }
            }
            for (int m = 0; m < self.lines.GetLength(0); m++)
            {
                sLeaser.sprites[self.firstSprite + self.firstLineSprite + m].color = ripple;
            }
            for (int n = 0; n < self.smallCircles.GetLength(0); n++)
            {
                sLeaser.sprites[self.firstSprite + self.firstSmallCircleSprite + n].color = ripple;
            }
        }
    }

    private static void TempleGuardGraphics_DrawSprites(On.TempleGuardGraphics.orig_DrawSprites orig, TempleGuardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        float telekinesisLerp = Mathf.Max(Mathf.Lerp(self.lastTelekin, self.telekinesis, timeStacker), Mathf.Min(1f, Mathf.Lerp(self.lastEyeBlinking, self.eyeBlinking, timeStacker))) * UnityEngine.Random.value;
        if (MiscUtils.IsBeaconOrPhoto(rCam.game.session))
        {
            sLeaser.sprites[self.EyeSprite(1)].color = new Color(Mathf.Lerp(0.1f, 0.5f, telekinesisLerp), 0f, Mathf.Lerp(0.1f, 1f, telekinesisLerp));
        }
    }

    private static void Arm_ApplyPalette(On.TempleGuardGraphics.Arm.orig_ApplyPalette orig, TempleGuardGraphics.Arm self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        Color rippleWhiteLerp = Color.Lerp(Plugin.Rose, new Color(1f, 1f, 1f), 0.5f);
        if (MiscUtils.IsBeaconOrPhoto(rCam.game.session))
        {
            sLeaser.sprites[self.firstSprite + 2].color = rippleWhiteLerp;
            for (int i = 0; i < self.beads.Length; i++)
            {
                float num = (float)i / (float)(self.beads.Length - 1);
                sLeaser.sprites[self.firstSprite + 3 + i].color = Color.Lerp(rippleWhiteLerp, palette.blackColor, 1.5f - num);
            }
        }
    }

    private static void TempleGuardGraphics_InitiateSprites(On.TempleGuardGraphics.orig_InitiateSprites orig, TempleGuardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (MiscUtils.IsBeaconOrPhoto(rCam.game.session))
        {
            for (int i = 0; i < 2; i++)
            {
                sLeaser.sprites[self.EyeSprite(i)] = new FSprite("toriiEye", true);
            }
            self.AddToContainer(sLeaser, rCam, null);
        }
    }
    #endregion
    #region Cicada
    static void CicadaGraphics_ctor(On.CicadaGraphics.orig_ctor orig, CicadaGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
        if (MiscUtils.IsBeaconOrPhoto(ow.room.game.session) && !cicadaCWT.TryGetValue(self, out _))
        {
            cicadaCWT.Add(self, new CicadaCWT());
        }
    }
    static void CicadaGraphics_AddToContainer(On.CicadaGraphics.orig_AddToContainer orig, CicadaGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        orig(self, sLeaser, rCam, newContatiner);
        if (sLeaser.sprites.Length > 14 && MiscUtils.IsBeaconOrPhoto(rCam.game.session) && cicadaCWT.TryGetValue(self, out CicadaCWT cwt))
        {
            sLeaser.sprites[cwt.glowSprite1].RemoveFromContainer();
            rCam.ReturnFContainer("ForegroundLights").AddChild(sLeaser.sprites[cwt.glowSprite1]);
            sLeaser.sprites[cwt.glowSprite2].RemoveFromContainer();
            rCam.ReturnFContainer("ForegroundLights").AddChild(sLeaser.sprites[cwt.glowSprite2]);
        }
    }
    static void CicadaGraphics_DrawSprites(On.CicadaGraphics.orig_DrawSprites orig, CicadaGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (MiscUtils.IsBeaconOrPhoto(rCam.game.session) && cicadaCWT.TryGetValue(self, out CicadaCWT cwt))
        {
            sLeaser.sprites[cwt.glowSprite1].scale = 8;
            sLeaser.sprites[cwt.glowSprite1].SetPosition(self.cicada.mainBodyChunk.pos - camPos);
            sLeaser.sprites[cwt.glowSprite2].scale = 24;
            sLeaser.sprites[cwt.glowSprite2].SetPosition(self.cicada.bodyChunks[0].pos + (80f * Custom.DirVec(self.cicada.bodyChunks[0].pos, self.cicada.bodyChunks[1].pos)) - camPos);

            Vector3 color = Custom.RGB2HSL(self.shieldColor);
            color.x = Mathf.Lerp(color.x, 20f / 360f, 0.9f);
            color.y += (1f - color.y) / 2f;
            color.z = Mathf.Lerp(color.z, 0.45f, 0.9f);
            Color rgbColor = Custom.HSL2RGB(color.x, color.y, color.z);
            sLeaser.sprites[cwt.glowSprite1].color = rgbColor;
            sLeaser.sprites[cwt.glowSprite2].color = rgbColor;
            sLeaser.sprites[self.ShieldSprite].color = rgbColor;
            sLeaser.sprites[self.cicada.gender ? self.EyesBSprite : self.EyesASprite].color = rgbColor;

            Color lerpColor = new Color(10f / 255f, 10f / 255f, 10f / 255f);
            sLeaser.sprites[self.BodySprite].color = Color.Lerp(sLeaser.sprites[self.BodySprite].color, lerpColor, 0.9f);
            sLeaser.sprites[self.HeadSprite].color = Color.Lerp(sLeaser.sprites[self.HeadSprite].color, lerpColor, 0.9f);
            sLeaser.sprites[self.HighlightSprite].color = Color.Lerp(sLeaser.sprites[self.HighlightSprite].color, lerpColor, 0.8f);
        }
    }
    static void CicadaGraphics_InitiateSprites(On.CicadaGraphics.orig_InitiateSprites orig, CicadaGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (MiscUtils.IsBeaconOrPhoto(rCam.game.session) && cicadaCWT.TryGetValue(self, out CicadaCWT cwt))
        {
            cwt.glowSprite1 = sLeaser.sprites.Length;
            cwt.glowSprite2 = sLeaser.sprites.Length + 1;
            Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 2);
            sLeaser.sprites[cwt.glowSprite1] = new FSprite("Futile_White")
            {
                shader = rCam.game.rainWorld.Shaders["LightSource"]
            };
            sLeaser.sprites[cwt.glowSprite2] = new FSprite("Futile_White")
            {
                shader = rCam.game.rainWorld.Shaders["LightSource"]
            };
            self.AddToContainer(sLeaser, rCam, null);
        }
    }
    #endregion
    #region Scavenger

    private static void ScavengerGraphics_GenerateColors(On.ScavengerGraphics.orig_GenerateColors orig, ScavengerGraphics self)
    {
        orig(self);
        if (self.scavenger.Template.type == PBEnums.CreatureTemplateType.UmbraScav)
        {
            self.bodyColor = new HSLColor(0.08184808f, 0.06207584f, 0.8753151f);
            self.headColor = new HSLColor(0.08184808f, 0.06207584f, 0.8753151f);
            self.decorationColor = new HSLColor(0.6535784f, 0.1437009f, 0.3652394f);
            self.eyeColor = new HSLColor(0.6535784f, 0.7f, 0.1f);
            self.bellyColor = new HSLColor(0.08184808f, 0.06207584f, 0.8753151f);
        }
        if (self.scavenger.Template.type == PBEnums.CreatureTemplateType.Citizen)
        {
            self.bodyColor = new HSLColor(0.67f, 0.9f, 0.95f);
            self.headColor = new HSLColor(0.67f, 0.9f, 0.95f);
            self.decorationColor = new HSLColor(0.67f, 0.9f, 0.95f);
            self.eyeColor = new HSLColor(0.67f, 0.9f, 0.95f);
            self.bellyColor = new HSLColor(0.67f, 0.9f, 0.95f);
        }
    }
    
    private static void ScavengerGraphics_DrawSprites(On.ScavengerGraphics.orig_DrawSprites orig, ScavengerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPosV2)
    {
        orig(self, sLeaser, rCam, timeStacker, camPosV2);
        if (self.scavenger.Template.type == PBEnums.CreatureTemplateType.Citizen) //to remove its eyes
        {
            for (int j = 0; j < 2; j++)
            {
                sLeaser.sprites[self.EyeSprite(j, 0)].isVisible = false;
                if (self.iVars.pupilSize > 0f)
                {
                    sLeaser.sprites[self.EyeSprite(j, 1)].isVisible = false;
                }
            }
        }
        // umbra scav stuff
        float2 float2 = math.lerp(self.drawPositions[self.headDrawPos, 1], self.drawPositions[self.headDrawPos, 0], timeStacker);
        float2 floatTheSequel = camPosV2.ToF2(); //@float in ScavengerGraphics.DrawSPrites
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            if (self.scavenger.Template.type == PBEnums.CreatureTemplateType.UmbraScav)
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

    private static void Scavenger_ctor(On.Scavenger.orig_ctor orig, Scavenger self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (self.Template.type == PBEnums.CreatureTemplateType.Citizen)
        {
            self.collisionLayer = 2;
        }
        if (self.Template.type == PBEnums.CreatureTemplateType.UmbraScav) //umbra scav stuff
        {
            self.abstractCreature.personality.aggression = 0.4f;
            self.abstractCreature.personality.bravery = 0.6f;
            self.abstractCreature.personality.dominance = 0.6f;
            self.abstractCreature.personality.energy = 0.8f;
            self.abstractCreature.personality.nervous = 0.2f;
            self.abstractCreature.personality.sympathy = 0.7f;
        }
    }
    
private static void ScavengerGraphics_AddToContainer(On.ScavengerGraphics.orig_AddToContainer orig, ScavengerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        int randomContainerInt = UnityEngine.Random.Range(0, 2);
#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
        string RandomContainerStr = randomContainerInt switch
        {
            0 => "Background",
            1 => "Midground",
            2 => "Foreground",
        };
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
        if (self.scavenger.Template.type == PBEnums.CreatureTemplateType.Citizen)
        {
            sLeaser.RemoveAllSpritesFromContainer();
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer(RandomContainerStr);
            }
            newContatiner.AddChild(sLeaser.containers[0]);
            for (int i = 0; i < self.FirstInFrontLimbSprite; i++)
            {
                newContatiner.AddChild(sLeaser.sprites[i]);
            }
        
            for (int m = self.FirstInFrontLimbSprite; m < self.FirstInFrontLimbSprite + 2; m++)
            {
                newContatiner.AddChild(sLeaser.sprites[m]);
            }
            newContatiner.AddChild(sLeaser.containers[1]);
        }
        orig(self, sLeaser,rCam,newContatiner);
    }
    #endregion
}
