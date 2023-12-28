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
        On.FlareBomb.DrawSprites += FlareBomb_DrawSprites;
        On.FlareBomb.HitByExplosion += FlareBomb_HitByExplosion;
    }

    private static void FlareBomb_HitByExplosion(On.FlareBomb.orig_HitByExplosion orig, FlareBomb self, float hitFac, Explosion explosion, int hitChunk)
    {
        if (self.mode != Weapon.Mode.OnBack) //THIS WILL PREVENT STORED FLARES FROM DETONATING AND BREAKING THE STORAGE SLOT
        {
            orig(self, hitFac, explosion, hitChunk);
        }
    }

    private static void FlareBomb_DrawSprites(On.FlareBomb.orig_DrawSprites orig, FlareBomb self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (MiscUtils.IsBeaconOrPhoto(self.thrownBy))
        {
            sLeaser.sprites[2].color = self.color;
        }
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

        //spinch: hopefully the below is better anti theft code
        if (obj is FlareBomb flarebomb && self.scavenger.room != null)
        {
            foreach (var abstrCrit in self.scavenger.room.game.Players)
            {
                if (abstrCrit.realizedCreature == null)
                    continue;

                if (scugCWT.TryGetValue(abstrCrit.realizedCreature as Player, out var cwt) && cwt.IsBeacon)
                {
                    if (cwt.Beacon.storage.storedFlares.Contains(flarebomb))
                        return 0; //if in beacon storage, dont steal
                }
            }
        }

#if false
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
#endif

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

                if (self.room.abstractRoom.creatures[i].creatureTemplate?.type == null)
                    continue;

                bool stunCreatures = MiscUtils.IsBeaconOrPhoto(self.room.game.StoryCharacter) || MiscUtils.IsBeaconOrPhoto(self.thrownBy);

                if (!stunCreatures && ModManager.CoopAvailable)
                {
                    //loop through all co-op players
                    foreach (var playersInGame in self.room.game.Players)
                    {
                        if (playersInGame.realizedCreature is Player player && MiscUtils.IsBeaconOrPhoto(player.SlugCatClass))
                        {
                            stunCreatures = true;
                            break;
                        }
                    }
                }

                if (stunCreatures)
                {
                    if (CreatureIsSpider(self.room.abstractRoom.creatures[i].creatureTemplate.type))
                    {
                        //die if the game slugcat or thrower or any co-op players is beacon or photo
                        self.room.abstractRoom.creatures[i].realizedCreature.firstChunk.vel += Custom.DegToVec(Random.value * 360f) * Random.value * 7f;
                        self.room.abstractRoom.creatures[i].realizedCreature.Die();
                    }
                    else if (self.room.abstractRoom.creatures[i].realizedCreature is not Player
                        && self.room.abstractRoom.creatures[i].creatureTemplate.type != CreatureTemplate.Type.Slugcat
                        && self.room.abstractRoom.creatures[i].creatureTemplate.type != MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
                    {
                        //u would think the first check for player works... well it does sometimes. apparently
                        //creature release all grasps and get stunned
                        if (self.room.abstractRoom.creatures[i].realizedCreature.grasps != null)
                        {
                            for (int graspNum = 0; graspNum < self.room.abstractRoom.creatures[i].realizedCreature.grasps.Length; graspNum++)
                                self.room.abstractRoom.creatures[i].realizedCreature.ReleaseGrasp(graspNum);
                        }
                        self.room.abstractRoom.creatures[i].realizedCreature.Stun(Random.Range(60, 100));
                    }
                }

                if (self.room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplateType.NightTerror)
                {
                    //this will affect night terror, regardless of what slugcat the StoryCharacter or thrownBy is
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
