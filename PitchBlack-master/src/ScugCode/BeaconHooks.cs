using AbstractObjectType = AbstractPhysicalObject.AbstractObjectType;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace PitchBlack;

public static class BeaconHooks
{
    public static void Apply()
    {
        //spinch: i also want to hook to Player.ThrowToGetFree to make beacon throw flashbangs if grasps are empty

        IL.Player.GrabUpdate += Player_GrabUpdate_IL;
        On.Player.Die += Player_Die;
        On.Player.Jump += Player_Jump;
        On.Player.SwallowObject += BeaconTransmuteIntoFlashbang;
        On.Player.GrabUpdate += Player_GrabUpdate;
        On.Player.GraphicsModuleUpdated += BeaconStorageGrafUpdate;
    }

    private static void Player_GrabUpdate_IL(ILContext il)
    {
        //spinch: give priority to auto-throwing flarebombs from storage over throwing slug on back
        ILCursor c = new(il);
        ILLabel label = il.DefineLabel();

        if (!c.TryGotoNext(MoveType.After,
            i => i.MatchCallOrCallvirt<Player.SlugOnBack>("get_HasASlug"),
            i => i.MatchBrfalse(out label)
            ))
        {
            return;
        }

        c.Emit(OpCodes.Ldarg_0);

        c.EmitDelegate((Player self) =>
        {
            if (!Plugin.scugCWT.TryGetValue(self, out var cwt) || !cwt.IsBeacon)
                return false;

            if (self.slugOnBack == null || !self.slugOnBack.HasASlug)
                return false;

            if (cwt.Beacon.storage.storedFlares.Count <= 0)
                return false;

            foreach (var item in self.grasps)
            {
                if (item != null && self.IsObjectThrowable(item.grabbed))
                {
                    return false;
                }
            }

            return true;
        });

        c.Emit(OpCodes.Brtrue, label);
    }

    private static void Player_Die(On.Player.orig_Die orig, Player self)
    {
        orig(self);
        //spinch: on death, all of beacon's stored flarebombs gets popped off
        if (Plugin.scugCWT.TryGetValue(self, out var cwt) && cwt.Beacon?.storage != null)
        {
            while (cwt.Beacon.storage.storedFlares.Count > 0)
            {
                FlareBomb fb = cwt.Beacon.storage.storedFlares.Pop();
                BeaconCWT.AbstractStoredFlare af = cwt.Beacon.storage.abstractFlare.Pop();

                fb.firstChunk.vel = self.mainBodyChunk.vel + RWCustom.Custom.RNV() * 3f * UnityEngine.Random.value;
                fb.ChangeMode(Weapon.Mode.Free);

                af?.Deactivate();
            }
        }
    }

    private static void Player_Jump(On.Player.orig_Jump orig, Player self)
    {
        orig(self);

        if (Plugin.BeaconName == self.slugcatStats.name)
        {
            if (Player.AnimationIndex.Flip == self.animation)
                self.jumpBoost *= 1f + 0.75f;
            else
                self.jumpBoost *= 1f + 0.1f;
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
        bool slugIsBeacon = Plugin.scugCWT.TryGetValue(self, out ScugCWT cwt) && cwt.IsBeacon;
        bool dontAutoThrowFlarebomb = self.FreeHand() == -1;

        if (slugIsBeacon)
        {
            //spinch: check grasps to see if beacon is holding a throwable object or a creature
            // if true, then don't auto throw the flarebombs from storage
            // done before orig because things can get thrown during orig
            for (int i = 0; i < 2; i++)
            {
                if (self.grasps[i] != null && (self.IsObjectThrowable(self.grasps[i].grabbed) || self.grasps[i].grabbed is Creature))
                {
                    if (self.slugOnBack != null
                        && self.input[0].pckp
                        && self.grasps[i].grabbed is FlareBomb
                        && cwt.Beacon.storage.storedFlares.Count < cwt.Beacon.storage.capacity)
                    {
                        //if you're trying to put a flarebomb into storage, don't put the slug on back to your hand
                        self.slugOnBack.interactionLocked = true;
                        self.slugOnBack.counter = 0;
                    }
                    dontAutoThrowFlarebomb = true;
                    break;
                }
            }
        }

        orig(self, eu);

        if (slugIsBeacon)
        {
            bool interactionLockFlarebombStorage = self.eatMeat > 0;
            if (!interactionLockFlarebombStorage)
            {
                //check grasps for food if not eating meat
                for (int i = 0; i < 2; i++)
                {
                    if (self.grasps[i]?.grabbed is IPlayerEdible)
                    {
                        interactionLockFlarebombStorage = true;
                        break;
                    }
                }
            }
            if (interactionLockFlarebombStorage)
            {
                //dont take flarebomb from storage if holding food or eating
                cwt.Beacon.storage.interactionLocked = true;
                cwt.Beacon.storage.counter = 0;
            }

            if (cwt.Beacon.storage != null)
            {
                if (!self.craftingObject)
                {
                    //dont increment if crafting
                    cwt.Beacon.storage.increment = self.input[0].pckp;
                    cwt.Beacon.storage.Update(eu);
                }
                
                if (!dontAutoThrowFlarebomb && self.input[0].thrw && !self.input[1].thrw && cwt.Beacon.storage.storedFlares.Count > 0)
                {
                    //auto throw flarebomb on an empty hand
                    cwt.Beacon.storage.FlarebombFromStorageToPaw(eu);
                    self.ThrowObject(self.FreeHand(), eu);
                    self.wantToThrow = 0;
                }
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