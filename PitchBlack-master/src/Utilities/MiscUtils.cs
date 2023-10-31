using static PitchBlack.Plugin;

namespace PitchBlack;

public class MiscUtils
{
    public static bool IsBeaconOrPhoto(SlugcatStats.Name slugName)
    {
        return null != slugName && (slugName == BeaconName || slugName == PhotoName);
    }
    public static bool IsBeaconOrPhoto(Creature crit)
    {
        return crit is Player player && IsBeaconOrPhoto(player.slugcatStats.name);
    }
    
    public static bool IsBeaconOrPhoto(Player player)
    {
        return player.slugcatStats?.name?.value == "Beacon" || player.slugcatStats?.name?.value == "Photomaniac";
    }
}
