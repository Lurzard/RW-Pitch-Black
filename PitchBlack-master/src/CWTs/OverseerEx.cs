using System;
using RWCustom;
using UnityEngine;
using static PitchBlack.Plugin;
using ChatlogID = MoreSlugcats.ChatlogData.ChatlogID;
using DataPearlType = DataPearl.AbstractDataPearl.DataPearlType;
using System.Linq;

namespace PitchBlack;

public class OverseerEx
{
    // Actually used to find a file of matching id. Name of the variable does not matter, but the "value" param of the constructor does.
    internal static ChatlogID chatlogIDTest = new("test");

    // CONVERSATION ID
    internal static ChatlogID PB_CC = new("PB_CC");
    internal static ChatlogID PB_DS = new("PB_DS");
    internal static ChatlogID PB_GW = new("PB_GW");
    internal static ChatlogID PB_HI = new("PB_HI");
    internal static ChatlogID PB_LF_bottom = new("PB_LF_bottom");
    internal static ChatlogID PB_LF_west = new("PB_LF_west");
    internal static ChatlogID PB_SB_filtration = new("PB_SB_filtration");
    internal static ChatlogID PB_SH = new("PB_SH");
    internal static ChatlogID PB_SI_top = new("PB_SI_top");
    internal static ChatlogID PB_SI_west = new("PB_SI_west");
    internal static ChatlogID PB_SL_bridge = new("PB_SL_bridge");
    internal static ChatlogID PB_SL_chimney = new("PB_SL_chimney");
    internal static ChatlogID PB_SL_moon = new("PB_SL_moon");
    internal static ChatlogID PB_SU = new("PB_SU");
    internal static ChatlogID PB_SU_filt = new("PB_SU_filt");
    internal static ChatlogID PB_UW = new("PB_UW");
    internal static ChatlogID PB_Devcom_1 = new("PB_Devcom_1");
    internal static ChatlogID PB_Devcom_2 = new("PB_Devcom_2");
    internal static ChatlogID PB_Devcom_3 = new("PB_Devcom_3");
    internal static ChatlogID PB_Devcom_4 = new("PB_Devcom_4");
    internal static ChatlogID PB_Devcom_5 = new("PB_Devcom_5");
    internal static ChatlogID PB_Devcom_6 = new("PB_Devcom_6");
    internal static ChatlogID PB_Devcom_7 = new("PB_Devcom_7");
    internal static ChatlogID PB_Devcom_8 = new("PB_Devcom_8");
    internal static ChatlogID PB_Devcom_9 = new("PB_Devcom_9");
    internal static ChatlogID PB_Devcom_10 = new("PB_Devcom_10");
    internal static ChatlogID PB_Devcom_11 = new("PB_Devcom_11");
    internal static ChatlogID PB_Devcom_12 = new("PB_Devcom_12");
    internal static ChatlogID PB_Devcom_13 = new("PB_Devcom_13");
    internal static ChatlogID PB_Devcom_14 = new("PB_Devcom_14");
    internal static ChatlogID PB_Devcom_15 = new("PB_Devcom_15");
    internal static ChatlogID PB_Techy = new("PB_Techy");
    internal static ChatlogID PB_SeerCarcass87 = new(nameof(PB_SeerCarcass87), true);
    internal static ChatlogID PB_SeerCarcass0 = new(nameof(PB_SeerCarcass0), true);
    internal static ChatlogID PB_SeerCarcass1 = new(nameof(PB_SeerCarcass1), true);
    internal static ChatlogID PB_SeerCarcass2 = new(nameof(PB_SeerCarcass2), true);
    internal static ChatlogID PB_SeerCarcass3 = new(nameof(PB_SeerCarcass3), true);
    internal static ChatlogID PB_SeerCarcass4 = new(nameof(PB_SeerCarcass4), true);
    internal static ChatlogID PB_SeerCarcass5 = new(nameof(PB_SeerCarcass5), true);

