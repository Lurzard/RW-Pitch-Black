#if PLAYTEST
namespace PitchBlack;

// Did not test this because it's so shrimple it should work and if it doesn't I really messed up - Moon
public class EchoMusic
{
    public static void Apply() {
        On.GhostWorldPresence.ctor += GhostWorldPresence_ctor;
    }
    private static void GhostWorldPresence_ctor(On.GhostWorldPresence.orig_ctor orig, GhostWorldPresence self, World world, GhostWorldPresence.GhostID ghostID)
    {
        orig(self, world, ghostID);
        if (MiscUtils.IsBeaconOrPhoto(world.game.session)) {
            self.songName = "ELSE_LXIX";
        }
    }
}
#endif