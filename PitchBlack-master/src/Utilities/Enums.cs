using System.Diagnostics.CodeAnalysis;

namespace PitchBlack;
public static class CreatureTemplateType
{
    [AllowNull] public static CreatureTemplate.Type NightTerror = new("NightTerror", true);
    [AllowNull] public static CreatureTemplate.Type LMiniLongLegs = new(nameof(LMiniLongLegs), true);
#if PLAYTEST
    [AllowNull] public static CreatureTemplate.Type Rotrat = new(nameof(Rotrat), true);
#endif

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
#if PLAYTEST
        if (Rotrat != null)
        {
            Rotrat.Unregister();
            Rotrat = null;
        }
#endif
    }
}

public static class SandboxUnlockID
{
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID NightTerror = new("NightTerror", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID LMiniLongLegs = new(nameof(LMiniLongLegs), true);
#if PLAYTEST
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID Rotrat = new(nameof(Rotrat), true);
#endif

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
#if PLAYTEST
        if (Rotrat != null)
        {
            Rotrat.Unregister();
            Rotrat = null;
        }
#endif
    }
}