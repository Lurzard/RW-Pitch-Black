using System.IO;
using System.Linq;
using UnityEngine;
using MonoMod.RuntimeDetour;
using static System.Reflection.BindingFlags;
using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace PitchBlack;

public static class WorldHooks
{
    public static void Apply()
    {
        On.Expedition.NeuronDeliveryChallenge.ValidForThisSlugcat += NeuronDeliveryChallenge_ValidForThisSlugcat;
        On.Expedition.PearlDeliveryChallenge.ValidForThisSlugcat += PearlDeliveryChallenge_ValidForThisSlugcat;
        //On.Room.Update += Room_Update;
        //new Hook(typeof(ElectricDeath).GetMethod("get_Intensity", Public | NonPublic | Instance), ElecIntensity);
        //On.Region.ctor_string_int_int_Timeline += Region_ctor_string_int_int_Timeline;
    }

    // Change the color of rot (not working)
    private static void Region_ctor_string_int_int_Timeline(On.Region.orig_ctor_string_int_int_Timeline orig, Region self, string name, int firstRoomIndex, int regionNumber, SlugcatStats.Timeline timelineIndex)
    {
        orig(self, name, firstRoomIndex, regionNumber, timelineIndex);
        if (timelineIndex != null && timelineIndex == PBEnums.Timeline.Beacon)
        {
            self.regionParams.corruptionEffectColor = RainWorld.RippleColor;
            self.regionParams.corruptionEyeColor = RainWorld.RippleColor;
        }
    }
    public static float ElecIntensity(Func<ElectricDeath, float> orig, ElectricDeath self)
    {
        if (MiscUtils.IsBeaconOrPhoto(self.room.game.session))
        {
            return 0.2f;
        }
        return orig(self);
    }

    //private static void Room_Update(On.Room.orig_Update orig, Room self)
    //{
    //    orig(self);
    //    // This will probably work, although I wonder if it will override the end-game rain storm. Oh well, do I look like I got time to test that? (don't let me answer that)
    //    // If the roomRain isn't null, it's a flashcat campaign, and the ElectricDeath setting isn't present
    //    // We could put the code `&& self.roomSettings.DangerType != RoomRain.DangerType.Thunder` to prevent it from raining in rooms with default no rain at all
    //    // Also pretty unhappy with using LinQ here, but hopefully it doesn't get tooo laggy. It shouldn't as long as rooms don't have too many effects
    //    if (self.abstractRoom.AnySkyAccess && self.roomRain != null && MiscUtils.IsBeaconOrPhoto(self.game?.session) && !self.roomSettings.effects.Exists(x => x.type == RoomSettings.RoomEffect.Type.ElectricDeath))
    //    {
    //        self.roomRain.intensity = Mathf.Max(0.1f, Mathf.Max(self.roomRain.intensity, self.roomRain.globalRain.Intensity));
    //    }
    //}

    public static bool PearlDeliveryChallenge_ValidForThisSlugcat(On.Expedition.PearlDeliveryChallenge.orig_ValidForThisSlugcat orig, Expedition.PearlDeliveryChallenge self, SlugcatStats.Name slugcat)
    {
        return orig(self, slugcat) && !MiscUtils.IsBeaconOrPhoto(slugcat);
    }

    public static bool NeuronDeliveryChallenge_ValidForThisSlugcat(On.Expedition.NeuronDeliveryChallenge.orig_ValidForThisSlugcat orig, Expedition.NeuronDeliveryChallenge self, SlugcatStats.Name slugcat)
    {
        return orig(self, slugcat) && !MiscUtils.IsBeaconOrPhoto(slugcat);
    }
}