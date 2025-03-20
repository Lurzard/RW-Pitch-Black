using HUD;
using IL.Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PitchBlack;

public class SpecialChanges
{
    public static Color RoseRGB = new Color(0.529f, 0.184f, 0.360f); //#872f5c
    public static Color DesaturatedGold = new Color(0.5294117647f, 0.47843137254f, 0.42352941176f);
    public static Color White = new Color(1f, 1f, 1f);
    public static Color SaturatedRose = RoseRGB * 2f;

    //karmaflower karma boost tracking!!
    private static bool karmaBoost = false;
    private static bool doubledKarmaBoost = false;
    private static bool fractalNightKarma = false;

    public static void Apply()
    {
        On.HUD.KarmaMeter.Update += KarmaMeter_Update;
        On.HUD.KarmaMeter.KarmaSymbolSprite += KarmaMeter_KarmaSymbolSprite;

        On.KarmaFlower.ApplyPalette += KarmaFlower_ApplyPalette;

        IL.VoidSpawnEgg.DrawSprites += VoidSpawnEgg_DrawSprites_IL;
        IL.VoidSpawnGraphics.ApplyPalette += VoidSpawnGraphics_ApplyPalette_IL;
        IL.VoidSpawnGraphics.DrawSprites += VoidSpawnGraphics_DrawSprites_IL;
        //On.VoidSpawnGraphics.DrawSprites += VoidSpawnGraphics_DrawSprites;
        //On.VoidSpawnGraphics.Antenna.DrawSprites += Antenna_DrawSprites;


        //On.VoidSpawnGraphics.ApplyPalette += VoidSpawnGraphics_ApplyPalette;
        //On.VoidSpawnGraphics.InitiateSprites += VoidSpawnGraphics_InitiateSprites;

        //On.VoidSpawnEgg.DrawSprites += VoidSpawnEgg_DrawSprites;
    }

    private static string KarmaMeter_KarmaSymbolSprite(On.HUD.KarmaMeter.orig_KarmaSymbolSprite orig, bool small, IntVector2 k)
    {
        int min = 0;
        if (ModManager.MSC && small)
        {
            min = -1;
        }
        if (k.x < 5)
        {
            return (small ? "smallKarma" : "karma") + Mathf.Clamp(k.x, min, 4).ToString();
        }
        return (small ? "smallKarma" : "karma") + Mathf.Clamp(k.x, 5, 10).ToString() + "-" + Mathf.Clamp(k.y, k.x, 10).ToString();
    }

    //private static void KarmaMeter_UpdateGraphic(On.HUD.KarmaMeter.orig_UpdateGraphic orig, KarmaMeter self)
    //{
    //    if (self.hud.owner != null && self.hud.owner is Player && (self.hud.owner as Player).room != null && (self.hud.owner as Player).SlugCatClass == Plugin.BeaconName && (self.hud.owner as Player).room.world.region != null)
    //    {
    //        if (((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma == 10)
    //        self.karmaSprite.element = Futile.atlasManager.GetElementWithName(FractalNightKarmaSprite(true, self.displayKarma));
    //    }
    //}

    //this might have to become an IL hook?
    private static void KarmaMeter_Update(On.HUD.KarmaMeter.orig_Update orig, KarmaMeter self)
    {
        orig(self);
        //if owner != null, player != null, room != null, Beacon, and region != null
        if (self.hud.owner != null && self.hud.owner is Player && (self.hud.owner as Player).room != null && (self.hud.owner as Player).SlugCatClass == Plugin.BeaconName && (self.hud.owner as Player).room.world.region != null)
        {
            if (self.showAsReinforced)
            {
                if (!karmaBoost && ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma < 9 && ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma != 4) //any karma except max or 4
                {
                    ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma++; //increasing karma once
                    (self.hud.owner as Player).room.game.cameras[0].hud.karmaMeter.UpdateGraphic();
                    karmaBoost = true;
                }

                else if (!doubledKarmaBoost && ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap == 4 && !karmaBoost) //same implementation as the first echo giving you 2 extra
                {
                    ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma++;
                    ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap = 6; //making both karma and karmaCap = 6
                    (self.hud.owner as Player).room.game.cameras[0].hud.karmaMeter.UpdateGraphic();
                    doubledKarmaBoost = true;
                }

                else if (!fractalNightKarma && !karmaBoost && !doubledKarmaBoost && ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma == 9)
                {
                    ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma = 10;
                    (self.hud.owner as Player).room.game.cameras[0].hud.karmaMeter.UpdateGraphic();
                    fractalNightKarma = true;
                }
            }

            if (!self.showAsReinforced && (karmaBoost || doubledKarmaBoost || fractalNightKarma))
            {
                if (karmaBoost)
                {
                    ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma--;
                    (self.hud.owner as Player).room.game.cameras[0].hud.karmaMeter.UpdateGraphic();
                    karmaBoost = false;
                }

                if (doubledKarmaBoost)
                {
                    ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma = 4;
                    ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap = 4;
                    (self.hud.owner as Player).room.game.cameras[0].hud.karmaMeter.UpdateGraphic();
                    doubledKarmaBoost = false;
                }

                if (fractalNightKarma)
                {
                    ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma = 9;
                    ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap = 9;
                    (self.hud.owner as Player).room.game.cameras[0].hud.karmaMeter.UpdateGraphic();
                    fractalNightKarma = false;
                }
            }
        }
    }

    public static string FractalNightKarmaSprite(bool small, IntVector2 k)
    {
        return small ? "smallKarma10" : "karma10";
    }

    private static void KarmaFlower_ApplyPalette(On.KarmaFlower.orig_ApplyPalette orig, KarmaFlower self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (rCam.room.game.IsStorySession && rCam.room.game.GetStorySession.saveStateNumber == Plugin.BeaconName)
        {
            self.color = White;
        }
    }

    #region Bensone's VoidSpawn Hooks

    //private static void VoidSpawnEgg_DrawSprites(On.VoidSpawnEgg.orig_DrawSprites orig, VoidSpawnEgg self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    //{
    //    if (rCam.room.game.IsStorySession && rCam.room.game.GetStorySession.saveStateNumber == Plugin.BeaconName)
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
    //    if (rCam.room.game.IsStorySession && rCam.room.game.GetStorySession.saveStateNumber == Plugin.BeaconName)
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
    //    if(rCam.room.game.IsStorySession && rCam.room.game.GetStorySession.saveStateNumber == Plugin.BeaconName)
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
                    return RoseRGB;
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
                        return SaturatedRose;
                    }
                    return Color.Lerp(SaturatedRose, RoseRGB, Mathf.InverseLerp(0.3f, 0.9f, self.darkness));
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
                    return RoseRGB;
                }
                return origColor;
            });
        }
    }

    #endregion
}
