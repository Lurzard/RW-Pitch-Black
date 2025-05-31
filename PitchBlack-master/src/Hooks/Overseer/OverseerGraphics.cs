using MonoMod.RuntimeDetour;
using System.Reflection;
using RWCustom;
using Colour = UnityEngine.Color;
using Mathf = UnityEngine.Mathf;

namespace PitchBlack;

public static class OverseerGraphics
{
    public static void Apply()
    {
        On.OverseerGraphics.ColorOfSegment += OverseerGraphics_ColorOfSegment;
        new Hook(typeof(global::OverseerGraphics).GetProperty("MainColor", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
            typeof(OverseerGraphics).GetMethod("OverseerGraphics_MainColor_get", BindingFlags.Static | BindingFlags.Public));
    }
    //Fixes funny coloring
    public static Colour OverseerGraphics_ColorOfSegment(On.OverseerGraphics.orig_ColorOfSegment orig, global::OverseerGraphics self, float f, float timeStacker)
    {
        Colour val = orig(self, f, timeStacker);
        //if ((self.overseer.abstractCreature.abstractAI as OverseerAbstractAI).ownerIterator == 87)
        if (self.overseer.room?.world.game.session is StoryGameSession story && MiscUtils.IsBeaconOrPhoto(story.game.StoryCharacter) && self.overseer.PlayerGuide)
        {
            return Colour.Lerp(
                Colour.Lerp(Custom.RGB2RGBA((self.MainColor + new Colour(0f, 0f, 1f) + self.earthColor * 8f) / 10f, 0.5f),
                Colour.Lerp(self.MainColor, Colour.Lerp(self.NeutralColor, self.earthColor, Mathf.Pow(f, 2f)), self.overseer.SandboxOverseer ? 0.15f : 0.5f),
                self.ExtensionOfSegment(f, timeStacker)), Custom.RGB2RGBA(self.MainColor, 0f),
                Mathf.Lerp(self.overseer.lastDying, self.overseer.dying, timeStacker));
        }
        return val;
        }
    public delegate Colour orig_OverseerMainColor(global::OverseerGraphics self);
    public static Colour OverseerGraphics_MainColor_get(orig_OverseerMainColor orig, global::OverseerGraphics self)
    {
        Colour val = orig(self);
        //if ((self.overseer.abstractCreature.abstractAI as OverseerAbstractAI).ownerIterator == 87)
        if (self.overseer.room?.world.game.session is StoryGameSession story && MiscUtils.IsBeaconOrPhoto(story.game.StoryCharacter) && self.overseer.PlayerGuide)
        {
            return Plugin.OverseerColor;
        }
        return val;
    }
}