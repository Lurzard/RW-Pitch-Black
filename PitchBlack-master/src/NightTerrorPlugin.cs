using BepInEx;
using BepInEx.Logging;
using Fisobs.Core;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Security.Permissions;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace NightTerror
{
    [BepInPlugin(_ID, nameof(NightTerror), "1.0.0")]
    sealed class NightTerrorPlugin : BaseUnityPlugin
    {
        [AllowNull] internal static ManualLogSource logger;
        const string _ID = "nko.nightterror";

        public void OnEnable()
        {
            logger = Logger;
            On.RainWorld.OnModsDisabled += (orig, self, newlyDisabledMods) =>
            {
                orig(self, newlyDisabledMods);
                for (var i = 0; i < newlyDisabledMods.Length; i++)
                {
                    if (newlyDisabledMods[i].id == _ID)
                    {
                        if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.NightTerror))
                            MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.NightTerror);
                        CreatureTemplateType.UnregisterValues();
                        SandboxUnlockID.UnregisterValues();
                        break;
                    }
                }
            };
            Content.Register(new RedCentipedeCritob());
        }

        public void OnDisable() => logger = default;
    }

}