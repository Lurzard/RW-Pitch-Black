using System;
using System.Diagnostics.CodeAnalysis;
using DevInterface;

namespace PitchBlack;

public static class PBCreatureTemplateType {
    [AllowNull] public static CreatureTemplate.Type NightTerror = new("NightTerror", true);
    [AllowNull] public static CreatureTemplate.Type LMiniLongLegs = new(nameof(LMiniLongLegs), true);
    [AllowNull] public static CreatureTemplate.Type Rotrat = new(nameof(Rotrat), true);
    [AllowNull] public static CreatureTemplate.Type UmbraScav = new(nameof(UmbraScav), true);
    [AllowNull] public static CreatureTemplate.Type FireGrub = new (nameof(FireGrub), true);

    public static void UnregisterValues() {
        if (NightTerror != null) {
            NightTerror.Unregister();
            NightTerror = null;
        }
        if (LMiniLongLegs != null) {
            LMiniLongLegs.Unregister();
            LMiniLongLegs = null;
        }
        if (Rotrat != null) {
            Rotrat.Unregister();
            Rotrat = null;
        }
        if (FireGrub != null) {
            FireGrub.Unregister();
            FireGrub = null;
        }
        if (UmbraScav != null) {
            UmbraScav.Unregister();
            UmbraScav = null;
        }
    }
}

public static class PBSandboxUnlockID {
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID NightTerror = new("NightTerror", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID LMiniLongLegs = new(nameof(LMiniLongLegs), true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID Rotrat = new(nameof(Rotrat), true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID FireGrub = new (nameof(FireGrub), true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID UmbraScav = new(nameof(UmbraScav), true);

    public static void UnregisterValues()
    {
        if (NightTerror != null) {
            NightTerror.Unregister();
            NightTerror = null;
        }
        if (LMiniLongLegs != null) {
            LMiniLongLegs.Unregister();
            LMiniLongLegs = null;
        }
        if (Rotrat != null) {
            Rotrat.Unregister();
            Rotrat = null;
        }
        if (FireGrub != null) {
            FireGrub.Unregister();
            FireGrub = null;
        }
        if (UmbraScav != null) {
            UmbraScav.Unregister();
            UmbraScav = null;
        }
    }
}

public class PBSoundID {
    public static SoundID Player_Activated_Thanatosis;
    public static SoundID Player_Deactivated_Thanatosis;
    public static SoundID Player_Deactivated_Thanatosis_From_Stun;
    public static SoundID Player_Died_From_Thanatosis;
    public static SoundID Player_Revived;

    public static void RegisterValues() {
        Player_Activated_Thanatosis = new SoundID("Player_Activated_Thanatosis", true);
        Player_Deactivated_Thanatosis = new SoundID("Player_Deactivated_Thanatosis", true);
        Player_Deactivated_Thanatosis_From_Stun = new SoundID("Player_Deactivated_Thanatosis_From_Stun", true);
        Player_Died_From_Thanatosis = new SoundID("Player_Died_From_Thanatosis", true);
        Player_Revived = new SoundID("Player_Revived", true);
    }

    public static void UnregisterValues() {
        SoundID activatedThanatosis = Player_Activated_Thanatosis;
        if (activatedThanatosis != null) {
            activatedThanatosis.Unregister();
        }

        SoundID deactivatedThanatosis = Player_Deactivated_Thanatosis;
        if (deactivatedThanatosis != null) {
            deactivatedThanatosis.Unregister();
        }

        SoundID deactivatedThanatosisFromStun = Player_Deactivated_Thanatosis_From_Stun;
        if (deactivatedThanatosisFromStun != null) {
            deactivatedThanatosisFromStun.Unregister();
        }

        SoundID playerDiedFromThanatosis = Player_Died_From_Thanatosis;
        if (playerDiedFromThanatosis != null) {
            playerDiedFromThanatosis.Unregister();
        }

        SoundID playerRevived = Player_Revived;
        if (playerRevived != null) {
            playerRevived.Unregister();
        }
    }
}

public class PBRoomEffectType {
    public static RoomSettings.RoomEffect.Type ElsehowView;
    public static RoomSettingsPage.DevEffectsCategories PitchBlack;

    public static void RegisterValues()
    {
        ElsehowView = new RoomSettings.RoomEffect.Type("Elsehow View", true);
        PitchBlack = new RoomSettingsPage.DevEffectsCategories("Pitch-Black", true);
}

    public static void UnregisterValues()
    {
        RoomSettings.RoomEffect.Type elsehowView = ElsehowView;
        if (elsehowView != null)
        {
            elsehowView.Unregister();
        }
        ElsehowView = null;
    }
}

//public class Plugin {
//    public static SlugcatStats.Name Beacon;

//    public static void RegisterValues() {
//        Beacon = new SlugcatStats.Name("Beacon", true);
//    }

//    public static void UnregisterValues() {
//        SlugcatStats.Name beacon = Beacon;
//        if (beacon != null) {
//            beacon.Unregister();
//        }
//    }
//}