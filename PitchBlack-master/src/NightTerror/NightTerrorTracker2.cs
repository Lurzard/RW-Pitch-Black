using PitchBlack;
using System.Collections.Generic;
using UnityEngine;
using Music;
using System;

namespace PitchBlack;

/*
public static class NTTrackerHooks
{
    public static NTTracker myTracker;// IDRK WHAT TO DO WITH THIS
    public static void Apply()
	{
		
        On.RainWorldGame.ctor += RainWorldGame_ctor;
        On.RainWorldGame.Update += RainWorldGame_Update;
	}

    private static void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        orig(self);
		Debug.Log("UPDATEME");
		if (myTracker is not null)
			myTracker.Update();
	}

    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);

        if (self.IsStorySession)
		{
            myTracker = new NTTracker(self);
			Debug.Log("ADDING TRACKER");
        }
            
    }
}
*/

public class NTTracker
{
    public NTTracker(RainWorldGame g)
    {
        Debug.Log("PURSUED TRACKER INIT");
        this.regionCooldowns = new List<string>();
        this.game = g;
        this.unrealizedCounter = 0;
        this.region = this.game.world.region.name;
        this.summoning = false;

    }


    public void SpawnPosition()
    {
        if (this.game != null && this.game.world != null && this.game.Players != null && this.game.Players.Count > 0 && this.game.Players[0] != null && this.game.Players[0].Room != null)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < this.game.world.NumberOfRooms; i++)
            {
                AbstractRoom abstractRoom = this.game.world.GetAbstractRoom(this.game.world.firstRoomIndex + i);
                if (abstractRoom != null && !abstractRoom.shelter && !abstractRoom.gate && abstractRoom.name != this.game.Players[0].Room.name)
                {
                    list.Add(i);
                }
            }
            AbstractRoom abstractRoom2 = this.game.world.GetAbstractRoom(this.game.world.firstRoomIndex + list[UnityEngine.Random.Range(0, list.Count)]);
            this.spawnPos = abstractRoom2.RandomNodeInRoom();
            Debug.Log("HUNTER LOCATION: " + abstractRoom2.name);
        }
    }


    public void SetUpPlayer()
    {
        if (this.game != null)
        {
            for (int i = 0; i < this.game.Players.Count; i++)
            {
                if (this.game.Players[i] != null && this.game.Players[i].realizedCreature != null && !this.game.Players[i].realizedCreature.dead)
                {
                    this.targetPlayer = (this.game.Players[i].realizedCreature as Player);
                    return;
                }
            }
            if (this.game.manager.musicPlayer != null)
            {
                Song song = this.game.manager.musicPlayer.song;
                if (((song != null) ? song.name : null) == "RW_20 - Polybius")
                {
                    this.game.manager.musicPlayer.FadeOutAllNonGhostSongs(100f);
                }
            }
            return;
        }
    }


    public void SetUpHunter()
    {
        if (this.game != null && this.game.world != null && this.regionSwitchCooldown <= 0)
        {
            //JUST IN CASE, GET RIDDA THE OLD ONE
            if (this.pursuer != null)
            {
                this.pursuer.Move(this.spawnPos); //TRY N RESPAWN THEM FIRST OR WE CAN'T GET RID OF THEM
                this.game.world.GetAbstractRoom(this.spawnPos).AddEntity(this.pursuer);

                if (this.pursuer.realizedCreature != null && this.pursuer.realizedCreature.room != null)
                {
                    this.pursuer.realizedCreature.RemoveFromRoom();
                }

                this.pursuer.Destroy();
                this.pursuer = null;
            }
            

            //this.pursuer = new AbstractCreature(this.game.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.RedCentipede), null, this.spawnPos, this.game.GetNewID());
            this.pursuer = new AbstractCreature(this.game.world, StaticWorld.GetCreatureTemplate(CreatureTemplateType.NightTerror), null, this.spawnPos, this.game.GetNewID());
            this.pursuer.voidCreature = true;
            this.pursuer.saveCreature = false;
            this.pursuer.ignoreCycle = true;
            this.pursuer.HypothermiaImmune = true;
            this.game.world.GetAbstractRoom(this.spawnPos).AddEntity(this.pursuer);
        }
    }


    public void Update()
    {
        
        if (this.game.world == null || !PBOptions.pursuer.Value || (this.game.session is StoryGameSession session && (session.saveStateNumber != Plugin.BeaconName)) )
        {
            return;
        }
        if (this.region != this.game.world.region.name)
        {
            this.region = this.game.world.region.name;
            this.regionSwitchCooldown = 2400;
        }
        this.regionSwitchCooldown--;
        if (this.pursuer != null && this.pursuer.Room != null)
        {
            if (this.pursuer.state.dead || this.region != this.pursuer.world.region.name || (this.pursuer.Room.shelter && this.pursuer.Room.realizedRoom != null && this.pursuer.Room.realizedRoom.shelterDoor.IsClosing))
            {
                //WE DON'T NEED THEM TO POOF INTO A CLOUD OF SMOKE (UNTIL WE LEAVE THE ROOM)
                /*
                if (this.pursuer.realizedCreature != null && this.pursuer.realizedCreature.room != null)
                {
                    this.pursuer.realizedCreature.room.AddObject(new ShockWave(this.pursuer.realizedCreature.mainBodyChunk.pos, 300f, 5f, 100, true));
                    this.pursuer.realizedCreature.room.PlaySound(SoundID.Coral_Circuit_Break, this.pursuer.realizedCreature.mainBodyChunk);
                    this.pursuer.realizedCreature.RemoveFromRoom();
                    this.game.cameras[0].hud.textPrompt.AddMessage("The pursuer retreats...", 10, 250, true, true);
                }
                */
                //ONLY DO THIS STUFF OF WE'RE OFFSCREEN
                if (this.pursuer.realizedCreature == null || this.pursuer.realizedCreature.room == null)
                {
                    //REMOVE THE CORPSE (AND THE PURSUER REFERENCE) SO WE CAN BEGIN THE PROCESS OF RESPAWNING
                    this.pursuer.Destroy();
                    if (this.pursuer.state.dead && !this.regionCooldowns.Contains(this.region))
                    {
                        this.regionCooldowns.Add(this.region);
                    }
                    this.pursuer = null;
					this.hackTimer = 0;
                    this.game.cameras[0].hud.textPrompt.AddMessage("DEBUG: Pursuer Removed...", 10, 250, true, true);
                    return;
                }
                    
            }
            this.SetUpPlayer();
            if (this.currentRoom != this.pursuer.Room.name)
            {
                this.currentRoom = this.pursuer.Room.name;
                Debug.Log("Pursuer moving to: " + this.currentRoom);
                //WE DON'T REALLY NEED THIS, IT'S JUST FLASHY
                /*
                for (int i = 0; i < this.pursuer.Room.connections.Length; i++)
                {
                    for (int j = 0; j < this.pursuer.world.game.AlivePlayers.Count; j++)
                    {
                        if (this.pursuer.Room.connections[i] == this.pursuer.world.game.AlivePlayers[j].pos.room && !this.warning)
                        {
                            this.game.cameras[0].hud.textPrompt.AddMessage("You are being pursued...", 10, 250, true, true);
                            this.warning = true;
                        }
                    }
                }
                */
            }
            if (this.pursuer.abstractAI != null && this.targetPlayer != null && this.ValidTrackRoom(this.targetPlayer.room))
            {
                WorldCoordinate worldCoordinate = this.destination;
                if (this.destination.room != this.pursuer.pos.room)
                {
                    this.destination = this.targetPlayer.abstractCreature.pos;
                    this.pursuer.abstractAI.SetDestination(this.destination);
                }
				
				if (this.pursuer.pos.room == this.targetPlayer.abstractCreature.pos.room)
				{
					if (this.hackTimer > 0)
						this.hackTimer--;
				}
				else
					this.hackTimer++;
				
				
            }
            if ((this.pursuer.realizedCreature == null && this.pursuer.timeSpentHere > relocateTimer) || this.hackTimer > relocateTimer)
            {
                Debug.Log("RE-LOCATING PURSUER!");
                this.game.cameras[0].hud.textPrompt.AddMessage("DEBUG: Relocating Pursuer", 10, 250, true, true);
                this.BeginSummon();
                return;
                /*
				//this.SpawnPosition();
				this.pursuer.Move(this.spawnPos);
				if (this.warning)
				{
					this.warning = false;
					//this.game.cameras[0].hud.textPrompt.AddMessage(ChallengeTools.IGT.Translate("The pursuer retreats..."), 10, 250, true, true);
					this.game.cameras[0].hud.textPrompt.AddMessage("The pursuer retreats...", 10, 250, true, true);
				}
				*/
            }
            if (this.pursuer.abstractAI.RealAI != null && this.targetPlayer != null)
            {
                this.pursuer.abstractAI.RealAI.tracker.SeeCreature(this.targetPlayer.abstractCreature);
                //if (ExpeditionData.devMode && !this.pursuer.state.dead && Input.GetKey(8))
                //{
                //	this.pursuer.Die();
                //}
                if (!this.warning)
                {
                    //this.game.cameras[0].hud.textPrompt.AddMessage("You are being pursued...", 10, 250, true, true);
                    this.warning = true;
                    return;
                }
            }
        }

        //ACTUALLY WE CAN TRY JUST SPAWNING THEM RIGHT ON TOP OF US BECAUSE THEY WONT ACTUALLY ENTER THE ROOM UNTIL WE LEAVE
        //WHICH IS PERFECT BECAUSE THEN THEY WON'T SPAWN IN FRONT OF US
        else if (!this.regionCooldowns.Contains(this.region) && this.game.world.rainCycle.CycleProgression > 0.1f)
        {
            BeginSummon();
        }
        //else
            //Debug.Log("IDLE");
    }

    public bool ValidTrackRoom(Room room)
    {
        if (room == null
            || RoomIsAStartingCabinetsRoom(room.roomSettings.name)
            || room.abstractRoom.shelter
            || room.abstractRoom.gate
        )
        {
            return false;
        }
        return true;
    }

    public static bool RoomIsAStartingCabinetsRoom(string roomName)
    {
        if (roomName == "SH_CABINETMERCHANT")
            return true;
        if (roomName.Substring(0, Math.Min(2, roomName.Length)) == "RM_") //ALSO DON'T TRACK IN THE ROT
            return true; 

        for (int i = 1; i <= 5; i++)
        {
            //spinch: nt gets to track SH_CABINETS6, as a treat
            if (roomName == $"SH_CABINETS{i}")
                return true;
        }

        return false;
    }


    //THIS WAS CLOSER TO HOW THE GAME DID IT, BUT IT JUST CHOSE LIKE A RANDOM ROOM
    /*
    public void BeginSummon()
    {
        this.SetUpPlayer();
        //this.spawnPos = this.targetPlayer.abstractCreature.pos; // this.game.world.GetAbstractRoom(this.targetPlayer.room);

        Debug.Log("SUMMONING DEBUG " + this.targetPlayer.playerState.playerNumber + " - " + this.targetPlayer.room + " - " + this.targetPlayer.mainBodyChunk.pos);
        this.spawnPos = this.targetPlayer.room.GetWorldCoordinate(this.targetPlayer.mainBodyChunk.pos); //GetWorldCoordinate
        this.summonSickness = 200;
        this.summoning = true;

        Debug.Log("SUMMONING PURSUER");
    }
    */

    //LETS DO THE ONE THAT SPAWNS RIGHT ON TOP OF US
    public void BeginSummon()
    {
        this.SetUpPlayer();
        if (this.targetPlayer?.room != null && this.ValidTrackRoom(this.targetPlayer.room))
        {
            this.spawnPos = this.targetPlayer.room.GetWorldCoordinate(this.targetPlayer.mainBodyChunk.pos); //GetWorldCoordinate
            this.SetUpHunter();
			this.hackTimer = 0;
            Debug.Log("SUMMONING PURSUER");
            this.game.cameras[0].hud.textPrompt.AddMessage("DEBUG: Summoning Pursuer", 10, 250, true, true);
        }
        //else
        //    Debug.Log("UNABLE TO SUMMON PURSUER");
    }


    public Player targetPlayer;

    public AbstractCreature pursuer;

    public string currentRoom;

    public string region;

    public WorldCoordinate spawnPos;

    public WorldCoordinate destination;

    public bool warning;

    public int unrealizedCounter;

    public List<string> regionCooldowns;

    public int regionSwitchCooldown;

    //TAKEN FROM BURDEN TRACKER
    public RainWorldGame game;

    //AD HOC
    public int summonSickness = 0;
    public bool summoning;
	public int hackTimer = 0;
    public int relocateTimer = PBOptions.pursuerAgro.Value * 2400; //IN SECONDS
}