    // DECLARED MODDED DATAPEARLS
    static readonly DataPearlType dataPearlTypeTest = new("test");
    static readonly DataPearlType PB_Devcom_pearl_1 = new("PB_Developer_Commentary_Pearl_1");
    static readonly DataPearlType PB_Devcom_pearl_2 = new("PB_Developer_Commentary_Pearl_2");
    static readonly DataPearlType PB_Devcom_pearl_3 = new("PB_Developer_Commentary_Pearl_3");
    static readonly DataPearlType PB_Devcom_pearl_4 = new("PB_Developer_Commentary_Pearl_4");
    static readonly DataPearlType PB_Devcom_pearl_5 = new("PB_Developer_Commentary_Pearl_5");
    static readonly DataPearlType PB_Devcom_pearl_6 = new("PB_Developer_Commentary_Pearl_6");
    static readonly DataPearlType PB_Devcom_pearl_7 = new("PB_Developer_Commentary_Pearl_7");
    static readonly DataPearlType PB_Devcom_pearl_8 = new("PB_Developer_Commentary_Pearl_8");
    static readonly DataPearlType PB_Devcom_pearl_9 = new("PB_Developer_Commentary_Pearl_9");
    static readonly DataPearlType PB_Devcom_pearl_10 = new("PB_Developer_Commentary_Pearl_10");
    static readonly DataPearlType PB_Devcom_pearl_11 = new("PB_Developer_Commentary_Pearl_11");
    static readonly DataPearlType PB_Devcom_pearl_12 = new("PB_Developer_Commentary_Pearl_12");
    static readonly DataPearlType PB_Devcom_pearl_13 = new("PB_Developer_Commentary_Pearl_13");
    static readonly DataPearlType PB_Devcom_pearl_14 = new("PB_Developer_Commentary_Pearl_14");
    static readonly DataPearlType PB_Devcom_pearl_15 = new("PB_Developer_Commentary_Pearl_15");
    static readonly DataPearlType PB_Techy_Pearl = new("PB_Techy_Pearl");
    static readonly Type[] compatibleReadTypes = new Type[] { typeof(DataPearl), typeof(OverseerCarcass) };
    internal WeakReference<Overseer> _Overseer;
    internal WeakReference<PlayerCarryableItem> TargetObject;
    internal WeakReference<Player> TargetPlayer;
    PearlPointer pearlCursor;
    Vector2 objectHoverPos;
    bool hasObjectTarget;
    bool readingObject;
    public OverseerEx(Overseer overseer)
    {
        _Overseer = new WeakReference<Overseer>(overseer);
        TargetObject = new WeakReference<PlayerCarryableItem>(null);
        TargetPlayer = new WeakReference<Player>(null);
        hasObjectTarget = false;
        readingObject = false;
    }
    public void Update()
    {
        if (!_Overseer.TryGetTarget(out Overseer overseer)) { return; }
        if (overseer.room == null) { Debug.Log($"Pitch Black OverseerEx {nameof(Update)}: room was null"); return; }
        if (overseer.room.game.session is not StoryGameSession
            || (overseer.room.game.session is StoryGameSession gameSession
            && gameSession.saveStateNumber != PBEnums.SlugcatStatsName.Beacon 
            && gameSession.saveStateNumber != PBEnums.SlugcatStatsName.Photomaniac)) { Debug.Log($"Pitch Black {nameof(Update)}: Not a correct StoryGameSession"); return; }
        if (!overseer.PlayerGuide) { return; }

        // These two might be necessary to keep the overseer from running away from the player? idk but keep them just in case
        overseer.AI.scaredDistance = -10f;
        overseer.AI.avoidPositions.Clear();

        if (hasObjectTarget) { goto Escape; }

        foreach (Player player in overseer.room.PlayersInRoom)
        {
            foreach (Creature.Grasp grasp in player.grasps)
            {
                // The Input.GetKey() here is solely for testing, will change to some other trigger once I get everything working
                // It was changed to holding pickup and up at the same time.
                // Also potentially ignore any Misc Pearls, is the commented out part of the if here
                if (grasp != null && CheckObjectTypeCompatability(grasp.grabbed) && (overseer.mode == Overseer.Mode.Watching || overseer.mode == Overseer.Mode.SittingInWall) && ((player.input[0].y > 0 && player.input[0].pckp) || InputChecker.CheckInput(1)) /*&& Input.GetKey(KeyCode.Y)&& (pearl.AbstractPearl.dataPearlType != DataPearlType.Misc || pearl.AbstractPearl.dataPearlType != DataPearlType.Misc2)*/)
                {
                    // Debug.Log("Pitch Black: Found pearl");
                    Vector2 potentialObjectHoverPos = (Custom.DirVec(overseer.mainBodyChunk.pos, player.mainBodyChunk.pos) * 90f) + overseer.mainBodyChunk.pos;
                    overseer.abstractCreature.abstractAI.destination = overseer.room.GetWorldCoordinate(player.bodyChunks[0].pos);

                    // Debug.Log($"Pitch Black: {!overseer.room.GetTile(potentialPearlHoverPos).Solid} and target pos: {potentialPearlHoverPos}");
                    if (Custom.DistLess(overseer.bodyChunks[0].pos, player.mainBodyChunk.pos, 500f) /*&& overseer.room.RayTraceTilesForTerrain((int)potentialPearlHoverPos.x, (int)potentialPearlHoverPos.y, (int)player.mainBodyChunk.pos.x, (int)player.mainBodyChunk.pos.y)*/ && !overseer.room.GetTile(potentialObjectHoverPos).Solid)
                    {
                        TargetObject = new WeakReference<PlayerCarryableItem>(grasp.grabbed as PlayerCarryableItem);
                        TargetPlayer = new WeakReference<Player>(player);
                        pearlCursor = new PearlPointer(grasp.grabbed.firstChunk.pos);
                        overseer.room.AddObject(pearlCursor);
                        grasp.Release();
                        hasObjectTarget = true;
                        objectHoverPos = potentialObjectHoverPos;
                        goto Escape;
                    }
                }
                // If the player is holding a compatible item in their grasp, follow them.
                else if (grasp != null && CheckObjectTypeCompatability(grasp.grabbed))
                {
                    overseer.abstractCreature.abstractAI.SetDestinationNoPathing(overseer.room.ToWorldCoordinate(player.mainBodyChunk.pos), false);
                    overseer.abstractCreature.abstractAI.MoveWithCreature(player.abstractCreature, false);
                }
            }
        }
    Escape:
        // If the target object is not set, return from here. Prevents anything below from running if the above code did not find a target object, or it is too far away to read.
        if (!TargetObject.TryGetTarget(out PlayerCarryableItem playerCarryableObj)) { return; }

        (overseer.abstractCreature.abstractAI as OverseerAbstractAI).freezeStandardRoamingOnTheseFrames = 40;
        playerCarryableObj.gravity = 0f;
        overseer.AI.lookAt = playerCarryableObj.firstChunk.pos;
        // Pearl's positioning/going to overseer is here, more animation can possibly be added, but this works.
        playerCarryableObj.firstChunk.vel += Custom.DirVec(playerCarryableObj.firstChunk.pos, objectHoverPos);
        playerCarryableObj.firstChunk.vel *= playerCarryableObj.airFriction * 0.9f;
        pearlCursor.lastPos = pearlCursor.pos;
        pearlCursor.pos = playerCarryableObj.firstChunk.pos;
        // Debug.Log($"Pitch Black: dist: {Vector2.Distance(overseer.mainBodyChunk.pos, overseer.AI.lookAt)}");
        // Debug.Log($"Pitch Black: magnitude: {porl.firstChunk.vel.magnitude}, dist: {Custom.DistLess(porl.firstChunk.pos, pearlHoverPos, 1f)}, targpos: {pearlHoverPos}");

        if (Custom.DistLess(playerCarryableObj.firstChunk.pos, objectHoverPos, 10f) && playerCarryableObj.firstChunk.vel.magnitude < 5f && !readingObject)
        {
            InteractWithTargetObject(playerCarryableObj);
        }

        foreach (Player player in overseer.room.PlayersInRoom)
        {
            foreach (var grasp in player.grasps)
            {
                if (grasp != null && grasp.grabbed == playerCarryableObj)
                {
                    Debug.Log($"Dropped object because player was carrying it, object was {grasp.grabbed.GetType()}");
                    DropObject();
                }
            }
        }
        /**********************
        Drop the object IF:
        - Object is slated for deletion
        - Overseer is slated for deletion
        - The target player exists, and the player's room isn't the overseer's, the player is dead, or slated for deletion
        - The distance of the object to it's hover goal is less than 1, and the player isn't stunned (Makes it drop the object after reading it)
        **********************/
        if (playerCarryableObj.slatedForDeletetion ||
            overseer.slatedForDeletetion ||
            (TargetPlayer.TryGetTarget(out Player p) && (p.room != overseer.room || p.dead || p.slatedForDeletetion)) ||
            (Custom.DistLess(playerCarryableObj.firstChunk.pos, objectHoverPos, 1f) && TargetPlayer.TryGetTarget(out Player pp) && pp.stun == 0))
        {
            Debug.Log("A condition was met, dropping object");
            DropObject();
        }
    }
    void InteractWithTargetObject(PlayerCarryableItem playerCarryableObj)
    {
        if (!_Overseer.TryGetTarget(out Overseer _)) { return; }
        if (!TargetObject.TryGetTarget(out PlayerCarryableItem _)) { return; }
        if (!TargetPlayer.TryGetTarget(out Player player)) { return; }
        readingObject = true;
        // Branch for reading porls
        if (playerCarryableObj is DataPearl porl)
        {
            Debug.Log("Pitch Black: Reading Pearl");
            ChatlogID id = PearlTypeToChatlogID(porl.AbstractPearl.dataPearlType);
            collectionSaveData[id.value] = true;
            MiscUtils.SaveCollectionData();
            player.InitChatLog(id);
        }
        // Branch for reading overseer eyes
        else if (playerCarryableObj is OverseerCarcass carcass)
        {
            Debug.Log($"Scanning a carcass, owner ID is {carcass.AbstrCarcass.ownerIterator}");
            player.InitChatLog(CarcassToChatlogID(carcass.AbstrCarcass.ownerIterator));
        }
    }
    public void DropObject()
    {
        hasObjectTarget = false;
        readingObject = false;
        pearlCursor.Destroy();
        pearlCursor = null;
        if (_Overseer.TryGetTarget(out Overseer overseer))
        {
            (overseer.abstractCreature.abstractAI as OverseerAbstractAI).freezeStandardRoamingOnTheseFrames = 0;
        }
        if (TargetObject.TryGetTarget(out PlayerCarryableItem obj))
        {
            obj.gravity = 1f;
            TargetObject = new WeakReference<PlayerCarryableItem>(null);
        }
        if (TargetPlayer.TryGetTarget(out Player _))
        {
            TargetPlayer = new WeakReference<Player>(null);
        }
    }
    private static bool CheckObjectTypeCompatability(PhysicalObject obj)
    {
        return compatibleReadTypes.Select(x => obj.GetType() == x).Contains(true);
    }
    // The number passed from the abstract overseer carcass' owner determines which dialogue is returned, with a default of the first normal chatlog broadcast.
    static ChatlogID CarcassToChatlogID(int ownerID)
    {
        if (ownerID == 87) { return PB_SeerCarcass87; }
        if (ownerID == 0) { return PB_SeerCarcass0; }
        if (ownerID == 1) { return PB_SeerCarcass1; }
        if (ownerID == 2) { return PB_SeerCarcass2; }
        if (ownerID == 3) { return PB_SeerCarcass3; }
        if (ownerID == 4) { return PB_SeerCarcass4; }
        if (ownerID == 5) { return PB_SeerCarcass5; }
        // Default if nothing else matches
        return ChatlogID.Chatlog_Broadcast0;
    }
    static ChatlogID PearlTypeToChatlogID(DataPearlType type)
    {
        if (type == dataPearlTypeTest) { return chatlogIDTest; }
        if (type == DataPearlType.CC) { return PB_CC; }
        if (type == DataPearlType.DS) { return PB_DS; }
        if (type == DataPearlType.GW) { return PB_GW; }
        if (type == DataPearlType.HI) { return PB_HI; }
        if (type == DataPearlType.LF_bottom) { return PB_LF_bottom; }
        if (type == DataPearlType.LF_west) { return PB_LF_west; }
        if (type == DataPearlType.SB_filtration) { return PB_SB_filtration; }
        if (type == DataPearlType.SH) { return PB_SH; }
        if (type == DataPearlType.SI_top) { return PB_SI_top; }
        if (type == DataPearlType.SI_west) { return PB_SI_west; }
        if (type == DataPearlType.SL_bridge) { return PB_SL_bridge; }
        if (type == DataPearlType.SL_chimney) { return PB_SL_chimney; }
        if (type == DataPearlType.SL_moon) { return PB_SL_moon; }
        if (type == DataPearlType.SU) { return PB_SU; }
        if (type == DataPearlType.UW) { return PB_UW; }

        if (type == PB_Devcom_pearl_1) { return PB_Devcom_1; }
        if (type == PB_Devcom_pearl_2) { return PB_Devcom_2; }
        if (type == PB_Devcom_pearl_3) { return PB_Devcom_3; }
        if (type == PB_Devcom_pearl_4) { return PB_Devcom_4; }
        if (type == PB_Devcom_pearl_5) { return PB_Devcom_5; }
        if (type == PB_Devcom_pearl_6) { return PB_Devcom_6; }
        if (type == PB_Devcom_pearl_7) { return PB_Devcom_7; }
        if (type == PB_Devcom_pearl_8) { return PB_Devcom_8; }
        if (type == PB_Devcom_pearl_9) { return PB_Devcom_9; }
        if (type == PB_Devcom_pearl_10) { return PB_Devcom_10; }
        if (type == PB_Devcom_pearl_11) { return PB_Devcom_11; }
        if (type == PB_Devcom_pearl_12) { return PB_Devcom_12; }
        if (type == PB_Devcom_pearl_13) { return PB_Devcom_13; }
        if (type == PB_Devcom_pearl_14) { return PB_Devcom_14; }
        if (type == PB_Devcom_pearl_15) { return PB_Devcom_15; }

        if (type == PB_Techy_Pearl) { return PB_Techy; }

        return new(Conversation.DataPearlToConversation(type).value);
    }
}
