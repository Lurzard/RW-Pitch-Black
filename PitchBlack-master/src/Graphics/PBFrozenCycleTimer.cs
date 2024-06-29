#if PLAYTEST
using HUD;
using RWCustom;
using UnityEngine;

namespace PitchBlack;

internal class PBFrozenCycleTimer
{
    static bool IsBeaconWorldState(RainMeter rm) => rm.hud.owner is Player player && player.room?.game.session is StoryGameSession session && MiscUtils.IsBeaconOrPhoto(session.saveStateNumber);
    static bool IsBeaconWorldState(RainCycle rc) => rc.world.game.session is StoryGameSession session && MiscUtils.IsBeaconOrPhoto(session.saveStateNumber);
    public static void Apply()
    {
        On.HUD.RainMeter.Draw += RainMeter_Draw;
        //On.RainCycle.GetDesiredCycleLength += RainCycle_GetDesiredCycleLength;
        //On.RainCycle.Update += RainCycle_Update;
    }
    public static void RainMeter_Draw(On.HUD.RainMeter.orig_Draw orig, RainMeter self, float timeStacker)
    {
        orig(self, timeStacker);
        if (IsBeaconWorldState(self) && self.lastFade >= 1f)
        {
            for (int c = 0; c < self.circles.Length; c++)
            {
                if (self.circles[c].lastRad >= 1f)
                {
                    self.circles[c].sprite.color = Custom.hexToColor("08FE00");
                }
            }
        }
    }
    public static int RainCycle_GetDesiredCycleLength(On.RainCycle.orig_GetDesiredCycleLength orig, RainCycle self)
    {
        Debug.Log($"cLength: {self.cycleLength}, base: {self.baseCycleLength}");
        if (IsBeaconWorldState(self))
        {
            self.cycleLength = 19726;
            self.baseCycleLength = self.cycleLength;
        }
        return orig(self);
    }

    public static void RainCycle_Update(On.RainCycle.orig_Update orig, RainCycle self)
    {
        orig(self);
        if (IsBeaconWorldState(self) && self.timer != Mathf.RoundToInt(self.cycleLength * 0.33f))
        {
            self.timer = Mathf.RoundToInt(self.cycleLength * 0.33f);
        }
    }
}
#endif