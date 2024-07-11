#if PLAYTEST
using System;
using RWCustom;
using System.IO;
using MonoMod.Cil;
using UnityEngine;
using MoreSlugcats;
using Mono.Cecil.Cil;
using static PitchBlack.Plugin;
using System.Runtime.CompilerServices;
using ChatlogID = MoreSlugcats.ChatlogData.ChatlogID;
using DataPearlType = DataPearl.AbstractDataPearl.DataPearlType;
using DataPeralTypeMSC = MoreSlugcats.MoreSlugcatsEnums.DataPearlType;
using System.Collections.Generic;
using System.Linq;

namespace PitchBlack;

public class OverseerHooks
{
    internal static ConditionalWeakTable<AbstractCreature, OverseerEx> OverseerPorlStuff = new ConditionalWeakTable<AbstractCreature, OverseerEx>();
    public static void Apply() {
        On.Player.Update += Player_Update;
        On.Overseer.ctor += Overseer_ctor;
        On.Overseer.Update += Overseer_Update;
        On.Overseer.SwitchModes += Overseer_SwitchModes;
        On.OverseerAI.HoverScoreOfTile += OverseerAI_HoverScoreOfTile;
        // On.MoreSlugcats.ChatlogData.getChatlog_ChatlogID += ChatlogData_getChatLog_id;
        On.Conversation.InitalizePrefixColor += Conversation_InitalizePrefixColor;
        IL.Overseer.Update += IL_Overseer_Update;
    }
    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        // Moon note: This code will probably make bacons in Jolly Coop, that are in different rooms, fight over the overseer. Could make a fix but only want to if it becomes a problem
        if (self.room?.world?.overseersWorldAI?.playerGuide != null && self.room.game.session is StoryGameSession session && MiscUtils.IsBeaconOrPhoto(session.saveStateNumber) && self.slugcatStats.name == BeaconName) {
            AbstractCreature overseerGuide = self.room.world.overseersWorldAI.playerGuide;
            if (overseerGuide.Room.name == self.abstractCreature.Room.name) { /*Debug.Log("PB: Overseer was in the same room")*/; return; }
            if (overseerGuide.realizedCreature != null) {
                overseerGuide.realizedCreature.NewRoom(self.room);
            }
            else {
                overseerGuide.Abstractize(self.room.GetWorldCoordinate(self.mainBodyChunk.pos));
            }
            // Debug.Log($"PB: overseer room now: {overseerGuide.Room.name}");
            // overseerGuide.ChangeRooms(self.room.GetWorldCoordinate(self.mainBodyChunk.pos));
        }
    }
    public static void Overseer_ctor(On.Overseer.orig_ctor orig, Overseer self, AbstractCreature abstractCreature, World world) {
        orig(self, abstractCreature, world);
        if (!self.PlayerGuide) { return; }
        if (!OverseerPorlStuff.TryGetValue(self.abstractCreature, out OverseerEx overseerExt)) {
            OverseerPorlStuff.Add(self.abstractCreature, new OverseerEx(self));
        }
        else {
            overseerExt._Overseer = new WeakReference<Overseer>(self);
        }
    }
    public static void Overseer_Update(On.Overseer.orig_Update orig, Overseer self, bool eu) {
        orig(self, eu);
        if (OverseerPorlStuff.TryGetValue(self.abstractCreature, out OverseerEx overseerPorlStuff)) {
            overseerPorlStuff.Update();
        }
    }
    public static void Overseer_SwitchModes(On.Overseer.orig_SwitchModes orig, Overseer self, Overseer.Mode newMode) {
        if (OverseerPorlStuff.TryGetValue(self.abstractCreature, out OverseerEx overseer) && overseer.TargetObject.TryGetTarget(out PlayerCarryableItem _)) {
            newMode = Overseer.Mode.SittingInWall;
        }
        orig(self, newMode);
    }
    public static float OverseerAI_HoverScoreOfTile(On.OverseerAI.orig_HoverScoreOfTile orig, OverseerAI self, IntVector2 testTile)
    {
        if (OverseerPorlStuff.TryGetValue(self.overseer.abstractCreature, out OverseerEx overseer) && overseer.TargetObject.TryGetTarget(out PlayerCarryableItem _) && testTile == self.tempHoverTile) {
            // Make other tiles soooo unappeasing the Overseer doesn't even attempt to warp to any.
                // See OverseerAI.UpdateTempHoverPosition() for where this method is used and see why it's -1000f
            return -1000f;
        }
        return orig(self, testTile);
    }
    public static void IL_Overseer_Update(ILContext il) {
        try {
            // This stuff just makes the overseer not do twitchy behavior stuff
            var cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdfld<OverseerAI>(nameof(OverseerAI.lookAt)))) {
                return;
            }

            if (!cursor.TryGotoNext(MoveType.After,  i => i.MatchLdcR4(out var _))) {
                Plugin.logger.LogDebug("MOD: IL hook failed 1");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((float val, Overseer self) => { if (self.room.game.session is StoryGameSession session && (session.saveStateNumber == BeaconName || session.saveStateNumber == BeaconName)) { return -94f; } return val; });

            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdcR4(out var _))) {
                Plugin.logger.LogDebug("MOD: IL hook failed 2");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((float val, Overseer self) => { if (self.room.game.session is StoryGameSession session && (session.saveStateNumber == BeaconName || session.saveStateNumber == BeaconName)) { return 1700f;} return val;  });

            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdcR4(out var _))) {
                Plugin.logger.LogDebug("MOD: IL hook failed 3");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((float val, Overseer self) => { if (self.room.game.session is StoryGameSession session && (session.saveStateNumber == BeaconName || session.saveStateNumber == BeaconName)) { return -0.2f;} return val;  });

            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdcR4(out var _))) {
                Plugin.logger.LogDebug("MOD: IL hook failed 4");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((float val, Overseer self) => { if (self.room.game.session is StoryGameSession session && (session.saveStateNumber == BeaconName || session.saveStateNumber == BeaconName)) { return 0.4f; } return val; });
            // cursor.Emit(OpCodes.Ldc_R4, 0.4f);
        } catch (Exception err) {
            logger.LogError(err);
        }
    }
    // public static string[] ChatlogData_getChatLog_id(On.MoreSlugcats.ChatlogData.orig_getChatlog_ChatlogID orig, ChatlogID id)
    // {
    //     // Add ids here to skip encryption. It's literally the same as the base method but without putting the ReadAllText results through decryption.
    //     if (id == OverseerEx.chatlogIDTest || id == OverseerEx.PB_CC || id == OverseerEx.PB_DS || id == OverseerEx.PB_GW || id == OverseerEx.PB_HI || id == OverseerEx.PB_LF_bottom || id == OverseerEx.PB_LF_west || id == OverseerEx.PB_MS || id == OverseerEx.PB_RM || id == OverseerEx.PB_SB_filtration || id == OverseerEx.PB_SH || id == OverseerEx.PB_SI_CWdeath || id == OverseerEx.PB_SI_funeral || id == OverseerEx.PB_SI_NSHdeath || id == OverseerEx.PB_SI_SRSdeath || id == OverseerEx.PB_SI_UIdeath || id == OverseerEx.PB_SK_Rod || id == OverseerEx.PB_SL_bridge || id == OverseerEx.PB_SL_chimney || id == OverseerEx.PB_SL_moon || id == OverseerEx.PB_SU || id == OverseerEx.PB_SU_filt || id == OverseerEx.PB_UW || id == OverseerEx.PB_VS) {
    //         string path = ChatlogData.UniquePath(id);
    //         string[] array2;
    //         if (File.Exists(path))
    //         {
    //             string[] array = File.ReadAllText(path).Split(new string[]
    //             {
    //                 "\r\n",
    //                 "\r",
    //                 "\n"
    //             }, StringSplitOptions.None);
    //             array2 = array;
    //         }
    //         else
    //         {
    //             array2 = new string[]
    //             {
    //                 "UNABLE TO ESTABLISH COMMUNICATION"
    //             };
    //         }
    //         return array2;
    //     }
    //     Debug.Log($"Pitch Black: Read pearl ID did not match, ID is {id}");
    //     return orig(id);
    // }
    public static void Conversation_InitalizePrefixColor(On.Conversation.orig_InitalizePrefixColor orig) {
        orig();
        Conversation.PrefixColors.Add("DOB", new Color(126f/255f, 0, 28f/255f));
    }
}
public class OverseerEx
{
    // Actually used to find a file of matching id. Name of the variable does not matter, but the "value" param of the constructor does.
    internal static ChatlogID chatlogIDTest = new("test");
    
