using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace PitchBlack;
#nullable enable

public class NTTracker
{
    private const string PURSUE_SONG_NAME = "RW_20 - Polybius";
    private readonly bool DEBUG_MESSAGES = false;
    public AbstractCreature? targetPlayer;
    public AbstractCreature? pursuer;
    public string region;
    public int regionSwitchCooldown = 2400; //MAKE IT START THIS WAY SO THAT IT COUNTS FOR THE BEGINNING CYCLE TIMER TOO
    //TAKEN FROM BURDEN TRACKER
    public RainWorldGame game;
    //AD HOC
    public int abstractTeleportTimer = 0;
    public readonly int abstractRelocateTime;
    public AbstractRoom? currentTargetRoom;
    public WorldCoordinate quickMoveToExit;
    public int quickMoveToExitCounter;
    // public AbstractRoom oldTargetRoom;
    public NTTracker(RainWorldGame rainGame)
    {
        Debug.Log("PURSUED TRACKER INIT");
        game = rainGame;
        region = game.world.region.name;
        // The value is multiplied by 2400 because that is 1 minute, given the game tickes at 40tps
        abstractRelocateTime = ModOptions.pursuerAgro.Value * 2400;
    }
    int FindDistanceToTarget(int searchDistance) {
        if (pursuer?.Room == null) {
            Debug.Log("Pursuer or it's room were null, returning -1");
            return -1;
        }
        if (pursuer.state.dead) {
            Debug.Log("Pursuer is dead, returning -1");
            return -1;
        }
        if (DEBUG_MESSAGES) Debug.Log("Pursuer attepmting to find distance to player room");

        List<AbstractRoom> alreadySearchedRooms = new List<AbstractRoom>();                 // Rooms that have already been searched in for the player
        List<AbstractRoom> potentialSearchRooms = new List<AbstractRoom>(){pursuer.Room};   // Rooms that are next to be searched. Cleared after every loop below after moving contents to actualSearchRooms.
        List<AbstractRoom> actualSearchRooms = new List<AbstractRoom>();                    // Stores rooms that are actively being searched. While accessing the rooms, it adds connected rooms that aren't in alreadySearchedRooms to potentialSearchRooms.
        // i is the search depth, how many rooms have been searched before finding a match
        for (int i = 0; i <= searchDistance; i++) {
            actualSearchRooms.AddRange(potentialSearchRooms);
            potentialSearchRooms.Clear();
            foreach (AbstractRoom absRoom in actualSearchRooms) {
                if (DEBUG_MESSAGES) Debug.Log("Pursuer searching room: " + absRoom.name);
                if (absRoom == targetPlayer?.Room) {
                    return i;
                }
                alreadySearchedRooms.Add(absRoom);
                for (int j = 0; j < absRoom.connections.Length; j++) {
                    AbstractRoom room = game.world.GetAbstractRoom(absRoom.connections[j]);
                    if (room != null && !alreadySearchedRooms.Contains(room) && !potentialSearchRooms.Contains(room)) {
                        potentialSearchRooms.Add(room);
                    }
                }
            }
            actualSearchRooms.Clear();
        }
        return -1;
    }
    /// <summary>
    /// Updates the target player to the next alive realized player in RainWorldGame's 'Player' list.
    /// </summary>
    public void UpdateTarget() {
        for (int i = 0; i < game.Players.Count; i++) {
            if (game.Players[i]?.realizedCreature is Player plyr && !plyr.dead) {
                targetPlayer = game.Players[i];
                break;
            }
        }
        return;
    }
    public void RecreatePursuer()
    {
        // If it has been enough time, it can change regions
        if (targetPlayer != null && game?.world != null) {
            //JUST IN CASE, GET RIDDA THE OLD ONE
            if (pursuer != null) {
                pursuer.realizedCreature?.Destroy(); //THIS MIGHT HAVE BEEN WHAT WE NEEDED
                pursuer.Destroy();
                pursuer = null;
                if (DEBUG_MESSAGES) {
                    Debug.Log($"DEBUG: Pursuer Removed... (Via {nameof(RecreatePursuer)})");
                }
            }
            
            ShortcutData shorcut = targetPlayer.Room.realizedRoom.shortcuts[Random.Range(0, targetPlayer.Room.realizedRoom.shortcuts.Length)];
            // This makes sure it comes from a room exit.
            while (shorcut.shortCutType != ShortcutData.Type.RoomExit) {
                shorcut = targetPlayer.Room.realizedRoom.shortcuts[Random.Range(0, targetPlayer.Room.realizedRoom.shortcuts.Length)];
            }
            WorldCoordinate spawnPos = shorcut.destinationCoord;

            pursuer = new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate(Enums.CreatureTemplateType.NightTerror), null, spawnPos, game.GetNewID());
            // pursuer.abstractAI.SetDestination(targetPlayer.pos);
            game.world.GetAbstractRoom(spawnPos).AddEntity(pursuer);
            pursuer.RealizeInRoom();
            pursuer.realizedCreature.enteringShortCut = new IntVector2?(targetPlayer.world.GetAbstractRoom(spawnPos.room).realizedRoom.ShortcutLeadingToNode(spawnPos.abstractNode).StartTile);
        }
    }
    public void Update()
    {
        if (game.world == null || pursuer == null) {
            return;
        }
        if (region == game.world.region.name) {
            regionSwitchCooldown = 2400;
        }
        else {
            regionSwitchCooldown--;
        }
        if (regionSwitchCooldown <= 0) {
            region = game.world.region.name;
        }

        UpdateTarget();
        
        #region Play pursue theme if it isn't already playing
        // Preform null check on player's room since we don't want to play the theme if both creatures are just in pipes.
        if (targetPlayer?.realizedCreature?.room != null && pursuer.realizedCreature?.room == targetPlayer.realizedCreature?.room && game.manager.musicPlayer.song?.name != PURSUE_SONG_NAME) {
            if (DEBUG_MESSAGES) Debug.Log("Pursuer starting chase theme");
            game.manager.musicPlayer.FadeOutAllSongs(40f);
            game.manager.musicPlayer.threatTracker.ghostMode = 0;
            MusicEvent musicEvent = new MusicEvent(){
                songName=PURSUE_SONG_NAME,
                prio = int.MaxValue,
                maxThreatLevel = int.MaxValue,
                cyclesRest = 0,
                oneSongPerCycle = false
            };
            game.manager.musicPlayer.GameRequestsSong(musicEvent);
        }
        #endregion

        #region Update pursue theme volume based on distance to player
        if (game.manager.musicPlayer?.song is Music.Song song && song.name == PURSUE_SONG_NAME && targetPlayer?.realizedCreature?.room != null) {
            int distance = FindDistanceToTarget(5);
            if (DEBUG_MESSAGES) Debug.Log($"pursuer distance: {distance}");
            if (distance > 5 || distance == -1) {
                game.manager.musicPlayer.FadeOutAllSongs(200);
            }
            else {
                song.baseVolume = Mathf.Lerp(0.45f, 0.08f, distance*0.2f);
            }
        }
        #endregion
        
        if (targetPlayer is AbstractCreature absCrit) {
            if (DEBUG_MESSAGES) Debug.Log("pursuer: yes player is a player");
            if (DEBUG_MESSAGES) Debug.Log(game.manager.musicPlayer?.song?.volume);
            if (currentTargetRoom != absCrit.Room) {
                // oldTargetRoom = currentTargetRoom;
                currentTargetRoom = absCrit.Room;
                if (DEBUG_MESSAGES) Debug.Log("Target moving to: " + currentTargetRoom?.name);
            }
        }

        if (targetPlayer?.realizedCreature?.room != null && targetPlayer.realizedCreature.room.ValidTrackRoom() && !targetPlayer.realizedCreature.inShortcut) {
            if (pursuer.Room == targetPlayer.Room) {
                abstractTeleportTimer = 0;
                //NEVER LOSE SIGHT OF OUR TARGET
                pursuer.abstractAI.RealAI?.tracker.SeeCreature(targetPlayer);
                pursuer.abstractAI.SetDestination(targetPlayer.pos);
            }
            else {
                abstractTeleportTimer++;
            }
        }

        // This is the bit that controls the pursuer following between rooms, as long as it doesn't get too far behind
        if (targetPlayer != null && pursuer.realizedCreature?.Consious == true && pursuer.Room.realizedRoom != null && targetPlayer.pos.room != pursuer.Room.index && pursuer.world.GetAbstractRoom(targetPlayer.pos.room) != null) {
            if (quickMoveToExitCounter > 0) {
                quickMoveToExitCounter--;
                if (quickMoveToExitCounter == 0 && quickMoveToExit.room == pursuer.Room.index && quickMoveToExit.NodeDefined) {
                    Debug.Log("Pursuer chasing player to new room...");
                    for (int i = 0; i < pursuer.realizedCreature.bodyChunks.Length; i++) {
                        pursuer.realizedCreature.bodyChunks[i].HardSetPosition(pursuer.Room.realizedRoom.MiddleOfTile(pursuer.Room.realizedRoom.ShortcutLeadingToNode(quickMoveToExit.abstractNode).StartTile));
                    }
                    pursuer.realizedCreature.enteringShortCut = new IntVector2?(pursuer.Room.realizedRoom.ShortcutLeadingToNode(quickMoveToExit.abstractNode).StartTile);
                    quickMoveToExit = new WorldCoordinate(pursuer.Room.index, -1, -1, -1);
                }
            }
            else {
                quickMoveToExit = new WorldCoordinate(pursuer.Room.index, -1, -1, -1);
                for (int i = 0; i < pursuer.Room.connections.Length; i++) {
                    if (pursuer.Room.connections[i] == targetPlayer.pos.room) {
                        quickMoveToExit.abstractNode = i;
                        quickMoveToExitCounter = 2 * pursuer.Room.realizedRoom.aimap.ExitDistanceForCreature(pursuer.realizedCreature.mainBodyChunk.pos, i, pursuer.creatureTemplate);
                        break;
                    }
                }
            }
        }
        else {
            quickMoveToExitCounter = 0;
        }

        if (DEBUG_MESSAGES) Debug.Log("region cooldown: " + regionSwitchCooldown + " Time spent here: " + pursuer.timeSpentHere + " Relocate Timer: " + abstractTeleportTimer + " TARGETROOM: " + currentTargetRoom?.name);


        //CHECK IF WE ARE DEAD OR IN A ROOM WE SHOULDN'T BE AND UHHH DON'T DESPAWN US UNLESS WE ARE OFFSCREEN?
        if ((pursuer.state.dead || region != pursuer.world.region.name || (pursuer.Room.shelter && pursuer.Room.realizedRoom != null && pursuer.Room.realizedRoom.shelterDoor.IsClosing)) && pursuer.realizedCreature?.room == null)
        {
            //REMOVE THE CORPSE (AND THE PURSUER REFERENCE) SO WE CAN BEGIN THE PROCESS OF RESPAWNING
            pursuer.Destroy();
            pursuer = null;
            abstractTeleportTimer = 0;
            if (DEBUG_MESSAGES) Debug.Log("DEBUG: Pursuer Removed...");
            return;
        }
        //CHECK IF WE'VE GOTTEN STUCK IN A ROOM OR ARE DUE TO TELEPORT 
        if ((pursuer.realizedCreature == null && abstractTeleportTimer > abstractRelocateTime && targetPlayer?.realizedCreature != null && targetPlayer.realizedCreature.room.ValidTrackRoom()) || regionSwitchCooldown <= 0)
        {
            BeginSummon();
        }
    }

    //LETS DO THE ONE THAT SPAWNS RIGHT ON TOP OF US
    public void BeginSummon()
    {
        UpdateTarget();
        if (targetPlayer?.realizedCreature?.room != null)
        {
            RecreatePursuer();
			abstractTeleportTimer = 0;
        }
    }
}