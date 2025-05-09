using UnityEngine;

namespace PitchBlack;

//code to do buncha scary stuff to scavs to create the Citizens
public class CitizenHooks
    {
    public static void Apply()
    {
        On.ScavengerGraphics.ApplyPalette += ScavengerGraphics_ApplyPalette;
    }

    //to make it solid white
    private static void ScavengerGraphics_ApplyPalette(On.ScavengerGraphics.orig_ApplyPalette orig, ScavengerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (self.scavenger.Template.type == PBEnums.CreatureTemplateType.Citizen)
        {
            Color blendedBodyColor = new Color(0.9f, 0.9f, 0.9f);
            Color blendedHeadColor = new Color(0.9f, 0.9f, 0.9f);
        }
    }
}

