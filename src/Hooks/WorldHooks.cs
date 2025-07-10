namespace PitchBlack;

public static class WorldHooks
{
    public static void Apply()
    {
        On.Region.ctor_string_int_int_Timeline += Region_ctor_string_int_int_Timeline;
    }

    /// <summary>
    /// Replaces rot effect+eye color for Beacon timeline
    /// (Not working..?)
    /// [Lur]
    /// </summary>
    private static void Region_ctor_string_int_int_Timeline(On.Region.orig_ctor_string_int_int_Timeline orig, Region self, string name, int firstRoomIndex, int regionNumber, SlugcatStats.Timeline timelineIndex)
    {
        orig(self, name, firstRoomIndex, regionNumber, timelineIndex);

        if (timelineIndex != null && timelineIndex == Enums.Timeline.Beacon)
        {
            self.regionParams.corruptionEffectColor = Colors.NightmareColor;
            self.regionParams.corruptionEyeColor = Colors.NightmareColor;
        }
    }
}