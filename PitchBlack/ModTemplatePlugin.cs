using BepInEx;
using System.Security;
using System.Security.Permissions;
using ModTemplate;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace ModTemplatePlugin
{
    [BepInEx.BepInPlugin(_ID, nameof(ModTemplatePlugin), "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        const string _ID = "niko.modtemplate";
    
        public void OnEnable()
        {
            Hooks.Apply();
        }
    }
}