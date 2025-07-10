using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using UnityEngine;
using static PitchBlack.Plugin;

namespace PitchBlack;

public static class FlareStorage
{
    /// <summary>
    /// Function that drops all stored flares from storage
    /// </summary>
    private static void DropAllFlares(Player self)
    {
        if (Plugin.scugCWT.TryGetValue(self, out ScugCWT scugCWT)
            && scugCWT is BeaconCWT beaconCWT
            && beaconCWT.storage != null)
        {
            while (beaconCWT.storage.storedFlares.Count > 0)
            {
                FlareBomb fb = beaconCWT.storage.storedFlares.Pop();
                BeaconCWT.AbstractStoredFlare af = beaconCWT.storage.abstractFlare.Pop();
                if (fb != null)
                {
                    fb.firstChunk.vel = self.mainBodyChunk.vel + Custom.RNV() * 3f * Random.value;
                    fb.ChangeMode(Weapon.Mode.Free);
                }
                af?.Deactivate();
            }
        }
    }
    
    // For making the food meter flash red
    public static int foodWarning = 0;
    
    public static void Apply()
    {
        IL.Player.GrabUpdate += Player_GrabUpdate_IL;
        On.HUD.FoodMeter.MeterCircle.Update += MeterCircle_Update;
        On.Player.GrabUpdate += Player_GrabUpdate;
        On.Player.Die += Player_Die;
        On.ShelterDoor.DoorClosed += ShelterDoor_DoorClosed;
        On.Creature.Abstractize += Creature_Abstractize;
        On.Player.SwallowObject += BeaconTransmuteIntoFlare;
        On.Player.ThrowObject += Player_ThrowObject;
        On.Player.GraphicsModuleUpdated += BeaconStorageGrafUpdate;
        On.Player.ObjectEaten += PlayerOnObjectEaten;
        On.Player.Regurgitate += Player_Regurgitate;
        On.Player.SwallowObject += Player_SwallowObject;
        On.Player.Grabability += Player_Grabability;
    }

    /// <summary>
    /// Makes it so Beacon does/doesn't touch flare storage.
    /// </summary>
    /// <returns>Determines whether you can or can't grab an object</returns>
    private static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        Player.ObjectGrabability result = orig(self, obj);
        
