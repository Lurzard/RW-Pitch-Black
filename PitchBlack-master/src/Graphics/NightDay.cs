#if false
using UnityEngine;
//using static SlugBase.JsonAny;
using Colour = UnityEngine.Color;

namespace PitchBlack;

public class NightDay
{
    //unfinished code
    public static void Apply()
    {
        On.AboveCloudsView.ctor += AboveCloudsView_ctor;
        //On.AboveCloudsView.AddElement += ACVSwitchSkies;
        On.AboveCloudsView.Update += ACVColorSwitch;

        On.RoofTopView.ctor += RTVSwitchSkies;
        On.RoofTopView.Update += RTVColorSwitch;
    }

    private static void AboveCloudsView_ctor(On.AboveCloudsView.orig_ctor orig, AboveCloudsView self, Room room, RoomSettings.RoomEffect effect)
    {
        orig(self, room, effect);
        if (ModManager.MSC && room.game.session is StoryGameSession story && MiscUtils.IsBeaconOrPhoto(story.saveStateNumber)) //campaign check
        {
            self.daySky = new BackgroundScene.Simple2DBackgroundIllustration(self, "AtC_NightSky", new Vector2(683f, 384f)); //switch day sky w night sky
            self.duskSky = new BackgroundScene.Simple2DBackgroundIllustration(self, "AtC_DuskSky-Rivulet", new Vector2(683f, 384f)); //switch dusk sky for riv's
            self.nightSky = new BackgroundScene.Simple2DBackgroundIllustration(self, "AtC_Sky", new Vector2(683f, 384f)); //switch night sky for day sky
        }
    }

    #region Invert AboveCloudsView

    //Change how colors blend for our campaigns
    private static void ACVColorSwitch(On.AboveCloudsView.orig_Update orig, AboveCloudsView self, bool eu)
    {
        orig(self, eu);

        if (ModManager.MSC && self.room?.game.session is StoryGameSession story && MiscUtils.IsBeaconOrPhoto(story.saveStateNumber)) //campaign check
        {
            //spinch: i dont even know if this works btw
            float num = 1320f;
            float num2 = self.room.world.rainCycle.dayNightCounter / num;
            float num3 = (self.room.world.rainCycle.dayNightCounter - num) / num;
            float num4 = (self.room.world.rainCycle.dayNightCounter - num) / (num * 1.25f);
            //Colour a = new(0.16078432f, 0.23137255f, 0.31764707f); //tumblr blue
            //Colour color = new(0.5176471f, 0.3254902f, 0.40784314f); //green
            //Colour color2 = new(0.04882353f, 0.0527451f, 0.06843138f); //black
            //Colour color3 = new(1f, 0.79f, 0.47f); //salmon pink
            Colour color4 = new(0.078431375f, 0.14117648f, 0.21176471f); //dark-ish navy blue

            //Day (Functionally)
            Colour a = new(0.04882353f, 0.0527451f, 0.06843138f); //a has color2 (black)

            //Dusk
            Colour color = new(1f, 0.79f, 0.47f); //riv's
            Colour color3 = new(0.7564706f, 0.3756863f, 0.3756863f); //riv's

            //Night (Functionally)
            Colour color2 = new(0.16078432f, 0.23137255f, 0.31764707f); //color2 has a (tumblr blue)

            Colour? color5 = null;
            Colour? color6 = null;

            if (self.spireLights != null)
            {
                if (num3 > 0f)
                {
                    self.spireLights.alpha = Mathf.Min(1f, num3);
                }
                else
                {
                    self.spireLights.alpha = 0f;
                }
            }
            if (self.pebblesLightning != null)
            {
                if (num3 > 0f)
                {
                    self.pebblesLightning.intensityMultiplier = Mathf.Min(1f, num3);
                }
                else
                {
                    self.pebblesLightning.intensityMultiplier = 0f;
                }
            }
            if (num2 > 0f && num2 < 1f)
            {
                self.daySky.alpha = 1f - num2;
                color5 = new Colour?(Colour.Lerp(a, color, num2));
                color6 = new Colour?(Colour.Lerp(Colour.white, color3, num2));
            }
            if (num2 >= 1f)
            {
                self.daySky.alpha = 0f;
                if (num3 > 0f && num3 < 1f)
                {
                    self.duskSky.alpha = 1f - num3;
                    color5 = new Colour?(Colour.Lerp(color, color2, num3));
                }
                if (num3 >= 1f)
                {
                    self.duskSky.alpha = 0f;
                    color5 = new Colour?(color2);
                }
                if (num4 > 0f && num4 < 1f)
                {
                    color6 = new Colour?(Colour.Lerp(color3, color4, num4));
                }
                if (num4 >= 1f)
                {
                    color6 = new Colour?(color4);
                }
            }

            if (color5 != null)
            {
                self.atmosphereColor = color5.Value;
                Shader.SetGlobalVector("_AboveCloudsAtmosphereColor", self.atmosphereColor);
            }
            if (color6 != null)
            {
                Shader.SetGlobalVector("_MultiplyColor", color6.Value);
            }
        }
    }

    #endregion

    #region Invert RoofTopView
    //Reverse Day Night skies for our campaigns
    private static void RTVSwitchSkies(On.RoofTopView.orig_ctor orig, RoofTopView self, Room room, RoomSettings.RoomEffect effect)
    {
        orig(self, room, effect);
        if (ModManager.MSC && self.room?.game.session is StoryGameSession story && MiscUtils.IsBeaconOrPhoto(story.saveStateNumber)) //campaign check
        {
            self.daySky = new BackgroundScene.Simple2DBackgroundIllustration(self, "Rf_NightSky", new Vector2(683f, 384f)); //switch day sky w night sky
            self.duskSky = new BackgroundScene.Simple2DBackgroundIllustration(self, "Rf_DuskSky-Rivulet", new Vector2(683f, 384f)); //switch dusk sky for riv's
            self.nightSky = new BackgroundScene.Simple2DBackgroundIllustration(self, "Rf_Sky", new Vector2(683f, 384f)); //switch night sky for day sky
        }
    }

    //Change how colors blend for our campaigns
    private static void RTVColorSwitch(On.RoofTopView.orig_Update orig, RoofTopView self, bool eu)
    {
        float num = 1320f;
        float num2 = self.room.world.rainCycle.dayNightCounter / num;
        float num3 = (self.room.world.rainCycle.dayNightCounter - num) / num;
        float num4 = (self.room.world.rainCycle.dayNightCounter - num) / (num * 1.25f);
        Colour a; //= new(0.16078432f, 0.23137255f, 0.31764707f);
        Colour color; //= new(0.5176471f, 0.3254902f, 0.40784314f);
        Colour color2; //= new(0.04882353f, 0.0527451f, 0.06843138f);
        Colour color3; //= new(1f, 0.79f, 0.47f);
        Colour color4 = new(0.078431375f, 0.14117648f, 0.21176471f);

        orig(self, eu);

        if (ModManager.MSC && self.room?.game.session is StoryGameSession story && MiscUtils.IsBeaconOrPhoto(story.saveStateNumber)) //campaign check
        {
            //Day (Functionally)
            a = new(0.04882353f, 0.0527451f, 0.06843138f); //now color2

            //Dusk
            color = new(1f, 0.79f, 0.47f); //riv's
            color3 = new(0.7564706f, 0.3756863f, 0.3756863f); //riv's

            //Night (Functionally)
            color2 = new(0.16078432f, 0.23137255f, 0.31764707f); //now a
        }
    }
    #endregion
}
#endif