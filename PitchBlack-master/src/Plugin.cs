using BepInEx;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Security;
using Fisobs.Core;
using RWCustom;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using UnityEngine;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]
#pragma warning restore CS0618 // Type or member is obsolete

namespace PitchBlack
{
    [BepInPlugin(MOD_ID, "Pitch Black", "0.1.0")]
    class Plugin : BaseUnityPlugin
    {
        public const string MOD_ID = "lurzard.pitchblack";
        public static readonly SlugcatStats.Name BeaconName = new("Beacon", false);
        public static readonly SlugcatStats.Name PhotoName = new("Photomaniac", false);

        //public static ConditionalWeakTable<Player, BeaconCWT> bCon = new ConditionalWeakTable<Player, BeaconCWT>();
        //public static ConditionalWeakTable<Player, PhotoCWT> pCon = new ConditionalWeakTable<Player, PhotoCWT>();
        public static ConditionalWeakTable<Player, ScugCWT> scugCWT = new();

        public static List<string> currentDialog = new();
        public static bool Speaking = false;
        public static AbstractCreature PBOverseer;
        public static int pbcooldown = 0;
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);

            Content.Register(new NightTerrorCritob());
            ScareEverything.Apply();

            ScugHooks.Apply();
            ScugGraphics.Apply();

            BeaconHooks.Apply();
            PhotoHooks.Apply();
            Crafting.Apply();
            
            PBOverseerGraphics.Apply();

            WorldChanges.Apply();

            On.FlareBomb.Update += DieToFlareBomb;
            On.RainWorld.OnModsDisabled += DisableMod;

