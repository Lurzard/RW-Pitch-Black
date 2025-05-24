using System;
using DevInterface;
using UnityEngine;

namespace PitchBlack;

public class DevHooks
{
    public static bool RoseSky;
    public static void Apply()
    {
        // For dev effects
        On.Room.Loaded += Room_Loaded;
        On.Room.NowViewed += Room_NowViewed;
        On.DevInterface.RoomSettingsPage.DevEffectGetCategoryFromEffectType += RoomSettingsPage_DevEffectGetCategoryFromEffectType;
        // For RippleMelt
        On.RoomCamera.ApplyPalette += ApplyPalette;
        On.DevInterface.EffectPanel.EffectPanelSlider.NubDragged += EffectPanelSlider_NubDragged;
        On.RainWorldGame.RawUpdate += RainWorldGame_RawUpdate;
        On.RoomCamera.DrawUpdate += RoomCamera_DrawUpdate;
        On.RoomCamera.MoveCamera_Room_int += RoomCamera_MoveCamera_Room_int;
        //On.AboveCloudsView.ctor += AboveCloudsView_ctor;
    }
    
    // Gonna use a RK bg instead -Lur
    //private static void AboveCloudsView_ctor(On.AboveCloudsView.orig_ctor orig, AboveCloudsView self, Room room, RoomSettings.RoomEffect effect)
    //{
    //    orig(self, room, effect);
    //    RoseSky = (effect.type == PBRoomEffectType.RoseSky);
    //    if (RoseSky)
    //    {
    //        self.atmosphereColor = new Color(0.219f, 0.098f, 0.137f);
    //        self.daySky = new BackgroundScene.Simple2DBackgroundIllustration(self, "Rose_Sky", new Vector2(683f, 384f));
    //        self.duskSky = new BackgroundScene.Simple2DBackgroundIllustration(self, "Rose_Sky", new Vector2(683f, 384f));
    //        self.nightSky = new BackgroundScene.Simple2DBackgroundIllustration(self, "Rose_Sky", new Vector2(683f, 384f));
    //    }
    //}

