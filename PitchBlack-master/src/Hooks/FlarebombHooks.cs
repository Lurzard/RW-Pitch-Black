using UnityEngine;
using RWCustom;

namespace PitchBlack;

public class FlarebombHooks
{
    #region not hooks
    public static bool CreatureIsWithinFlareBombRange(FlareBomb self, Vector2 creaturePos) {
        return Custom.DistLess(self.firstChunk.pos, creaturePos, self.LightIntensity * 600f)
            || (Custom.DistLess(self.firstChunk.pos, creaturePos, self.LightIntensity * 1600f) && self.room.VisualContact(self.firstChunk.pos, creaturePos));
    }
    public static bool TooBrightForMe(CreatureTemplate.Type creatureTemplateType) {
        // Creatures that die to FlareBombs (these should be nerfed to apply with creature's apparent sight)
        return creatureTemplateType == CreatureTemplate.Type.BigSpider
            || creatureTemplateType == CreatureTemplate.Type.SpitterSpider
            || creatureTemplateType == DLCSharedEnums.CreatureTemplateType.MotherSpider
            || creatureTemplateType == CreatureTemplate.Type.DropBug
            || creatureTemplateType == CreatureTemplate.Type.EggBug
            || creatureTemplateType == DLCSharedEnums.CreatureTemplateType.Yeek
            || creatureTemplateType == CreatureTemplate.Type.Leech
            || creatureTemplateType == CreatureTemplate.Type.SeaLeech
            || creatureTemplateType == CreatureTemplate.Type.JetFish
            || creatureTemplateType == CreatureTemplate.Type.Snail
            || creatureTemplateType == CreatureTemplate.Type.CicadaA
            || creatureTemplateType == CreatureTemplate.Type.CicadaB
            || creatureTemplateType == CreatureTemplate.Type.SmallNeedleWorm
            || creatureTemplateType == CreatureTemplate.Type.LanternMouse
            || creatureTemplateType == CreatureTemplate.Type.GarbageWorm
            || creatureTemplateType == CreatureTemplate.Type.MirosBird
            ;
    }
    #endregion
    
    public static void Apply()
    {
        On.ScavengerAI.CollectScore_PhysicalObject_bool += ScavengerAI_CollectScore_PhysicalObject_bool;
        On.FlareBomb.Update += DieToFlareBomb;
        On.FlareBomb.DrawSprites += FlareBomb_DrawSprites;
        On.FlareBomb.HitByExplosion += FlareBomb_HitByExplosion;
    }
    //THIS WILL PREVENT STORED FLARES FROM DETONATING AND BREAKING THE STORAGE SLOT
    private static void FlareBomb_HitByExplosion(On.FlareBomb.orig_HitByExplosion orig, FlareBomb self, float hitFac, Explosion explosion, int hitChunk) {
        if (self.mode != Weapon.Mode.OnBack) {
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
    // Makes it so that if flarebombs are being stored by Beacon, Scavs are not interested in the bombs at all
    private static int ScavengerAI_CollectScore_PhysicalObject_bool(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered) {
        int val = orig(self, obj, weaponFiltered);

        //spinch: hopefully the below is better anti theft code
        if (obj is FlareBomb flarebomb && self.scavenger.room != null) {
            foreach (var abstrCrit in self.scavenger.room.game.Players) {
                if (abstrCrit.realizedCreature == null) {
                    continue;
                }
                //if in beacon storage, dont steal
                if (Plugin.scugCWT.TryGetValue(abstrCrit.realizedCreature as Player, out ScugCWT scugCWT) && scugCWT is BeaconCWT beaconCWT && beaconCWT.storage.storedFlares.Contains(flarebomb)) {
                    return 0;
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

                if (self.room.abstractRoom.creatures[i].creatureTemplate?.type == null)
                    continue;

                bool stunCreatures = MiscUtils.IsBeaconOrPhoto(self.room.game.StoryCharacter) || MiscUtils.IsBeaconOrPhoto(self.thrownBy);

                // If the flarebomb was thrown and there exists a realized player that is one of ours, then the flarebomb needs to stun creatures.
                /*
                IMPORTANT: THIS SHOULD BE REPLACED BY A SINGLE CHECK AT THE START OF THE CYCLE TO SEE IF A PB SLUGCAT EXISTS SO THAT THIS DOES NOT BREAK IF THE PB SLUGCATS BECOME UNREALIZED
                */
                if (!stunCreatures && ModManager.CoopAvailable)
                {
                    foreach (var playersInGame in self.room.game.Players)
                    {
                        if (playersInGame.realizedCreature is Player player && MiscUtils.IsBeaconOrPhoto(player.SlugCatClass))
                        {
                            stunCreatures = true;
                            break;
                        }
                    }
                }

                // If the flarebomb should stun creatures:
                if (stunCreatures)
                {
                    if (TooBrightForMe(self.room.abstractRoom.creatures[i].creatureTemplate.type))
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

                if (self.room.abstractRoom.creatures[i].creatureTemplate.IsNightTerror())
                {

                    // To be balanced somewhat, the NT will ALWAYS drop players
                    self.room.abstractRoom.creatures[i].realizedCreature.NightTerrorReleasePlayersInGrasp();

                    //writhe in pain
                    self.room.AddObject(new CreatureSpasmer(self.room.abstractRoom.creatures[i].realizedCreature, false, 40));
                }
            }
        }
    }
}