            //On.Player.GraspsCanBeCrafted += Player_GraspsCanBeCrafted;
            //On.Player.SpitUpCraftedObject += Player_SpitUpCraftedObject;
            //On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            //On.Player.CanBeSwallowed += Player_CanBeSwallowed;
            //On.Player.SwallowObject += Player_SwallowObject1;
            //On.Player.Grabability += GrabCoalescipedes;
        }

        /// <summary>
        /// If you're not using this, I'm stealing it
        /// </summary>
        public void LoadResources(RainWorld rainWorld)
        {
            Futile.atlasManager.LoadAtlas("atlases/photosplt");
            Futile.atlasManager.LoadAtlas("atlases/nightTerroratlas");
        }

        public static void DisableMod(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
        {
            orig(self, newlyDisabledMods);
            foreach (ModManager.Mod mod in newlyDisabledMods)
            {
                if (mod.id == MOD_ID)
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
                if (self.room.abstractRoom.creatures[i].realizedCreature != null
                    && CreatureIsWithinFlareBombRange(self, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos)
                    && !self.room.abstractRoom.creatures[i].realizedCreature.dead)
                {
                    if (CreatureIsSpider(self.room.abstractRoom.creatures[i].creatureTemplate.type))
                    {
                        self.room.abstractRoom.creatures[i].realizedCreature.firstChunk.vel += Custom.DegToVec(Random.value * 360f) * Random.value * 7f;
                        if (self.thrownBy != null && self.thrownBy.abstractCreature != null)
                            self.room.abstractRoom.creatures[i].realizedCreature.SetKillTag(self.thrownBy.abstractCreature);
                        self.room.abstractRoom.creatures[i].realizedCreature.Die();
                    }
                }
            }
        }
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

        #region Unused Code
        //public static Player.ObjectGrabability GrabCoalescipedes(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        //{
        //    orig(self, obj);
        //    if (obj is Spider)
        //    {
        //         return ObjectGrabability.OneHand;
        //    }
        //    else
        //    {
        //        return orig(self, obj);
        //    }
        //}
        //public static Color OverseerGraphics_MainColor_get(orig_OverseerMainColor orig, OverseerGraphics self)
        //{
        //    Color res = orig(self);
        //    if ((self.overseer.abstractCreature.abstractAI as OverseerAbstractAI).ownerIterator == 87)
        //    {
        //        res = new Color(0.05098039215f, 0.01176470588f, 0.09019607843f);
        //    }
        //    return res;
        //}
        // GraspIsNotElectricSpear Method seems not to exist anywhere, so these methods remain commented out :3
        /*private void Player_SwallowObject1(On.Player.orig_SwallowObject orig, Player self, int grasp)
        {
            orig(self, grasp);

            if (Plugin.PhotoName == self.slugcatStats.name && AbstractObjectType.Spear == self.objectInStomach.type && self.FoodInStomach > 0 && GraspIsNonElectricSpear(self.objectInStomach as AbstractSpear))
            {
                AddNewSpear(self, self.objectInStomach.ID);
                self.objectInStomach = null;

                if (self.FoodInStomach >= 2 && self.grasps[1]?.grabbed.abstractPhysicalObject is AbstractSpear slugGrasp && GraspIsNonElectricSpear(slugGrasp))
                {
                    if (self.room.game.session is StoryGameSession story)
                        story.RemovePersistentTracker(slugGrasp);

                    self.ReleaseGrasp(1);

                    slugGrasp.LoseAllStuckObjects();
                    slugGrasp.realizedObject.RemoveFromRoom();
                    self.room.abstractRoom.RemoveEntity(slugGrasp);
                    AddNewSpear(self, slugGrasp.ID);
                }
            }
        }*/
        /*private bool Player_CanBeSwallowed(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject testObj)
        {
            return orig(self, testObj) || Plugin.PhotoName == self.slugcatStats.name && testObj is Spear spear && self.FoodInStomach > 0 && GraspIsNonElectricSpear(spear.abstractSpear);
        }*/
        //public static void AddNewSpear(Player player, EntityID entityID)
        //{
        //    AbstractPhysicalObject item = new AbstractSpear(player.room.world, null, player.abstractPhysicalObject.pos, player.room.game.GetNewID(), false, true);

        //    player.room.abstractRoom.AddEntity(item);
        //    item.RealizeInRoom();
        //    player.SubtractFood(1);
        //    if (-1 != player.FreeHand())
        //        player.SlugcatGrab(item.realizedObject, player.FreeHand());
        //}

        //private void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        //{
        //    orig(self, sLeaser, rCam, timeStacker, camPos);
        //    if (self.player.slugcatStats.name == Plugin.BeaconName || self.player.slugcatStats.name == Plugin.PhotoName)
        //    {
        //        var fsprite = sLeaser.sprites[3];
        //        if (fsprite?.element?.name is string text && text.StartsWith("Head"))
        //        {
        //            foreach (var atlas in Futile.atlasManager._atlases)
        //            {
        //                if (atlas._elementsByName.TryGetValue("Beacon" + text, out var element))
        //                {
        //                    fsprite.element = element;
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //}//Arti Crafting
        //private void Player_SpitUpCraftedObject(On.Player.orig_SpitUpCraftedObject orig, Player player)
        //{
        //    if (player.slugcatStats.name == Plugin.PhotoName)
        //    {
        //        for (int i = 0; i < player.grasps.Length; i++)
        //        {
        //            AbstractPhysicalObject hands = player.grasps[i].grabbed.abstractPhysicalObject;
        //            if (player.playerState.foodInStomach <= 0) { return; }

        //            if (hands is AbstractSpear spear && !spear.explosive)
        //            {
        //                if (player.room.game.session is StoryGameSession story)
        //                    story.RemovePersistentTracker(hands);

        //                player.ReleaseGrasp(i);

        //                hands.LoseAllStuckObjects();
        //                hands.realizedObject.RemoveFromRoom();
        //                player.room.abstractRoom.RemoveEntity(hands);

        //                AbstractPhysicalObject abstractSpear = new AbstractSpear(player.room.world, null, player.abstractPhysicalObject.pos, player.room.game.GetNewID(), false, true);

        //                player.room.abstractRoom.AddEntity(abstractSpear);
        //                abstractSpear.RealizeInRoom();

        //                if (-1 != player.FreeHand())
        //                    player.SlugcatGrab(abstractSpear.realizedObject, player.FreeHand());
        //            }
        //        }
        //        return;
        //    }
        //    orig(player);
        //}
        //private bool Player_GraspsCanBeCrafted(On.Player.orig_GraspsCanBeCrafted orig, Player self)
        //{
        //    if (self.slugcatStats.name == Plugin.BeaconName || self.slugcatStats.name == Plugin.PhotoName && self.input[0].y > 0) return true;
        //    return orig(self);
        //} // Allow crafts
        #endregion
    }
}