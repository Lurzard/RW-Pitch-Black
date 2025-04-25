using UnityEngine;
using System;
using RWCustom;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

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
        //On.TempleGuardGraphics.ApplyPalette += TempleGuardGraphics_ApplyPalette;
        On.TempleGuardGraphics.Arm.ApplyPalette += Arm_ApplyPalette;
        On.TempleGuardGraphics.Halo.InitiateSprites += Halo_InitiateSprites;
        On.TempleGuardGraphics.Halo.GlyphSwapper.InitiateSprites += GlyphSwapper_InitiateSprites;
    }

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
            sLeaser.sprites[self.EyeSprite(1)].color = new Color(Mathf.Lerp(0.1f,  0.5f, telekinesisLerp), 0f, Mathf.Lerp(0.1f, 1f, telekinesisLerp));
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

    //private static void TempleGuardGraphics_ApplyPalette(On.TempleGuardGraphics.orig_ApplyPalette orig, TempleGuardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    // {
    //        orig(self, sLeaser, rCam, palette);
    //   if (MiscUtils.IsBeaconOrPhoto(rCam.game.session))
    //    {
    //        for (int i = 0; i <= self.HeadSprite; i++)
    //        {
    //            sLeaser.sprites[i].color = Color.white;
    //        }
    //    }
    // }

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

    #region Cicadas
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
