using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Security.Permissions;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using PitchBlack;
using AbstractObjectType = AbstractPhysicalObject.AbstractObjectType;
using MSC_AbstractObjectType = MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.IO;
using System.Linq;

#pragma warning disable CS0618 // Do not remove the following line.
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace SlugTemplate
{
    [BepInPlugin(MOD_ID, "Pitch Black", "0.1.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "lurzard.pitchblack";

        public static readonly PlayerFeature<float> SuperJump = PlayerFloat("beacon/super_jump");
        public static readonly PlayerFeature<bool> ExplodeOnDeath = PlayerBool("beacon/explode_on_death");
        public static readonly GameFeature<float> MeanLizards = GameFloat("beacon/mean_lizards");
        public static readonly SlugcatStats.Name BeaconName = new SlugcatStats.Name("Beacon", false);
        public static readonly SlugcatStats.Name PhotoName = new SlugcatStats.Name("Photomaniac", false);
        public static ConditionalWeakTable<Player, BeaconCWT> bCon = new();
        public static ConditionalWeakTable<Player, PhotoCWT> pCon = new();
        bool GraspIsNonElectricSpear(AbstractSpear spear)
        {
            return !(spear.explosive || spear.hue > 0 || spear.electric && spear.electricCharge >= 3);
        }


        // Add hooks
        public void OnEnable()
        {

            // Put your custom hooks here!
            On.Player.Jump += Player_Jump;
            On.Lizard.ctor += Lizard_ctor;
            //On.Player.GraspsCanBeCrafted += Player_GraspsCanBeCrafted;
           //On.Player.SpitUpCraftedObject += Player_SpitUpCraftedObject;
            //On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            //On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.Player.ctor += Player_ctor;
            On.Player.Grabability += Player_Grabability;
            On.Player.SwallowObject += Player_SwallowObject;
            On.Player.GrabUpdate += Player_GrabUpdate;
            On.Player.ctor += Player_ctor1;
            On.Player.GraphicsModuleUpdated += Player_GraphicsModuleUpdated;
            On.Player.GrabUpdate += Player_GrabUpdate1;
            On.Player.Update += Player_Update;
            On.Player.CanBeSwallowed += Player_CanBeSwallowed;
            On.Player.SwallowObject += Player_SwallowObject1;
            Hook overseercolorhook = new Hook(typeof(OverseerGraphics).GetProperty("MainColor", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(), typeof(Plugin).GetMethod("OverseerGraphics_MainColor_get", BindingFlags.Static | BindingFlags.Public));
            On.Region.GetProperRegionAcronym += Region_GetProperRegionAcronym;
            Whiskers.Hooks();
        }
        private string Region_GetProperRegionAcronym(On.Region.orig_GetProperRegionAcronym orig, SlugcatStats.Name character, string baseAcronym)
        {
            string text = baseAcronym;
            if (character.ToString() == "Beacon") 
            {
                switch (text)
                {
                    case "SS":
                        text = "RM";
                        break;
                    case "SL":
                        text = "LM";
                        break;
                }

                foreach (var path in AssetManager.ListDirectory("World", true, false)
                    .Select(p => AssetManager.ResolveFilePath($"World{Path.DirectorySeparatorChar}{Path.GetFileName(p)}{Path.DirectorySeparatorChar}equivalences.txt"))
                    .Where(File.Exists)
                    .SelectMany(p => File.ReadAllText(p).Trim().Split(',')))
                {
                    var parts = path.Contains("-") ? path.Split('-') : new[] { path };
                    if (parts[0] == baseAcronym && (parts.Length == 1 || character.value.Equals(parts[1], System.StringComparison.OrdinalIgnoreCase)))
                    {
                        text = Path.GetFileName(path).ToUpper();
                        break;
                    }
                }
                return text;
            }

            if (character.ToString() == "Photomaniac") //Ok so maybe this might've been a bad way to do this :(
            {
                switch (text)
                {
                    case "SS":
                        text = "RM";
                        break;
                    case "SL":
                        text = "LM";
                        break;
                }

                foreach (var path in AssetManager.ListDirectory("World", true, false)
                    .Select(p => AssetManager.ResolveFilePath($"World{Path.DirectorySeparatorChar}{Path.GetFileName(p)}{Path.DirectorySeparatorChar}equivalences.txt"))
                    .Where(File.Exists)
                    .SelectMany(p => File.ReadAllText(p).Trim().Split(',')))
                {
                    var parts = path.Contains("-") ? path.Split('-') : new[] { path };
                    if (parts[0] == baseAcronym && (parts.Length == 1 || character.value.Equals(parts[1], System.StringComparison.OrdinalIgnoreCase)))
                    {
                        text = Path.GetFileName(path).ToUpper();
                        break;
                    }
                }
                return text;
            }
            return orig(character, baseAcronym);
        }

        public delegate Color orig_OverseerMainColor(OverseerGraphics self);
        public static Color OverseerGraphics_MainColor_get(orig_OverseerMainColor orig, OverseerGraphics self)
        {
            Color res = orig(self);
            if ((self.overseer.abstractCreature.abstractAI as OverseerAbstractAI).ownerIterator == 87)
            {
                res = new Color(0.24705882352f, 0.13725490196f, 0.96862745098f);
            }
            return res;
        }

        private void Player_SwallowObject1(On.Player.orig_SwallowObject orig, Player self, int grasp)
        {
            orig(self, grasp);

            if (Plugin.PhotoName == self.slugcatStats.name && AbstractObjectType.Spear == self.objectInStomach.type && self.FoodInStomach > 0 && GraspIsNonElectricSpear(self.objectInStomach as AbstractSpear))
            {
                AddNewSpear(self, self.objectInStomach.ID);
                self.objectInStomach = null;

                if (self.grasps[1]?.grabbed.abstractPhysicalObject is AbstractSpear slugGrasp && GraspIsNonElectricSpear(slugGrasp))
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
        }
        public static void AddNewSpear(Player player, EntityID entityID)
        {
            AbstractPhysicalObject item = new AbstractSpear(player.room.world, null, player.abstractPhysicalObject.pos, player.room.game.GetNewID(), false, true);

            player.room.abstractRoom.AddEntity(item);
            item.RealizeInRoom();
            player.SubtractFood(1);
            if (-1 != player.FreeHand())
                player.SlugcatGrab(item.realizedObject, player.FreeHand());
        }

        private bool Player_CanBeSwallowed(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject testObj)
        {
            return orig(self, testObj) || Plugin.PhotoName == self.slugcatStats.name && testObj is Spear spear && self.FoodInStomach > 0 && GraspIsNonElectricSpear(spear.abstractSpear);
        }

        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            if (!pCon.TryGetValue(self, out PhotoCWT e))
            {
                Logger.LogDebug("dduheudfhfhueufihfuiefufefh");
                return;
            }
            // Parry
            e.UpdateParryCD(self);
            if (self.Consious && (self.wantToJump > 0) && self.input[0].pckp && (self.input[0].y < 0 || self.bodyMode == Player.BodyModeIndex.Crawl))
            {
                e.ThundahParry(self);
            }

            // Sparking when close to death VFX
            if (self.room != null && e.parryNum > e.parryMax - 5)
            {
                self.room.AddObject(new Spark(self.mainBodyChunk.pos, RWCustom.Custom.RNV(), Color.white, null, 4, 8));
            }
        }

        private void Player_GrabUpdate1(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            orig(self, eu);
            if (self.slugcatStats.name == Plugin.BeaconName)
            {
                if (bCon.TryGetValue(self, out BeaconCWT b))
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (self.grasps[i] != null && self.grasps[i].grabbed is IPlayerEdible)
                        {
                            b.storage.interactionLocked = true;
                            b.storage.counter = 0;
                        }
                    }
                    if (b.storage != null)
                    {
                        b.storage.increment = self.input[0].pckp;
                        b.storage.Update(eu);
                    }
                }
            }
        }

        private void Player_GraphicsModuleUpdated(On.Player.orig_GraphicsModuleUpdated orig, Player self, bool actuallyViewed, bool eu)
        {
            if (self.slugcatStats.name == Plugin.BeaconName)
            {
                if (bCon.TryGetValue(self, out BeaconCWT b))
                {
                    if (b.storage != null)
                    {
                        b.storage.GraphicsModuleUpdated(actuallyViewed, eu);
                    }
                }
            }
            orig(self, actuallyViewed, eu);
        }

        private void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            orig(self, eu);
            if (self.slugcatStats.name == Plugin.BeaconName)
            {
                if (bCon.TryGetValue(self, out BeaconCWT b))
                {
                    if (b.storage != null)
                    {
                        b.storage.increment = self.input[0].pckp;
                        b.storage.Update(eu);
                    }
                }
            }
        }

        private void Player_ctor1(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self,abstractCreature,world);
            if (self.slugcatStats.name == Plugin.BeaconName)
            {
                bCon.Add(self, new BeaconCWT (self));
                
            }
            if(!bCon.TryGetValue(self, out BeaconCWT b))
            {

            }
            if (self.slugcatStats.name == Plugin.PhotoName)
            {
                pCon.Add(self, new PhotoCWT());
            }
            if(!pCon.TryGetValue(self, out PhotoCWT p))
            {

            }
        }

        private void Player_SwallowObject(On.Player.orig_SwallowObject orig, Player self, int grasp)
        {
            orig(self, grasp);
            if (self.slugcatStats.name == Plugin.BeaconName)
            {
                if (self.playerState.foodInStomach <= 0) 
                { 
                    return; 
                }
                if (self.objectInStomach.type == AbstractPhysicalObject.AbstractObjectType.Rock)
                {
                    self.objectInStomach = new AbstractConsumable(self.room.world, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null, self.room.GetWorldCoordinate(self.firstChunk.pos), self.room.game.GetNewID(), -1, -1, null);
                    self.SubtractFood(1);
                }
            }
        }

        private Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            var result = orig(self, obj);
            if (self.slugcatStats.name == Plugin.BeaconName)
            {
                if (bCon.TryGetValue(self, out BeaconCWT b))
                {
                    if (b.storage != null && obj is FlareBomb flare)
                    {
                        foreach (FlareBomb storedFlare in b.storage.storedFlares)
                        {
                            if (storedFlare == flare)
                            {
                                return Player.ObjectGrabability.CantGrab;
                            }
                        }
                    }
                }
            }
            return self.slugcatStats.name == Plugin.PhotoName && obj is Weapon ? Player.ObjectGrabability.OneHand : orig(self, obj);
        }

        private void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (self.slugcatStats.name == Plugin.PhotoName)
            {
                self.playerState.isPup = true;
            }
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            Futile.atlasManager.LoadAtlas("atlases/beaconatlas");
        }

        private void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.player.slugcatStats.name == Plugin.BeaconName || self.player.slugcatStats.name == Plugin.PhotoName)
            {
                var fsprite = sLeaser.sprites[3];
                if (fsprite?.element?.name is string text && text.StartsWith("Head"))
                {
                    foreach (var atlas in Futile.atlasManager._atlases)
                    {
                        if (atlas._elementsByName.TryGetValue("Beacon" + text, out var element))
                        {
                            fsprite.element = element;
                            break;
                        }
                    }
                }
            }
        }

        //Arti Crafting
        private void Player_SpitUpCraftedObject(On.Player.orig_SpitUpCraftedObject orig, Player player)
        {
            if (player.slugcatStats.name == Plugin.PhotoName)
            {
                for (int i = 0; i < player.grasps.Length; i++)
                {
                    AbstractPhysicalObject hands = player.grasps[i].grabbed.abstractPhysicalObject;
                    if (player.playerState.foodInStomach <= 0) { return; }
                    
                    if (hands is AbstractSpear spear && !spear.explosive)
                    {
                        if (player.room.game.session is StoryGameSession story)
                            story.RemovePersistentTracker(hands);

                        player.ReleaseGrasp(i);

                        hands.LoseAllStuckObjects();
                        hands.realizedObject.RemoveFromRoom();
                        player.room.abstractRoom.RemoveEntity(hands);

                        AbstractPhysicalObject abstractSpear = new AbstractSpear(player.room.world, null, player.abstractPhysicalObject.pos, player.room.game.GetNewID(), false, true);

                        player.room.abstractRoom.AddEntity(abstractSpear);
                        abstractSpear.RealizeInRoom();

                        if (-1 != player.FreeHand())
                            player.SlugcatGrab(abstractSpear.realizedObject, player.FreeHand());
                    }
                }
                return;
            }
            orig(player);
        }

        private bool Player_GraspsCanBeCrafted(On.Player.orig_GraspsCanBeCrafted orig, Player self)
        {
            if (self.slugcatStats.name == Plugin.BeaconName || self.slugcatStats.name == Plugin.PhotoName && self.input[0].y > 0) return true;
            return orig(self);
        } // Allow crafts

        // Implement MeanLizards
        private void Lizard_ctor(On.Lizard.orig_ctor orig, Lizard self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);

            if(MeanLizards.TryGet(world.game, out float meanness))
            {
                self.spawnDataEvil = Mathf.Min(self.spawnDataEvil, meanness);
            }
        }


        // Implement SuperJump
        private void Player_Jump(On.Player.orig_Jump orig, Player self)
        {
            orig(self);

            if (SuperJump.TryGet(self, out var power))
            {
                self.jumpBoost *= 1f + power;
            }
        }
    }
}