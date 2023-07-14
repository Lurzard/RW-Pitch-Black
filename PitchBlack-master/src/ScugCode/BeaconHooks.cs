using AbstractObjectType = AbstractPhysicalObject.AbstractObjectType;

namespace PitchBlack;

public static class BeaconHooks
{
    public static void Apply()
    {
        //On.Player.ThrowToGetFree += Player_ThrowToGetFree;
        On.Player.Jump += Player_Jump;
        On.Player.SwallowObject += BeaconTransmuteIntoFlashbang;
        On.Player.GrabUpdate += Player_GrabUpdate;
        On.Player.GraphicsModuleUpdated += BeaconStorageGrafUpdate;
    }

    //private static void Player_ThrowToGetFree(On.Player.orig_ThrowToGetFree orig, Player self, bool eu)
    //{
    //    //spinch: doesnt work & i dont feel like coding anymore today
    //    if (Plugin.scugCWT.TryGetValue(self, out ScugCWT cwt) && cwt.IsBeacon)
    //    {
    //        bool dontThrowFlarebomb = self.FreeHand() == -1;
    //        if (Plugin.BeaconName == self.slugcatStats.name)
    //        {
    //            for (int i = 0; i < 2; i++)
    //            {
    //                if (self.grasps[i]?.grabbed is Weapon)
    //                {
    //                    dontThrowFlarebomb = true;
    //                    break;
    //                }
    //            }
    //        }

    //        if (!dontThrowFlarebomb && cwt.Beacon.storage.storedFlares.Count > 0)
    //        {
    //            //auto throw flarebomb on an empty hand
    //            int freeHand = self.FreeHand();
    //            cwt.Beacon.storage.FlarebombFromStorageToPaw(eu);
    //            self.ThrowObject(freeHand, eu);
    //        }
    //    }

    //    orig(self, eu);
    //}

    private static void Player_Jump(On.Player.orig_Jump orig, Player self)
    {
        orig(self);

        if (Plugin.BeaconName == self.slugcatStats.name)
        {
            if (Player.AnimationIndex.Flip == self.animation)
                self.jumpBoost *= 1.75f;
            //else if (self.rollDirection != 0
            //    && (Player.AnimationIndex.BellySlide == self.animation
            //    || Player.AnimationIndex.RocketJump == self.animation
            //    || Player.AnimationIndex.Roll == self.animation))
            //{
            //    foreach (var bodyChunk in self.bodyChunks)
            //    {
            //        bodyChunk.vel.x *= 2f;
            //    }
            //}
        }
    }

    public static void BeaconTransmuteIntoFlashbang(On.Player.orig_SwallowObject orig, Player self, int grasp)
    {
        orig(self, grasp);
        if (self.slugcatStats.name == Plugin.BeaconName && self.playerState.foodInStomach > 0 && self.objectInStomach.type == AbstractObjectType.Rock)
        {
            self.objectInStomach = new AbstractConsumable(self.room.world, AbstractObjectType.FlareBomb, null, self.abstractCreature.pos, self.room.game.GetNewID(), -1, -1, null);
            self.SubtractFood(1);
        }
    }
    public static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
    {
        bool dontAutoThrowFlarebomb = self.FreeHand() == -1;
        if (Plugin.BeaconName == self.slugcatStats.name)
        {
            for (int i = 0; i < 2; i++)
            {
                if (self.grasps[i]?.grabbed != null && self.IsObjectThrowable(self.grasps[i].grabbed))
                {
                    dontAutoThrowFlarebomb = true;
                    break;
                }
            }
        }

        orig(self, eu);

        if (Plugin.scugCWT.TryGetValue(self, out ScugCWT cwt) && cwt.IsBeacon)
        {
            for (int i = 0; i < 2; i++)
            {
                if (self.grasps[i]?.grabbed is IPlayerEdible || self.eatMeat > 0)
                {
                    //dont take flarebomb from storage if holding food
                    cwt.Beacon.storage.interactionLocked = true;
                    cwt.Beacon.storage.counter = 0;
                }
            }
            if (cwt.Beacon.storage != null && !self.craftingObject)
            {
                //dont increment if crafting
                cwt.Beacon.storage.increment = self.input[0].pckp;
                cwt.Beacon.storage.Update(eu);
            }

            if (!dontAutoThrowFlarebomb && self.input[0].thrw && !self.input[1].thrw && cwt.Beacon.storage.storedFlares.Count > 0)
            {
                //auto throw flarebomb on an empty hand
                int freeHand = self.FreeHand();
                cwt.Beacon.storage.FlarebombFromStorageToPaw(eu);
                self.ThrowObject(freeHand, eu);
            }
        }
    }

    public static void BeaconStorageGrafUpdate(On.Player.orig_GraphicsModuleUpdated orig, Player self, bool actuallyViewed, bool eu)
    {
        if (Plugin.scugCWT.TryGetValue(self, out ScugCWT cwt) && cwt.IsBeacon)
        {
            cwt.Beacon.storage?.GraphicsModuleUpdated(eu);
        }
        orig(self, actuallyViewed, eu);
    }


    // Here it is bois, go get it (๑•̀ㅂ•́)و
    //public static void BeaconCollarStorageUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
    //{
    //    orig(self, eu);
    //    if (Plugin.scugCWT.TryGetValue(self, out ScugCWT cwt) && cwt.IsBeacon)
    //    {
    //        if (cwt.Beacon.storage != null)
    //        {
    //            cwt.Beacon.storage.increment = self.input[0].pckp;
    //            cwt.Beacon.storage.Update(eu);
    //        }
    //    }
    //}
}