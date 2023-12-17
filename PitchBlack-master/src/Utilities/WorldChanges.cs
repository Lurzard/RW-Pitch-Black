using System.IO;
using System.Linq;
using UnityEngine;

namespace PitchBlack;
public static class WorldChanges
{
    public static void Apply()
    {
        On.Region.GetProperRegionAcronym += Region_GetProperRegionAcronym;
        On.RoofTopView.ctor += RoofTopView_ctor;
        On.AboveCloudsView.ctor += AboveCloudsView_ctor;
        On.KarmaFlower.ApplyPalette += KarmaFlower_ApplyPalette;
    }

    private static void AboveCloudsView_ctor(On.AboveCloudsView.orig_ctor orig, AboveCloudsView self, Room room, RoomSettings.RoomEffect effect)
    {
        orig(self, room, effect);
        if (room.game.GetStorySession.saveStateNumber == Plugin.BeaconName)
        {
            self.atmosphereColor = new Color(0.05098039215f, 0.09019607843f, 0.2f);
            Color atmocolor = new Color(0.05098039215f, 0.09019607843f, 0.2f);
            Shader.SetGlobalVector("_AboveCloudsAtmosphereColor", self.atmosphereColor);
            Shader.SetGlobalVector("_MultiplyColor", atmocolor);
        }
    }

    private static void KarmaFlower_ApplyPalette(On.KarmaFlower.orig_ApplyPalette orig, KarmaFlower self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (rCam.room.game.IsStorySession && rCam.room.game.GetStorySession.saveStateNumber == Plugin.BeaconName)
        {
            self.color = new HSLColor(0.702f, 96f, 0.53f).rgb;
        }
    }

    private static void RoofTopView_ctor(On.RoofTopView.orig_ctor orig, RoofTopView self, Room room, RoomSettings.RoomEffect effect)
    {
        orig(self, room, effect);
        if (room.game.GetStorySession.saveStateNumber == Plugin.BeaconName)
        {
            self.atmosphereColor = new Color(0.04882353f, 0.0527451f, 0.06843138f);
            Color atmocolor = new Color(0.78039217f, 0.41568628f, 0.39607844f);
            Shader.SetGlobalVector("_AboveCloudsAtmosphereColor", self.atmosphereColor);
            Shader.SetGlobalVector("_MultiplyColor", atmocolor);
        }
    }

    public static string Region_GetProperRegionAcronym(On.Region.orig_GetProperRegionAcronym orig, SlugcatStats.Name character, string baseAcronym)
    {
        string text = baseAcronym;

        if (MiscUtils.IsBeaconOrPhoto(character))
        {
            switch (text)
            {
                case "SL":
                    text = "LM";
                    break;
                case "DS":
                    text = "UG";
                    break;
                case "SS":
                    text = "RM";
                    break;
            }

            foreach (var path in AssetManager.ListDirectory("World", true, false)
                .Select(p => AssetManager.ResolveFilePath($"World{Path.DirectorySeparatorChar}{Path.GetFileName(p)}{Path.DirectorySeparatorChar}equivalences.txt"))
                .Where(File.Exists)
                .SelectMany(p => File.ReadAllText(p).Trim().Split(',')))
            {
                var parts = path.Contains("-") ? path.Split('-') : new[] { path };
                if (parts[0] == baseAcronym && (parts.Length == 1 || character.value.Equals(parts[1], System.StringComparison.OrdinalIgnoreCase)))
                {
                    text = Path.GetFileName(path).ToUpper();
                    break;
                }
            }
            return text;
        }

        return orig(character, baseAcronym);
    }
}