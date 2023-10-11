using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HUD;
using On;
using RWCustom;

namespace PitchBlack;

internal class PBFrozenCycleTimer
{
    public static void Apply()
    {
        On.HUD.RainMeter.Draw += NighttimePipColors;

        On.HUD.RainMeter.Update += NighttimeCycleVisualPause;

        On.RainCycle.GetDesiredCycleLength += NighttimeCyclePause1;
        On.RainCycle.Update += NighttimeCyclePause2;
    }

    // - 'RainWorld' does not contain a defenition for 'game'
    // - 'BeaconName' does not exist in the current context

    public static void NighttimePipColors(On.HUD.RainMeter.orig_Draw orig, RainMeter rm, float timeStacker)
    {
        orig(rm, timeStacker);
        if (RainWorld.game.session is StoryGameSession session && session.game == BeaconName && rm.lastFade > 0)
        {
            for (int c = 0; c < rm.circles.Length; c++)
            {
                if (rm.circles[c].lastRad > 0)
                {
                    rm.circles[c].sprite.color = Custom.hexToColor("08FE00"); // Needs "using RWCustom;"
                }
            }
        }
    }


    // You have two options, that I can see, for pausing the cycle timer.
    // Option 1 needs only 1 hook:
    public static void NighttimeCycleVisualPause(On.HUD.RainMeter.orig_Update orig, RainMeter rm)
    {
        orig(rm);
        if (RainWorld.game.session is StoryGameSession session && session.game == BeaconName && rm.lastFade > 0)
        {
            for (int i = 0; i < rm.circles.Length; i++)
            {
                int timerCutoffPoint = (int)(rm.circles.Length * 0.33f);
                rm.circles[i].rad = i < timerCutoffPoint ? 2f : 0;
                rm.circles[i].snapRad = rm.circles[i].rad;
            }
        } // This keeps the timer VISUALS paused even if the actual timer is progressing.
    }

    // Option 2 needs 2 hooks, but this pauses the actual timer itself, not just the visuals.
    public static int NighttimeCyclePause1(On.RainCycle.orig_GetDesiredCycleLength orig, RainCycle rc)
    {
        if (RainWorld.game.session is StoryGameSession session && session.game == BeaconName)
        {
            rc.cycleLength = 10;
            rc.baseCycleLength = rc.cycleLength;
        }
        return orig(rc);
    }

    public static void NighttimeCyclePause2(On.RainCycle.orig_Update orig, RainCycle rc)
    {
        orig(rc);
        if (RainWorld.game.session is StoryGameSession session && session.game == BeaconName && rc.timer != rc.cycleLength * 0.66f) //if you want to freeze the timer at 2/3rds of the way done
        {
            rc.timer = 10;
        }
    }
}
