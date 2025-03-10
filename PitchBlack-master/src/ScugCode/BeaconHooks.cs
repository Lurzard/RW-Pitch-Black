using AbstractObjectType = AbstractPhysicalObject.AbstractObjectType;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;
using MoreSlugcats;
using System;
using RWCustom;
using UnityEngine.PlayerLoop;

namespace PitchBlack;

public static class BeaconHooks
{
    public static void Apply()
    {
        //spinch: i also want to hook to Player.ThrowToGetFree to make beacon throw flashbangs if grasps are empty

        IL.Player.GrabUpdate += Player_GrabUpdate_IL;
        On.Player.Die += Player_Die;
        On.Player.PermaDie += Player_PermaDie;
        On.Player.Jump += Player_Jump;
        On.Player.SwallowObject += BeaconTransmuteIntoFlashbang;
        On.Player.GrabUpdate += Player_GrabUpdate;
        On.Player.GraphicsModuleUpdated += BeaconStorageGrafUpdate;
        On.Player.Update += BeaconPlayerUpdate;
        On.Player.ThrowObject += Player_ThrowObject;
        On.Creature.Abstractize += Creature_Abstractize;
        On.PlayerGraphics.Update += PlayerGraphics_Update;
        On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
        On.SlugcatHand.EngageInMovement += SlugcatHand_EngageInMovement;
        On.ShelterDoor.DoorClosed += ShelterDoor_DoorClosed;
        //qol storage stoppers for other hold-grab functions
        On.Player.SwallowObject += Player_SwallowObject;
        On.Player.Regurgitate += Player_Regurgitate;
        On.Player.ObjectEaten += Player_ObjectEaten;
        On.HUD.FoodMeter.MeterCircle.Update += MeterCircle_Update;
        On.PlayerGraphics.ApplyPalette += PlayerGraphics_ApplyPalette;
        On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites1;
    }

    //add math to fade beacon color in real time instead of immediately 
    private static void PlayerGraphics_DrawSprites1(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!self.player.IsBeacon(out var beacon)) return;

        bool slugIsBeacon = Plugin.scugCWT.TryGetValue(self.player, out ScugCWT cwt) && cwt.IsBeacon;
        int flares = cwt.Beacon.storage.storedFlares.Count;

        Color flareColor1 = new Color(0.10588235294f, 0.06666666666f, 0.25882352941f); //#1b1142
        Color flareColor2 = new Color(0.16470588235f, 0.0862745098f, 0.47843137254f); //#2a167a
        Color flareColor3 = new Color(0.18039215686f, 0.05490196078f, 0.67843137254f); //#2e0ead
        Color flareColor4 = new Color(0.2f, 0f, 1f);

        for (int i = 0; i < 2; i++)
        {
            if (flares == 0)
            {
                beacon.BeaconColor = Color.Lerp(beacon.BeaconDefaultColor, flareColor1, 0.45f);
            }
            if (flares == 1)
            {
                beacon.BeaconColor = Color.Lerp(beacon.BeaconDefaultColor, flareColor2, 0.55f);
            }
            if (flares == 2)
            {
                beacon.BeaconColor = Color.Lerp(beacon.BeaconDefaultColor, flareColor3, 0.60f);
            }
            if (flares == 3)
            {
                beacon.BeaconColor = Color.Lerp(beacon.BeaconDefaultColor, flareColor3, 0.80f);
            }
            if (flares == 4)
            {
                beacon.BeaconColor = Color.Lerp(beacon.BeaconDefaultColor, flareColor4, 0.92f);
            }
        }

        Color color = self.player.ShortCutColor();
        color = beacon.BeaconColor;

