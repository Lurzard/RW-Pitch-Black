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

namespace PitchBlack;

public class OverseerEx
{
    internal static ConditionalWeakTable<AbstractCreature, OverseerEx> OverseerPorlStuff = new ConditionalWeakTable<AbstractCreature, OverseerEx>();
    // Actually used to find a file of matching id. Name does not matter, but the "value" param does.
    static ChatlogID chatlogIDTest = new("test");
    static ChatlogID PB_CC = new ("PB_CC");
    static ChatlogID PB_DS = new ("PB_DS");
    static ChatlogID PB_GW = new ("PB_GW");
    static ChatlogID PB_HI = new ("PB_HI");
    static ChatlogID PB_LF_bottom = new ("PB_LF_bottom");
    static ChatlogID PB_LF_west = new ("PB_LF_west");
    static ChatlogID PB_MS = new ("PB_MS");
    static ChatlogID PB_RM = new ("PB_RM");
    static ChatlogID PB_SB_filtration = new ("PB_SB_filtration");
    static ChatlogID PB_SH = new ("PB_SH");
    static ChatlogID PB_SI_CWdeath = new ("PB_SI_CWdeath");
    static ChatlogID PB_SI_funeral = new ("PB_SI_funeral");
    static ChatlogID PB_SI_NSHdeath = new ("PB_SI_NSHdeath");
    static ChatlogID PB_SI_SRSdeath = new ("PB_SI_SRSdeath");
    static ChatlogID PB_SI_UIdeath = new ("PB_SI_UIdeath");
    static ChatlogID PB_SK_Rod = new ("PB_SK_Rod");
    static ChatlogID PB_SL_bridge = new ("PB_SL_bridge");
    static ChatlogID PB_SL_chimney = new ("PB_SL_chimney");
    static ChatlogID PB_SL_moon = new ("PB_SL_moon");
    static ChatlogID PB_SU = new ("PB_SU");
    static ChatlogID PB_SU_filt = new ("PB_SU_filt");
    static ChatlogID PB_UW = new ("PB_UW");
    static ChatlogID PB_VS = new ("PB_VS");
    // Used to test against
    static DataPearlType dataPearlTypeTest = new ("test");
    static DataPearlType PB_SK_Rod_DPT = new ("PB_SK_Rod");
    #region Hooks
    public static void Apply() {
        On.Overseer.ctor += Overseer_ctor;
        On.Overseer.Update += Overseer_Update;
        On.Overseer.SwitchModes += Overseer_SwitchModes;
        On.OverseerAI.HoverScoreOfTile += OverseerAI_HoverScoreOfTile;
        On.MoreSlugcats.ChatlogData.getChatlog_ChatlogID += ChatlogData_getChatLog_id;
        On.Conversation.InitalizePrefixColor += Conversation_InitalizePrefixColor;
        IL.Overseer.Update += IL_Overseer_Update;
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
        if (OverseerPorlStuff.TryGetValue(self.abstractCreature, out OverseerEx overseer) && overseer.TargetPearl.TryGetTarget(out DataPearl _)) {
            newMode = Overseer.Mode.SittingInWall;
        }
        orig(self, newMode);
    }
    public static float OverseerAI_HoverScoreOfTile(On.OverseerAI.orig_HoverScoreOfTile orig, OverseerAI self, IntVector2 testTile)
    {
        if (OverseerPorlStuff.TryGetValue(self.overseer.abstractCreature, out OverseerEx overseer) && overseer.TargetPearl.TryGetTarget(out DataPearl _) && testTile == self.tempHoverTile) {
            // Make other tiles soooo unappeasing the Overseer doesn't even attempt to warp to any.
                // See OverseerAI.UpdateTempHoverPosition() for where this method is used and see why it's -1000f
            return -1000f;
        }
        return orig(self, testTile);
    }
    public static void IL_Overseer_Update(ILContext il) {
        // This stuff just makes the overseer not do twitchy behavior stuff
        var cursor = new ILCursor(il);

        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdfld<OverseerAI>(nameof(OverseerAI.lookAt)))) {
            return;
        }

        if (!cursor.TryGotoNext(MoveType.After,  i => i.MatchLdcR4(out var _))) {
            Plugin.Logger.LogDebug("MOD: IL hook failed 1");
            return;
        }
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((float val, Overseer self) => { if (self.room.game.session is StoryGameSession session && (session.saveStateNumber == BeaconName || session.saveStateNumber == BeaconName)) { return -94f; } return val; });
        cursor.Emit(OpCodes.Ldc_R4);

        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdcR4(out var _))) {
            Plugin.Logger.LogDebug("MOD: IL hook failed 2");
            return;
        }
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((float val, Overseer self) => { if (self.room.game.session is StoryGameSession session && (session.saveStateNumber == BeaconName || session.saveStateNumber == BeaconName)) { return 1700f;} return val;  });
        cursor.Emit(OpCodes.Ldc_R4);

        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdcR4(out var _))) {
            Plugin.Logger.LogDebug("MOD: IL hook failed 3");
            return;
        }
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((float val, Overseer self) => { if (self.room.game.session is StoryGameSession session && (session.saveStateNumber == BeaconName || session.saveStateNumber == BeaconName)) { return -0.2f;} return val;  });
        cursor.Emit(OpCodes.Ldc_R4);

        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdcR4(out var _))) {
            Plugin.Logger.LogDebug("MOD: IL hook failed 4");
            return;
        }
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((float val, Overseer self) => { if (self.room.game.session is StoryGameSession session && (session.saveStateNumber == BeaconName || session.saveStateNumber == BeaconName)) { return 0.4f; } return val; });
        cursor.Emit(OpCodes.Ldc_R4, 0.4f);
    }
    public static string[] ChatlogData_getChatLog_id(On.MoreSlugcats.ChatlogData.orig_getChatlog_ChatlogID orig, ChatlogID id)
    {
        // Add ids here to skip encryption. It's literally the same as the base method but without putting the ReadAllText results through decryption.
        if (id == chatlogIDTest || id == PB_CC || id == PB_DS || id == PB_GW || id == PB_HI || id == PB_LF_bottom || id == PB_LF_west || id == PB_MS || id == PB_RM || id == PB_SB_filtration || id == PB_SH || id == PB_SI_CWdeath || id == PB_SI_funeral || id == PB_SI_NSHdeath || id == PB_SI_SRSdeath || id == PB_SI_UIdeath || id == PB_SK_Rod || id == PB_SL_bridge || id == PB_SL_chimney || id == PB_SL_moon || id == PB_SU || id == PB_SU_filt || id == PB_UW || id == PB_VS) {
            string path = ChatlogData.UniquePath(id);
            string[] array2;
            if (File.Exists(path))
            {
                string[] array = File.ReadAllText(path).Split(new string[]
                {
                    "\r\n",
                    "\r",
                    "\n"
                }, StringSplitOptions.None);
                array2 = array;
            }
            else
            {
                array2 = new string[]
                {
                    "UNABLE TO ESTABLISH COMMUNICATION"
                };
            }
            return array2;
        }
        return orig(id);
    }
    public static void Conversation_InitalizePrefixColor(On.Conversation.orig_InitalizePrefixColor orig) {
        orig();
        Conversation.PrefixColors.Add("DOB", new Color(126f/255f, 0, 28f/255f));
    }
    #endregion
    WeakReference<Overseer> _Overseer;
    WeakReference<DataPearl> TargetPearl;
    WeakReference<Player> TargetPlayer;
    PearlPointer pearlCursor;
    Vector2 pearlHoverPos;
    bool hasPearlTarget;
    bool readingPearl;
    int timer;
    public OverseerEx(Overseer overseer) {
        _Overseer = new WeakReference<Overseer>(overseer);
        TargetPearl = new WeakReference<DataPearl>(null);
        TargetPlayer = new WeakReference<Player>(null);
        hasPearlTarget = false;
        readingPearl = false;
    }
    public void Update() {
        if (!_Overseer.TryGetTarget(out Overseer overseer)) { return; }
        if (overseer.room == null) { Debug.Log("Pitch Black: room was null"); return; }
        if (overseer.room.game.session is not StoryGameSession || (overseer.room.game.session is StoryGameSession gameSession && gameSession.saveStateNumber != BeaconName && gameSession.saveStateNumber != PhotoName)) { Debug.Log("Pitch Black: Not a correct StoryGameSession"); return; }
        if (!overseer.PlayerGuide) { return; }

        // These two might be necessary to keep the overseer from running away from the player? idk but keep them just in case
        overseer.AI.scaredDistance = -10f;
        overseer.AI.avoidPositions.Clear();

        if (hasPearlTarget) { goto Escape; }

        foreach (Player player in overseer.room.PlayersInRoom) {
            foreach (Creature.Grasp grasp in player.grasps) {
                // The Input.GetKey() here is solely for testing, will change to some other trigger once I get everything working
                    // It was changed to holding pickup and up at the same time.
                // Also potentially ignore any Misc Pearls, is the commented out part of the if here
                if (grasp != null && grasp.grabbed is DataPearl pearl && (overseer.mode == Overseer.Mode.Watching || overseer.mode == Overseer.Mode.SittingInWall) && player.input[0].y > 0 && player.input[0].pckp /*&& Input.GetKey(KeyCode.Y)&& (pearl.AbstractPearl.dataPearlType != DataPearlType.Misc || pearl.AbstractPearl.dataPearlType != DataPearlType.Misc2)*/) {
                    // Debug.Log("Pitch Black: Found pearl");
                    Vector2 potentialPearlHoverPos = (Custom.DirVec(overseer.mainBodyChunk.pos, player.mainBodyChunk.pos) * 90f) + overseer.mainBodyChunk.pos;
                    overseer.abstractCreature.abstractAI.destination = overseer.room.GetWorldCoordinate(player.bodyChunks[0].pos);

                    // Debug.Log($"Pitch Black: {!overseer.room.GetTile(potentialPearlHoverPos).Solid} and target pos: {potentialPearlHoverPos}");
                    if (Custom.DistLess(overseer.bodyChunks[0].pos, player.mainBodyChunk.pos, 500f) /*&& overseer.room.RayTraceTilesForTerrain((int)potentialPearlHoverPos.x, (int)potentialPearlHoverPos.y, (int)player.mainBodyChunk.pos.x, (int)player.mainBodyChunk.pos.y)*/ && !overseer.room.GetTile(potentialPearlHoverPos).Solid) {
                        TargetPearl = new WeakReference<DataPearl>(pearl);
                        TargetPlayer = new WeakReference<Player>(player);
                        pearlCursor = new PearlPointer(pearl.firstChunk.pos);
                        overseer.room.AddObject(pearlCursor);
                        grasp.Release();
                        hasPearlTarget = true;
                        pearlHoverPos = potentialPearlHoverPos;
                        goto Escape;
                    }
                }
                else if (grasp != null && grasp.grabbed is DataPearl) 
                {
                    overseer.abstractCreature.abstractAI.SetDestinationNoPathing(overseer.room.ToWorldCoordinate(player.mainBodyChunk.pos), false);
                    overseer.abstractCreature.abstractAI.MoveWithCreature(player.abstractCreature, false);
                }
            }
        }
        Escape:
        // If the pearl is not set, return from here. Prevents anything below from running if the above code did not find a pearl, or it is too far away to read.
        if (!TargetPearl.TryGetTarget(out DataPearl porl)) { return; }

        timer++;
        (overseer.abstractCreature.abstractAI as OverseerAbstractAI).freezeStandardRoamingOnTheseFrames = 40;
        porl.gravity = 0f;
        overseer.AI.lookAt = porl.firstChunk.pos;
        // Pearl's positioning/going to overseer is here, more animation can possibly be added, but this works.
        porl.firstChunk.vel += Custom.DirVec(porl.firstChunk.pos, pearlHoverPos);
        porl.firstChunk.vel *= porl.airFriction * 0.9f;
        pearlCursor.lastPos = pearlCursor.pos;
        pearlCursor.pos = porl.firstChunk.pos;
        // Debug.Log($"Pitch Black: dist: {Vector2.Distance(overseer.mainBodyChunk.pos, overseer.AI.lookAt)}");
        // Debug.Log($"Pitch Black: magnitude: {porl.firstChunk.vel.magnitude}, dist: {Custom.DistLess(porl.firstChunk.pos, pearlHoverPos, 1f)}, targpos: {pearlHoverPos}");
        if (Custom.DistLess(porl.firstChunk.pos, pearlHoverPos, 10f) && porl.firstChunk.vel.magnitude < 5f && !readingPearl) {
            // Debug.Log("Pitch Black: Reading Pearl");
            ReadPorl(porl.AbstractPearl.dataPearlType);
        }
        foreach (Player player in overseer.room.PlayersInRoom) {
            foreach (var grasp in player.grasps) {
                if (grasp != null && grasp.grabbed == porl) {
                    DropPearl();
                }
            }
        }
        if (porl.slatedForDeletetion || overseer.slatedForDeletetion || (TargetPlayer.TryGetTarget(out Player p) && (p.room != overseer.room || p.dead || p.slatedForDeletetion))) {
            DropPearl();
        }
    }
    public void ReadPorl(DataPearlType type) {
        if (!_Overseer.TryGetTarget(out Overseer _)) { return; }
        if (!TargetPearl.TryGetTarget(out DataPearl _)) { return; }
        if (!TargetPlayer.TryGetTarget(out Player player)) { return; }
        readingPearl = true;
        player.InitChatLog(PearlTypeToChatlogID(type));
    }
    public void DropPearl() {
        hasPearlTarget = false;
        readingPearl = false;
        pearlCursor.Destroy();
        pearlCursor = null;
        timer = 0;
        if (_Overseer.TryGetTarget(out Overseer overseer)) {
            (overseer.abstractCreature.abstractAI as OverseerAbstractAI).freezeStandardRoamingOnTheseFrames = 0;
        }
        if (TargetPearl.TryGetTarget(out DataPearl porl)) {
            porl.gravity = 1f;
            TargetPearl = new WeakReference<DataPearl>(null);
        }
        if (TargetPlayer.TryGetTarget(out Player _)) {
            TargetPlayer = new WeakReference<Player>(null);
        }
    }
    private ChatlogID PearlTypeToChatlogID(DataPearlType type)
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
        if (type == PB_SK_Rod_DPT) { return PB_SK_Rod; }
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