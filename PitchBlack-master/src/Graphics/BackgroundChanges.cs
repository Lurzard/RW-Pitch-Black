using UnityEngine;

namespace PitchBlack;
 public class BackgroundChanges
{
    public static void Apply()
    {
        On.RoofTopView.ctor += RoofTopView_ctor;
        On.AboveCloudsView.ctor += AboveCloudsView_ctor;
    }

    private static void AboveCloudsView_ctor(On.AboveCloudsView.orig_ctor orig, AboveCloudsView self, Room room, RoomSettings.RoomEffect effect)
    {
        orig(self, room, effect);
        if (room.game.IsStorySession && room.game.GetStorySession.saveStateNumber == Plugin.BeaconName)
        {
            if (room.world.region.name == "UW")
            {
                self.atmosphereColor = new Color(14f / 255f, 19f / 255f, 28f / 255f);
                Color atmocolor = new Color(55f / 255f, 68f / 255f, 89f / 255f);
                Shader.SetGlobalVector("_AboveCloudsAtmosphereColor", self.atmosphereColor);
                Shader.SetGlobalVector("_MultiplyColor", atmocolor);
            }
        }
    }

    private static void RoofTopView_ctor(On.RoofTopView.orig_ctor orig, RoofTopView self, Room room, RoomSettings.RoomEffect effect)
    {
        orig(self, room, effect);
        if (room.game.GetStorySession.saveStateNumber == Plugin.BeaconName)
        {
            if (room.world.region.name == "UW")
            {
                self.atmosphereColor = new Color(14f / 255f, 19f / 255f, 28f / 255f);
                Color atmocolor = new Color(14f / 255f, 19f / 255f, 28f / 255f);
                Shader.SetGlobalVector("_AboveCloudsAtmosphereColor", self.atmosphereColor);
                Shader.SetGlobalVector("_MultiplyColor", atmocolor);
            }
        }
    }
}
