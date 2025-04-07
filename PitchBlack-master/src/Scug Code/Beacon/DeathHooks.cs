using RWCustom;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MoreSlugcats;
using JetBrains.Annotations;
using System.Drawing;
using JollyCoop;

namespace PitchBlack;

public class DeathHooks {

    #region failed implementation
    //this ended up not working so I gave up and moved back to working in Player.Update instead of here
    //If you'd want to help out I would appreciate it so much :tears:

    //public static void Thanatosis(Player self) {
    //    if (Plugin.scugCWT.TryGetValue(self, out ScugCWT scugCWT) && scugCWT is BeaconCWT beaconCWT) {
    //        //Referencing Player.WatcherUpdate()

    //        if (!self.input[0].spec) {
    //            beaconCWT.inputsNeedReset = false;
    //        }
    //        //blacklisted animations
    //        bool animations = 
    //            self.animation != Player.AnimationIndex.BellySlide && 
    //            self.animation != Player.AnimationIndex.CrawlTurn && 
    //            self.animation != Player.AnimationIndex.CorridorTurn && 
    //            self.animation != Player.AnimationIndex.Flip && 
    //            self.animation != Player.AnimationIndex.Roll && 
    //            self.animation != Player.AnimationIndex.GrapplingSwing &&
    //            self.animation != Player.AnimationIndex.RocketJump;
    //        //tracking activation
    //        if (/*oiple karma > 0.5f &&*/beaconCWT.performingActivationTimer > 0f || self.input[0].spec && !beaconCWT.inputsNeedReset && animations) {
    //            //if >= 4f oiple karma do a cool effect later
    //            //starting state
    //            if (beaconCWT.startingStateOnActivate == -1) {
    //                beaconCWT.startingStateOnActivate = (beaconCWT.isDead ? 1 : 0);
    //            }
    //            beaconCWT.activateThanatosisTimer++;
    //            //toggling
    //            if (beaconCWT.activateThanatosisTimer == ((beaconCWT.startingStateOnActivate == 0) ? beaconCWT.enterIntoThanatosisDuration : beaconCWT.exitOutOfThanatosisDuration) && beaconCWT.performingActivationTimer == 0) {
    //                KILL(self); //Go to Thanatosis
    //                beaconCWT.performingActivationTimer++;
    //                if (beaconCWT.performingActivationTimer >= beaconCWT.performingActivationDuration) {
    //                    beaconCWT.performingActivationTimer = 0;
    //                    //this.cancelCamoCooldown = 120;
    //                    beaconCWT.inputsNeedReset = true;
    //                }
    //                else if (beaconCWT.activateThanatosisTimer >= beaconCWT.enterIntoThanatosisDuration) {
    //                    beaconCWT.performingActivationTimer = 1;
    //                }
    //            }
    //            else {
    //                if (beaconCWT.activateThanatosisTimer > 0) {
    //                    beaconCWT.activateThanatosisTimer = 0;
    //                    beaconCWT.performingActivationTimer = 0;
    //                    self.canJump = 0;
    //                    self.wantToJump = 0;
    //                    //this.cancelCamoCooldown = 120;
    //                    beaconCWT.inputsNeedReset = true;
    //                }
    //                //if (cancelCamoCooldown > 0){}
    //                beaconCWT.startingStateOnActivate = -1;
    //            }
    //        }
    //        //Referencing CamoUpdate
    //        if (beaconCWT.isDead && self.Consious) {
    //            KILL(self); //Go to Thanatosis
    //        }
    //        if (beaconCWT.isDead) {
    //            beaconCWT.inThanatosisTime++;
    //            if (beaconCWT.inThanatosisTime >= ThanatosisLimit) {
    //                //this.camoRechargePenalty = 400;
    //                rwg.GameOver(null);
    //            }
    //        }
    //        else {
    //            beaconCWT.inThanatosisTime = 0;
    //        }
    //        //float incrementing
    //        if (beaconCWT.isDead && beaconCWT.thanatosisProgress < 1f)
    //            beaconCWT.thanatosisProgress += 0.01f;
    //        else if (!beaconCWT.isDead && beaconCWT.thanatosisProgress > 0f)
    //            beaconCWT.thanatosisProgress -= 0.01f;
    //    }
    //}
    #endregion

