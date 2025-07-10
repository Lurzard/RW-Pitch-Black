namespace PitchBlack;

public static class WorldHooks
{
    public static void Apply()
    {
        On.Region.ctor_string_int_int_RainWorldGame_Timeline += Region_ctor_string_int_int_RainWorldGame_Timeline;
    }

    /// <summary>
    /// Replace rot eye+effect color for Beacon
    /// </summary>
    /// <param name="timelineIndex">1.10 Slugcat timeline</param>
    private static void Region_ctor_string_int_int_RainWorldGame_Timeline(On.Region.orig_ctor_string_int_int_RainWorldGame_Timeline orig, Region self, string name, int firstRoomIndex, int regionNumber, RainWorldGame game, SlugcatStats.Timeline timelineIndex)
    {
        orig(self, name, firstRoomIndex, regionNumber, game, timelineIndex);

        if (timelineIndex != null && timelineIndex == Enums.Timeline.Beacon)
        {
            self.regionParams.corruptionEffectColor = Colors.NightmareColor;
            self.regionParams.corruptionEyeColor = Colors.NightmareColor;
        }
    }
}