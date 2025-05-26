using System;
using RWCustom;
using MonoMod.Cil;
using UnityEngine;
using Mono.Cecil.Cil;
using static PitchBlack.Plugin;
using System.Runtime.CompilerServices;

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
    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu) {
        orig(self, eu);
        // Moon note: This code will probably make bacons in Jolly Coop, that are in different rooms, fight over the overseer. Could make a fix but only want to if it becomes a problem
        if (self.room?.world?.overseersWorldAI?.playerGuide != null && self.room.game.session is StoryGameSession session && MiscUtils.IsBeaconOrPhoto(session.saveStateNumber) && self.slugcatStats.name == PBEnums.SlugcatStatsName.Beacon) {
            AbstractCreature overseerGuide = self.room.world.overseersWorldAI.playerGuide;
            if (overseerGuide.Room.name == self.abstractCreature.Room.name) { /*Debug.Log("PB: Overseer was in the same room");*/ return; }
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
            cursor.EmitDelegate((float val, Overseer self) => { if (self.room.game.session is StoryGameSession session && (session.saveStateNumber == PBEnums.SlugcatStatsName.Beacon || session.saveStateNumber == PBEnums.SlugcatStatsName.Beacon)) { return -94f; } return val; });

            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdcR4(out var _))) {
                Plugin.logger.LogDebug("MOD: IL hook failed 2");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((float val, Overseer self) => { if (self.room.game.session is StoryGameSession session && (session.saveStateNumber == PBEnums.SlugcatStatsName.Beacon || session.saveStateNumber == PBEnums.SlugcatStatsName.Beacon)) { return 1700f;} return val;  });

            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdcR4(out var _))) {
                Plugin.logger.LogDebug("MOD: IL hook failed 3");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((float val, Overseer self) => { if (self.room.game.session is StoryGameSession session && (session.saveStateNumber == PBEnums.SlugcatStatsName.Beacon || session.saveStateNumber == PBEnums.SlugcatStatsName.Beacon)) { return -0.2f;} return val;  });

            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdcR4(out var _))) {
                Plugin.logger.LogDebug("MOD: IL hook failed 4");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((float val, Overseer self) => { if (self.room.game.session is StoryGameSession session && (session.saveStateNumber == PBEnums.SlugcatStatsName.Beacon || session.saveStateNumber == PBEnums.SlugcatStatsName.Beacon)) { return 0.4f; } return val; });
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
    public static void Conversation_InitalizePrefixColor(On.Conversation.orig_InitalizePrefixColor orig)
    {
        orig();
        Conversation.PrefixColors.Add("??:", Custom.hexToColor("f02961"));
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
        sLeaser.sprites[2].color = Custom.hexToColor("f02961");
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