    public static float ThanatosisLimit
    {
        get
        {
            return 1600f; //soon to be based on Karma
        }
    }

    public static void Apply() {

    }

    //public static void KILL(Player self)
    //{
    //    //Actual Thanatosis, referencing CamoToggle
    //    if (Plugin.scugCWT.TryGetValue(self, out ScugCWT scugCWT) && scugCWT is BeaconCWT beaconCWT)
    //    {
    //        beaconCWT.comparebool = beaconCWT.isDead;
    //        beaconCWT.isDead = !beaconCWT.isDead;
    //        if (beaconCWT.comparebool != beaconCWT.isDead)
    //        {
    //            self.room.PlaySound(beaconCWT.isDead ? PBSoundID.Player_Activated_Thanatosis : PBSoundID.Player_Deactivated_Thanatosis, self.mainBodyChunk);
    //            self.dead = beaconCWT.isDead ? true : false;
    //        }
    //    }
    //}

    #region old class
    //private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    //{
    //    orig(self, eu);
    //    if (Plugin.scugCWT.TryGetValue(self, out ScugCWT scugCWT) && scugCWT is BeaconCWT beaconCWT)
    //    {

    //        if (self.input[0].spec)
    //        {
    //            beaconCWT.deathToggle = beaconCWT.isDead; //notes current state of isDead
    //            beaconCWT.isDead = !beaconCWT.isDead; //flips isDead once
    //            if (beaconCWT.deathToggle != beaconCWT.isDead && self.room != null)
    //            {
    //                if they're different, which they will be
    //                self.room.PlaySound(beaconCWT.isDead ? PBSoundID.Player_Activated_Thanatosis : PBSoundID.Player_Deactivated_Thanatosis, self.mainBodyChunk);
    //                DieAndThanatosis(self); //completely seperate implementation of player.Die();
    //            }
    //            else if (self.Consious) beaconCWT.isDead = false; //to revert isDead
    //        }
    //        if (beaconCWT.isDead)
    //        {
    //            beaconCWT.inThanatosisTime++;
    //            if (beaconCWT.thanatosisLerp < ThanatosisLimit) beaconCWT.thanatosisLerp++;
    //        }
    //        if (beaconCWT.inThanatosisTime >= ThanatosisLimit)
    //        {
    //            Debug.Log("WAAAIIIIT DONT DO THAT!!!");
    //            self.dead = true;
    //            beaconCWT.isDead = false;
    //        }
    //        if (!beaconCWT.isDead && (beaconCWT.inThanatosisTime > 0 || beaconCWT.thanatosisLerp > 0)) beaconCWT.thanatosisLerp--;
    //        else beaconCWT.inThanatosisTime = 0;
    //    }
    //    this will error if everything inside is not also static
    //}

    //*******************
    //THANATOSIS TRACKING
    //*******************

    //public static void DieAndThanatosis(Player self)
    //{ //self.Die & base.Die stuff
    //    Room realizedRoom = self.room;
    //    if (realizedRoom == null) realizedRoom = self.abstractCreature.world.GetAbstractRoom(self.abstractCreature.pos).realizedRoom; //probbaly a safeguard
    //    if (self.AI == null)
    //    { //this checks for SlugNPCAI btw, will be null
    //        if (realizedRoom != null)
    //        {
    //            if (realizedRoom.game.setupValues.invincibility) return;
    //            if (!self.dead && !isDead)
    //            { //if dead true and isDead false
    //                isDeadForReal = true;
    //                realizedRoom.game.GameOver(null);
    //                realizedRoom.PlaySound(PBSoundID.Player_Died_From_Thanatosis, self.mainBodyChunk);
    //            }
    //            if (beaconCWT.isDead)
    //            { //Creature.Die
    //                self.Blink(20);
    //                self.dead = true;
    //                self.LoseAllGrasps();
    //                self.abstractCreature.Die();
    //            }
    //            else
    //            { //BigSpider revive
    //                self.Stun(20);
    //                self.dead = false;
    //                self.abstractCreature.abstractAI.SetDestination(self.abstractCreature.pos);
    //            }
    //            later figure out how to revert GameOver to come back from actual death
    //        }
    //    }
    #endregion
}
