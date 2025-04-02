namespace PitchBlack;

public class SyncMenuRegion
{
    public static void Apply() {
        On.WinState.CycleCompleted += WinState_CycleCompleted;
        On.Menu.FastTravelScreen.Singal += Menu_FastTravelScreen_Singal;
    }
    private static void Menu_FastTravelScreen_Singal(On.Menu.FastTravelScreen.orig_Singal orig, Menu.FastTravelScreen self, Menu.MenuObject sender, string message)
    {
        orig(self, sender, message);
        if (MiscUtils.IsBeaconOrPhoto(self.activeWorld?.game?.session)) {
            MiscUtils.TryReplaceCollectionMenuBackground(self.activeWorld.name);
        }
    }
    private static void WinState_CycleCompleted(On.WinState.orig_CycleCompleted orig, WinState self, RainWorldGame game)
    {
        orig(self, game);
        if (MiscUtils.IsBeaconOrPhoto(game.session)) {
            foreach (var record in game.GetStorySession.playerSessionRecords) {
                MiscUtils.TryReplaceCollectionMenuBackground(record.wentToSleepInRegion);
            }
        }
    }
}