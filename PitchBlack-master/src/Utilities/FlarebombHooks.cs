using UnityEngine;
using RWCustom;
using static PitchBlack.Plugin;

namespace PitchBlack;

public class FlarebombHooks
{
    public static void Apply()
    {
        On.ScavengerAI.CollectScore_PhysicalObject_bool += ScavengerAI_CollectScore_PhysicalObject_bool;
        On.FlareBomb.Update += DieToFlareBomb;
    }

    #region not hooks
    public static bool CreatureIsWithinFlareBombRange(FlareBomb self, Vector2 creaturePos)
    {
        return Custom.DistLess(self.firstChunk.pos, creaturePos, self.LightIntensity * 600f)
            || (Custom.DistLess(self.firstChunk.pos, creaturePos, self.LightIntensity * 1600f) && self.room.VisualContact(self.firstChunk.pos, creaturePos));
    }
    public static bool CreatureIsSpider(CreatureTemplate.Type creatureTemplateType)
    {
        return creatureTemplateType == CreatureTemplate.Type.BigSpider
            || creatureTemplateType == CreatureTemplate.Type.SpitterSpider
            || creatureTemplateType == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.MotherSpider;
    }
    #endregion

    private static int ScavengerAI_CollectScore_PhysicalObject_bool(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
    {
        int val = orig(self, obj, weaponFiltered);

        if (obj is FlareBomb flarebomb && obj.room != null)
        {
            foreach (AbstractCreature abstrCrit in flarebomb.room.game.Players)
            {
                if (abstrCrit.realizedCreature == null)
                    continue;

                if (scugCWT.TryGetValue(abstrCrit.realizedCreature as Player, out var cwt) && cwt.IsBeacon)
                {
                    if (cwt.Beacon.storage.storedFlares.Contains(flarebomb))
                        val = 0; //if in beacon storage, dont steal
                }
            }
        }

        return val;
    }
    //Spider, SpitterSpider, and MotherSpider die to FlareBombs with this mod enabled
    public static void DieToFlareBomb(On.FlareBomb.orig_Update orig, FlareBomb self, bool eu)
    {
        orig(self, eu);
        for (int i = 0; i < self.room.abstractRoom.creatures.Count; i++)
        {
            if (self.room.abstractRoom.creatures[i].realizedCreature != null
                && CreatureIsWithinFlareBombRange(self, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos)
                && !self.room.abstractRoom.creatures[i].realizedCreature.dead)
            {
                if (self.thrownBy?.abstractCreature != null)
                    self.room.abstractRoom.creatures[i].realizedCreature.SetKillTag(self.thrownBy.abstractCreature);

                if (CreatureIsSpider(self.room.abstractRoom.creatures[i].creatureTemplate.type)
                    && (MiscUtils.IsBeaconOrPhoto(self.room.game.StoryCharacter) || MiscUtils.IsBeaconOrPhoto(self.thrownBy))
                    )
                {
                    //die if the game slugcat or thrower is beacon or photo
                    self.room.abstractRoom.creatures[i].realizedCreature.firstChunk.vel += Custom.DegToVec(Random.value * 360f) * Random.value * 7f;
                    self.room.abstractRoom.creatures[i].realizedCreature.Die();
                }
                else if (self.room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplateType.NightTerror)
                {
                    if (NightTerrorHooks.NightTerrorInfo.TryGetValue(self.room.abstractRoom.creatures[i].realizedCreature as Centipede, out var NTInfo))
                    {
                        NTInfo.fleeing = 40 * 18;

                        Vector2 displacement = self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos - self.firstChunk.pos;
                        NTInfo.fleeTo = self.firstChunk.pos + 9999999 * displacement;
                    }

                    //50% chance of the night terror releasing you from its grasp
                    Random.InitState(self.abstractPhysicalObject.ID.number);
                    if (Random.value <= 0.5)
                    {
                        (self.room.abstractRoom.creatures[i].realizedCreature as Centipede).NightTerrorReleasePlayersInGrasp();
                    }

                    //writhe in pain
                    self.room.AddObject(new CreatureSpasmer(self.room.abstractRoom.creatures[i].realizedCreature, false, 40));
                }
            }
        }
    }
}
