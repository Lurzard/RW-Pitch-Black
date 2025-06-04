using AbstractObjectType = AbstractPhysicalObject.AbstractObjectType;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;
using RWCustom;
using Random = UnityEngine.Random;
using System;
using static PitchBlack.Plugin;

namespace PitchBlack;

public static class BeaconHooks
{
    // Show a food bar warning if we don't have enough food to make a flare -WW
    public static int foodWarning = 0;
    
    #region Not Hooks
    
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
    
    private static void BeaconUpdate(Player self)
    {
        ThanatosisUpdate(self);
        if (self.SlugCatClass == PBEnums.SlugcatStatsName.Beacon)
        {
            if (scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT beaconCWT)
            {
                if (beaconCWT.dontThrowTimer > 0)
                {
                    beaconCWT.dontThrowTimer--;
                }
                // Detect darkness for player blinking if room is too bright
                if (self.room != null)
                {
                    if (self.room.Darkness(self.mainBodyChunk.pos) < 0.15f || MiscUtils.RegionBlindsBeacon(self.room))
                    {
                        if (beaconCWT.brightSquint == 0)
                        {
                            beaconCWT.brightSquint = 40 * 6;
                            self.Blink(8);
                        }

                        // Tick down, but not all the way
                        if (beaconCWT.brightSquint > 1)
                            beaconCWT.brightSquint--;
                        else if (beaconCWT.brightSquint == 1)
                            self.Blink(5);
                    }
                    // Otherwise, tick down
                    else if (beaconCWT.brightSquint > 0)
                    {
                        beaconCWT.brightSquint--;
                    }
                }
            }
        }
    }
    
    #region Thanatosis stuff
    
    /// <summary>
    /// Camera effect for Thanatosis using Watcher's RippleDeath shader
    /// </summary>
    private static void ThanatosisDeathIntensity(Player self)
    {
        if (scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT beaconCWT)
        {
            if (beaconCWT.isDead)
            {
                // Calculation made by MaxDubstep <3
                float timeCounter = beaconCWT.thanatosisCounter; //x
                float minKarmaSafeTime = 12 * 40f; //tc
                float maxKarmaSafeTime = 40 * 40f; // Tc
                float beginningIntensity = 0.4f; //l
                float endIntensity = 0.45f; //m
                float windUpTime = 3 * 40f; //wc
                float rampUpTime = 3 * 40f; //Wc
                float plateauDuration = (qualiaLevel - 1) * (maxKarmaSafeTime - (windUpTime + rampUpTime) * 2) / 4 + minKarmaSafeTime - windUpTime - rampUpTime; //c
                // Starting plateau
                if (timeCounter < windUpTime)
                {
                    self.rippleDeathIntensity = Mathf.Sqrt(timeCounter) * beginningIntensity / Mathf.Sqrt(windUpTime);
                }
                // Middle of plateau
                if ((timeCounter < windUpTime + plateauDuration) && timeCounter >= windUpTime)
                {
                    self.rippleDeathIntensity = (timeCounter - windUpTime) * (endIntensity - beginningIntensity) / plateauDuration + beginningIntensity;
                }
                // Ending DIE INTENSITY!!!!
                if (timeCounter >= windUpTime + plateauDuration + (rampUpTime / 2))
                {
                    float increment = 0.008f;
                    int mult = 4;
                    self.rippleDeathIntensity += increment;
                    increment += 0.008f * mult;
                    mult += 4;
                }
            }
            if ((beaconCWT.diedInThanatosis || self.dead) && self.rippleDeathIntensity < 0.12f)
            {
                self.rippleDeathIntensity += 0.004f;
            }
            if (self.rippleDeathIntensity > 0 && !beaconCWT.isDead)
            {
                self.rippleDeathIntensity -= 0.002f;
            }
        }
    }

