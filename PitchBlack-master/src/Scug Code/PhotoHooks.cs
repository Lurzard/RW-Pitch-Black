namespace PitchBlack;
public static class PhotoHooks
{
    public static void Apply() {
        On.Player.Update += PhotoParry;
        On.Creature.Violence += Creature_Violence;
    }
    public static void PhotoParry(On.Player.orig_Update orig, Player self, bool eu) {
        orig(self, eu);
        if (Plugin.scugCWT.TryGetValue(self, out ScugCWT cwt) && cwt is PhotoCWT photoCWT) {
            photoCWT.Update(self);
        }
    }

    private static void Creature_Violence(On.Creature.orig_Violence orig, Creature self, BodyChunk source, UnityEngine.Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus) {
        if (PBRemixMenu.elecImmune.Value && type == Creature.DamageType.Electric && self is Player player && MiscUtils.IsPhoto(player))
            return; //WW- SKIP! ELECTRICITY IMMUNITY!
        //Centipedes with a higher mass will still kill you instantly because they just call Die()
        orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
    }
}