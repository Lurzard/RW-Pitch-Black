using UnityEngine;
using System;
using RWCustom;
using System.Runtime.CompilerServices;

namespace PitchBlack;

internal class PBCicadas
{
    class CicadaCWT
    {
        public int glowSprite1;
        public int glowSprite2;
        //public int lightBulbSprite; unused at the moment
    }

    static ConditionalWeakTable<CicadaGraphics, CicadaCWT> cicadaCWT = new ConditionalWeakTable<CicadaGraphics, CicadaCWT>();

    public static void Apply()
    {
        On.CicadaGraphics.InitiateSprites += CicadaGraphics_InitiateSprites;
        On.CicadaGraphics.DrawSprites += CicadaGraphics_DrawSprites;
        On.CicadaGraphics.AddToContainer += CicadaGraphics_AddToContainer;
        On.CicadaGraphics.ctor += CicadaGraphics_ctor;
    }

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
}