    /// <summary>
    /// Determines whether to toggle Thanatosis ON or OFF accordingly
    /// </summary>
    private static void ToggleThanatosis(Player self)
    {
        var GotCWTData = scugCWT.TryGetValue(self, out ScugCWT c);
        if (GotCWTData && c is BeaconCWT bCWT)
        {
            bCWT.inputForThanatosisCounter++;
            if (bCWT.inputForThanatosisCounter == 24)
            {
                bCWT.deathToggle = bCWT.isDead;
                bCWT.isDead = !bCWT.isDead;
                // Toggling
                if (bCWT.deathToggle != bCWT.isDead)
                {
                    logger.LogDebug($"[THANATOSIS] Toggle reached! Toggling Thanatosis: {bCWT.isDead}. Ripple Layer: {self.abstractCreature.rippleLayer}.");
                    self.abstractCreature.rippleLayer = bCWT.isDead ? 1 : 0;
                    self.room.PlaySound(
                        bCWT.isDead
                            ? PBEnums.SoundID.Player_Activated_Thanatosis
                            : PBEnums.SoundID.Player_Deactivated_Thanatosis, self.mainBodyChunk);
                }
            }

        }
    }
    
    private static void InThanatosis(Player self)
    {
        var GotCWTData = scugCWT.TryGetValue(self, out ScugCWT c);
        if (GotCWTData && c is BeaconCWT bCWT)
        {
            // Spawn a DreamSpawn
            if (!bCWT.spawnLeftBody)
            {
                MiscUtils.MaterializeDreamSpawn(self.room, self.mainBodyChunk.pos, PBEnums.VoidSpawn.SpawnSource.Death);
                bCWT.spawnLeftBody = true;
            }

            // Input removing is done in IL_Player_checkInput

            // Increase time
            if (bCWT.thanatosisCounter <= bCWT.inThanatosisLimit)
            {
                bCWT.thanatosisCounter++;
                if (bCWT.thanatosisLerp < 0.92f)
                {
                    bCWT.thanatosisLerp += 0.01f;
                }
                if (!bCWT.graspsNeedToBeReleased)
                {
                    self.LoseAllGrasps();
                    //DropAllFlares(self);
                    bCWT.graspsNeedToBeReleased = true;
                }
            }
        }
    }
    
    private static void OutsideThanatosis(Player self)
    {
        var GotCWTData = scugCWT.TryGetValue(self, out ScugCWT c);
        if (GotCWTData && c is BeaconCWT bCWT)
        {
            bCWT.graspsNeedToBeReleased = false;
            bCWT.spawnLeftBody = false;
            if (bCWT.thanatosisCounter > 0)
            {
                bCWT.thanatosisCounter--;
                if (bCWT.thanatosisLerp > 0f)
                {
                    bCWT.thanatosisLerp -= 0.01f;
                }
            }
            if (bCWT.thanatosisLerp < 0.01f)
            {
                bCWT.thanatosisCounter = 0;
                self.abstractCreature.rippleLayer = 0;
            }
        }
    }
    
    private static void ThanatosisUpdate(Player self)
    {
        ThanatosisDeathIntensity(self);
        if (scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT bCWT)
        {
            if (!self.input[0].spec)
            {
                bCWT.inputForThanatosisCounter = 0;
            }
            if (canIDoThanatosisYet && self.input[0].spec)
            {
                logger.LogDebug($"[THANATOSIS] Doing input for Thanatosis, input time: {bCWT.inputForThanatosisCounter}.");
                ToggleThanatosis(self);
            }
            if ((bCWT.thanatosisCounter >= bCWT.inThanatosisLimit || (bCWT.isDead && self.dead)) && !bCWT.diedInThanatosis)
            {
                logger.LogDebug($"[THANATOSIS] Time limit reached. Time: {bCWT.thanatosisCounter}/{bCWT.inThanatosisLimit}.");
                bCWT.diedInThanatosis = true;
            }
            else if (!bCWT.diedInThanatosis)
            {
                bCWT.thanatosisDeathBumpNeedsToPlay = false;
            }
            // DIE!!!!!!! rippleDeathTime handles calling self.Die btw
            if (bCWT.diedInThanatosis && !bCWT.thanatosisDeathBumpNeedsToPlay && self.rippleDeathTime == 80)
            {
                #region Playing sounds
                self.room.PlaySound(PBEnums.SoundID.Player_Died_From_Thanatosis);
                self.room.PlaySound(SoundID.Gate_Rails_Collide);
                self.room.PlaySound(SoundID.Gate_Rails_Collide);
                #endregion
                bCWT.thanatosisDeathBumpNeedsToPlay = true;
            }
            switch (bCWT.isDead)
            {
                case true:
                    InThanatosis(self);
                    break;
                case false:
                    OutsideThanatosis(self);
                    break;
            }
        }
    }
    
