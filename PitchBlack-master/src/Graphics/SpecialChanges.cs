﻿using HUD;
using IL.Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
using Watcher;
using System.Xml;
using System.Linq.Expressions;

namespace PitchBlack;

// Everything to do with changes to certain void/karma/gold things in the game
public class SpecialChanges
{

    // Changing stuff for PB for KarmaFlower & KarmaFlowerPatch
    public static bool PBFlowerMode(RoomCamera rCam)
    {
        if (rCam.room.game.IsStorySession &&
            rCam.room.game.GetStorySession.saveStateNumber == Plugin.BeaconName)
        {
            return true;
        }
        return false;
    }

    public static Color flowerColor;

    // PB's RippleSymbolSprite
    // Todo: All of the RippleLevel Karma stuff remade but for this, as well as savedata
    public static string QualiaSymbolSprite(bool small, float qualiaLevel)
    {
        double num = Math.Round((double)(qualiaLevel * 2f), MidpointRounding.AwayFromZero) / 2.0;
        return (small ? "smallQualia" : "qualia") + num.ToString("#.0", CultureInfo.InvariantCulture);
    }

    public static void Apply()
    {

        // [TODO] -Lur
        // - KarmaLadder
        // ...

        // NOTE: ApplyPalette to change color, InitiateSprites to change Shader
        // KarmaFlowerPatch
        On.Watcher.KarmaFlowerPatch.ApplyPalette += KarmaFlowerPatch_ApplyPalette;
        On.Watcher.KarmaFlowerPatch.InitiateSprites += KarmaFlowerPatch_InitiateSprites;
        // KarmaFlower
        On.KarmaFlower.ApplyPalette += KarmaFlower_ApplyPalette;
        On.KarmaFlower.InitiateSprites += KarmaFlower_InitiateSprites;

        // NOTE: Needs proper Beacon checks to not apply to all slugs
        // KarmaMeter
        On.HUD.KarmaMeter.UpdateGraphic += KarmaMeter_UpdateGraphic;
        On.HUD.KarmaMeter.ctor += KarmaMeter_ctor;

        // All VoidSpawn code is in RippleSpawn.cs
        #region pre 1.10 VoidSpawn hooks
        // VoidSpawn (iirc only the ILs work? bensone did so many... -Lur)
        //IL.VoidSpawnEgg.DrawSprites += VoidSpawnEgg_DrawSprites_IL;
        //IL.VoidSpawnGraphics.ApplyPalette += VoidSpawnGraphics_ApplyPalette_IL;
        //IL.VoidSpawnGraphics.DrawSprites += VoidSpawnGraphics_DrawSprites_IL;
        //On.VoidSpawnGraphics.DrawSprites += VoidSpawnGraphics_DrawSprites;
        //On.VoidSpawnGraphics.Antenna.DrawSprites += Antenna_DrawSprites;
        //On.VoidSpawnGraphics.ApplyPalette += VoidSpawnGraphics_ApplyPalette;
        //On.VoidSpawnGraphics.InitiateSprites += VoidSpawnGraphics_InitiateSprites;
        //On.VoidSpawnEgg.DrawSprites += VoidSpawnEgg_DrawSprites;
        #endregion
    }

    private static void KarmaFlowerPatch_InitiateSprites(On.Watcher.KarmaFlowerPatch.orig_InitiateSprites orig, KarmaFlowerPatch self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (PBFlowerMode(rCam) && MiscUtils.IsRealscapeRegion(rCam))
        {
            for (int i = 0; i < self.flowers.Length; i++)
            {
                sLeaser.sprites[self.EffectSprite(i, 2)].shader = rCam.room.game.rainWorld.Shaders["RippleGlow"];
            }
        }
    }
    private static void KarmaFlowerPatch_ApplyPalette(On.Watcher.KarmaFlowerPatch.orig_ApplyPalette orig, KarmaFlowerPatch self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (PBFlowerMode(rCam))
        {
            if (MiscUtils.IsRealscapeRegion(rCam))
            {
                flowerColor = Color.white;
            }
            if (MiscUtils.IsNightmarescapeRegion(rCam))
            {
                flowerColor = RainWorld.GoldRGB;
            }
            self.petalColor = flowerColor;
        }
    }

    private static void KarmaFlower_InitiateSprites(On.KarmaFlower.orig_InitiateSprites orig, KarmaFlower self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (PBFlowerMode(rCam) && MiscUtils.IsRealscapeRegion(rCam))
        {
            sLeaser.sprites[self.EffectSprite(2)].shader = rCam.room.game.rainWorld.Shaders["RippleGlow"];
        }
    }
    private static void KarmaFlower_ApplyPalette(On.KarmaFlower.orig_ApplyPalette orig, KarmaFlower self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (PBFlowerMode(rCam))
        {
            if (MiscUtils.IsRealscapeRegion(rCam))
            {
                flowerColor = Color.white;
            }
            if (MiscUtils.IsNightmarescapeRegion(rCam))
            {
                flowerColor = RainWorld.GoldRGB;
            }
            self.color = flowerColor;
        }
    }

