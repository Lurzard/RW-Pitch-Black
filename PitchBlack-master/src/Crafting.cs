using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Security.Permissions;

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


        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);

            // Put your custom hooks here!
            On.Player.Jump += Player_Jump;
            On.Player.Die += Player_Die;
            On.Lizard.ctor += Lizard_ctor;
            On.Player.GraspsCanBeCrafted += Player_GraspsCanBeCrafted;
            On.Player.SpitUpCraftedObject += Player_SpitUpCraftedObject;
            On.Player.CraftingResults += Player_CraftingResults;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.Player.ctor += Player_ctor;
            On.Player.Grabability += Player_Grabability;
        }

        private Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            if (self.slugcatStats.name == Plugin.PhotoName)
            {
                return Player.ObjectGrabability.OneHand;
            }
            if (!(obj is Spear))
            {
                return Player.ObjectGrabability.OneHand;
            }
            return Player.ObjectGrabability.BigOneHand;
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

        //Crafting
        private AbstractPhysicalObject.AbstractObjectType Player_CraftingResults(On.Player.orig_CraftingResults orig, Player self)
        {
            if (self.slugcatStats.name == Plugin.BeaconName || self.slugcatStats.name == Plugin.PhotoName)
            {
                if (self.FoodInStomach > 0)
                {
                    Creature.Grasp[] array = self.grasps;
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (array[0].grabbed is IPlayerEdible && (array[0].grabbed as IPlayerEdible).Edible &&
                   array[1].grabbed is IPlayerEdible && (array[1].grabbed as IPlayerEdible).Edible) return null;
                    }
                    if (array[0].grabbed is Rock) return AbstractPhysicalObject.AbstractObjectType.FlareBomb;
                    if (array[1].grabbed is Rock) return AbstractPhysicalObject.AbstractObjectType.FlareBomb;
                    if (array[0].grabbed is Spear && !(array[0].grabbed as Spear).bugSpear && !(array[0].grabbed as Spear).abstractSpear.electric && !(array[0].grabbed as Spear).abstractSpear.explosive) return AbstractPhysicalObject.AbstractObjectType.Spear;
                    if (array[1].grabbed is Spear && !(array[1].grabbed as Spear).bugSpear && !(array[1].grabbed as Spear).abstractSpear.electric && !(array[1].grabbed as Spear).abstractSpear.explosive) return AbstractPhysicalObject.AbstractObjectType.Spear;
                    return null;
                }
            }
           return orig(self);
        }

        //Arti Crafting
        private void Player_SpitUpCraftedObject(On.Player.orig_SpitUpCraftedObject orig, Player player)
        {
            if (player.slugcatStats.name == Plugin.BeaconName || player.slugcatStats.name == Plugin.PhotoName)
            {
                for (int i = 0; i < player.grasps.Length; i++)
                {
                    if (player.grasps[i] != null)
                    {
                        AbstractPhysicalObject hands = player.grasps[i].grabbed.abstractPhysicalObject;
                        AbstractPhysicalObject hands1 = player.grasps[0].grabbed.abstractPhysicalObject;
                        AbstractPhysicalObject hands2 = player.grasps[1].grabbed.abstractPhysicalObject;
                        if (hands1.type== AbstractPhysicalObject.AbstractObjectType.Spear && !(hands1 as AbstractSpear).explosive)
                        {
                            AbstractSpear abstractSpear = ElectricSpearCrafting(player, i, hands1);
                            if (player.FreeHand() != -1)
                            {
                                player.SlugcatGrab(abstractSpear.realizedObject, player.FreeHand());
                            }
                            return;
                        }
                        else if (hands2.type == AbstractPhysicalObject.AbstractObjectType.Spear && !(hands2 as AbstractSpear).explosive)
                        {
                            AbstractSpear abstractSpear = ElectricSpearCrafting(player, i, hands2);
                            if (player.FreeHand() != -1)
                            {
                                player.SlugcatGrab(abstractSpear.realizedObject, player.FreeHand());
                            }
                            return;
                        }
                        if (hands1.type == AbstractPhysicalObject.AbstractObjectType.Rock)
                        {
                            player.ReleaseGrasp(i);
                            hands1.realizedObject.RemoveFromRoom();
                            player.room.abstractRoom.RemoveEntity(hands1);
                            player.SubtractFood(1);
                            AbstractPhysicalObject abstractObject = new AbstractPhysicalObject(player.room.world, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null, player.abstractCreature.pos, player.room.game.GetNewID());
                            player.room.abstractRoom.AddEntity(abstractObject);
                            abstractObject.RealizeInRoom();
                            if (player.FreeHand() != -1)
                            {
                                player.SlugcatGrab(abstractObject.realizedObject, player.FreeHand());
                            }
                            return;
                        }

                    }
                }
            }
        }

        //Extracted method for crafting electric spears
        private static AbstractSpear ElectricSpearCrafting(Player player, int i, AbstractPhysicalObject hands1)
        {
            player.ReleaseGrasp(i);
            hands1.realizedObject.RemoveFromRoom();
            player.room.abstractRoom.RemoveEntity(hands1);
            player.SubtractFood(1);
            AbstractSpear abstractSpear = new AbstractSpear(player.room.world, null, player.abstractCreature.pos, player.room.game.GetNewID(), false, true);
           // abstractSpear.explosive = false;
           // abstractSpear.electric = true;
            player.room.abstractRoom.AddEntity(abstractSpear);
            abstractSpear.RealizeInRoom();
            return abstractSpear;
        }

        private bool Player_GraspsCanBeCrafted(On.Player.orig_GraspsCanBeCrafted orig, Player self)
        {
            if (self.slugcatStats.name == Plugin.BeaconName || self.slugcatStats.name == Plugin.BeaconName && self.input[0].y > 0) return true;
            return orig(self);
        } // Allow crafts

        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
        }

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

        // Implement ExlodeOnDeath
        private void Player_Die(On.Player.orig_Die orig, Player self)
        {
            bool wasDead = self.dead;
            if (self.slugcatStats.name == Plugin.BeaconName || self.slugcatStats.name == Plugin.PhotoName)
                orig(self);

            var room = self.room;
            var pos = self.mainBodyChunk.pos;
            var color = self.ShortCutColor();
            room.AddObject(new Explosion(room, self, pos, 7, 250f, 6.2f, 2f, 280f, 0.25f, self, 0.7f, 160f, 1f));
            room.AddObject(new Explosion.ExplosionLight(pos, 280f, 1f, 7, color));
            room.AddObject(new Explosion.ExplosionLight(pos, 230f, 1f, 3, new Color(1f, 1f, 1f)));
            room.AddObject(new ExplosionSpikes(room, pos, 14, 30f, 9f, 7f, 170f, color));
            room.AddObject(new ShockWave(pos, 330f, 0.045f, 5, false));

            room.ScreenMovement(pos, default, 1.3f);
            room.PlaySound(SoundID.Flare_Bomb_Burn, pos);
            room.InGameNoise(new Noise.InGameNoise(pos, 9000f, self, 1f));
            //AstractConsumable bomb = new AbstractConsumable(self.room.world, AbstractConsumable.AbstractObjectType.FlareBomb, null, self.coord, self, self.room.world.game.GetNewID(), -1);
            //bomb.RealizeInRoom();
            //self.room.AddObject(bomb.realizedConsumable);
        }
    }
}