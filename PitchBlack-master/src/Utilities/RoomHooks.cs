using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack.Utilities;

public static class RoomHooks
{
    public static Apply()
    {
        // update RoomSettings.Load when ending in -2 (pebblesEnergyTaken == true)
        IL.RoomSettings.Load += RoomSettings_Load;
    }

    private static void RoomSettings_Load(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        //reached if MSC is enabled, uses second set of settings when cell is taken
        if (c.TryGotoNext(x => x.MatchCallvirt<String>("Substring")))
        {
            c.Remove();
            c.Emit(OpCodes.Ldc_I4_5);
            c.Emit(OpCodes.Ldarg_S);
            c.EmitDelegate<Func<string, bool, string, string, string>>((region, includeRootDirectory, additionalAppend, playerChar) =>
            {
                string path = "";
                path = WorldLoader.FindRoomFile(region, false, "-2_settings-" + playerChar + ".txt");
                if (!File.Exists(path))
                {
                    path = WorldLoader.FindRoomFile(region, false, "-2_settings.txt");
                }
                return path;
            });
        }
        else base.Logger.LogError("Couldn't ILHook RoomSettings.Load! (for MSC)");

        //reached if MSC is disabled, doesn't use second set of settings when pebblesEneryTaken == true (which shouldn't be possible normally)
        if (c.TryGotoNext(x => x.MatchCallvirt<String>("Substring")))
        {
            c.Remove();
            c.Emit(OpCodes.Ldc_I4_5);
            c.Emit(OpCodes.Ldarg_S);
            c.EmitDelegate<Func<string, bool, string, string, string>>((region, includeRootDirectory, additionalAppend, playerChar) =>
            {
                string path = "";
                path = WorldLoader.FindRoomFile(region, false, "_settings-" + playerChar + ".txt");
                if (!File.Exists(path))
                {
                    path = WorldLoader.FindRoomFile(region, false, "_settings.txt");
                }
                return path;
            });
        }
        else base.Logger.LogError("Couldn't ILHook RoomSettings.Load! (for Vanilla)");
    }
}