    private static void KarmaMeter_ctor(On.HUD.KarmaMeter.orig_ctor orig, KarmaMeter self, HUD.HUD hud, FContainer fContainer, IntVector2 displayKarma, bool showAsReinforced)
    {
        orig(self, hud, fContainer, displayKarma, showAsReinforced);
        if (Plugin.qualiaLevel >= 1f)
        {
            self.baseColor = Plugin.SaturatedRipple;
            self.karmaSprite.color = self.baseColor;
        }
    }
    private static void KarmaMeter_UpdateGraphic(On.HUD.KarmaMeter.orig_UpdateGraphic orig, KarmaMeter self)
    {
        orig(self);
        if (Plugin.qualiaLevel >= 1f)
        {
            // For when New sprites are added
            //this.karmaSprite.element = Futile.atlasManager.GetElementWithName(KarmaMeter.RippleSymbolSprite(true, (this.hud.owner as Player).rippleLevel));
            self.baseColor = Plugin.SaturatedRipple;
            self.karmaSprite.color = self.baseColor;
        }
    }

    //private static void KarmaMeter_UpdateGraphic(On.HUD.KarmaMeter.orig_UpdateGraphic orig, KarmaMeter self)
    //{
    //    if (self.hud.owner != null && self.hud.owner is Player && (self.hud.owner as Player).room != null && (self.hud.owner as Player).SlugCatClass == PBSlugcatStatsName.BeaconName && (self.hud.owner as Player).room.world.region != null)
    //    {
    //        if (((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma == 10)
    //        self.karmaSprite.element = Futile.atlasManager.GetElementWithName(FractalNightKarmaSprite(true, self.displayKarma));
    //    }
    //}

    #region Bensone's VoidSpawn Hooks

    //private static void VoidSpawnEgg_DrawSprites(On.VoidSpawnEgg.orig_DrawSprites orig, VoidSpawnEgg self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    //{
    //    if (rCam.room.game.IsStorySession && rCam.room.game.GetStorySession.saveStateNumber == PBSlugcatStatsName.BeaconName)
    //    {
    //        self.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    //        sLeaser.sprites[0].x = Mathf.Lerp(self.lastPos.x, self.pos.x, timeStacker) - camPos.x;
    //        sLeaser.sprites[0].y = Mathf.Lerp(self.lastPos.y, self.pos.y, timeStacker) - camPos.y;
    //        sLeaser.sprites[0].scale = Mathf.Lerp(self.lastRad, self.rad, timeStacker) / 8f;
    //        sLeaser.sprites[0].alpha = 1f / Mathf.Lerp(self.lastRad, self.rad, timeStacker);
    //        if (self.spawn.graphicsModule != null)
    //        {
    //            sLeaser.sprites[0].color = RoseRGB;
    //        }
    //    }
    //    else
    //        orig(self, sLeaser, rCam, timeStacker, camPos);
    //}

    //private static void VoidSpawnGraphics_InitiateSprites(On.VoidSpawnGraphics.orig_InitiateSprites orig, VoidSpawnGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    //{
    //    if (rCam.room.game.IsStorySession && rCam.room.game.GetStorySession.saveStateNumber == PBSlugcatStatsName.BeaconName)
    //    {
    //        sLeaser.sprites = new FSprite[self.totalSprites];
    //        self.InitiateSprites(sLeaser, rCam);

    //        sLeaser.sprites[self.BodyMeshSprite] = TriangleMesh.MakeLongMesh(self.spawn.mainBody.Length, pointyTip: false, customColor: true);
    //        sLeaser.sprites[self.BodyMeshSprite].shader = rCam.game.rainWorld.Shaders["LightSource"];
    //        sLeaser.sprites[self.BodyMeshSprite].color = RoseRGB;

    //        sLeaser.sprites[self.GlowSprite] = new FSprite("Futile_White");
    //        sLeaser.sprites[self.GlowSprite].shader = rCam.game.rainWorld.Shaders["FlatWaterLight"];
    //        sLeaser.sprites[self.GlowSprite].color = RoseRGB;
    //        if (self.hasOwnGoldEffect)
    //        {
    //            sLeaser.sprites[self.EffectSprite] = new FSprite("Futile_White");
    //            sLeaser.sprites[self.EffectSprite].shader = rCam.game.rainWorld.Shaders["LightSource"];
    //            sLeaser.sprites[self.EffectSprite].color = RoseRGB;
    //        }

    //        sLeaser.sprites[self.EffectSprite].color = RoseRGB;
    //        sLeaser.sprites[self.GlowSprite].color = RoseRGB;
    //        sLeaser.sprites[self.BodyMeshSprite].color = RoseRGB;
    //        self.meshColor = RoseRGB;