    private static void RoomCamera_MoveCamera_Room_int(On.RoomCamera.orig_MoveCamera_Room_int orig, RoomCamera self, Room room, int camPos)
    {
        orig(self, room, camPos);
        if (room.roomSettings.GetEffectAmount(PBRoomEffectType.RippleMelt) > 0f)
        {
            self.levelGraphic.shader = self.game.rainWorld.Shaders["LevelMelt"];
            self.levelGraphic.alpha = room.roomSettings.GetEffectAmount(PBRoomEffectType.RippleMelt);
        }
    }
    private static void ApplyPalette(On.RoomCamera.orig_ApplyPalette orig, RoomCamera self)
    {
        orig(self);
        if (self.room != null && self.room.roomSettings.GetEffectAmount(PBRoomEffectType.RippleMelt) > 0f)
        {
            self.SetUpFullScreenEffect("Bloom");
            self.fullScreenEffect.shader = self.game.rainWorld.Shaders["LevelMelt2"];
            self.lightBloomAlphaEffect = PBRoomEffectType.RippleMelt;
            self.fullScreenEffect.alpha = self.room.roomSettings.GetEffectAmount(PBRoomEffectType.RippleMelt);
            return;
        }
    }
    private static void RoomCamera_DrawUpdate(On.RoomCamera.orig_DrawUpdate orig, RoomCamera self, float timeStacker, float timeSpeed)
    {
        orig(self, timeStacker, timeSpeed);
        Vector2 vector = Vector2.Lerp(self.lastPos, self.pos, timeStacker);
        vector = new Vector2(Mathf.Floor(vector.x), Mathf.Floor(vector.y));
        if (self.room != null)
        {
            if (self.lightBloomAlphaEffect == PBRoomEffectType.RippleMelt && self.fullScreenEffect != null)
                if (self.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidSea) > 0f)
                {
                    self.lightBloomAlpha *= self.voidSeaGoldFilter;
                    self.fullScreenEffect.color = new Color(Mathf.InverseLerp(-1200f, -6000f, vector.y) * Mathf.InverseLerp(0.9f, 0f, self.screenShake), 0f, 0f);
                    self.fullScreenEffect.isVisible = self.lightBloomAlpha > 0f;
                }
                else
                {
                    self.fullScreenEffect.color = new Color(0f, 0f, 0f);
                }
        }
    }
    private static void RainWorldGame_RawUpdate(On.RainWorldGame.orig_RawUpdate orig, RainWorldGame self, float dt)
    {
        orig(self, dt);
        float num = 0f;
        bool flag = false;
        float num2 = flag ? Mathf.Lerp(self.framesPerSecond, 8f, num) : Mathf.Lerp(self.framesPerSecond, 15f, num);
        for (int j = 0; j < self.session.Players.Count; j++)
        {
            if (self.session.Players[j].realizedCreature != null && self.session.Players[j].realizedCreature.room != null && self.session.Players[j].realizedCreature.room.roomSettings.GetEffectAmount(PBRoomEffectType.RippleMelt) > 0f)
            {
                num2 = Math.Min(num2, Mathf.Lerp(num2, 15f, self.session.Players[j].realizedCreature.room.roomSettings.GetEffectAmount(PBRoomEffectType.RippleMelt) * Mathf.InverseLerp(-7000f, -2000f, self.session.Players[j].realizedCreature.mainBodyChunk.pos.y)));
            }
        }
    }
    // NOTE: Taken from other Watcher backgrounds, seems related to the building shader from AnicentUrbanBuildings. Only for ElsehowView.
    private static void Room_NowViewed(On.Room.orig_NowViewed orig, Room self)
    {
        orig(self);
        for (int i = 0; i < self.roomSettings.effects.Count; i++)
        {
            if (self.roomSettings.effects[i].type == PBRoomEffectType.ElsehowView
                || self.roomSettings.effects[i].type == PBRoomEffectType.RoseSky)
            {
                Shader.SetGlobalFloat(RainWorld.ShadPropRimFix, 1f);
            }
        }
    }
    private static void EffectPanelSlider_NubDragged(On.DevInterface.EffectPanel.EffectPanelSlider.orig_NubDragged orig, EffectPanel.EffectPanelSlider self, float nubPos)
    {
        orig(self, nubPos);
        if (self.effect.type == PBRoomEffectType.RippleMelt)
        {
            self.owner.room.game.cameras[0].levelGraphic.alpha = self.effect.amount;
            if (self.owner.room.game.cameras[0].fullScreenEffect != null)
            {
                self.owner.room.game.cameras[0].fullScreenEffect.alpha = self.effect.amount;
            }
        }
    }
    // Actually adds our effects' objects (because I didn't make them utilize POM yet) -Lur
    private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        orig(self);
        for (int num = 0; num < self.roomSettings.effects.Count; num++)
        {
            // ElsehowView
            if (self.roomSettings.effects[num].type == PBRoomEffectType.ElsehowView)
            {
                self.AddObject(new ElsehowView(self, self.roomSettings.effects[num]));
            }
            // RippleSpawn
            if (self.roomSettings.effects[num].type == PBRoomEffectType.RippleSpawn)
            {
                self.AddObject(new RippleSpawnKeeper(self, self.roomSettings.effects[num]));
            }
            // RippleMelt
            if (self.roomSettings.effects[num].type == PBRoomEffectType.RippleMelt)
            {
                self.AddObject(new PBMeltLights(self.roomSettings.effects[num], self));
            }
            // EclipseView
            if (self.roomSettings.effects[num].type == PBRoomEffectType.RoseSky)
            {
                self.AddObject(new AboveCloudsView(self, self.roomSettings.effects[num]));
            }
        }
        //for (int num9 = 1; num9 <= 2; num9++)
        // {
        //    int num10 = 0;
        //    if (self.roomSettings.placedObjects[num10].type == PBPlacedObjectType.DreamerSpot && self.game.IsStorySession && self.game.StoryCharacter ==
        //    )
        //     {
        //        GhostWorldPresence ghostWorldPresence = null;
        //        if (ghostWorldPresence == null)
        //        {
        //            ghostWorldPresence = new GhostWorldPresence(self.world, PBGhostID.Dreamer);
        //            ghostWorldPresence.ghostRoom = self.abstractRoom;
        //        }
        //        self.AddObject(new Dreamer(self, self.roomSettings.placedObjects[num10], ghostWorldPresence));
        //    }
        // }
    }
    // Adds to PitchBlack dev effects catagory
    private static RoomSettingsPage.DevEffectsCategories RoomSettingsPage_DevEffectGetCategoryFromEffectType(On.DevInterface.RoomSettingsPage.orig_DevEffectGetCategoryFromEffectType orig, RoomSettingsPage self, RoomSettings.RoomEffect.Type type)
    {
        RoomSettingsPage.DevEffectsCategories res = orig(self, type);
        if (type == PBRoomEffectType.ElsehowView
            || type == PBRoomEffectType.RippleSpawn
            || type == PBRoomEffectType.RippleMelt
            || type == PBRoomEffectType.RoseSky)
        {
            res = PBRoomEffectType.PitchBlackCatagory;
        }
        return res;
    }
}
