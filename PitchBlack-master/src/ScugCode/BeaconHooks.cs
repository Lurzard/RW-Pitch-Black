using AbstractObjectType = AbstractPhysicalObject.AbstractObjectType;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;
using MoreSlugcats;

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
        On.PlayerGraphics.Update += PlayerGraphics_Update;
    }

    private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        orig(self);
        bool slugIsBeacon = Plugin.scugCWT.TryGetValue(self.player, out ScugCWT cwt) && cwt.IsBeacon;

        if (slugIsBeacon && self.player.room.Darkness(self.player.mainBodyChunk.pos) > 0f) //ASSUMING THIS WILL BE TRUE MOST OF THE TIME IN BEACON'S WORLD STATE
        {
            if (self.lightSource == null) //GIVE THIS BACK SINCE ORIG PROBABLY TOOK IT
            {
                self.lightSource = new LightSource(self.player.mainBodyChunk.pos, false, Color.Lerp(new Color(1f, 1f, 1f), (ModManager.MSC && self.player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup) ? self.player.ShortCutColor() : PlayerGraphics.SlugcatColor(self.CharacterForColor), 0.5f), self.player);
                self.lightSource.requireUpKeep = true;
                self.lightSource.setRad = new float?(300f);
                self.lightSource.setAlpha = new float?(1f);
                self.player.room.AddObject(self.lightSource);
            }

            float glowStr; //DEFAULT, NO FLASHBANGS
            int flares = cwt.Beacon.storage.storedFlares.Count;
            //CURVED LIGHT SCALE?
            switch (flares)
            {
                case 0:
                    glowStr = 200;
                    break;
                case 1:
                    glowStr = 300; //+100
                    break;
                case 2:
                    glowStr = 400; //+100
                    break;
                case 3:
                    glowStr = 475; //+75
                    break;
                case 4:
                    glowStr = 550; //+75
                    break;
                default:
                    glowStr = 550 + (25 * (flares - 4)); //+25
                    break;
            }

            //ROTUND WORLD SHENANIGANS
            float baseWeight = (0.7f * self.player.slugcatStats.bodyWeightFac) / 2f;
            glowStr *= (self.player.bodyChunks[0].mass / baseWeight) / 2f;
            //AT +2 BONUS PIPS THIS IS ROUGHLY 125% RAD. CAPPING AT 150%

            //IF WE HAVE THE_GLOW, DON'T LET OUR GLOW STRENGTH UNDERCUT THAT
            if (self.player.glowing && glowStr < 300)
                glowStr = 300;

            self.lightSource.setRad = glowStr;
            self.lightSource.stayAlive = true;
            self.lightSource.setPos = new Vector2?(self.player.mainBodyChunk.pos);

        }
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
        
        //spinch: on death, all of beacon's stored flarebombs gets popped off
        if (Plugin.scugCWT.TryGetValue(self, out var cwt) && cwt.Beacon?.storage != null)
        {
            while (cwt.Beacon.storage.storedFlares.Count > 0)
            {
                FlareBomb fb = cwt.Beacon.storage.storedFlares.Pop();
                BeaconCWT.AbstractStoredFlare af = cwt.Beacon.storage.abstractFlare.Pop();

                if (fb != null)
                {
                    fb.firstChunk.vel = self.mainBodyChunk.vel + RWCustom.Custom.RNV() * 3f * UnityEngine.Random.value;
                    fb.ChangeMode(Weapon.Mode.Free);
                }

                af?.Deactivate();
            }
        }
        //WW: but do that BEFORE orig in case our death unrealizes us
        orig(self);
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
            bool interactLockStorage = self.eatMeat > 0;

            if (!Plugin.RotundWorldEnabled && self.FoodInStomach >= self.MaxFoodInStomach)
            {
                //spinch: if bacon is full and rotund isnt enabled, put flarebomb into storage
                //dont even check for grasps if bacon's holding a food item
                goto JustGoOverHere;
            }

            if (!interactLockStorage)
            {
                //check grasps for food if not eating meat
                for (int i = 0; i < 2; i++)
                {
                    if (self.grasps[i]?.grabbed is IPlayerEdible)
                    {
                        interactLockStorage = true;
                        break;
                    }
                }
            }

            JustGoOverHere:

            if (interactLockStorage)
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
                    int handWithFlarebomb = cwt.Beacon.storage.FlarebombFromStorageToPaw(eu);
                    self.ThrowObject(handWithFlarebomb, eu);
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