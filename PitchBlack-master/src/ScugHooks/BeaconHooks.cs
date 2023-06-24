using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.IO;
using System.Linq;
using RWCustom;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Security;
using MonoMod.Cil;
using static Player;
using Fisobs.Core;

namespace PitchBlack
{
    public static class BeaconHooks
    {
        public static void Apply() {
            On.Player.Grabability += BeaconDontWantToTouchCollar;
            On.Player.SwallowObject += BeaconTransmuteIntoFlashbang;
            On.Player.GrabUpdate += BeaconCollarStorageUpdate;
            On.Player.GrabUpdate += Player_GrabUpdate1;
            On.Player.ctor += BeaconCtor;
            On.Player.GraphicsModuleUpdated += BeaconStorageGrafUpdate;
        }
        public static Player.ObjectGrabability BeaconDontWantToTouchCollar(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            var result = orig(self, obj);
            if (self.slugcatStats.name == Plugin.BeaconName)
            {
                if (Plugin.bCon.TryGetValue(self, out BeaconCWT b))
                {
                    if (b.storage != null && obj is FlareBomb flare)
                    {
                        foreach (FlareBomb storedFlare in b.storage.storedFlares)
                        {
                            if (storedFlare == flare)
                            {
                                return Player.ObjectGrabability.CantGrab;
                            }
                        }
                    }
                }
            }
            return self.slugcatStats.name == Plugin.PhotoName && obj is Weapon ? Player.ObjectGrabability.OneHand : orig(self, obj);
        }
        public static void BeaconTransmuteIntoFlashbang(On.Player.orig_SwallowObject orig, Player self, int grasp)
        {
            orig(self, grasp);
            if (self.slugcatStats.name == Plugin.BeaconName)
            {
                if (self.playerState.foodInStomach <= 0) 
                { 
                    return; 
                }
                if (self.objectInStomach.type == AbstractPhysicalObject.AbstractObjectType.Rock)
                {
                    self.objectInStomach = new AbstractConsumable(self.room.world, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null, self.room.GetWorldCoordinate(self.firstChunk.pos), self.room.game.GetNewID(), -1, -1, null);
                    self.SubtractFood(1);
                }
            }
        }
        public static void BeaconCollarStorageUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            orig(self, eu);
            if (self.slugcatStats.name == Plugin.BeaconName)
            {
                if (Plugin.bCon.TryGetValue(self, out BeaconCWT b))
                {
                    if (b.storage != null)
                    {
                        b.storage.increment = self.input[0].pckp;
                        b.storage.Update(eu);
                    }
                }
            }
        }
        public static void Player_GrabUpdate1(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            orig(self, eu);
            if (self.slugcatStats.name == Plugin.BeaconName)
            {
                if (Plugin.bCon.TryGetValue(self, out BeaconCWT b))
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (self.grasps[i] != null && self.grasps[i].grabbed is IPlayerEdible)
                        {
                            b.storage.interactionLocked = true;
                            b.storage.counter = 0;
                        }
                    }
                    if (b.storage != null)
                    {
                        b.storage.increment = self.input[0].pckp;
                        b.storage.Update(eu);
                    }
                }
            }
        }
        public static void BeaconCtor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self,abstractCreature,world);
            if (self.slugcatStats.name == Plugin.BeaconName)
            {
                Plugin.bCon.Add(self, new BeaconCWT (self));
                
            }
            if(!Plugin.bCon.TryGetValue(self, out BeaconCWT b))
            {

            }
            if (self.slugcatStats.name == Plugin.PhotoName)
            {
                Plugin.pCon.Add(self, new PhotoCWT());
            }
            if(!Plugin.pCon.TryGetValue(self, out PhotoCWT p))
            {

            }
        }
        public static void BeaconStorageGrafUpdate(On.Player.orig_GraphicsModuleUpdated orig, Player self, bool actuallyViewed, bool eu)
        {
            if (self.slugcatStats.name == Plugin.BeaconName)
            {
                if (Plugin.bCon.TryGetValue(self, out BeaconCWT b))
                {
                    if (b.storage != null)
                    {
                        b.storage.GraphicsModuleUpdated(actuallyViewed, eu);
                    }
                }
            }
            orig(self, actuallyViewed, eu);
        }
    }
}