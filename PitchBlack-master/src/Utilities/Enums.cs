using System.Diagnostics.CodeAnalysis;

namespace PitchBlack
{
    public static class CreatureTemplateType
    {
        [AllowNull] public static CreatureTemplate.Type NightTerror = new("NightTerror", true);

        public static void UnregisterValues()
        {
            if (NightTerror != null)
            {
                NightTerror.Unregister();
                NightTerror = null;
            }
        }
    }

    public static class SandboxUnlockID
    {
        [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID NightTerror = new("NightTerror", true);

        public static void UnregisterValues()
        {
            if (NightTerror != null)
            {
                NightTerror.Unregister();
                NightTerror = null;
            }
        }
    }
}