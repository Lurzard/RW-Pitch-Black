using System;
using System.Globalization;
using HUD;
using RWCustom;

namespace PitchBlack;

public static class KarmaMeterHooks
{
    public static void Apply()
    {
        On.HUD.KarmaMeter.UpdateGraphic += KarmaMeter_UpdateGraphic;
        On.HUD.KarmaMeter.ctor += KarmaMeter_ctor;
        On.HUD.KarmaMeter.Update += KarmaMeter_Update;
    }

    private static void KarmaMeter_Update(On.HUD.KarmaMeter.orig_Update orig, KarmaMeter self)
    {
        orig(self);
        if (self.hud.owner is Player p
            && p.SlugCatClass == PBEnums.SlugcatStatsName.Beacon
            && p.rippleLevel >= 1f)
        {
            self.karmaSprite.element = Futile.atlasManager.GetElementWithName(MiscUtils.QualiaSymbolSprite(true, p.rippleLevel));
            self.baseColor = Plugin.SaturatedRose;
            if (self.showAsReinforced)
            {
                self.karmaSprite.element = Futile.atlasManager.GetElementWithName(MiscUtils.SidewaysSymbolSprite(true, p.rippleLevel));
                self.baseColor = RainWorld.RippleGold;
            }
            self.karmaSprite.color = self.baseColor;
        }
    }

    private static void KarmaMeter_ctor(On.HUD.KarmaMeter.orig_ctor orig, KarmaMeter self, HUD.HUD hud, FContainer fContainer, IntVector2 displayKarma, bool showAsReinforced)
    {
        orig(self, hud, fContainer, displayKarma, showAsReinforced);
        if (hud.owner is Player &&
            (hud.owner as Player).SlugCatClass == PBEnums.SlugcatStatsName.Beacon &&
            (hud.owner as Player).rippleLevel >= 1f)
        {
            self.karmaSprite.element = Futile.atlasManager.GetElementWithName(MiscUtils.QualiaSymbolSprite(true, (self.hud.owner as Player).rippleLevel));
            self.baseColor = Plugin.SaturatedRose;
            if (self.showAsReinforced)
            {
                self.karmaSprite.element = Futile.atlasManager.GetElementWithName(MiscUtils.SidewaysSymbolSprite(true, (self.hud.owner as Player).rippleLevel));
                self.baseColor = RainWorld.SaturatedGold;
            }
            self.karmaSprite.color = self.baseColor;
        }
    }

    private static void KarmaMeter_UpdateGraphic(On.HUD.KarmaMeter.orig_UpdateGraphic orig, KarmaMeter self)
    {
        orig(self);
        if ((self.hud.owner as Player).SlugCatClass == PBEnums.SlugcatStatsName.Beacon &&
            (self.hud.owner as Player).rippleLevel >= 1f)
        {
            self.karmaSprite.element = Futile.atlasManager.GetElementWithName(MiscUtils.QualiaSymbolSprite(true, (self.hud.owner as Player).rippleLevel));
            self.baseColor = Plugin.SaturatedRose;
            if (self.showAsReinforced)
            {
                self.karmaSprite.element = Futile.atlasManager.GetElementWithName(MiscUtils.SidewaysSymbolSprite(true, (self.hud.owner as Player).rippleLevel));
                self.baseColor = RainWorld.SaturatedGold;
            }
            self.karmaSprite.color = self.baseColor;
        }
    }
}