using System.Diagnostics.CodeAnalysis;
using DevInterface;

namespace PitchBlack;

public static class Enums
{
    public static class SlugcatStatsName
    {
        public static readonly SlugcatStats.Name Beacon = new("Beacon", false);
        // Most code for Photo has been gutted (for now.. idk) -Lur
        public static readonly SlugcatStats.Name Photomaniac = new(nameof(Photomaniac), false);
    }
    public static class Timeline
    {
        public static readonly SlugcatStats.Timeline Beacon = new("Beacon", false);
    }

    public static class CreatureTemplateType
    {
        [AllowNull] public static CreatureTemplate.Type LMiniLongLegs = new(nameof(LMiniLongLegs), true);
        [AllowNull] public static CreatureTemplate.Type NightTerror = new(nameof(NightTerror), true);
        [AllowNull] public static CreatureTemplate.Type Rotrat = new(nameof(Rotrat), true);
        [AllowNull] public static CreatureTemplate.Type Citizen = new(nameof(Citizen), true);

        public static void UnregisterValues()
        {
            if (LMiniLongLegs != null)
            {
                LMiniLongLegs.Unregister();
                LMiniLongLegs = null;
            }
            if (NightTerror != null)
            {
                NightTerror.Unregister();
                NightTerror = null;
            }
            if (Rotrat != null)
            {
                Rotrat.Unregister();
                Rotrat = null;
            }
            if (Citizen != null)
            {
                Citizen.Unregister();
                Citizen = null;
            }
        }
    }
    public static class SandboxUnlockID
    {
        [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID LMiniLongLegs = new(nameof(LMiniLongLegs), true);
        [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID NightTerror = new(nameof(NightTerror), true);
        [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID RotRat = new(nameof(RotRat), true);

        public static void UnregisterValues()
        {
            if (LMiniLongLegs != null)
            {
                LMiniLongLegs.Unregister();
                LMiniLongLegs = null;
            }
            if (NightTerror != null)
            {
                NightTerror.Unregister();
                NightTerror = null;
            }
            if (RotRat != null)
            {
                RotRat.Unregister();
                RotRat = null;
            }
        }
    }
    
    public static class RoomEffectType
    {
        // I just threw this in here, it's used with the others.
        public static RoomSettingsPage.DevEffectsCategories PitchBlackCatagory = new ("Pitch-Black", true);
        // Actual effects
        public static RoomSettings.RoomEffect.Type ElsehowView = new("ElsehowView", true);
        public static RoomSettings.RoomEffect.Type RippleSpawn = new("RippleSpawn", true);
        public static RoomSettings.RoomEffect.Type RippleMelt = new("RippleMelt", true);
        public static RoomSettings.RoomEffect.Type RoseSky = new("RoseSky", true);
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
        }
    }
}