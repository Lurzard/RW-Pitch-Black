using System;
using System.Collections.Generic;
using HUD;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Watcher;

namespace PitchBlack;

// TODO:
// Spawning in room (through DreamerSpot object)

public class Dreamer : Ghost
{
    // Used for potential graphics edits
    public bool NightmareDreamer
    {
        get
        {
            return room.world.region.name == "UD";
        }
    }
    public Dreamer(Room room, PlacedObject placedObject, GhostWorldPresence worldGhost) : base(room, placedObject, worldGhost)
    {
    }
    public override void InitializeSprites()
    {
        // Smaller scale and shorter snout
        scale = 0.65f;
        snoutSegments = 7;
        // dont use SpinningTop head
        //totalStaticSprites++;
        base.InitializeSprites();
    }
    public override void Update(bool eu)
    {
        if (conversationFinished)
        {
            RaiseQualiaLevel(room);
        }
        base.Update(eu);
    }
    public override void StartConversation()
    {
        Conversation.ID id = Conversation.ID.None;
        Vector2 vector = NextMinMaxQualiaLevel(room);
        float y = vector.y;
        float x = vector.x;
        // Determine convo by RippleLevel x/y
        if (room.game.cameras[0].hud.dialogBox == null)
        {
            room.game.cameras[0].hud.InitDialogBox();
        }
        // Currently this will just test if they can talk
        currentConversation = new DreamerConversation(id, this, room.game.cameras[0].hud.dialogBox);
        if (id == Conversation.ID.None)
        {
            currentConversation.events.Add(new Conversation.TextEvent(currentConversation, 0, "hullo!", 300));
        }
    }
    public static void RaiseQualiaLevel(Room room)
    {
        Vector2 vector = NextMinMaxQualiaLevel(room);
        (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.minimumRippleLevel = vector.x;
        (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.maximumRippleLevel = vector.y;
        (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.rippleLevel = vector.y;
        room.game.cameras[0].hud.karmaMeter.UpdateGraphic();
        room.game.cameras[0].hud.karmaMeter.forceVisibleCounter = Mathf.Max(room.game.cameras[0].hud.karmaMeter.forceVisibleCounter, 120);
    }
    public static Vector2 NextMinMaxQualiaLevel(Room room)
    {
        float x = 0;
        float num = Mathf.Min(5f, (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.maximumRippleLevel + 0.5f);
        if ((room.game.session as StoryGameSession).saveState.deathPersistentSaveData.maximumRippleLevel >= 5f)
        {
            x = Mathf.Min(5f, (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.minimumRippleLevel + 0.5f);
        }
        else
        {
            x = Mathf.Max(1f, num - 2f);
        }
        return new Vector2(x, num);
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }
    public class DreamerConversation : GhostConversation
    {
        public DreamerConversation(ID id, Ghost ghost, DialogBox dialogBox) : base(id, ghost, dialogBox)
        {
        }
        public override void Update()
        {
            base.Update();
        }
        public override void AddEvents()
        {
            // Conversations
        }
    }
}

