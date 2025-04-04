using System.IO;
using System.Linq;
using UnityEngine;
using MonoMod.RuntimeDetour;
using static System.Reflection.BindingFlags;
using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace PitchBlack;

public static class WorldChanges
{
    public static void Apply()
    {
        On.Expedition.NeuronDeliveryChallenge.ValidForThisSlugcat += NeuronDeliveryChallenge_ValidForThisSlugcat;
        On.Expedition.PearlDeliveryChallenge.ValidForThisSlugcat += PearlDeliveryChallenge_ValidForThisSlugcat;
        On.Room.Update += Room_Update;
        new Hook(typeof(ElectricDeath).GetMethod("get_Intensity", Public | NonPublic | Instance), ElecIntensity);
        IL.Menu.SlugcatSelectMenu.SlugcatPageContinue.ctor += SlugcatSelectMenu_SlugcatPageContinue_ctor;
    }
    private static void SlugcatSelectMenu_SlugcatPageContinue_ctor(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchCallOrCallvirt<Int32>("ToString")))
        {
            Plugin.logger.LogError($"Pitch Black: Error in {nameof(SlugcatSelectMenu_SlugcatPageContinue_ctor)}");
            return;
        }
        cursor.Emit(OpCodes.Ldarg, 4);
        cursor.EmitDelegate((string cycleNum, SlugcatStats.Name slugcatNumber) =>
        {
            if (MiscUtils.IsBeaconOrPhoto(slugcatNumber))
            {
                int startingRange = 0;
                try
                {
                    startingRange = Convert.ToInt32(cycleNum);
                }
                catch (Exception err)
                {
                    Debug.Log($"Pitch Black: cycle number was not, in fact, a number!\n{err}");
                    startingRange = cycleNum.Length;
                }
                return MiscUtils.GenerateRandomString(startingRange, startingRange+50);
            }
            return cycleNum;
        });
    }
    public static float ElecIntensity(Func<ElectricDeath, float> orig, ElectricDeath self)
    {
        if (MiscUtils.IsBeaconOrPhoto(self.room.game.session))
        {
            return 0.2f;
        }
        return orig(self);
    }
    
    private static void Room_Update(On.Room.orig_Update orig, Room self)
    {
        orig(self);
        // This will probably work, although I wonder if it will override the end-game rain storm. Oh well, do I look like I got time to test that? (don't let me answer that)
        // If the roomRain isn't null, it's a flashcat campaign, and the ElectricDeath setting isn't present
        // We could put the code `&& self.roomSettings.DangerType != RoomRain.DangerType.Thunder` to prevent it from raining in rooms with default no rain at all
        // Also pretty unhappy with using LinQ here, but hopefully it doesn't get tooo laggy. It shouldn't as long as rooms don't have too many effects
        if (self.abstractRoom.AnySkyAccess && self.roomRain != null && MiscUtils.IsBeaconOrPhoto(self.game?.session) && !self.roomSettings.effects.Exists(x => x.type == RoomSettings.RoomEffect.Type.ElectricDeath))
        {
            self.roomRain.intensity = Mathf.Max(0.1f, Mathf.Max(self.roomRain.intensity, self.roomRain.globalRain.Intensity));
        }

    }

    public static bool PearlDeliveryChallenge_ValidForThisSlugcat(On.Expedition.PearlDeliveryChallenge.orig_ValidForThisSlugcat orig, Expedition.PearlDeliveryChallenge self, SlugcatStats.Name slugcat)
    {
        return orig(self, slugcat) && !MiscUtils.IsBeaconOrPhoto(slugcat);
    }

    public static bool NeuronDeliveryChallenge_ValidForThisSlugcat(On.Expedition.NeuronDeliveryChallenge.orig_ValidForThisSlugcat orig, Expedition.NeuronDeliveryChallenge self, SlugcatStats.Name slugcat)
    {
        return orig(self, slugcat) && !MiscUtils.IsBeaconOrPhoto(slugcat);
    }
}