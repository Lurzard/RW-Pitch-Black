using JollyCoop.JollyMenu;
using SlugBase.DataTypes;
using SlugBase.Features;
using SlugBase;
using Colour = UnityEngine.Color;
using JollyColorMode = Options.JollyColorMode;
using RWCustom;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Exception = System.Exception;

namespace PitchBlack;

public static class JollyMenuHooks
{
    public static bool ContainsBeaconOrPhoto(string str)
    {
        return str.Contains(PBEnums.SlugcatStatsName.Photomaniac.value) || str.Contains(PBEnums.SlugcatStatsName.Beacon.value);
    }

    public static void Apply()
    {
        //the 2 hooks below are for the very small icons right of the portraits in the jolly menu
        On.PlayerGraphics.JollyFaceColorMenu += PlayerGraphics_JollyFaceColorMenu; //colours for default/auto player 1 colour mode are incorrect
        On.PlayerGraphics.JollyUniqueColorMenu += PlayerGraphics_JollyUniqueColorMenu; //beacon's unique sprite is the flarebombs, so i have to return white

        On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.HasUniqueSprite += SymbolButtonTogglePupButton_HasUniqueSprite;
        On.JollyCoop.JollyMenu.JollyPlayerSelector.GetPupButtonOffName += JollyPlayerSelector_GetPupButtonOffName;
        On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.LoadIcon += SymbolButtonTogglePupButton_LoadIcon;
        IL.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.LoadIcon += SymbolButtonTogglePupButton_LoadIcon_IL;
        On.JollyCoop.JollyMenu.SymbolButtonToggle.LoadIcon += SymbolButtonToggle_LoadIcon;
    }

    #region colour
    public static Colour PlayerGraphics_JollyFaceColorMenu(On.PlayerGraphics.orig_JollyFaceColorMenu orig, SlugcatStats.Name slugName, SlugcatStats.Name reference, int playerNumber)
    {
        Colour val = orig(slugName, reference, playerNumber);
        if (MiscUtils.IsBeaconOrPhoto(slugName))
        {
            if (SlugBaseCharacter.TryGet(slugName, out SlugBaseCharacter charac)
            && charac.Features.TryGet(PlayerFeatures.CustomColors, out ColorSlot[] customColours))
            {
                if (JollyColorMode.DEFAULT == Custom.rainWorld.options.jollyColorMode || JollyColorMode.AUTO == Custom.rainWorld.options.jollyColorMode && 0 == playerNumber)
                    return customColours[1].GetColor(-1);
            }
        }
        return val;
    }

    public static Colour PlayerGraphics_JollyUniqueColorMenu(On.PlayerGraphics.orig_JollyUniqueColorMenu orig, SlugcatStats.Name slugName, SlugcatStats.Name reference, int playerNumber)
    {
        //beacon's unique sprite is the flarebombs, so i have to return white
        Colour val = orig(slugName, reference, playerNumber);
        if (PBEnums.SlugcatStatsName.Beacon == slugName)
        {
            return Colour.white;
        }
        else if (PBEnums.SlugcatStatsName.Photomaniac == slugName)
        {
            if (SlugBaseCharacter.TryGet(slugName, out SlugBaseCharacter charac)
                && charac.Features.TryGet(PlayerFeatures.CustomColors, out ColorSlot[] customColours)
                && customColours.Length > 2)
            {
                if (JollyColorMode.DEFAULT == Custom.rainWorld.options.jollyColorMode || JollyColorMode.AUTO == Custom.rainWorld.options.jollyColorMode && 0 == playerNumber)
                    return customColours[2].GetColor(-1);
            }
        }
        return val;
    }
    #endregion

    public static bool SymbolButtonTogglePupButton_HasUniqueSprite(On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.orig_HasUniqueSprite orig, SymbolButtonTogglePupButton self)
    {
        return orig(self) || ContainsBeaconOrPhoto(self.symbolNameOff);
    }

    private static string JollyPlayerSelector_GetPupButtonOffName(On.JollyCoop.JollyMenu.JollyPlayerSelector.orig_GetPupButtonOffName orig, JollyPlayerSelector self)
    {
        string val = orig(self);
        if (MiscUtils.IsBeaconOrPhoto(self.JollyOptions(self.index).playerClass))
            return self.JollyOptions(self.index).playerClass.value + "_pup_off";
        return val;
    }

    private static void SymbolButtonTogglePupButton_LoadIcon(On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.orig_LoadIcon orig, SymbolButtonTogglePupButton self)
    {
        orig(self);
        if (self.uniqueSymbol != null)
        {
            if (self.uniqueSymbol.fileName.Contains(PBEnums.SlugcatStatsName.Photomaniac.value))
            {
                self.uniqueSymbol.pos.y = self.size.y / 2f;
            }
        }
    }

    private static void SymbolButtonTogglePupButton_LoadIcon_IL(ILContext il)
    {
        ILCursor c = new(il);

        if (!c.TryGotoNext(MoveType.After, i => i.MatchLdcR4(4)))
            return;

        if (!c.TryGotoNext(MoveType.Before, i => i.MatchRet()))
            return;

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate((SymbolButtonTogglePupButton self) =>
        {
            //orig checks if fileName.Contains("on"), which is always true for beacon
            //so unique sprite's y pos is that of the pup's always
            //and then it's ret right they do that after so i have to IL
            //note: uniqueSymbol.fileName is ""unique_" + this.symbolNameOff", so there is no "unique_beacon_pup_on" file
            if (self.uniqueSymbol.fileName.Contains(PBEnums.SlugcatStatsName.Beacon.value) && !self.isToggled)
            {
                self.uniqueSymbol.pos.y = self.size.y / 2f;
            }
        });
    }

    private static void SymbolButtonToggle_LoadIcon(On.JollyCoop.JollyMenu.SymbolButtonToggle.orig_LoadIcon orig, SymbolButtonToggle self)
    {
        if (ContainsBeaconOrPhoto(self.symbolNameOff) && self.isToggled) //isToggled meaning you have pup toggled on
        {
            try
            {
                if (self.symbolNameOff.Contains(PBEnums.SlugcatStatsName.Beacon.value))
                    self.symbol.fileName = "beacon_pup_on";
                else
                    self.symbol.fileName = "photomaniac_pup_on";
                self.symbol.LoadFile();
                self.symbol.sprite.SetElementByName(self.symbol.fileName);
            }
            catch (Exception e)
            {
                Debug.LogException(e); //Debug errors
            }
            finally
            {
                self.symbol.fileName = self.symbolNameOn; //otherwise it forces you to be a pup in menu forever
            }

            return;
        }
        orig(self);
    }
}
