using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace PitchBlack;

public class FireGrubGraphics : TubeWormGraphics
{
    public FireGrub Grub => (FireGrub)owner;
    private Color trimColor;
    private Color blackColor;
    private float litRoom;
    private LightSource lightSource;
    private float lastCharge;
    private FSprite finalText;
    private List<FSprite> particles = new List<FSprite>();
    public FireGrubGraphics(FireGrub ow) : base(ow)
    {
        HSLColor baseHSL = Grub.hslColor;
        color = baseHSL.rgb;
        HSLColor trimHSL = baseHSL;
        trimHSL.lightness *= 0.2f;
        trimHSL.saturation = Mathf.Clamp01(trimHSL.saturation * 0.2f);
        trimColor = trimHSL.rgb;
        lastCharge = Grub.charge;
        finalText = new FSprite("Futile_White"){width = Grub.room.game.manager.rainWorld.options.ScreenSize.x, height = Grub.room.game.manager.rainWorld.options.ScreenSize.y};
        particles.Add(new FSprite("Circle20"){x = Grub.mainBodyChunk.pos.x, y = Grub.mainBodyChunk.pos.y});
        particles.Add(new FSprite("Circle20"){x = Grub.mainBodyChunk.pos.x, y = Grub.mainBodyChunk.pos.y});
        particles.Add(new FSprite("Circle20"){x = Grub.mainBodyChunk.pos.x, y = Grub.mainBodyChunk.pos.y});
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        int initLength = sLeaser.sprites.Length;
        Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length+4);
        sLeaser.sprites[initLength] = finalText;
        for (int i = 1; i < sLeaser.sprites.Length - initLength; i++) {
            sLeaser.sprites[initLength+i] = particles[i-1];
        }
        AddToContainer(sLeaser, rCam, null);
    }
    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        base.AddToContainer(sLeaser, rCam, newContatiner);
        var newContainer = rCam.ReturnFContainer("Midground");
        finalText.RemoveFromContainer();
        newContainer.AddChild(finalText);
        foreach (FSprite sprite in particles) {
            sprite.RemoveFromContainer();
            newContainer.AddChild(sprite);
        }
    }

    public override void Reset()
    {
        base.Reset();
        lastCharge = Grub.charge;
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        blackColor = palette.blackColor;
    }

    Color ApplyGlow(Color col, float dist, Color mBase, float glowAlpha)
    {
        Color addCol = mBase * Mathf.InverseLerp(100f, 0f, dist) * 0.5f * glowAlpha;
        addCol.a = 0f;
        col += addCol;
        return col;
    }

    private Color ModifyColor(Color col, float timeStacker)
    {
        float glow = 1f - litRoom;
        glow *= Mathf.Lerp(lastCharge, Grub.charge, timeStacker);
        return Color.Lerp(col, blackColor, (1f - glow) * 0.6f);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        // Set colors
        RoomPalette palette = rCam.currentPalette;

        float glowAlpha = (1f - litRoom) * Mathf.Lerp(lastCharge, Grub.charge, timeStacker);
        Vector2 centerPos = (Grub.bodyChunks[0].pos + Grub.bodyChunks[1].pos) * 0.5f;
        Color mBase = ModifyColor(color, timeStacker);
        Color mTrim = ModifyColor(trimColor, timeStacker);

        sLeaser.sprites[0].color = mBase;
        sLeaser.sprites[1].color = mTrim;
        for (int i = 3; i < 5; i++)
        {
            float dist = Vector2.Distance(sLeaser.sprites[i].GetPosition() + camPos, centerPos);
            sLeaser.sprites[i].color = Color.Lerp(palette.fogColor, Custom.HSL2RGB(0.95f, 1f, 0.865f), 0.5f);
            sLeaser.sprites[i].color = ApplyGlow(sLeaser.sprites[i].color, dist, mBase, glowAlpha);
        }

        TriangleMesh tm = (TriangleMesh)sLeaser.sprites[2];

        for (int i = 0; i < tm.verticeColors.Length; i++)
        {
            float num = Mathf.Clamp01(Mathf.Sin(i / (tm.verticeColors.Length - 1f) * Mathf.PI));
            tm.verticeColors[i] = Color.Lerp(palette.fogColor, Custom.HSL2RGB(Mathf.Lerp(0.95f, 1f, num), 1f, Mathf.Lerp(0.75f, 0.9f, Mathf.Pow(num, 0.15f))), 0.5f);
            float dist = Vector2.Distance(tm.vertices[i] + camPos, centerPos);
            tm.verticeColors[i] = ApplyGlow(tm.verticeColors[i], dist, mBase, glowAlpha);
        }

        // Controlling the fire particles
        finalText.SetPosition(Grub.room.game.manager.rainWorld.options.ScreenSize * 0.5f);
        foreach (FSprite sprite in particles) {
            sprite.SetPosition(Grub.mainBodyChunk.pos - camPos);
        }
    }

    public override void Update()
    {
        base.Update();
        lastCharge = Grub.charge;

        Vector2 pos = Grub.bodyChunks[0].pos;
        float darkness = Grub.room.Darkness(pos);
        litRoom = Mathf.Pow(1f - Mathf.InverseLerp(0f, 0.5f, darkness), 3f);

        if (lightSource != null)
        {
            lightSource.stayAlive = true;
            lightSource.setPos = pos;
            lightSource.setRad = 280f * Grub.charge;
            lightSource.setAlpha = 0.8f * Mathf.Pow(Grub.charge, 0.5f);
            lightSource.color = color;
            if (lightSource.slatedForDeletetion || darkness == 0f) {
                lightSource.Destroy();
                lightSource = null;
            }
        }
        else if (darkness > 0f)
        {
            lightSource = new LightSource(pos, false, trimColor, Grub)
            {
                requireUpKeep = true
            };
            Grub.room.AddObject(lightSource);
        }
    }
}