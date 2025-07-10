using System.Diagnostics.CodeAnalysis;

namespace PitchBlack;

public static class Enums
{
    public static class SlugcatStatsName
    {
        public static readonly SlugcatStats.Name Beacon = new(nameof(Beacon), true);
        // Most code for Photo has been gutted (for now.. idk) -Lur
        public static readonly SlugcatStats.Name Photomaniac = new(nameof(Photomaniac), true);
    }
    public static class Timeline
    {
        public static readonly SlugcatStats.Timeline Beacon = new(nameof(Beacon), true);
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
}