    #endregion
    
    #endregion
    
    public static void Apply()
    {
        IL.Player.GrabUpdate += Player_GrabUpdate_IL;
        On.Player.Die += Player_Die;
        On.Player.PermaDie += Player_PermaDie;
        On.Player.Jump += Player_Jump;
        On.Player.SwallowObject += BeaconTransmuteIntoFlashbang;
        On.Player.GrabUpdate += Player_GrabUpdate;
        On.Player.GraphicsModuleUpdated += BeaconStorageGrafUpdate;
        On.Player.Update += Player_Update;
        On.Player.ThrowObject += Player_ThrowObject;
        On.Creature.Abstractize += Creature_Abstractize;
        On.SlugcatHand.EngageInMovement += SlugcatHand_EngageInMovement;
        On.ShelterDoor.DoorClosed += ShelterDoor_DoorClosed;
        // QOL storage stoppers for other hold-grab functions
        On.Player.SwallowObject += Player_SwallowObject;
        On.Player.Regurgitate += Player_Regurgitate;
        On.Player.ObjectEaten += Player_ObjectEaten;
        On.HUD.FoodMeter.MeterCircle.Update += MeterCircle_Update;
        On.SlugcatStats.SlugcatToTimeline += SlugcatStats_SlugcatToTimeline;
        IL.Player.checkInput += IL_Player_checkInput;
    }

