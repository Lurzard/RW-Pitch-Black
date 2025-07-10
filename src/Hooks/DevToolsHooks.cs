using System;
using DevInterface;
using UnityEngine;

namespace PitchBlack;

public static class DevToolsHooks
{
    /// <summary>
    /// Effects and such need to be added to the 3 hooks
    /// - Room.Loaded to add the object
    /// - Room.NowViewed for backgrounds to apply a fix
    /// - RoomSettingsPage.DevEffectGetCategoryFromEffectType for adding effect to Pitch-Black page in Devtools Effects
    /// </summary> -Lur
    
    public static void Apply()
    {
        On.Room.NowViewed += Room_NowViewed;
        On.Room.Loaded += Room_Loaded;
        On.DevInterface.RoomSettingsPage.DevEffectGetCategoryFromEffectType += RoomSettingsPage_DevEffectGetCategoryFromEffectType;
    }
    
    // Actually adds our effects' objects (because I didn't make them utilize POM yet) -Lur
    private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        orig(self);
        for (int num = 0; num < self.roomSettings.effects.Count; num++)
        {
            // ElsehowView
            if (self.roomSettings.effects[num].type == Enums.RoomEffectType.ElsehowView)
            {
                self.AddObject(new ElsehowView(self, self.roomSettings.effects[num]));
            }
        }
    }
    
    private static RoomSettingsPage.DevEffectsCategories RoomSettingsPage_DevEffectGetCategoryFromEffectType(On.DevInterface.RoomSettingsPage.orig_DevEffectGetCategoryFromEffectType orig, RoomSettingsPage self, RoomSettings.RoomEffect.Type type)
    {
        RoomSettingsPage.DevEffectsCategories res = orig(self, type);
        if (type == Enums.RoomEffectType.ElsehowView)
        {
            res = Enums.RoomEffectType.PitchBlackCatagory;
        }
        return res;
    }
    
    // Background shader fix, seems mandatory.
    private static void Room_NowViewed(On.Room.orig_NowViewed orig, Room self)
    {
        orig(self);
        for (int i = 0; i < self.roomSettings.effects.Count; i++)
        {
            if (self.roomSettings.effects[i].type == Enums.RoomEffectType.ElsehowView)
            {
                Shader.SetGlobalFloat(RainWorld.ShadPropRimFix, 1f);
            }
        }
    }
    
    
}