    // CONVERSATION ID
    internal static ChatlogID PB_CC = new ("PB_CC");
    internal static ChatlogID PB_DS = new ("PB_DS");
    internal static ChatlogID PB_GW = new ("PB_GW");
    internal static ChatlogID PB_HI = new ("PB_HI");
    internal static ChatlogID PB_LF_bottom = new ("PB_LF_bottom");
    internal static ChatlogID PB_LF_west = new ("PB_LF_west");
    internal static ChatlogID PB_MS = new ("PB_MS");
    internal static ChatlogID PB_RM = new ("PB_RM");
    internal static ChatlogID PB_SB_filtration = new ("PB_SB_filtration");
    internal static ChatlogID PB_SH = new ("PB_SH");
    internal static ChatlogID PB_SI_CWdeath = new ("PB_SI_CWdeath");
    internal static ChatlogID PB_SI_funeral = new ("PB_SI_funeral");
    internal static ChatlogID PB_SI_NSHdeath = new ("PB_SI_NSHdeath");
    internal static ChatlogID PB_SI_SRSdeath = new ("PB_SI_SRSdeath");
    internal static ChatlogID PB_SI_UIdeath = new ("PB_SI_UIdeath");
    internal static ChatlogID PB_SL_bridge = new ("PB_SL_bridge");
    internal static ChatlogID PB_SL_chimney = new ("PB_SL_chimney");
    internal static ChatlogID PB_SL_moon = new ("PB_SL_moon");
    internal static ChatlogID PB_SU = new ("PB_SU");
    internal static ChatlogID PB_SU_filt = new ("PB_SU_filt");
    internal static ChatlogID PB_UW = new ("PB_UW");
    internal static ChatlogID PB_VS = new ("PB_VS");
    internal static ChatlogID PB_SK_Rod = new("PB_SK_Rod");
    internal static ChatlogID PB_Devcom_1 = new ("PB_Devcom_1");
    internal static ChatlogID PB_Devcom_2 = new ("PB_Devcom_2");
    internal static ChatlogID PB_Devcom_3 = new ("PB_Devcom_3");
    internal static ChatlogID PB_Devcom_4 = new ("PB_Devcom_4");
    internal static ChatlogID PB_Devcom_5 = new ("PB_Devcom_5");
    internal static ChatlogID PB_Devcom_6 = new ("PB_Devcom_6");
    internal static ChatlogID PB_Devcom_7 = new ("PB_Devcom_7");
    internal static ChatlogID PB_Devcom_8 = new ("PB_Devcom_8");
    internal static ChatlogID PB_Devcom_9 = new ("PB_Devcom_9");
    internal static ChatlogID PB_Devcom_10 = new ("PB_Devcom_10");
    internal static ChatlogID PB_Devcom_11 = new ("PB_Devcom_11");
    internal static ChatlogID PB_Devcom_12 = new ("PB_Devcom_12");
    internal static ChatlogID PB_Devcom_13 = new ("PB_Devcom_13");
    internal static ChatlogID PB_Devcom_14 = new ("PB_Devcom_14");
    internal static ChatlogID PB_Devcom_15 = new ("PB_Devcom_15");
    internal static ChatlogID PB_Techy = new("PB_Techy");
    internal static ChatlogID PB_SeerCarcass87 = new (nameof(PB_SeerCarcass87), true);
    internal static ChatlogID PB_SeerCarcass0 = new (nameof(PB_SeerCarcass0), true);

