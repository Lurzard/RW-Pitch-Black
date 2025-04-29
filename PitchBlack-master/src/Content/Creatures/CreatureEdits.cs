using UnityEngine;
using System;
using RWCustom;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Collections.Generic;
using static PitchBlack.PBExtEnums;

namespace PitchBlack;

internal class CreatureEdits
{
    class CicadaCWT
    {
        public int glowSprite1;
        public int glowSprite2;
        // Seems unused -Lur
        //public int lightBulbSprite;
    }
    //CWTs
    static ConditionalWeakTable<CicadaGraphics, CicadaCWT> cicadaCWT = new ConditionalWeakTable<CicadaGraphics, CicadaCWT>();

    public static void Apply()
    {
        //Cicadas
        On.CicadaGraphics.InitiateSprites += CicadaGraphics_InitiateSprites;
        On.CicadaGraphics.DrawSprites += CicadaGraphics_DrawSprites;
        On.CicadaGraphics.AddToContainer += CicadaGraphics_AddToContainer;
        On.CicadaGraphics.ctor += CicadaGraphics_ctor;
        //Guardians
        On.TempleGuardGraphics.InitiateSprites += TempleGuardGraphics_InitiateSprites;
        On.TempleGuardGraphics.DrawSprites += TempleGuardGraphics_DrawSprites;
        On.TempleGuardGraphics.Arm.ApplyPalette += Arm_ApplyPalette;
        On.TempleGuardGraphics.Halo.InitiateSprites += Halo_InitiateSprites;
        On.TempleGuardGraphics.Halo.GlyphSwapper.InitiateSprites += GlyphSwapper_InitiateSprites;
        //VoidSpawn (Hooks for DreamSpawn type)
        On.VoidSpawn.GenerateBody += VoidSpawn_GenerateBody;
        On.VoidSpawnGraphics.ctor += VoidSpawnGraphics_ctor;
        On.VoidSpawnGraphics.InitiateSprites += VoidSpawnGraphics_InitiateSprites;
        On.VoidSpawnGraphics.DrawSprites += VoidSpawnGraphics_DrawSprites;
        On.VoidSpawnGraphics.UpdateGlowSpriteColor += VoidSpawnGraphics_UpdateGlowSpriteColor;
        On.VoidSpawnGraphics.Antenna.InitiateSprites += Antenna_InitiateSprites;
        On.VoidSpawnGraphics.Antenna.DrawSprites += Antenna_DrawSprites;
    }

    #region DreamSpawn
    private static void Antenna_DrawSprites(On.VoidSpawnGraphics.Antenna.orig_DrawSprites orig, VoidSpawnGraphics.Antenna self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.vsGraphics.spawn.variant == SpawnType.DreamSpawn)
        {
            sLeaser.sprites[self.firstSprite].shader = rCam.game.rainWorld.Shaders["DreamSpawnBody"];
        }
    }

    private static void Antenna_InitiateSprites(On.VoidSpawnGraphics.Antenna.orig_InitiateSprites orig, VoidSpawnGraphics.Antenna self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (self.vsGraphics.spawn.variant == SpawnType.DreamSpawn)
        {
            sLeaser.sprites[self.firstSprite].shader = rCam.game.rainWorld.Shaders["DreamSpawnBody"];
        }
    }

    private static void VoidSpawnGraphics_UpdateGlowSpriteColor(On.VoidSpawnGraphics.orig_UpdateGlowSpriteColor orig, VoidSpawnGraphics self, RoomCamera.SpriteLeaser sLeaser)
    {
        orig(self, sLeaser);
        if (self.spawn.variant == SpawnType.DreamSpawn)
        {
            if (self.dayLightMode)
            {
                sLeaser.sprites[self.GlowSprite].color = Plugin.SaturatedRose;
                return;
            }
            sLeaser.sprites[self.GlowSprite].color = Color.Lerp(Plugin.SaturatedRose, Plugin.Rose, 1f);
        }
    }

    private static void VoidSpawnGraphics_DrawSprites(On.VoidSpawnGraphics.orig_DrawSprites orig, VoidSpawnGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.spawn.variant == SpawnType.DreamSpawn)
        {
            if (!self.spawn.culled)
            {
                sLeaser.sprites[self.BodyMeshSprite].shader = rCam.game.rainWorld.Shaders["DreamSpawnBody"];
                if (self.hasOwnGoldEffect)
                {
                    //sLeaser.sprites[self.EffectSprite].shader = rCam.game.rainWorld.Shaders["BlackGLow"];
                }
                for (int k = 0; k < (sLeaser.sprites[self.BodyMeshSprite] as TriangleMesh).verticeColors.Length; k++)
                {
                    (sLeaser.sprites[self.BodyMeshSprite] as TriangleMesh).verticeColors[k] = new Color(self.meshColor.r, self.meshColor.g, self.meshColor.b, self.AlphaFromGlowDist((sLeaser.sprites[self.BodyMeshSprite] as TriangleMesh).vertices[k], self.glowPos - camPos));
                }
            }
        }
    }

    private static void VoidSpawnGraphics_InitiateSprites(On.VoidSpawnGraphics.orig_InitiateSprites orig, VoidSpawnGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (self.spawn.variant == SpawnType.DreamSpawn)
        {
            sLeaser.sprites[self.BodyMeshSprite].shader = rCam.game.rainWorld.Shaders["DreamSpawnBody"];
            if (self.hasOwnGoldEffect)
            {
                sLeaser.sprites[self.EffectSprite].shader = rCam.game.rainWorld.Shaders["BlackGlow"];
            }
        }
        self.AddToContainer(sLeaser, rCam, null);
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
        if (self.variant == VoidSpawn.SpawnType.RippleAmoeba || self.variant == SpawnType.DreamAmoeba)
        {
            sizeMult = 2f;
            segments = UnityEngine.Random.Range(5, 8);
        }
        else if (self.variant == VoidSpawn.SpawnType.RippleJelly || self.variant == SpawnType.DreamJelly)
        {
            segments = UnityEngine.Random.Range(3, 4);
        }
        else if (self.variant == VoidSpawn.SpawnType.RippleNoodle || self.variant == SpawnType.DreamNoodle)
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

        else if (self.variant == SpawnType.DreamBiter)
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
        if (self.spawn.variant == SpawnType.DreamSpawn)
        {
            // Taken from RippleAmoeba
            self.antennae.Add(new VoidSpawnGraphics.TailAntenna(self, self.totalSprites, segments, 12f * thickness, self.spawn.mainBody[self.spawn.mainBody.Length - 1].rad, 0f, 0.1f * thickness, 2, 2.2f));
            self.AddSubModule(self.antennae[self.antennae.Count - 1]);
        }
    }
    #endregion
    #region TempleGuard
    static Color ripple = Plugin.PBRipple_Color;

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
        Color rippleWhiteLerp = Color.Lerp(Plugin.PBRipple_Color, new Color(1f, 1f, 1f), 0.5f);
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
}