        if (obj is FlareBomb flare && obj.room != null) 
        {
            foreach (AbstractCreature abstrCrit in flare.room.game.Players) 
            {
                if (abstrCrit.realizedCreature == null) 
                {
                    continue;
                }
                
                if (scugCWT.TryGetValue(abstrCrit.realizedCreature as Player, out ScugCWT cwt) && cwt is BeaconCWT beaconCWT) 
                {
                    if (beaconCWT.storage.storedFlares.Contains(flare))
                    {
                        return Player.ObjectGrabability.CantGrab;   
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Hooks for making heldCraft true and stopping recursive storage input stuff
    /// </summary>
    #region Hooks
    
    private static void PlayerOnObjectEaten(On.Player.orig_ObjectEaten orig, Player self, IPlayerEdible edible)
    {
        orig(self, edible);
        
        if (scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT cwt && self.input[0].pckp)
        {
            cwt.heldCraft = true;
        }
    }
    
    private static void Player_Regurgitate(On.Player.orig_Regurgitate orig, Player self)
    {
        orig(self);
        
        if (scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT cwt && self.input[0].pckp)
        {
            cwt.heldCraft = true;
        }
    }
    
    private static void Player_SwallowObject(On.Player.orig_SwallowObject orig, Player self, int grasp)
    {
        orig(self, grasp);
        if (scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT cwt && self.input[0].pckp)
        {
            cwt.heldCraft = true;
        }
    }
    
    #endregion

    /// <summary>
    /// Update visual of storage (I think)
    /// </summary>
    private static void BeaconStorageGrafUpdate(On.Player.orig_GraphicsModuleUpdated orig, Player self, bool actuallyViewed, bool eu)
    {
        if (scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT cwt)
        {
            cwt.storage?.GraphicsModuleUpdated(eu);
        }
        
        orig(self, actuallyViewed, eu);
    }

    /// <summary>
    /// Adds a timer inbetween flare throws from storage + a fancy color
    /// </summary>
    private static void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
    {
        if (self.grasps[grasp] != null && (self.grasps[grasp].grabbed is Weapon) && scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT cwt)
        {
            // brief period of don't throw a flashbang
            cwt.dontThrowTimer = 15;
            if (self.grasps[grasp].grabbed is FlareBomb flare)
            {
                // Purple-ish recoloration
                flare.color = new Color(0.4f, 0f, 1f);
                // This perfectly tracks the interval of a Flashbang exploding -Lur
                cwt.dontThrowTimer = 60;
            }
        }
        
        orig(self, grasp, eu);
    }

    /// <summary>
    /// Transmutes rocks into flares
    /// </summary>
    private static void BeaconTransmuteIntoFlare(On.Player.orig_SwallowObject orig, Player self, int grasp)
    {
        orig(self, grasp);
        
        if (self.slugcatStats.name == Enums.SlugcatStatsName.Beacon && self.playerState.foodInStomach > 0 && self.objectInStomach.type == AbstractPhysicalObject.AbstractObjectType.Rock)
        {
            self.objectInStomach = new AbstractConsumable(self.room.world, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null, self.abstractCreature.pos, self.room.game.GetNewID(), -1, -1, null);
            self.SubtractFood(1);

            if (scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT cwt)
            {
                // don't unstore another flare until we've let go of the button.
                cwt.heldCraft = true;
            }
        }
    }

    /// <summary>
    /// Beacon drops flares from storage when abstractized
    /// </summary>
    private static void Creature_Abstractize(On.Creature.orig_Abstractize orig, Creature self)
    {
        if (self is Player player && player.slugcatStats?.name == Enums.SlugcatStatsName.Beacon)
        {
            DropAllFlares(player);
        }
        orig(self);
    }

    /// <summary>
    /// Co-op flare refunding in shelters
    /// [WW]
    /// </summary>
    private static void ShelterDoor_DoorClosed(On.ShelterDoor.orig_DoorClosed orig, ShelterDoor self)
    {
        // not ass foolproof as I would've preferred, but it is enough...
        if (ModManager.CoopAvailable)
        {
            // This might work, as long as players are still realized.
            foreach (AbstractCreature player in self.room.game.AlivePlayers)
            {
                if (player.realizedCreature is Player p && scugCWT.TryGetValue(p, out ScugCWT c) && c is BeaconCWT beaconCWT)
                {
                    for (int i = 0; i < beaconCWT.coopRefund; i++)
                    {
                        AbstractConsumable item = new(self.room.world, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null, self.room.LocalCoordinateOfNode(0), self.room.game.GetNewID(), -1, -1, null);
                        self.room.abstractRoom.AddEntity(item);
                        item.RealizeInRoom();
                        self.room.AddObject(item.realizedObject);
                    }
                    beaconCWT.coopRefund = 0;
                }
            }
        }
        
        orig(self);
    }

    /// <summary>
    /// Player drops all stored flares on death
    /// </summary>
    private static void Player_Die(On.Player.orig_Die orig, Player self)
    {
        if (ModManager.CoopAvailable && scugCWT.TryGetValue(self, out var c) && c is BeaconCWT cwt && cwt.storage != null)
        {
            // refund amount of flares that were in storage
            cwt.coopRefund = Mathf.Max(cwt.coopRefund, cwt.storage.storedFlares.Count);
        }
        
        // on death, all of beacon's stored flarebombs gets popped off -spinch
        DropAllFlares(self);
        
        // but do that BEFORE orig in case our death unrealizes us -WW
        
        orig(self);
    }

    /// <summary>
    /// Safety code for interactions between flare storage, eating, and other players on back
    /// [spinch + WW]
    /// </summary>
    private static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
    {
        scugCWT.TryGetValue(self, out ScugCWT c);
        bool dontAutoThrowFlarebomb = (c is BeaconCWT cwt && cwt.dontThrowTimer > 0) || self.FreeHand() == -1 || self.input[0].pckp;

        if (c is BeaconCWT bCWT1)
        {
            /* check grasps to see if beacon is holding a throwable object or a creature
            if true, then don't auto throw the flarebombs from storage
            done before orig because things can get thrown during orig -spinch */
            for (int i = 0; i < 2; i++)
            {
                if (self.grasps[i] != null && (self.IsObjectThrowable(self.grasps[i].grabbed) || self.grasps[i].grabbed is Creature))
                {
                    if (self.slugOnBack != null && self.input[0].pckp && self.grasps[i].grabbed is FlareBomb && bCWT1.storage.storedFlares.Count < bCWT1.storage.capacity)
                    {
                        // if you're trying to put a flarebomb into storage, don't put the slug on back to your hand
                        self.slugOnBack.interactionLocked = true;
                        self.slugOnBack.counter = 0;
                    }
                    dontAutoThrowFlarebomb = true;
                    break;
                }
            }
        }

        // remember this for later
        int preFreeHand = self.FreeHand();
        orig(self, eu);

        if (c is BeaconCWT bCWT)
        {
            // check for auto-store flashbangs if our hands are full
            if (self.input[0].pckp && !self.input[1].pckp && self.pickUpCandidate != null && self.pickUpCandidate is FlareBomb flare && bCWT.storage.storedFlares.Count < bCWT.storage.capacity)
            {
                // if we're holding two items or one big two-handed item
                if (preFreeHand == -1)
                {
                    bCWT.storage.FlarebombtoStorage(flare);
                    self.wantToPickUp = 0;
                    return;
                }
            }

            bool interactLockStorage = self.eatMeat > 0;

            /* if bacon is full and rotund isnt enabled, put flarebomb into storage
            dont even check for grasps if bacon's holding a food item -spinch */
            if (!Plugin.RotundWorldEnabled && self.FoodInStomach >= self.MaxFoodInStomach)
            {
                goto JustGoOverHere;
            }

            if (!interactLockStorage)
            {
                // check grasps for food if not eating meat
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

            // don't unstore a flare right after we've created one
            if (interactLockStorage || bCWT.heldCraft)
            {
                // dont take flarebomb from storage if holding food or eating
                bCWT.storage.interactionLocked = true;
                bCWT.storage.counter = 0;
            }

            if (bCWT.heldCraft && !self.input[0].pckp)
                // once we let go of grab we can move storage again
                bCWT.heldCraft = false;

            if (bCWT.storage != null)
            {
                // also, past a certain point stop incrementing because we are clearly trying to regurgitate something
                if (!self.craftingObject && self.swallowAndRegurgitateCounter < 45)
                {
                    // dont increment if crafting
                    bCWT.storage.increment = self.input[0].pckp && !bCWT.heldCraft;
                    bCWT.storage.Update(eu);
                }

                if (!dontAutoThrowFlarebomb && self.input[0].thrw && !self.input[1].thrw && bCWT.storage.storedFlares.Count > 0)
                {
                    // auto throw flarebomb on an empty hand
                    int handWithFlarebomb = bCWT.storage.FlarebombFromStorageToPaw(eu);
                    self.ThrowObject(handWithFlarebomb, eu);
                    self.wantToThrow = 0;
                }
            }
        }
    }

    /// <summary>
    /// Show a food bar warning if we don't have enough food to make a flare.
    /// [WW]
    /// </summary>
    private static void MeterCircle_Update(On.HUD.FoodMeter.MeterCircle.orig_Update orig, HUD.FoodMeter.MeterCircle self)
    {
        orig(self);
        
        if (foodWarning > 0 && !self.meter.IsPupFoodMeter && self.number < self.meter.ShowSurvivalLimit && (self.meter.hud.owner.GetOwnerType() == HUD.HUD.OwnerType.Player))
        {
            if (self.meter.timeCounter % 20 > 10)
            {
                self.rads[0, 0] *= 0.96f;
                self.circles[0].color = 1;
            }
            self.circles[0].fade = 1f;
            self.meter.visibleCounter = 80;
            foodWarning--;
        }
    }

    /// <summary>
    /// Give priority to auto-throwing flarebombs from storage over throwing slug on back.
    /// [spinch]
    /// </summary>
    private static void Player_GrabUpdate_IL(ILContext il)
    {
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
            if (self.slugOnBack == null || !self.slugOnBack.HasASlug || !scugCWT.TryGetValue(self, out var c) || c is not BeaconCWT cwt || cwt.storage.storedFlares.Count <= 0)
            {
                return false;
            }

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
}