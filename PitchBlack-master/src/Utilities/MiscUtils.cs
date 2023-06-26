using static PitchBlack.Plugin;

namespace PitchBlack;

public class MiscUtils
{
    public static bool SlugIsInMod(SlugcatStats.Name slugName)
    {
        return null != slugName && (slugName == BeaconName || slugName == PhotoName);
    }
}
