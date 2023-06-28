using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.IO;
using System.Linq;
using RWCustom;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Security;
using MonoMod.Cil;
using static Player;
using Fisobs.Core;

namespace PitchBlack
{
    public static class WorldChanges
    {
        public static void Apply()
        {
            On.Region.GetProperRegionAcronym += Region_GetProperRegionAcronym;
        }
        public static string Region_GetProperRegionAcronym(On.Region.orig_GetProperRegionAcronym orig, SlugcatStats.Name character, string baseAcronym)
        {
            string text = baseAcronym;
            //if (character.ToString() == "Beacon") 
            if (Plugin.BeaconName == character)
            {
                switch (text)
                {
                    case "SS":
                        text = "RM";
                        break;
                    case "SL":
                        text = "LM";
                        break;
                    case "DS":
                        text = "UG";
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

            //if (character.ToString() == "Photomaniac") //Ok so maybe this might've been a bad way to do this :(
            if (Plugin.PhotoName == character)
            {
                switch (text)
                {
                    case "SS":
                        text = "RM";
                        break;
                    case "SL":
                        text = "LM";
                        break;
                    case "DS":
                        text = "UG";
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
}