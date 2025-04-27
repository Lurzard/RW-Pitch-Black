using System;
using System.Diagnostics.CodeAnalysis;
using BepInEx.Logging;
using DevInterface;

namespace PitchBlack;

// These will all eventually just be changed to not need to be registered, except Fisobs enums of course.

public static class PBCreatureTemplateType
{
    [AllowNull] public static CreatureTemplate.Type NightTerror = new("NightTerror", true);
    [AllowNull] public static CreatureTemplate.Type LMiniLongLegs = new(nameof(LMiniLongLegs), true);
    [AllowNull] public static CreatureTemplate.Type Rotrat = new(nameof(Rotrat), true);
    [AllowNull] public static CreatureTemplate.Type UmbraScav = new(nameof(UmbraScav), true);
    [AllowNull] public static CreatureTemplate.Type FireGrub = new(nameof(FireGrub), true);
    public static void UnregisterValues()
    {
        if (NightTerror != null)
        {
            NightTerror.Unregister();
            NightTerror = null;
        }
        if (LMiniLongLegs != null)
        {
            LMiniLongLegs.Unregister();
            LMiniLongLegs = null;
        }
        if (Rotrat != null)
        {
            Rotrat.Unregister();
            Rotrat = null;
        }
        if (FireGrub != null)
        {
            FireGrub.Unregister();
            FireGrub = null;
        }
        if (UmbraScav != null)
        {
            UmbraScav.Unregister();
            UmbraScav = null;
        }
    }
}

public static class PBSandboxUnlockID
{
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID NightTerror = new("NightTerror", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID LMiniLongLegs = new(nameof(LMiniLongLegs), true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID Rotrat = new(nameof(Rotrat), true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID FireGrub = new(nameof(FireGrub), true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID UmbraScav = new(nameof(UmbraScav), true);
    public static void UnregisterValues()
    {
        if (NightTerror != null)
        {
            NightTerror.Unregister();
            NightTerror = null;
        }
        if (LMiniLongLegs != null)
        {
            LMiniLongLegs.Unregister();
            LMiniLongLegs = null;
        }
        if (Rotrat != null)
        {
            Rotrat.Unregister();
            Rotrat = null;
        }
        if (FireGrub != null)
        {
            FireGrub.Unregister();
            FireGrub = null;
        }
        if (UmbraScav != null)
        {
            UmbraScav.Unregister();
            UmbraScav = null;
        }
    }
}

public class PBSoundID
{
    public static SoundID Player_Activated_Thanatosis = new SoundID("Player_Activated_Thanatosis", true);
    public static SoundID Player_Deactivated_Thanatosis = new SoundID("Player_Deactivated_Thanatosis", true);
    public static SoundID Player_Deactivated_Thanatosis_From_Stun = new SoundID("Player_Deactivated_Thanatosis_From_Stun", true);
    public static SoundID Player_Died_From_Thanatosis = new SoundID("Player_Died_From_Thanatosis", true);
    public static SoundID Player_Revived = new SoundID("Player_Revived", true);
    public static void UnregisterValues()
    {
        if (Player_Activated_Thanatosis != null)
        {
            Player_Activated_Thanatosis.Unregister();
        }
        if (Player_Deactivated_Thanatosis != null)
        {
            Player_Deactivated_Thanatosis.Unregister();
        }
        if (Player_Deactivated_Thanatosis_From_Stun != null)
        {
            Player_Deactivated_Thanatosis_From_Stun.Unregister();
        }
        if (Player_Died_From_Thanatosis != null)
        {
            Player_Died_From_Thanatosis.Unregister();
        }
        if (Player_Revived != null)
        {
            Player_Revived.Unregister();
        }
    }
}

public class PBRoomEffectType
{
    // I just threw this in here, it's used with the others.
    public static RoomSettingsPage.DevEffectsCategories PitchBlackCatagory = new RoomSettingsPage.DevEffectsCategories("Pitch-Black", true);
    // Actual effects
    public static RoomSettings.RoomEffect.Type ElsehowView = new RoomSettings.RoomEffect.Type("ElsehowView", true);
    public static RoomSettings.RoomEffect.Type RippleSpawn = new RoomSettings.RoomEffect.Type("RippleSpawn", true);
    public static RoomSettings.RoomEffect.Type RippleMelt = new RoomSettings.RoomEffect.Type("RippleMelt", true);
    public static void UnregisterValues()
    {
        if (PitchBlackCatagory != null)
        {
            PitchBlackCatagory.Unregister();
            PitchBlackCatagory = null;
        }
        if (ElsehowView != null)
        {
            ElsehowView.Unregister();
            ElsehowView = null;
        }
        if (RippleSpawn != null)
        {
            RippleSpawn.Unregister();
            RippleSpawn = null;
        }
        if (RippleMelt != null)
        {
            RippleMelt.Unregister();
            RippleMelt = null;
        }
    }
}

public class PBEndGameID
{
    public static WinState.EndgameID Hunted = new WinState.EndgameID("Hunted", true);
    public static void UnregisterValues()
    {
        if (Hunted != null)
        {
            Hunted.Unregister();
            Hunted = null;
        }
    }
}

public class PBSceneID
{
    public static Menu.MenuScene.SceneID Endgame_Hunted = new Menu.MenuScene.SceneID("Engame_Hunted", true);
    public static void UnregisterValues()
    {
        if (Endgame_Hunted != null)
        {
            Endgame_Hunted.Unregister();
            Endgame_Hunted = null;
        }
    }
}

//SpawnType moving inside DeathSpawn.cs