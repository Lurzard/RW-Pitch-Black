using UnityEngine;

namespace PitchBlack;

//code to do buncha scary stuff to scavs to create the Citizens
public class CitizenHooks
    {
    public static void Apply()
    {
        On.Scavenger.Update += Scavenger_Update;
        On.Scavenger.Grab += Scavenger_Grab;
    }

    private static bool Scavenger_Grab(On.Scavenger.orig_Grab orig, Scavenger self, PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
    {
        if (self.Template.type == PBEnums.CreatureTemplateType.Citizen)
        {
            return false;
        }
        return orig(self, obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);
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

