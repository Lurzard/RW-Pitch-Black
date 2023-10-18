using HUD;
using RWCustom;
using static PitchBlack.Plugin;

namespace PitchBlack;

internal class PBFrozenCycleTimer
{
    static bool IsBeaconWorldState(RainMeter rm) => rm.hud.owner is Player player && player.room?.game.session is StoryGameSession session && session.saveStateNumber == BeaconName;
    static bool IsBeaconWorldState(RainCycle rc) => rc.world.game.session is StoryGameSession session && session.saveStateNumber == BeaconName;
    public static void Apply()
    {
        On.HUD.RainMeter.Draw += NighttimePipColors;
        // On.HUD.RainMeter.Update += NighttimeCycleVisualPause;
        On.RainCycle.GetDesiredCycleLength += NighttimeCyclePause1;
        On.RainCycle.Update += NighttimeCyclePause2;
    }
    public static void NighttimePipColors(On.HUD.RainMeter.orig_Draw orig, RainMeter self, float timeStacker)
    {
        orig(self, timeStacker);
        if (IsBeaconWorldState(self) && self.lastFade > 0)
        {
            for (int c = 0; c < self.circles.Length; c++)
            {
                if (self.circles[c].lastRad > 0)
                {
                    self.circles[c].sprite.color = Custom.hexToColor("08FE00"); // Needs "using RWCustom;"
                }
            }
        }
    }


    // You have two options, that I can see, for pausing the cycle timer.
    // Option 1 needs only 1 hook:
    public static void NighttimeCycleVisualPause(On.HUD.RainMeter.orig_Update orig, RainMeter self)
    {
        orig(self);
        if (IsBeaconWorldState(self) && self.lastFade > 0)
        {
            for (int i = 0; i < self.circles.Length; i++)
            {
                int timerCutoffPoint = (int)(self.circles.Length * 0.33f);
                self.circles[i].rad = i < timerCutoffPoint ? 2f : 0;
                self.circles[i].snapRad = self.circles[i].rad;
            }
        } // This keeps the timer VISUALS paused even if the actual timer is progressing.
    }

    // Option 2 needs 2 hooks, but this pauses the actual timer itself, not just the visuals.
    public static int NighttimeCyclePause1(On.RainCycle.orig_GetDesiredCycleLength orig, RainCycle self)
    {
        if (IsBeaconWorldState(self))
        {
            self.cycleLength = 10;
            self.baseCycleLength = self.cycleLength;
        }
        return orig(self);
    }

    public static void NighttimeCyclePause2(On.RainCycle.orig_Update orig, RainCycle self)
    {
        orig(self);
        if (IsBeaconWorldState(self) && self.timer != self.cycleLength * 0.66f) //if you want to freeze the timer at 2/3rds of the way done
        {
            self.timer = 10;
        }
    }
}
