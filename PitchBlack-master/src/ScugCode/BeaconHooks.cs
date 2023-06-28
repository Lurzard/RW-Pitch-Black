namespace PitchBlack
{
    public static class BeaconHooks
    {
        public static void Apply()
        {
            On.Player.SwallowObject += BeaconTransmuteIntoFlashbang;
            On.Player.GrabUpdate += Player_GrabUpdate;
            On.Player.GraphicsModuleUpdated += BeaconStorageGrafUpdate;
        }
        public static void BeaconTransmuteIntoFlashbang(On.Player.orig_SwallowObject orig, Player self, int grasp)
        {
            orig(self, grasp);
            if (self.slugcatStats.name == Plugin.BeaconName && self.playerState.foodInStomach > 0 && self.objectInStomach.type == AbstractPhysicalObject.AbstractObjectType.Rock)
            {
                self.objectInStomach = new AbstractConsumable(self.room.world, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null, self.room.GetWorldCoordinate(self.firstChunk.pos), self.room.game.GetNewID(), -1, -1, null);
                self.SubtractFood(1);
            }
        }
        public static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            orig(self, eu);
            if (Plugin.scugCWT.TryGetValue(self, out ScugCWT cwt) && cwt.IsBeacon)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (self.grasps[i]?.grabbed is IPlayerEdible)
                    {
                        cwt.Beacon.storage.interactionLocked = true;
                        cwt.Beacon.storage.counter = 0;
                    }
                }
                if (cwt.Beacon.storage != null)
                {
                    cwt.Beacon.storage.increment = self.input[0].pckp;
                    cwt.Beacon.storage.Update(eu);
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
        public static void BeaconCollarStorageUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            orig(self, eu);
            if (Plugin.scugCWT.TryGetValue(self, out ScugCWT cwt) && cwt.IsBeacon)
            {
                if (cwt.Beacon.storage != null)
                {
                    cwt.Beacon.storage.increment = self.input[0].pckp;
                    cwt.Beacon.storage.Update(eu);
                }
            }
        }
    }
}