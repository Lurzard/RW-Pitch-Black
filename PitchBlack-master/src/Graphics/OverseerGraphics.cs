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
    public static class OverseerGraphics1
    {
        public static void Apply()
        {
            On.OverseerGraphics.ColorOfSegment += OverseerGraphics_ColorOfSegment;
            Hook overseercolorhook = new Hook(typeof(OverseerGraphics).GetProperty("MainColor", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(), typeof(Plugin).GetMethod("OverseerGraphics_MainColor_get", BindingFlags.Static | BindingFlags.Public));
        }
        //Fixes funny coloring
        public static Color OverseerGraphics_ColorOfSegment(On.OverseerGraphics.orig_ColorOfSegment orig, OverseerGraphics self, float f, float timeStacker)
        {
            if ((self.overseer.abstractCreature.abstractAI as OverseerAbstractAI).ownerIterator == 87)
            {
                return Color.Lerp(Color.Lerp(Custom.RGB2RGBA((self.MainColor + new Color(0f, 0f, 1f) + self.earthColor * 8f) / 10f, 0.5f), Color.Lerp(self.MainColor, Color.Lerp(self.NeutralColor, self.earthColor, Mathf.Pow(f, 2f)), self.overseer.SandboxOverseer ? 0.15f : 0.5f), self.ExtensionOfSegment(f, timeStacker)), Custom.RGB2RGBA(self.MainColor, 0f), Mathf.Lerp(self.overseer.lastDying, self.overseer.dying, timeStacker));
            }
            else
            {
                return orig(self, f, timeStacker);
            }
        }
    }
}