    // DECLARED MODDED DATAPEARLS
    static readonly DataPearlType dataPearlTypeTest = new ("test");
    static readonly DataPearlType SK_Rod_Pearl = new ("SK_Rod_Pearl");
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
    static readonly List<Type> compatibleReadTypes = new() { typeof(DataPearl), typeof(OverseerCarcass)};
    internal WeakReference<Overseer> _Overseer;
    internal WeakReference<PlayerCarryableItem> TargetObject;
    internal WeakReference<Player> TargetPlayer;
    PearlPointer pearlCursor;
    Vector2 objectHoverPos;
    bool hasObjectTarget;
    bool readingObject;
    public OverseerEx(Overseer overseer) {
        _Overseer = new WeakReference<Overseer>(overseer);
        TargetObject = new WeakReference<PlayerCarryableItem>(null);
        TargetPlayer = new WeakReference<Player>(null);
        hasObjectTarget = false;
        readingObject = false;
    }
    public void Update() {
        if (!_Overseer.TryGetTarget(out Overseer overseer)) { return; }
        if (overseer.room == null) { Debug.Log($"Pitch Black {nameof(Update)}: room was null"); return; }
        if (overseer.room.game.session is not StoryGameSession || (overseer.room.game.session is StoryGameSession gameSession && gameSession.saveStateNumber != BeaconName && gameSession.saveStateNumber != PhotoName)) { Debug.Log($"Pitch Black {nameof(Update)}: Not a correct StoryGameSession"); return; }
        if (!overseer.PlayerGuide) { return; }

        // These two might be necessary to keep the overseer from running away from the player? idk but keep them just in case
        overseer.AI.scaredDistance = -10f;
        overseer.AI.avoidPositions.Clear();

        if (hasObjectTarget) { goto Escape; }

        foreach (Player player in overseer.room.PlayersInRoom) {
            foreach (Creature.Grasp grasp in player.grasps) {
                // The Input.GetKey() here is solely for testing, will change to some other trigger once I get everything working
                    // It was changed to holding pickup and up at the same time.
                // Also potentially ignore any Misc Pearls, is the commented out part of the if here
                if (grasp != null && compatibleReadTypes.Select(x => grasp.grabbed.GetType() == x).Contains(true) && (overseer.mode == Overseer.Mode.Watching || overseer.mode == Overseer.Mode.SittingInWall) && player.input[0].y > 0 && player.input[0].pckp /*&& Input.GetKey(KeyCode.Y)&& (pearl.AbstractPearl.dataPearlType != DataPearlType.Misc || pearl.AbstractPearl.dataPearlType != DataPearlType.Misc2)*/) {
                    // Debug.Log("Pitch Black: Found pearl");
                    Vector2 potentialObjectHoverPos = (Custom.DirVec(overseer.mainBodyChunk.pos, player.mainBodyChunk.pos) * 90f) + overseer.mainBodyChunk.pos;
                    overseer.abstractCreature.abstractAI.destination = overseer.room.GetWorldCoordinate(player.bodyChunks[0].pos);

                    // Debug.Log($"Pitch Black: {!overseer.room.GetTile(potentialPearlHoverPos).Solid} and target pos: {potentialPearlHoverPos}");
                    if (Custom.DistLess(overseer.bodyChunks[0].pos, player.mainBodyChunk.pos, 500f) /*&& overseer.room.RayTraceTilesForTerrain((int)potentialPearlHoverPos.x, (int)potentialPearlHoverPos.y, (int)player.mainBodyChunk.pos.x, (int)player.mainBodyChunk.pos.y)*/ && !overseer.room.GetTile(potentialObjectHoverPos).Solid) {
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
                else if (grasp != null && compatibleReadTypes.Select(x => grasp.grabbed.GetType() == x).Contains(true))
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

        if (Custom.DistLess(playerCarryableObj.firstChunk.pos, objectHoverPos, 10f) && playerCarryableObj.firstChunk.vel.magnitude < 5f && !readingObject) {
            InteractWithTargetObject(playerCarryableObj);
        }

        foreach (Player player in overseer.room.PlayersInRoom) {
            foreach (var grasp in player.grasps) {
                if (grasp != null && grasp.grabbed == playerCarryableObj) {
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
    void InteractWithTargetObject(PlayerCarryableItem playerCarryableObj) {
        if (!_Overseer.TryGetTarget(out Overseer _)) { return; }
        if (!TargetObject.TryGetTarget(out PlayerCarryableItem _)) { return; }
        if (!TargetPlayer.TryGetTarget(out Player player)) { return; }
        readingObject = true;
        // Branch for reading porls
        if (playerCarryableObj is DataPearl porl) {
            Debug.Log("Pitch Black: Reading Pearl");
            ChatlogID id = PearlTypeToChatlogID(porl.AbstractPearl.dataPearlType);
            collectionSaveData[id.value] = true;
            MiscUtils.SaveCollectionData();
            player.InitChatLog(id);
        }
        // Branch for reading overseer eyes
        else if (playerCarryableObj is OverseerCarcass carcass) {
            Debug.Log($"Scanning a carcass, owner ID is {carcass.AbstrCarcass.ownerIterator}");
            player.InitChatLog(CarcassToChatlogID(carcass.AbstrCarcass.ownerIterator));
        }
    }
    public void DropObject() {
        hasObjectTarget = false;
        readingObject = false;
        pearlCursor.Destroy();
        pearlCursor = null;
        if (_Overseer.TryGetTarget(out Overseer overseer)) {
            (overseer.abstractCreature.abstractAI as OverseerAbstractAI).freezeStandardRoamingOnTheseFrames = 0;
        }
        if (TargetObject.TryGetTarget(out PlayerCarryableItem obj)) {
            obj.gravity = 1f;
            TargetObject = new WeakReference<PlayerCarryableItem>(null);
        }
        if (TargetPlayer.TryGetTarget(out Player _)) {
            TargetPlayer = new WeakReference<Player>(null);
        }
    }
    // The number passed from the abstract overseer carcass' owner determines which dialogue is returned, with a default of the first normal chatlog broadcast.
    static ChatlogID CarcassToChatlogID(int ownerID) {
        if (ownerID == 87) { return PB_SeerCarcass87; }
        if (ownerID == 0) { return PB_SeerCarcass0; }

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
        if (type == DataPeralTypeMSC.MS) { return PB_MS; }
        if (type == DataPearlType.SB_filtration) { return PB_SB_filtration; }
        if (type == DataPearlType.SH) { return PB_SH; }
        if (type == DataPearlType.SI_top) { return PB_SI_CWdeath; }
        if (type == DataPearlType.SI_west) { return PB_SI_funeral; }
        if (type == DataPeralTypeMSC.SI_chat3) { return PB_SI_NSHdeath; }
        if (type == DataPeralTypeMSC.SI_chat4) { return PB_SI_SRSdeath; }
        if (type == DataPeralTypeMSC.SI_chat5) { return PB_SI_UIdeath; }
        if (type == DataPearlType.SL_bridge) { return PB_SL_bridge; }
        if (type == DataPearlType.SL_chimney) { return PB_SL_chimney; }
        if (type == DataPearlType.SL_moon) { return PB_SL_moon; }
        if (type == DataPearlType.SU) { return PB_SU; }
        if (type == DataPearlType.UW) { return PB_UW; }
        if (type == DataPeralTypeMSC.SU_filt) { return PB_SU_filt; }
        if (type == DataPeralTypeMSC.VS) { return PB_VS; }
        if (type == DataPeralTypeMSC.MS) { return PB_MS; }
        if (type == DataPeralTypeMSC.RM) { return PB_RM; }

        if (type == SK_Rod_Pearl) { return PB_SK_Rod; }
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

        return new (Conversation.DataPearlToConversation(type).value);
    }
}
class PearlPointer : CosmeticSprite
{
    float timer;
    public PearlPointer(Vector2 pos) : base()
    {
        this.pos = pos;
        this.lastPos = pos;
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        sLeaser.sprites = new FSprite[3];
        sLeaser.sprites[0] = new FSprite("Futile_White", true);
        sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["FlatLight"];
        sLeaser.sprites[0].color = Color.black;

        sLeaser.sprites[1] = new FSprite("Futile_White", true);
        sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["LightSource"];

        sLeaser.sprites[2] = new FSprite("pearlCursor", true);
        sLeaser.sprites[2].shader = rCam.game.rainWorld.Shaders["Hologram"];
        sLeaser.sprites[2].scale = 1.3f;
        this.AddToContainer(sLeaser, rCam, null);
    }
    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        base.AddToContainer(sLeaser, rCam, newContatiner);
        if (newContatiner == null)
        {
            newContatiner = rCam.ReturnFContainer("Background");
        }
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].RemoveFromContainer();
            newContatiner.AddChild(sLeaser.sprites[i]);
            if (i == 2) {
                sLeaser.sprites[i].MoveToFront();
            }
            else {
                sLeaser.sprites[i].MoveToBack();
            }
        }
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        sLeaser.sprites[0].scale = (150f + Mathf.Lerp(0f, 18f, Mathf.Abs(Mathf.Sin(timer*Mathf.PI)))) / 8f;
        sLeaser.sprites[1].scale = (150f + Mathf.Lerp(0f, 18f, Mathf.Abs(Mathf.Sin(timer*Mathf.PI))) * 2f ) / 8f;
        sLeaser.sprites[2].scale = Mathf.Lerp(1.3f, 1.7f, Mathf.Abs(Mathf.Cos(timer*Mathf.PI)));
        sLeaser.sprites[2].rotation += Mathf.Lerp(2.6f, 1.5f, Mathf.Abs(Mathf.Cos(timer*Mathf.PI)));
        foreach (FSprite sprite in sLeaser.sprites) {
            sprite.SetPosition(Vector2.Lerp(lastPos-camPos, pos-camPos, timeStacker));
        }
        // Couldn't get this to work
        // sLeaser.sprites[2]._localVertices[0] = sLeaser.sprites[2].GetPosition() + Custom.RotateAroundOrigo(Vector2.Lerp(new Vector2(-5f, 5f), new Vector2(-10f, 10f), Mathf.Abs(Mathf.Cos(timer*Mathf.PI))), Mathf.Cos(timer*Mathf.PI));
        // sLeaser.sprites[2]._localVertices[1] = sLeaser.sprites[2].GetPosition() + Custom.RotateAroundOrigo(Vector2.Lerp(new Vector2(-5f, 5f), new Vector2(-10f, 10f), Mathf.Abs(Mathf.Cos(timer*Mathf.PI))), Mathf.Cos((timer + 0.5f)*Mathf.PI));
        // sLeaser.sprites[2]._localVertices[2] = sLeaser.sprites[2].GetPosition() + Custom.RotateAroundOrigo(Vector2.Lerp(new Vector2(-5f, 5f), new Vector2(-10f, 10f), Mathf.Abs(Mathf.Cos(timer*Mathf.PI))), Mathf.Cos((timer + 1f)*Mathf.PI));
        // sLeaser.sprites[2]._localVertices[3] = sLeaser.sprites[2].GetPosition() + Custom.RotateAroundOrigo(Vector2.Lerp(new Vector2(-5f, 5f), new Vector2(-10f, 10f), Mathf.Abs(Mathf.Cos(timer*Mathf.PI))), Mathf.Cos((timer + 1.5f)*Mathf.PI));
        if (slatedForDeletetion) {
            sLeaser.CleanSpritesAndRemove();
        }
    }
    public override void Update(bool eu)
    {
        base.Update(eu);
        timer += 1f/160f;
    }
}
#endif