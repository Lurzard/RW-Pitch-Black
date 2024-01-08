using static PitchBlack.Plugin;

namespace PitchBlack;

public class MiscUtils
{
    #region Bacon or Photo checks
    public static bool IsBeaconOrPhoto(GameSession session)
    {
        return (session is StoryGameSession s) && IsBeaconOrPhoto(s.saveStateNumber);
    }
    public static bool IsBeaconOrPhoto(Creature crit)
    {
        return crit is Player player && IsBeaconOrPhoto(player.slugcatStats.name);
    }
    public static bool IsBeaconOrPhoto(SlugcatStats.Name slugName)
    {
        return null != slugName && (slugName == BeaconName || slugName == PhotoName);
    }
    #endregion
    #region Bacon Checks
    public static bool IsBeacon(GameSession session) {
        return (session is StoryGameSession s) && IsBeacon(s.saveStateNumber);
    }
    public static bool IsBeacon(Creature crit) {
        return (crit is Player player) && IsBeacon(player.slugcatStats.name);
    }
    public static bool IsBeacon(SlugcatStats.Name name) {
        return name != null && name == BeaconName;
    }
    #endregion
    #region Photo Checks
    public static bool IsPhoto(GameSession session) {
        return (session is StoryGameSession s) && IsPhoto(s.saveStateNumber);
    }
    public static bool IsPhoto(Creature crit) {
        return (crit is Player player) && IsPhoto(player.slugcatStats.name);
    }
    public static bool IsPhoto(SlugcatStats.Name name) {
        return name != null && name == PhotoName;
    }
    #endregion
}