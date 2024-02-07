using System.Diagnostics.CodeAnalysis;

namespace PitchBlack;
public static class CreatureTemplateType
{
    [AllowNull] public static CreatureTemplate.Type NightTerror = new("NightTerror", true);
    [AllowNull] public static CreatureTemplate.Type LMiniLongLegs = new(nameof(LMiniLongLegs), true);
    [AllowNull] public static CreatureTemplate.Type Rotrat = new(nameof(Rotrat), true);

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
    }
}

public static class SandboxUnlockID
{
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID NightTerror = new("NightTerror", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID LMiniLongLegs = new(nameof(LMiniLongLegs), true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID Rotrat = new(nameof(Rotrat), true);

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
    }
}