#if false
using System;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using static System.Reflection.BindingFlags;

namespace PitchBlack;

public class OhNoMoonAndPebblesAreDeadGuys
{
    public static void Apply()
    {
        On.Oracle.Update += Oracle_Update;
        new Hook(typeof(SSOracleRotBehavior).GetMethod("get_EyesClosed", Public | NonPublic | Instance), (Func<SSOracleRotBehavior, bool> orig, SSOracleRotBehavior self) => (self.oracle.stun > 0 && MiscUtils.IsBeaconOrPhoto(self.oracle.room?.game?.session)) || orig(self));
    }
    public static void Oracle_Update(On.Oracle.orig_Update orig, Oracle self, bool eu)
    {
        if (self.room?.world?.region != null && MiscUtils.IsBeaconOrPhoto(self.room.game.session) && (self.oracleBehavior is SSOracleRotBehavior || self.oracleBehavior is SSOracleBehavior))
        {
            self.stun = 9999;
        }
        orig(self, eu);
    }
}
#endif