    //        for (int i = 0; i < (sLeaser.sprites[self.BodyMeshSprite] as TriangleMesh).verticeColors.Length; i++)
    //        {
    //            (sLeaser.sprites[self.BodyMeshSprite] as TriangleMesh).verticeColors[i] = RoseRGB;
    //        }

    //        self.AddToContainer(sLeaser, rCam, null);
    //    }
    //    else
    //        orig(self, sLeaser, rCam);
    //}

    //private static void VoidSpawnGraphics_ApplyPalette(On.VoidSpawnGraphics.orig_ApplyPalette orig, VoidSpawnGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    //{
    //    if(rCam.room.game.IsStorySession && rCam.room.game.GetStorySession.saveStateNumber == PBSlugcatStatsName.BeaconName)
    //    {
    //        self.darkness = palette.darkness;
    //        if (self.dayLightMode)
    //        {
    //            sLeaser.sprites[self.GlowSprite].color = SaturatedRose;
    //        }
    //        else
    //        {
    //            sLeaser.sprites[self.GlowSprite].color = Color.Lerp(SaturatedRose, RoseRGB, Mathf.InverseLerp(0.3f, 0.9f, self.darkness));
    //        }
    //    }
    //    else
    //        orig(self, sLeaser, rCam, palette);
    //}

    //private static void Antenna_DrawSprites(On.VoidSpawnGraphics.Antenna.orig_DrawSprites orig, VoidSpawnGraphics.Antenna self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    //{
    //    orig(self, sLeaser, rCam, timeStacker, camPos);

    //    sLeaser.sprites[self.firstSprite].color = RoseRGB;

    //    for (int i = 0; i < (sLeaser.sprites[self.firstSprite] as TriangleMesh).verticeColors.Length; i++)
    //    {
    //        (sLeaser.sprites[self.firstSprite] as TriangleMesh).verticeColors[i] = RoseRGB;
    //    }
    //}

    private static void VoidSpawnEgg_DrawSprites_IL(ILContext il)
    {
        var cursor = new ILCursor(il);
        if (cursor.TryGotoNext(MoveType.After,
            i => i.MatchCallvirt(typeof(FSprite).GetMethod("set_color"))))
        {
            // Load the RoomCamera argument onto the stack.
            cursor.Emit(OpCodes.Ldarg_2);

            cursor.EmitDelegate<Func<Color, RoomCamera, Color>>((origColor, rCam) =>
            {
                // Apply changes only if the campaing is Beacon?
                if (rCam.room.game.IsStorySession &&
                    rCam.room.game.GetStorySession.saveStateNumber == Plugin.BeaconName)
                {
                    return Plugin.Rose;
                }
                return origColor;
            });
        }
        else
        {
            Debug.LogError("ILHook: Failed to find the sprite color set call in VoidSpawnEgg.DrawSprites.");
        }
    }

    private static void VoidSpawnGraphics_ApplyPalette_IL(ILContext il)
    {
        var cursor = new ILCursor(il);

        if (cursor.TryGotoNext(MoveType.After,
            i => i.MatchCallvirt(typeof(FSprite).GetMethod("set_color"))))
        {
            // Load 'this' and RoomCamera onto the stack.
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_2);

            cursor.EmitDelegate<Func<Color, VoidSpawnGraphics, RoomCamera, Color>>((origColor, self, rCam) =>
            {
                if (rCam.room.game.IsStorySession &&
                    rCam.room.game.GetStorySession.saveStateNumber == Plugin.BeaconName)
                {
                    if (self.dayLightMode)
                    {
                        return Plugin.SaturatedRose;
                    }
                    return Color.Lerp(Plugin.SaturatedRose, Plugin.Rose, Mathf.InverseLerp(0.3f, 0.9f, self.darkness));
                }
                return origColor;
            });
        }
        else
        {
            Debug.LogError("ILHook: Failed to find the glow sprite color set call in VoidSpawnGraphics.ApplyPalette.");
        }
    }

    private static void VoidSpawnGraphics_DrawSprites_IL(ILContext il)
    {
        var cursor = new ILCursor(il);

        ConstructorInfo colorCtor = typeof(Color).GetConstructor(new Type[] { typeof(float), typeof(float), typeof(float), typeof(float) });
        if (colorCtor == null)
        {
            Debug.LogError("ILHook: Could not find the Color.");
            return;
        }

        while (cursor.TryGotoNext(MoveType.After, i => i.MatchNewobj(colorCtor)))
        {
            // Load 'this' and RoomCamera onto the stack.
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_2);

            cursor.EmitDelegate<Func<Color, VoidSpawnGraphics, RoomCamera, Color>>((origColor, self, rCam) =>
            {
                if (rCam.room.game.IsStorySession &&
                    rCam.room.game.GetStorySession.saveStateNumber == Plugin.BeaconName)
                {
                    return Plugin.Rose;
                }
                return origColor;
            });
        }
    }

    #endregion
}
