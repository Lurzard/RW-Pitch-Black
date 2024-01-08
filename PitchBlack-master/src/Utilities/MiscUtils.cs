using static PitchBlack.Plugin;

namespace PitchBlack;

public class MiscUtils
{
    public static bool IsBeaconOrPhoto(GameSession session)
    {
        if (session is StoryGameSession s) {
            return IsBeaconOrPhoto(s.saveStateNumber);
        }
        return false;
    }
    public static bool IsBeaconOrPhoto(SlugcatStats.Name slugName)
    {
        return null != slugName && (slugName == BeaconName || slugName == PhotoName);
    }
    public static bool IsBeaconOrPhoto(Creature crit)
    {
        return crit is Player player && IsBeaconOrPhoto(player.slugcatStats.name);
    }
}
