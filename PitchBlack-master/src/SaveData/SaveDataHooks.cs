using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SlugBase.SaveData;

namespace PitchBlack;

// WIP!!!
public static class SaveDataHooks
{
    // I don't understand SaveData too much so I'm following instructions from Hootis, hopefully it works -Lur
    // Changed some instances of miscWorld stuff to deathpersistent

    // To prevent typos
    public static string dataKey = "beacon_data";

    // Gets DeathPersistentSaveData
    public static BeaconSaveDataDeathPersistent GetDeathPersistent(this DeathPersistentSaveData data)
    {
        if (!data.GetSlugBaseData().TryGet(dataKey, out BeaconSaveDataDeathPersistent save))
        {
            data.GetSlugBaseData().Set(dataKey, save = new());
        }
        return save;
    }
    // Updates DeathPersistentSaveData
    public static void UpdateSaveBeforeCycle(SaveState self)
    {
        var deathData = self.deathPersistentSaveData.GetDeathPersistent();
        // If campaign just started, reset data
        if (self.cycleNumber == 0)
        {
            deathData.Reset();
        }
    }


    // Extension method for RainWorld, to be used in the WinState.CycleCompleted hook
    public static BeaconSaveDataDeathPersistent GetBeaconSaveData(this RainWorld rainWorld)
    {
        if (!rainWorld.progression.miscProgressionData.GetSlugBaseData().TryGet(dataKey, out BeaconSaveDataDeathPersistent save))
        {
            rainWorld.progression.miscProgressionData.GetSlugBaseData().Set(dataKey, save = new());
        }
        return save;
    }

    // Hooks
    public static void Apply()
    {
        On.SaveState.LoadGame += SaveState_LoadGame;
        On.WinState.CycleCompleted += WinState_CycleCompleted;
    }

    private static void WinState_CycleCompleted(On.WinState.orig_CycleCompleted orig, WinState self, RainWorldGame game)
    {
        if (game.IsStorySession && game.StoryCharacter == PBEnums.SlugcatStatsName.Beacon)
        {
            game.rainWorld.progression.miscProgressionData.GetSlugBaseData().Set(dataKey, game.rainWorld.GetBeaconSaveData());
        }
        orig(self, game);
    }

    private static void SaveState_LoadGame(On.SaveState.orig_LoadGame orig, SaveState self, string str, RainWorldGame game)
    {
        orig(self, str, game);
        if (self.saveStateNumber != PBEnums.SlugcatStatsName.Beacon) { return; }
        // Goes through and gets the save data class
        UpdateSaveBeforeCycle(self);
    }
}
