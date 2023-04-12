using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Security.Permissions;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using PitchBlack;

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
        public static ConditionalWeakTable<Player, Beacon> bCon = new();


        // Add hooks
        public void OnEnable()
        {

            // Put your custom hooks here!
            On.Player.Jump += Player_Jump;
            On.Lizard.ctor += Lizard_ctor;
            On.Player.GraspsCanBeCrafted += Player_GraspsCanBeCrafted;
            On.Player.SpitUpCraftedObject += Player_SpitUpCraftedObject;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.Player.ctor += Player_ctor;
            On.Player.Grabability += Player_Grabability;
            On.Player.SwallowObject += Player_SwallowObject;
            On.Player.GrabUpdate += Player_GrabUpdate;
            On.Player.ctor += Player_ctor1;
            On.Player.GraphicsModuleUpdated += Player_GraphicsModuleUpdated;
        }

        private void Player_GraphicsModuleUpdated(On.Player.orig_GraphicsModuleUpdated orig, Player self, bool actuallyViewed, bool eu)
        {
            if (self.slugcatStats.name == Plugin.BeaconName)
            {
                if (bCon.TryGetValue(self, out Beacon b))
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
                if (bCon.TryGetValue(self, out Beacon b))
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
                bCon.Add(self, new Beacon (self));
                
            }
            if(!bCon.TryGetValue(self, out Beacon b))
            {

            }
        }

        private void Player_SwallowObject(On.Player.orig_SwallowObject orig, Player self, int grasp)
        {
            orig(self, grasp);
            if (self.slugcatStats.name == Plugin.BeaconName || self.slugcatStats.name == PhotoName)
            {
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
            if (self.slugcatStats.name == Plugin.PhotoName)
            {
                if (bCon.TryGetValue(self, out Beacon b))
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
                if (obj is Weapon)
                {
                    return Player.ObjectGrabability.OneHand;
                }
            }
            return result;
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
            if (player.slugcatStats.name == Plugin.BeaconName || player.slugcatStats.name == Plugin.PhotoName)
            {
                for (int i = 0; i < player.grasps.Length; i++)
                {
                    AbstractPhysicalObject hands = player.grasps[i].grabbed.abstractPhysicalObject;

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