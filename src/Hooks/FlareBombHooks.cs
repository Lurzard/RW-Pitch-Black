using UnityEngine;

namespace PitchBlack;

public static class FlareBombHooks
{
    public static void Apply()
    {
        On.ScavengerAI.CollectScore_PhysicalObject_bool += ScavengerAI_CollectScore_PhysicalObject_bool;
        On.FlareBomb.DrawSprites += FlareBomb_DrawSprites;
        On.FlareBomb.HitByExplosion += FlareBomb_HitByExplosion;
        On.FlareBomb.Update += FlareBomb_Update;
    }

    private static void FlareBomb_Update(On.FlareBomb.orig_Update orig, FlareBomb self, bool eu)
    {
        orig(self, eu);
        
        //<Flarebomb stunning and KILLING creatures>
    }

    /// <summary>
    /// Prevent stored flares from detonating, which otherwise would break the storage slot.
    /// </summary>
    private static void FlareBomb_HitByExplosion(On.FlareBomb.orig_HitByExplosion orig, FlareBomb self, float hitFac, Explosion explosion, int hitChunk)
    {
        if (self.mode != Weapon.Mode.OnBack)
        {
            orig(self, hitFac, explosion, hitChunk);   
        }
    }

    /// <summary>
    /// Colors FlareBomb glow sprite appropriately when thrown by Beacon.
    /// </summary>
    private static void FlareBomb_DrawSprites(On.FlareBomb.orig_DrawSprites orig, FlareBomb self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        
        if (MiscUtils.IsBeacon(self.thrownBy))
        {
            sLeaser.sprites[2].color = new Color(0.4f, 0f, 1f);
        }
    }

    /// <summary>
    /// Makes Scavengers not interested in stealing flares from beacon's storage if option is disabled.
    /// Implements scavStealing configurable from ModOptions.cs
    /// [spinch]
    /// </summary>
    private static int ScavengerAI_CollectScore_PhysicalObject_bool(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
    {
        var val = orig(self, obj, weaponFiltered);

        if (!ModOptions.scavStealing.Value)
        {
            if (obj is FlareBomb flarebomb && self.scavenger.room != null) {
                foreach (var abstrCrit in self.scavenger.room.game.Players) {
                    if (abstrCrit.realizedCreature == null) {
                        continue;
                    }
                    if (Plugin.scugCWT.TryGetValue(abstrCrit.realizedCreature as Player, out ScugCWT scugCWT) && scugCWT is BeaconCWT beaconCWT && beaconCWT.storage.storedFlares.Contains(flarebomb)) {
                        return 0;
                    }
                }
            }   
        }
        
        return val;
    }
}