    private static void IL_Player_checkInput(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        try
        {
            // This matches to line 104 (IL_00C8) in IL view, or in the middle of line 26 in C# view, and puts the cursor after the call instruction.
            cursor.GotoNext(MoveType.After, i => i.MatchCall(typeof(RWInput), nameof(RWInput.PlayerInput)));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc_0);

            cursor.EmitDelegate((Player.InputPackage originalInputs, Player self, int num) =>
            {
                // This needs a proper check for if the player is in thanatosis
                if (Plugin.scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT beaconCWT &&
                beaconCWT.isDead &&
                Plugin.qualiaLevel <= 3f)
                {
                    // Create new inputs
                    Player.InputPackage newInputs = new Player.InputPackage(self.room.game.rainWorld.options.controls[num].gamePad, self.room.game.rainWorld.options.controls[num].GetActivePreset(), 0, 0, false, false, false, false, false, originalInputs.spec);
                    newInputs.downDiagonal = 0;
                    newInputs.analogueDir = Vector2.zero;

                    // Set animation and body mode
                    self.animation = Player.AnimationIndex.Dead;
                    self.bodyMode = Player.BodyModeIndex.Dead;

                    // Put new values on the stack
                    return newInputs;
                }
                // If the prior condition is not met, just return the original inputs to the stack.
                return originalInputs;
            });
            Plugin.logger.LogDebug($"PB {nameof(IL_Player_checkInput)} applied successfully");
        }
        catch (Exception err)
        {
            Plugin.logger.LogDebug($"PB {nameof(IL_Player_checkInput)} could not match IL.\n{err}");
        }
    }

    // NOTE: Moved this higher so it taks significantly less time to scroll down for -Lur
    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        BeaconUpdate(self);
        orig(self, eu);
    }

    private static SlugcatStats.Timeline SlugcatStats_SlugcatToTimeline(On.SlugcatStats.orig_SlugcatToTimeline orig, SlugcatStats.Name slugcat)
    {
        orig(slugcat);
        if (slugcat == PBEnums.SlugcatStatsName.Beacon)
        {
            return PBEnums.Timeline.Beacon;
        }
        return orig(slugcat);
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
            if (self.slugOnBack == null || !self.slugOnBack.HasASlug || !Plugin.scugCWT.TryGetValue(self, out var c) || c is not BeaconCWT cwt || cwt.storage.storedFlares.Count <= 0)
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
            //num = 0.65f + 0.35f * Mathf.Sin((float)self.meter.timeCounter / 20f * 3.1415927f * 2f);
            self.circles[0].fade = 1f;
            self.meter.visibleCounter = 80;
            foodWarning--;
        }
    }

    private static void ShelterDoor_DoorClosed(On.ShelterDoor.orig_DoorClosed orig, ShelterDoor self)
    {
        //IT'S NOT FOOLPROOF, BUT IT'S GOOD ENOUGH...
        if (ModManager.CoopAvailable)
        {
            // This might work, as long as players are still realized.
            foreach (AbstractCreature player in self.room.game.AlivePlayers)
            {
                if (player.realizedCreature is Player p && Plugin.scugCWT.TryGetValue(p, out ScugCWT c) && c is BeaconCWT beaconCWT)
                {
                    for (int i = 0; i < beaconCWT.coopRefund; i++)
                    {
                        AbstractConsumable item = new(self.room.world, AbstractObjectType.FlareBomb, null, self.room.LocalCoordinateOfNode(0), self.room.game.GetNewID(), -1, -1, null);
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

    public static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
    {
        Plugin.scugCWT.TryGetValue(self, out ScugCWT scugCWT);
        bool dontAutoThrowFlarebomb = (scugCWT is BeaconCWT cWT && cWT.dontThrowTimer > 0) || self.FreeHand() == -1 || self.input[0].pckp;

        if (scugCWT is BeaconCWT beaconCWT)
        {
            //spinch: check grasps to see if beacon is holding a throwable object or a creature
            // if true, then don't auto throw the flarebombs from storage
            // done before orig because things can get thrown during orig
            for (int i = 0; i < 2; i++)
            {
                if (self.grasps[i] != null && (self.IsObjectThrowable(self.grasps[i].grabbed) || self.grasps[i].grabbed is Creature))
                {
                    if (self.slugOnBack != null && self.input[0].pckp && self.grasps[i].grabbed is FlareBomb && beaconCWT.storage.storedFlares.Count < beaconCWT.storage.capacity)
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

        int preFreeHand = self.FreeHand(); //remember this for later
        orig(self, eu);

        if (scugCWT is BeaconCWT beaconCWT1)
        {
            //CHECK FOR AUTO-STORE FLASHBANGS IF OUR HANDS ARE FULL
            if (self.input[0].pckp && !self.input[1].pckp && self.pickUpCandidate != null && self.pickUpCandidate is FlareBomb flare && beaconCWT1.storage.storedFlares.Count < beaconCWT1.storage.capacity)
            {
                //IF WE'RE HOLDING TWO ITEMS OR ONE BIG TWO HANDED ITEM
                if (preFreeHand == -1)
                {
                    beaconCWT1.storage.FlarebombtoStorage(flare);
                    self.wantToPickUp = 0;
                    return;
                }
            }

            bool interactLockStorage = self.eatMeat > 0;

            //spinch: if bacon is full and rotund isnt enabled, put flarebomb into storage
            //dont even check for grasps if bacon's holding a food item
            if (!Plugin.RotundWorldEnabled && self.FoodInStomach >= self.MaxFoodInStomach)
            {
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

            //DON'T UNSTORE A FLASHBANG RIGHT AFTER WE'VE CRAFTED ONE
            if (interactLockStorage || beaconCWT1.heldCraft)
            {
                //dont take flarebomb from storage if holding food or eating
                beaconCWT1.storage.interactionLocked = true;
                beaconCWT1.storage.counter = 0;
            }

            if (beaconCWT1.heldCraft && !self.input[0].pckp)
                beaconCWT1.heldCraft = false; //ONCE WE LET GO OF GRAB, WE CAN MOVE STORAGE AGAIN

            if (beaconCWT1.storage != null)
            {
                //ALSO, PAST A CERTAIN POINT STOP INCRIMENTING BECAUSE WE ARE CLEARLY TRYING TO REGURGITATE SOMETHING
                if (!self.craftingObject && self.swallowAndRegurgitateCounter < 45)
                {
                    //dont increment if crafting
                    beaconCWT1.storage.increment = self.input[0].pckp && !beaconCWT1.heldCraft;
                    beaconCWT1.storage.Update(eu);
                }

                if (!dontAutoThrowFlarebomb && self.input[0].thrw && !self.input[1].thrw && beaconCWT1.storage.storedFlares.Count > 0)
                {
                    //auto throw flarebomb on an empty hand
                    int handWithFlarebomb = beaconCWT1.storage.FlarebombFromStorageToPaw(eu);
                    self.ThrowObject(handWithFlarebomb, eu);
                    self.wantToThrow = 0;
                }
            }
        }
    }

    private static void Player_Die(On.Player.orig_Die orig, Player self)
    {
        if (ModManager.CoopAvailable && Plugin.scugCWT.TryGetValue(self, out var c) && c is BeaconCWT cwt && cwt.storage != null)
        {
            cwt.coopRefund = Mathf.Max(cwt.coopRefund, cwt.storage.storedFlares.Count);
        }
        //spinch: on death, all of beacon's stored flarebombs gets popped off
        DropAllFlares(self);
        //WW: but do that BEFORE orig in case our death unrealizes us
        orig(self);
    }
    private static void Player_PermaDie(On.Player.orig_PermaDie orig, Player self)
    {
        if (ModManager.CoopAvailable && Plugin.scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT cwt && cwt.storage != null)
        {
            cwt.coopRefund = Mathf.Max(cwt.coopRefund, cwt.storage.storedFlares.Count);
        }
        orig(self);
    }
    private static void Creature_Abstractize(On.Creature.orig_Abstractize orig, Creature self)
    {
        if (self is Player player && player.slugcatStats?.name == PBEnums.SlugcatStatsName.Beacon)
        {
            DropAllFlares(player);
        }
        orig(self);
    }

    private static void Player_Jump(On.Player.orig_Jump orig, Player self)
    {
        orig(self);

        if (PBEnums.SlugcatStatsName.Beacon == self.slugcatStats.name)
        {
            if (Player.AnimationIndex.Flip == self.animation)
                self.jumpBoost *= 1f + 0.55f;
            else
                self.jumpBoost *= 1f + 0.1f;
        }
    }

    public static void BeaconTransmuteIntoFlashbang(On.Player.orig_SwallowObject orig, Player self, int grasp)
    {
        orig(self, grasp);
        if (self.slugcatStats.name == PBEnums.SlugcatStatsName.Beacon && self.playerState.foodInStomach > 0 && self.objectInStomach.type == AbstractObjectType.Rock)
        {
            self.objectInStomach = new AbstractConsumable(self.room.world, AbstractObjectType.FlareBomb, null, self.abstractCreature.pos, self.room.game.GetNewID(), -1, -1, null);
            self.SubtractFood(1);

            if (Plugin.scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT cwt)
            {
                //DON'T UNSTORE ANOTHER FLASHBANG UNTIL WE'VE LET GO OF THE BUTTON
                cwt.heldCraft = true;
            }
        }
    }

    private static bool SlugcatHand_EngageInMovement(On.SlugcatHand.orig_EngageInMovement orig, SlugcatHand self)
    {
        Player myPlayer = self.owner.owner as Player;
        if (Plugin.scugCWT.TryGetValue(myPlayer, out ScugCWT scugCWT) && scugCWT is BeaconCWT beaconCWT && beaconCWT.brightSquint > 1)
        {
            PlayerGraphics myGraphics = myPlayer.graphicsModule as PlayerGraphics;

            //OKAY WE HAVE NO ACCESS TO EYE POSITION SO WE GOTTA DO THIS...
            //NEVERMIND IT'D BE WAY LESS WORK TO JUST TRANSFER THE EYE POS
            //Vector2 targPos = cwt.Beacon.eyePos + myGraphics.lookDirection;
            Vector2 shieldDir = myGraphics.lookDirection;
            if (Mathf.Abs(shieldDir.x) <= 0.3 || myPlayer.input[0].x != 0)
                shieldDir.x = myPlayer.flipDirection;
            //if (shieldDir.y <= 0)
            //    shieldDir.y = 0.35f;
            shieldDir.y = Mathf.Clamp(shieldDir.y, 0.35f, 0.75f) - 0.2f;

            int tuchingHand = shieldDir.x <= 0 ? 0 : 1;
            if (self.limbNumber == tuchingHand)
            {
                self.mode = Limb.Mode.HuntAbsolutePosition;
                self.huntSpeed = 15f;
                Vector2 targPos = (myPlayer.graphicsModule as PlayerGraphics).head.pos + (shieldDir * 15) + (myPlayer.graphicsModule as PlayerGraphics).head.vel;
                self.absoluteHuntPos = targPos - Custom.DirVec(myPlayer.bodyChunks[0].pos, targPos) * 3f;
                return false;
            }

        }


        return orig(self);
    }
    private static void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
    {
        if (self.grasps[grasp] != null && (self.grasps[grasp].grabbed is Weapon) && Plugin.scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT cwt)
        {
            cwt.dontThrowTimer = 15; //BRIEF PERIOD OF DON'T THROW A FLASHBANG
            //MAYBE A FANCY COLOR?...
            if (self.grasps[grasp].grabbed is FlareBomb flare)
            {
                flare.color = new Color(0.4f, 0f, 1f); //WE COULD GIVE IT A FUNKY COLOR, IF WE WANT... //yurpnuke -Lur
                cwt.dontThrowTimer = 60; //DON'T THROW ANOTHER ONE FOR A WHILE //This perfectly tracks the interval of a Flashbang exploding -Lur
            }
        }
        orig(self, grasp, eu);
    }
    public static void BeaconStorageGrafUpdate(On.Player.orig_GraphicsModuleUpdated orig, Player self, bool actuallyViewed, bool eu)
    {
        if (Plugin.scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT cwt)
        {
            cwt.storage?.GraphicsModuleUpdated(eu);
        }
        orig(self, actuallyViewed, eu);
    }
    private static void Player_ObjectEaten(On.Player.orig_ObjectEaten orig, Player self, IPlayerEdible edible)
    {
        orig(self, edible);
        if (Plugin.scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT cwt && self.input[0].pckp)
        {
            cwt.heldCraft = true;
        }
    }
    private static void Player_Regurgitate(On.Player.orig_Regurgitate orig, Player self)
    {
        orig(self);
        if (Plugin.scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT cwt && self.input[0].pckp)
        {
            cwt.heldCraft = true;
        }
    }
    private static void Player_SwallowObject(On.Player.orig_SwallowObject orig, Player self, int grasp)
    {
        orig(self, grasp);
        if (Plugin.scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT cwt && self.input[0].pckp)
        {
            cwt.heldCraft = true;
        }
    }
}