        for (int sprites = 0; sprites < sLeaser.sprites.Length;  sprites++)
        {
            if(sprites != 9) sLeaser.sprites[sprites].color = beacon.BeaconColor;
            if(sprites == 9) sLeaser.sprites[sprites].color = beacon.BeaconEyeColor;
            if(sprites == 10) sLeaser.sprites[sprites].color = Custom.hexToColor("f02961");
        }
    }

    private static void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);

        if (!self.player.IsBeacon(out var beacon)) return;

        beacon.BeaconDefaultColor = Color.Lerp(palette.blackColor, Custom.HSL2RGB(0.63055557f, 0.54f, 0.5f), Mathf.Lerp(0.08f, 0.04f, palette.darkness));
        beacon.BeaconEyeColor = new Color(1f, 1f, 1f);
    }

    public static int coopRefund = 0; //flashbangs to recover after respawning in jollycoop
    public static int foodWarning = 0;
    //SHOW A FOOD BAR WARNING IF WE DON'T HAVE ENOUGH FOOD TO MAKE A FLASHBANG
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

    private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
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


        if (slugIsBeacon)
        {
            if (cwt.Beacon.brightSquint > 10)
            {
                if (self.blink <= 0)
                {
                    if (UnityEngine.Random.value < 0.35f)
                        self.player.Blink(Mathf.FloorToInt(Mathf.Lerp(3f, 8f, UnityEngine.Random.value)));
                    //else
                    //    sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName("Face" + "Stunned");
                }
                self.head.vel -= self.lookDirection * 3f;
            }
        }

        orig(self);
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

    public static void DropAllFlares(Player self)
    {
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
    }

    private static void Player_Die(On.Player.orig_Die orig, Player self)
    {
        if (ModManager.CoopAvailable && Plugin.scugCWT.TryGetValue(self, out var cwt) && cwt.Beacon?.storage != null)
        {
            coopRefund = Mathf.Max(coopRefund, cwt.Beacon.storage.storedFlares.Count);
        }

        //spinch: on death, all of beacon's stored flarebombs gets popped off
        DropAllFlares(self);
        //WW: but do that BEFORE orig in case our death unrealizes us
        orig(self);
    }

    private static void Player_PermaDie(On.Player.orig_PermaDie orig, Player self)
    {
        if (ModManager.CoopAvailable && Plugin.scugCWT.TryGetValue(self, out var cwt) && cwt.Beacon?.storage != null)
        {
            coopRefund = Mathf.Max(coopRefund, cwt.Beacon.storage.storedFlares.Count);
        }
        orig(self);
    }

    private static void Creature_Abstractize(On.Creature.orig_Abstractize orig, Creature self)
    {
        if (self is Player player && player.slugcatStats?.name?.value == "Beacon")
            DropAllFlares(player);

        orig(self);
    }

    private static void ShelterDoor_DoorClosed(On.ShelterDoor.orig_DoorClosed orig, ShelterDoor self)
    {
        //IT'S NOT FOOLPROOF, BUT IT'S GOOD ENOUGH...
        if (ModManager.CoopAvailable)
        {
            //Debug.Log("WARP AND REVIVE! HOW MANY BANGS " + coopRefund);
            for (int i = 0; i < coopRefund; i++)
            {
                AbstractConsumable item = new(self.room.world, AbstractObjectType.FlareBomb, null, self.room.LocalCoordinateOfNode(0), self.room.game.GetNewID(), -1, -1, null);
                self.room.abstractRoom.AddEntity(item);
                item.RealizeInRoom();
                self.room.AddObject(item.realizedObject as PhysicalObject);
                //Debug.Log("SPAWNED BANG ");
            }
            coopRefund = 0;
        }

        orig(self);
    }

    private static void Player_Jump(On.Player.orig_Jump orig, Player self)
    {
        orig(self);

        if (Plugin.BeaconName == self.slugcatStats.name)
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
        if (self.slugcatStats.name == Plugin.BeaconName && self.playerState.foodInStomach > 0 && self.objectInStomach.type == AbstractObjectType.Rock)
        {
            self.objectInStomach = new AbstractConsumable(self.room.world, AbstractObjectType.FlareBomb, null, self.abstractCreature.pos, self.room.game.GetNewID(), -1, -1, null);
            self.SubtractFood(1);
            if (Plugin.scugCWT.TryGetValue(self, out ScugCWT cwt))
                cwt.Beacon.heldCraft = true; //DON'T UNSTORE ANOTHER FLASHBANG UNTIL WE'VE LET TO OF THE BUTTON
        }
    }
    public static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
    {
        bool slugIsBeacon = Plugin.scugCWT.TryGetValue(self, out ScugCWT cwt) && cwt.IsBeacon;
        bool dontAutoThrowFlarebomb = self.FreeHand() == -1 || self.input[0].pckp || (slugIsBeacon && cwt.Beacon.dontThrowTimer > 0);

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

        int preFreeHand = self.FreeHand(); //remember this for later
        orig(self, eu);

        if (slugIsBeacon)
        {
            //CHECK FOR AUTO-STORE FLASHBANGS IF OUR HANDS ARE FULL
            if (self.input[0].pckp && !self.input[1].pckp && self.pickUpCandidate != null && self.pickUpCandidate is FlareBomb flare && cwt.Beacon.storage.storedFlares.Count < cwt.Beacon.storage.capacity)
            {
                if (preFreeHand == -1) //IF WE'RE HOLDING TWO ITEMS OR ONE BIG TWO HANDED ITEM
                {
                    cwt.Beacon.storage.FlarebombtoStorage(self.pickUpCandidate as FlareBomb);
                    self.wantToPickUp = 0;
                    return;
                }
            }

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

            if (interactLockStorage || cwt.Beacon.heldCraft) //DON'T UNSTORE A FLASHBANG RIGHT AFTER WE'VE CRAFTED ONE
            {
                //dont take flarebomb from storage if holding food or eating
                cwt.Beacon.storage.interactionLocked = true;
                cwt.Beacon.storage.counter = 0;
            }

            if (cwt.Beacon.heldCraft && !self.input[0].pckp)
                cwt.Beacon.heldCraft = false; //ONCE WE LET GO OF GRAB, WE CAN MOVE STORAGE AGAIN

            if (cwt.Beacon.storage != null)
            {
                if (!self.craftingObject && self.swallowAndRegurgitateCounter < 45) //ALSO, PAST A CERTAIN POINT STOP INCRIMENTING BECAUSE WE ARE CLEARLY TRYING TO REGURGITATE SOMETHING
                {
                    //dont increment if crafting
                    cwt.Beacon.storage.increment = self.input[0].pckp && !cwt.Beacon.heldCraft;
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

    private static void BeaconPlayerUpdate(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        if (Plugin.scugCWT.TryGetValue(self, out ScugCWT cwt) && cwt.IsBeacon)
        {
            if (cwt.Beacon.dontThrowTimer > 0)
                cwt.Beacon.dontThrowTimer--;

            //DETECT DARKNESS FOR BLINKING
            if (self.room != null)
            {
                //Debug.Log("ROOM DARKNESS " + self.room.Darkness(self.mainBodyChunk.pos));
                if (self.room.Darkness(self.mainBodyChunk.pos) < 0.15f)
                {
                    //
                    if (cwt.Beacon.brightSquint == 0)
                    {
                        cwt.Beacon.brightSquint = 40 * 6;
                        self.Blink(8);
                    }

                    //TICK DOWN, BUT NOT ALL THE WAY
                    if (cwt.Beacon.brightSquint > 1)
                        cwt.Beacon.brightSquint--;
                    else if (cwt.Beacon.brightSquint == 1)
                        self.Blink(5);
                }
                else
                {
                    //TICK DOWN
                    if (cwt.Beacon.brightSquint > 0)
                        cwt.Beacon.brightSquint--;
                }
            }
        }
    }

    private static bool SlugcatHand_EngageInMovement(On.SlugcatHand.orig_EngageInMovement orig, SlugcatHand self)
    {
        Player myPlayer = self.owner.owner as Player;
        if (Plugin.scugCWT.TryGetValue(myPlayer, out ScugCWT cwt) && cwt.IsBeacon && cwt.Beacon.brightSquint > 1)
        {
            
            PlayerGraphics myGraphics = (myPlayer.graphicsModule as PlayerGraphics);
            
            //Vector2 tarLoc = (myPlayer.graphicsModule as PlayerGraphics).head.pos;
            //self.absoluteHuntPos = tarLoc - Custom.DirVec(myPlayer.bodyChunks[0].pos, tarLoc) * 3f;
            Vector2 baseLoc = (myPlayer.graphicsModule as PlayerGraphics).head.pos;
            //Vector2 targPos = (myPlayer.graphicsModule as PlayerGraphics).objectLooker.mostInterestingLookPoint;


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

                //self.absoluteHuntPos = myHelper.bodyChunks[0].pos - Custom.DirVec(myPlayer.bodyChunks[0].pos, myHelper.bodyChunks[0].pos) * 8f;
                self.absoluteHuntPos = targPos - Custom.DirVec(myPlayer.bodyChunks[0].pos, targPos) * 3f;
                return false;
            }
            
        }
        

        return orig(self);
    }


    private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (Plugin.scugCWT.TryGetValue(self.player, out ScugCWT cwt) && cwt.IsBeacon)
        {
            //cwt.Beacon.eyePos = sLeaser.sprites[9].GetPosition();

            if (cwt.Beacon.brightSquint > (40 * 3.5f))
                sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName("Face" + "Stunned");
            /*
            else if (cwt.Beacon.brightSquint > 10)
            {
                if (self.blink <= 0)
                {
                    if (UnityEngine.Random.value < 0.1f)
                        self.player.Blink(Mathf.FloorToInt(Mathf.Lerp(3f, 8f, UnityEngine.Random.value)));
                    else
                        sLeaser.sprites[9].scaleY = 0.7f;
                    //else
                    //    sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName("Face" + "Stunned");
                }

                sLeaser.sprites[9].GetLocalMousePosition -= ;
            }
            if ((cwt.Beacon.brightSquint == 0 && sLeaser.sprites[9].scaleY == 0.7) || self.blink > 0)
                sLeaser.sprites[9].scaleY = 1;
            */
            if (cwt.Beacon.brightSquint > 10)
            {
                sLeaser.sprites[9].x -= self.lookDirection.x * 2;
                sLeaser.sprites[9].y -= self.lookDirection.y * 2;
            }
                

        }
    }

    private static void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
    {
        if (self.grasps[grasp] != null && (self.grasps[grasp].grabbed is Weapon) && Plugin.scugCWT.TryGetValue(self, out ScugCWT cwt) && cwt.IsBeacon)
        {
            cwt.Beacon.dontThrowTimer = 15; //BRIEF PERIOD OF DON'T THROW A FLASHBANG
            //MAYBE A FANCY COLOR?...
            if (self.grasps[grasp].grabbed is FlareBomb flare)
            {
                flare.color = new Color(0.2f, 0f, 1f); //Color(0.2f, 0f, 1f); //WE COULD GIVE IT A FUNKY COLOR, IF WE WANT...
                cwt.Beacon.dontThrowTimer = 60; //DON'T THROW ANOTHER ONE FOR A WHILE
            }
        }
        orig(self, grasp, eu);
    }

    

    public static void BeaconStorageGrafUpdate(On.Player.orig_GraphicsModuleUpdated orig, Player self, bool actuallyViewed, bool eu)
    {
        if (Plugin.scugCWT.TryGetValue(self, out ScugCWT cwt) && cwt.IsBeacon)
        {
            cwt.Beacon.storage?.GraphicsModuleUpdated(eu);
        }
        orig(self, actuallyViewed, eu);
    }


    private static void Player_ObjectEaten(On.Player.orig_ObjectEaten orig, Player self, IPlayerEdible edible)
    {
        orig(self, edible);
        if (Plugin.scugCWT.TryGetValue(self, out ScugCWT cwt) && cwt.IsBeacon && self.input[0].pckp)
            cwt.Beacon.heldCraft = true;
    }

    private static void Player_Regurgitate(On.Player.orig_Regurgitate orig, Player self)
    {
        orig(self);
        if (Plugin.scugCWT.TryGetValue(self, out ScugCWT cwt) && cwt.IsBeacon && self.input[0].pckp)
            cwt.Beacon.heldCraft = true;
    }

    private static void Player_SwallowObject(On.Player.orig_SwallowObject orig, Player self, int grasp)
    {
        orig(self, grasp);
        if (Plugin.scugCWT.TryGetValue(self, out ScugCWT cwt) && cwt.IsBeacon && self.input[0].pckp)
            cwt.Beacon.heldCraft = true;
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