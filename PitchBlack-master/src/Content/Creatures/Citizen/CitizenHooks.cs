using UnityEngine;

namespace PitchBlack;

//code to do buncha scary stuff to scavs to create the Citizens
public class CitizenHooks
    {
    public static void Apply()
    {
        On.Scavenger.Update += Scavenger_Update;
    }

    private static void Scavenger_Update(On.Scavenger.orig_Update orig, Scavenger self, bool eu)
    {
        if (self.Template.type == PBEnums.CreatureTemplateType.Citizen)
        {
            self.CollideWithObjects = false;
        }
        orig(self, eu);
        if (self.Template.type == PBEnums.CreatureTemplateType.Citizen)
        {
            self.CollideWithObjects = false;
        }
    }
}

