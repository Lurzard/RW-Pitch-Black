namespace PitchBlack;

// Did not test this because it's so shrimple it should work and if it doesn't I really messed up - Moon
public class EchoPresence
{
    public static void Apply() {
        On.GhostWorldPresence.ctor += GhostWorldPresence_ctor;
    }
    private static void GhostWorldPresence_ctor(On.GhostWorldPresence.orig_ctor orig, GhostWorldPresence self, World world, GhostWorldPresence.GhostID ghostID)
    {
        orig(self, world, ghostID);
        if (MiscUtils.IsBeaconOrPhoto(world.game.session))
        {
            if (ghostID == GhostWorldPresence.GhostID.CC)
            {
                self.songName = "ELSE_LXIX"; //PB_E - Else1
            }
            else if (ghostID == GhostWorldPresence.GhostID.SI)
            {
                self.songName = "ELSELXIX"; //PB_E - Else7
            }
            else if (ghostID == GhostWorldPresence.GhostID.LF)
            {
                self.songName = "ELSELXIX"; //PB_E - Else5
            }
            else if (ghostID == GhostWorldPresence.GhostID.SH)
            {
                self.songName = "ELSE_LXIX"; //PB_E - Else3
            }
            else if (ghostID == GhostWorldPresence.GhostID.UW)
            {
                self.songName = "ELSE_LXIX"; //PB_E - Else4
            }
            else if (ghostID == GhostWorldPresence.GhostID.SB)
            {
                self.songName = "ELSE_LXIX"; //PB_E - Else2
            }
        }
    }
}