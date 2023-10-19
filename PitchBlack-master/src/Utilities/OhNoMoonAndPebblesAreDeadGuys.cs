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
        // new Hook(typeof(SSOracleBehavior).GetMethod("get_EyesClosed", Public | NonPublic | Instance), (Func<SSOracleBehavior, bool> orig, SSOracleBehavior self) => (self.oracle.stun > 0 && self.oracle.room.game.session is StoryGameSession session && (session.saveStateNumber == Plugin.BeaconName || session.saveStateNumber == Plugin.PhotoName)) || orig(self));
        new Hook(typeof(SSOracleRotBehavior).GetMethod("get_EyesClosed", Public | NonPublic | Instance), (Func<SSOracleRotBehavior, bool> orig, SSOracleRotBehavior self) => (self.oracle.stun > 0 && self.oracle.room.game.session is StoryGameSession session && (session.saveStateNumber == Plugin.BeaconName || session.saveStateNumber == Plugin.PhotoName)) || orig(self));
    }
    public static void Oracle_Update(On.Oracle.orig_Update orig, Oracle self, bool eu)
    {
        if (self.room.world.region != null && (self.room.world.region.name == "RM" || self.room.world.region.name == "LM"))
        {
            if (self.room?.game.session is StoryGameSession session && (session.saveStateNumber == Plugin.BeaconName || session.saveStateNumber == Plugin.PhotoName) && (self.oracleBehavior is MoreSlugcats.SSOracleRotBehavior || self.oracleBehavior is SSOracleBehavior))
            {
                self.stun = 9999;
            }
            orig(self, eu);
        }
    }
}