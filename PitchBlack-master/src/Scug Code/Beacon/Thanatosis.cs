using RWCustom;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MoreSlugcats;
using JetBrains.Annotations;
using System.Drawing;
using JollyCoop;

namespace PitchBlack;

public class Thanatosis
{

    public static void Apply() {
        On.Player.Update += Player_Update;
        //On.Player.Die += Player_Die;
        //On.Creature.Die += Creature_Die;
    }

    //private static void Creature_Die(On.Creature.orig_Die orig, Creature self)
    //{
    //    orig(self);
    //    if (player.SlugCatClass != Plugin.Beacon) return;
    //    CreatureDieAndThanatosis();
    //}

    //private static void Player_Die(On.Player.orig_Die orig, Player self) {
    //    orig(self);
    //    if (self.SlugCatClass != Plugin.Beacon) return;
    //    DieAndThanatosis();
    //}

    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu) {
        orig(self, eu);
        BeaconUpdate();
    }

    //the hooks need static to access our methods
    //our methods need static variables to use them
    //so I caved and made everything static, at least until I figure it out
    // -Lur

    public static Player player; //self.
    public static Creature creature; //base.
    public static RainWorldGame rwg; //used to call GameOver

    //*******************
    //THANATOSIS TRACKING
    //*******************

    public static int usedThanatosis;

    //these should probably be moved to BeaconCWT, but they need to be static rn
    public static bool isDead; //state tracking
    public static int inThanatosisTime; //tracking current time spent in Thanatosis
    public static int startingThanatosisOnActivate = -1; //actually determine which state to enter
    public static bool inputNeedsReset;

    public static float ThanatosisLimit {
        get {
            return 1600f; //soon to be based on QualiaLevel
        }
    }

    public static void BeaconUpdate() {
        if (Plugin.scugCWT.TryGetValue(player, out ScugCWT scugCWT) && scugCWT is BeaconCWT beaconCWT) {
            if (!player.input[0].jmp) {
                inputNeedsReset = false;
            }
            //both inputs will be changed to spec eventually, I'll say that when I try, the code doesn't know what it is!
            if  (player.input[0].jmp) {
                if (startingThanatosisOnActivate == -1) {
                    startingThanatosisOnActivate = isDead ? 1 : 0;
                }

                if (startingThanatosisOnActivate == 0) {
                    ToggleThanatosis(); //butterfly effects into DieAndThanatosis
                }
            }
            ThanatosisUpdate();
        }
    }

    public static void ThanatosisUpdate() {
        if (Plugin.scugCWT.TryGetValue(player, out ScugCWT scugCWT) && scugCWT is BeaconCWT beaconCWT) {
            if (isDead) inThanatosisTime++;
            if (inThanatosisTime >= ThanatosisLimit) {
                    Debug.Log("WAAAIIIIT DONT DO THAT!!!");
                    rwg.GameOver(null);
                }
            else inThanatosisTime = 0;
        }
    }

    public static void ToggleThanatosis() {
        bool death = isDead; //notes current state of isDead
        isDead = !isDead; //flips isDead once

        if (creature.Consious) isDead = false; //to revert isDead

        if (death != isDead && player.room != null) { //if they're different, which they will be
            //sound design for isDead true or false :cooking:
            player.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Ghost_Ping_Base, player.mainBodyChunk);
            player.room.PlaySound( isDead ? SoundID.In_Room_Deer_Summoned : SoundID.Distant_Deer_Summoned, player.mainBodyChunk, false, 0.35f, UnityEngine.Random.value * 0.5f + 0.8f);
            player.Blink(isDead ? 40 : 20);
            player.Stun(isDead ? 20 : 10);
            //creature.dead = isDead ? true : false; //theoretically echo into all the death code? but unfortunately that calls GameOver
            DieAndThanatosis(); //completely seperate implementation of player.Die();
        }
    }

    public static void DieAndThanatosis() { //self.Die & base.Die stuff
        Room realizedRoom = player.room;
        if (realizedRoom == null) realizedRoom = creature.abstractCreature.world.GetAbstractRoom(creature.abstractCreature.pos).realizedRoom; //probbaly a safeguard
        if (player.AI == null) { //this checks for SlugNPCAI btw, will be null
            if (realizedRoom != null) {
                if (realizedRoom.game.setupValues.invincibility) return;
                if (!creature.dead && !isDead) {
                    realizedRoom.game.GameOver(null);
                    realizedRoom.PlaySound(SoundID.UI_Slugcat_Die, creature.mainBodyChunk);
                }
                if (isDead) { //Creature.Die code
                    creature.dead = true;
                    creature.LoseAllGrasps();
                    creature.abstractCreature.Die();
                }
                else { //BigSpider revive
                    creature.dead = false;
                    creature.abstractCreature.abstractAI.SetDestination(creature.abstractCreature.pos);
                }
            }
        }
    }
}
