using System;
using System.Security.Permissions;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

/*
 * This file contains fixes to some common problems when modding Rain World.
 * Unless you know what you're doing, you shouldn't modify anything here.
 */

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace PitchBlack
{
    public static class Extras
    {
        static bool _initialized;

        public static void Apply()
        {
            On.FlareBomb.Update += DieToFlareBomb;
            On.RainWorld.OnModsDisabled += DisableMod;
            WorldChanges.Apply();
            PitchBlackCrafting.CraftingHookApply();
            //On.RainWorld.OnModsInit += LoadBeaconAtlas;
        }
        public static void DisableMod(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
        {
            orig(self, newlyDisabledMods);
            for (var i = 0; i < newlyDisabledMods.Length; i++)
            {
                if (newlyDisabledMods[i].id == Plugin.MOD_ID)
                {
                    if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.NightTerror))
                        MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.NightTerror);
                    CreatureTemplateType.UnregisterValues();
                    SandboxUnlockID.UnregisterValues();
                    break;
                }
            }
        }
        //Spider, SpitterSpider, and MotherSpider die to FlareBombs with this mod enabled
        public static void DieToFlareBomb(On.FlareBomb.orig_Update orig, FlareBomb self, bool eu)
        {
            orig(self, eu);
            for (int i = 0; i < self.room.abstractRoom.creatures.Count; i++)
            {
                if (self.room.abstractRoom.creatures[i].realizedCreature != null && (Custom.DistLess(self.firstChunk.pos, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, self.LightIntensity * 600f) || (Custom.DistLess(self.firstChunk.pos, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, self.LightIntensity * 1600f) && self.room.VisualContact(self.firstChunk.pos, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos))))
                {
                    if (self.room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.BigSpider && !self.room.abstractRoom.creatures[i].realizedCreature.dead)
                    {
                        self.room.abstractRoom.creatures[i].realizedCreature.firstChunk.vel += Custom.DegToVec(Random.value * 360f) * Random.value * 7f;
                        self.room.abstractRoom.creatures[i].realizedCreature.Die();
                    }
                    if (self.room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.SpitterSpider && !self.room.abstractRoom.creatures[i].realizedCreature.dead)
                    {
                        self.room.abstractRoom.creatures[i].realizedCreature.firstChunk.vel += Custom.DegToVec(Random.value * 360f) * Random.value * 7f;
                        self.room.abstractRoom.creatures[i].realizedCreature.Die();
                    }
                    if (self.room.abstractRoom.creatures[i].creatureTemplate.type == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.MotherSpider && !self.room.abstractRoom.creatures[i].realizedCreature.dead)
                    {
                        self.room.abstractRoom.creatures[i].realizedCreature.firstChunk.vel += Custom.DegToVec(Random.value * 360f) * Random.value * 7f;
                        self.room.abstractRoom.creatures[i].realizedCreature.Die();
                    }
                }
            };
        }
        public static void LoadBeaconAtlas(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            Futile.atlasManager.LoadAtlas("atlases/beaconatlas");
        }
        // Ensure resources are only loaded once and that failing to load them will not break other mods
        public static On.RainWorld.hook_OnModsInit WrapInit(Action<RainWorld> loadResources)
        {
            return (orig, self) =>
            {
                orig(self);

                try
                {
                    if (!_initialized)
                    {
                        _initialized = true;
                        loadResources(self);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            };
        }
}
}