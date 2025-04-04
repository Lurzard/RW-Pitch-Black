using RWCustom;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MoreSlugcats;
using JetBrains.Annotations;
using System.Drawing;
using JollyCoop;

namespace PitchBlack;

public class Thanatosis {
    public static void Apply() {
        On.Player.Update += Player_Update;
    }
    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu) {
        orig(self, eu);
        if (Plugin.scugCWT.TryGetValue(self, out ScugCWT scugCWT) && scugCWT is BeaconCWT beaconCWT) {
            //both inputs will be changed to spec eventually, I'll say that when I try, the code doesn't know what it is!
            // Toggles Thanatosis if jump is pressed
            if (self.input[0].jmp) {
                //butterfly effects into DieAndThanatosis
                bool death = isDead; //notes current state of isDead
                isDead = !isDead; //flips isDead once

                if (self.Consious) isDead = false; //to revert isDead

                if (death != isDead && self.room != null) { //if they're different, which they will be
                    //sound design for isDead true or false :cooking:
                    self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Ghost_Ping_Base, self.mainBodyChunk);
                    self.room.PlaySound( isDead ? SoundID.In_Room_Deer_Summoned : SoundID.Distant_Deer_Summoned, self.mainBodyChunk, false, 0.35f, Random.value * 0.5f + 0.8f);
                    //creature.dead = isDead ? true : false; //theoretically echo into all the death code? but unfortunately that calls GameOver
                    DieAndThanatosis(self); //completely seperate implementation of player.Die();
                }
            }
            if (isDead) inThanatosisTime++;
            if (inThanatosisTime >= ThanatosisLimit) {
                Debug.Log("WAAAIIIIT DONT DO THAT!!!");
                self.dead = true;
                isDead = false;
                //rwg.GameOver(null);
                }
            else inThanatosisTime = 0;
        }
        //this will error if everything inside is not also static
    }
    //the hooks need static to access our methods
    //our methods need static variables to use them
    //so I caved and made everything static, at least until I figure it out
    // -Lur

    //*******************
    //THANATOSIS TRACKING
    //*******************

    //these should probably be moved to BeaconCWT, but they need to be static rn
    public static bool isDead; //state tracking
    public static int inThanatosisTime; //tracking current time spent in Thanatosis

    public static float ThanatosisLimit {
        get {
            return 1600f; //soon to be based on QualiaLevel
        }
    }
    public static void DieAndThanatosis(Player self) { //self.Die & base.Die stuff
        Room realizedRoom = self.room;
        if (realizedRoom == null) realizedRoom = self.abstractCreature.world.GetAbstractRoom(self.abstractCreature.pos).realizedRoom; //probbaly a safeguard
        if (self.AI == null) { //this checks for SlugNPCAI btw, will be null
            if (realizedRoom != null) {
                if (realizedRoom.game.setupValues.invincibility) return;
                if (!self.dead && !isDead) {
                    realizedRoom.game.GameOver(null);
                    realizedRoom.PlaySound(SoundID.UI_Slugcat_Die, self.mainBodyChunk);
                }
                if (isDead) { //Creature.Die
                    self.Blink(20);
                    self.dead = true;
                    self.LoseAllGrasps();
                    self.abstractCreature.Die();
                }
                else { //BigSpider revive
                    self.Stun(20);
                    self.dead = false;
                    self.abstractCreature.abstractAI.SetDestination(self.abstractCreature.pos);
                }
            }
        }
    }
}
