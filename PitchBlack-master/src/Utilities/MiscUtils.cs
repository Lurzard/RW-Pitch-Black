using static PitchBlack.Plugin;

namespace PitchBlack;

public class MiscUtils
{
    public static bool IsBeaconOrPhoto(SlugcatStats.Name slugName)
    {
        return slugName != null && (slugName == BeaconName || slugName == PhotoName);
    }
    public static bool IsBeaconOrPhoto(Creature crit)
    {
        return crit is Player player && IsBeaconOrPhoto(player.slugcatStats.name);
    }
    
    public static bool IsBeaconOrPhoto(Player player)
    {
        return player.slugcatStats.name == BeaconName || player.slugcatStats.name == PhotoName;
    }

    public static bool IsBeaconWorldstate(RainWorldGame game)
    {
        return game.session is StoryGameSession session && session.saveStateNumber == BeaconName;
    }

    public static bool IsPhotoWorldstate(RainWorldGame game)
    {
        return game.session is StoryGameSession session && session.saveStateNumber == PhotoName;
    }
}
