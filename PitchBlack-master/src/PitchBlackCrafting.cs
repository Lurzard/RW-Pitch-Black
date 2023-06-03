using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using Colour = UnityEngine.Color;
using Random = UnityEngine.Random;
using MoreSlugcats;
using PitchBlack;
using SlugTemplate;

namespace PitchBlack
{
    public class PitchBlackCrafting
    {
        enum WhatsThatSpear
        {
            None,
            Normal,
            Electric_NotFull,
            Electric_Full,
            Explosive
        }

        public static void Hook()
        {
            IL.Player.GrabUpdate += Player_GrabUpdate;
            On.Player.Grabability += Player_Grabability;
            On.Player.GraspsCanBeCrafted += Player_GraspsCanBeCrafted;
            On.Player.SpitUpCraftedObject += Player_SpitUpCraftedObject;
        }

        public static void Player_GrabUpdate(ILContext il)
        {
            try
            {
                ILCursor c = new(il);
                ILLabel label = il.DefineLabel();

                if (!c.TryGotoNext(MoveType.After, /*i => i.MatchLdarg(0),*/ i => i.MatchCallOrCallvirt<Player>("FreeHand"), i => i.MatchLdcI4(-1)))
                {
                    Debug.Log("Unable to find Player.FreeHand() == -1 in ILhook");
                    return;
                }

                c.Index++;

                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<Player, bool>>(self =>
                {
                    return self.slugcatStats.name == Plugin.PhotoName;
                });
                c.Emit(OpCodes.Brtrue_S, label);

                c.GotoNext(MoveType.Before, i => i.MatchLdarg(0), i => i.MatchCallOrCallvirt<Player>("GraspsCanBeCrafted"));
                c.MarkLabel(label);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        public static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            return self.slugcatStats.name == Plugin.PhotoName && obj is Spear ? Player.ObjectGrabability.OneHand : orig(self, obj);
        }

        public static bool Player_GraspsCanBeCrafted(On.Player.orig_GraspsCanBeCrafted orig, Player self)
        {
            if (Plugin.PhotoName != self.slugcatStats.name)
            {
                return orig(self);
            }

            for (int i = 0; i < 2; i++)
            {
                if (self.grasps[i]?.grabbed.abstractPhysicalObject is AbstractSpear spearInHand)
                {
                    if (spearInHand.hue > 0)
                        continue;
                    if (self.FoodInStomach > 0)
                        return true;
                }
            }
            return false;
        }

        public static void Player_SpitUpCraftedObject(On.Player.orig_SpitUpCraftedObject orig, Player self)
        {
            if (Plugin.PhotoName != self.slugcatStats.name)
            {
                orig(self);
                return;
            }

            WhatsThatSpear[] spearsInGrasps = { WhatsThatSpear.None, WhatsThatSpear.None };
            bool CanCraftTwoSpears = self.FoodInStomach >= 2;

            for (int i = 0; i < 2; i++)
            {
                if (self.grasps[i]?.grabbed.abstractPhysicalObject is AbstractSpear spear)
                {
                    if (spear.explosive)
                        spearsInGrasps[i] = WhatsThatSpear.Explosive;
                    else if (spear.electric)
                    {
                        if (spear.electricCharge >= 3)
                            spearsInGrasps[i] = WhatsThatSpear.Electric_Full;
                        else
                            spearsInGrasps[i] = WhatsThatSpear.Electric_NotFull;
                    }
                    else
                        spearsInGrasps[i] = WhatsThatSpear.Normal;
                }
            }

            foreach (WhatsThatSpear enumType in Enum.GetValues(typeof(WhatsThatSpear)))
            {
                //loop thru the whole enum
                //im trying to setup priority checking system here dangit, it follows the order of the spreadsheet

                for (int i = 0; i < 2; i++)
                {
                    if (spearsInGrasps[i] != enumType)
                        continue;
                    //Debug.Log("enumType is " + enumType.ToString());
                    int otherHand = Mathf.Abs(i - 1); //|1-1| = 0, |0-1| = 1, so its just the other hand
                    switch (enumType)
                    {
                        case WhatsThatSpear.None:
                            HasNoneGrasp(otherHand);
                            return;
                        case WhatsThatSpear.Normal:
                            HasNormalSpear(i, otherHand);
                            return;
                        case WhatsThatSpear.Electric_NotFull:
                            HasElectricSpear_NotFull(i, otherHand);
                            return;
                        case WhatsThatSpear.Electric_Full:
                            HasElectricSpear_Full(otherHand);
                            return;
                        case WhatsThatSpear.Explosive:
                            HasExplosiveSpear(otherHand);
                            return;
                    }
                }
            }

            #region going thru the spreadsheet
            void HasNoneGrasp(int spearIndex)
            {
                WhatsThatSpear otherGrasp = spearsInGrasps[spearIndex];
                if (WhatsThatSpear.Normal == otherGrasp || WhatsThatSpear.Electric_NotFull == otherGrasp)
                    SpawnElectricSpear(spearIndex);
                else if (WhatsThatSpear.Electric_Full == otherGrasp)
                    SmallZap();
                else if (WhatsThatSpear.Explosive == otherGrasp)
                    SmallDetonate(spearIndex);
            }
            void HasNormalSpear(int spearBeingChecked, int otherSpearIndex)
            {
                WhatsThatSpear otherGrasp = spearsInGrasps[otherSpearIndex];
                if (WhatsThatSpear.Normal == otherGrasp || WhatsThatSpear.Electric_NotFull == otherGrasp)
                {
                    SpawnElectricSpear(0);
                    SpawnElectricSpear(1);
                }
                else if (WhatsThatSpear.Electric_Full == otherGrasp)
                {
                    SpawnElectricSpear(spearBeingChecked);
                    if (CanCraftTwoSpears)
                        Flash();
                }
                else if (WhatsThatSpear.Explosive == otherGrasp)
                {
                    SpawnElectricSpear(spearBeingChecked);
                    if (CanCraftTwoSpears)
                        SmallDetonate(otherSpearIndex);
                }
            }
            void HasElectricSpear_NotFull(int spearBeingChecked, int otherSpearIndex)
            {
                WhatsThatSpear otherGrasp = spearsInGrasps[otherSpearIndex];
                if (WhatsThatSpear.Electric_NotFull == otherGrasp)
                {
                    SpawnElectricSpear(0);
                    SpawnElectricSpear(1);
                }
                else if (WhatsThatSpear.Electric_Full == otherGrasp)
                {
                    SpawnElectricSpear(spearBeingChecked);
                    if (CanCraftTwoSpears)
                        SmallZap();
                }
                else if (WhatsThatSpear.Explosive == otherGrasp)
                {
                    if (CanCraftTwoSpears)
                    {
                        SmallDetonate(otherSpearIndex);
                        Flash();
                        Stun();
                    }
                    else
                        SpawnElectricSpear(Mathf.Abs(otherSpearIndex - 1));
                }
            }
            void HasElectricSpear_Full(int otherSpearIndex)
            {
                WhatsThatSpear otherGrasp = spearsInGrasps[otherSpearIndex];
                if (WhatsThatSpear.Electric_Full == otherGrasp)
                {
                    if (CanCraftTwoSpears)
                        BigZap();
                    else
                        HasNoneGrasp(Mathf.Abs(otherSpearIndex - 1));
                }
                else if (WhatsThatSpear.Explosive == otherGrasp)
                {
                    if (CanCraftTwoSpears)
                    {
                        //regular full elec-explosive interaction if have enough food to "craft" both
                        BigDetonate(otherSpearIndex); //this already just kills you from the explosion
                        Flash();
                    }
                    else
                        HasNoneGrasp(0); //act like the 2nd hand is empty
                }
            }
            void HasExplosiveSpear(int otherSpearIndex)
            {
                WhatsThatSpear otherGrasp = spearsInGrasps[otherSpearIndex];
                if (WhatsThatSpear.Explosive == otherGrasp)
                {
                    BigDetonateBothGrasps(); //1 pip checking is already in this method
                }
            }
            #endregion

            #region electric stuff
            void SmallZap()
            {
                Debug.Log("Small zap");
                Flash();
                Stun();
            }
            void BigZap()
            {
                Debug.Log("Big zap");
                //like zapcoil
                self.room.PlaySound(SoundID.Zapper_Zap, self.firstChunk.pos, 1f, 1f);
                self.room.AddObject(new ZapCoil.ZapFlash(self.firstChunk.pos, 6f));
                ShortCircuitBothGrasps();
                //kills you
                self.Die();
            }
            void ShortCircuitBothGrasps()
            {
                if (self.grasps[0]?.grabbed is ElectricSpear spear1 && self.grasps[1]?.grabbed is ElectricSpear spear2)
                {
                    Debug.Log("Short circuit both grasps");
                    spear1.ShortCircuit();
                    spear2.ShortCircuit();
                }
                else
                {
                    Debug.Log("Failed to short circuit both grasps! Resorting to short circuiting both grasps indivudually");
                    for (int i = 0; i < 2; i++)
                    {
                        if (self.grasps[i]?.grabbed is ElectricSpear spear)
                            spear.ShortCircuit();
                    }
                }
            }
            #endregion

            #region explosive stuff
            void SmallDetonate(int index)
            {
                Debug.Log("Small detonate");
                SmallDetonateEffects();
                self.room.PlaySound(SoundID.Fire_Spear_Explode, self.firstChunk.pos);
                self.room.InGameNoise(new Noise.InGameNoise(self.firstChunk.pos, 8000f, self, 1f));
                DeleteGrasp(index);
                StunnedFace();
            }
            void BigDetonate(int index)
            {
                if (self.grasps[index]?.grabbed is ExplosiveSpear spear)
                {
                    DetonateEffects();
                    Debug.Log("Big detonate grasp at index " +  index);
                    spear.Explode();
                }
                else
                    Debug.Log("Failed to detonate grasp! Player grasp at index " + index + " is not an explosive spear.");
                //dont even need to call self.Die() tbh
            }
            void BigDetonateBothGrasps()
            {
                //because calling BigDetonate twice only detonates 1
                if (self.grasps[0]?.grabbed is ExplosiveSpear spear1 && self.grasps[1]?.grabbed is ExplosiveSpear spear2)
                {
                    DetonateEffects();
                    Debug.Log("Big detonate both grasps");
                    if (CanCraftTwoSpears)
                        spear2.Explode();
                    spear1.Explode();
                }
                else
                {
                    Debug.Log("Failed to detonate both grasps! Resorting to denoting both grasps indivudually");
                    BigDetonate(0);
                    BigDetonate(1); //if the previous works, this one fails because grasp is null now
                }
            }
            #endregion

            #region cosmetic effects (sort of for Stun())
            void SmallDetonateEffects()
            {
                //yoinked from ExplosiveSpear.MiniExplode()
                self.room.AddObject(new SootMark(self.room, self.firstChunk.pos, 50f, true)); //except this, its from Explode()

                Colour explodeColour = new(1f, 0.4f, 0.3f);
                self.room.AddObject(new Explosion.ExplosionLight(self.firstChunk.pos, 40f, 1f, 2, explodeColour));
                for (int k = 0; k < 8; k++)
                {
                    Vector2 a = RWCustom.Custom.RNV();
                    self.room.AddObject(new Spark(self.firstChunk.pos + a * Random.value * 10f, a * Mathf.Lerp(6f, 18f, Random.value), explodeColour, null, 4, 18));
                }
                self.room.AddObject(new ShockWave(self.firstChunk.pos, 30f, 0.035f, 2, false));
            }
            void DetonateEffects()
            {
                //yoinked from ScavengerBomb
                Colour explodeColour = new(1f, 0.4f, 0.3f);
                self.room.AddObject(new ExplosionSpikes(self.room, self.firstChunk.pos, 14, 30f, 9f, 7f, 170f, explodeColour));
                self.room.AddObject(new ShockWave(self.firstChunk.pos, 330f, 0.045f, 5, false));
                self.room.AddObject(new Smoke.BombSmoke(self.room, self.firstChunk.pos, null, explodeColour));
            }
            void Stun()
            {
                self.Stun(20); //just a little shock
            }
            void StunnedFace()
            {
                (self.graphicsModule as PlayerGraphics).blink = 0;
                self.Blink(130);
            }
            void Flash()
            {
                self.room.AddObject(new Explosion.ExplosionLight(self.firstChunk.pos, 100f, 1f, 10, new Colour(0.7f, 1f, 1f)));
                self.room.PlaySound(SoundID.Death_Lightning_Spark_Spontaneous, self.firstChunk.pos, 1f, 1.5f + Random.value * 1.5f);
            }
            #endregion

            #region crafting
            void DeleteGrasp(int index)
            {
                Debug.Log("Delete player grasp at index " + index);
                AbstractPhysicalObject grasp = self.grasps[index].grabbed.abstractPhysicalObject;

                if (self.room.game.session is StoryGameSession story)
                    story.RemovePersistentTracker(grasp);

                self.ReleaseGrasp(index);

                grasp.LoseAllStuckObjects();
                grasp.realizedObject.RemoveFromRoom();
                self.room.abstractRoom.RemoveEntity(grasp);
            }
            void SpawnElectricSpear(int index)
            {
                if (self.FoodInStomach <= 0)
                    return;

                self.SubtractFood(1);
                self.room.PlaySound(SoundID.Zapper_Zap, self.firstChunk.pos, 1f, 1.5f + Random.value * 1.5f);
                self.room.AddObject(new Explosion.ExplosionLight(self.firstChunk.pos, 80f, 1f, 6, new Colour(0.7f, 1f, 1f)));

                Debug.Log("Spawning new electric spear");

                AbstractPhysicalObject item = new AbstractSpear(self.room.world, null, self.abstractPhysicalObject.pos, self.grasps[index].grabbed.abstractPhysicalObject.ID, false, true);
                self.room.abstractRoom.AddEntity(item);
                item.RealizeInRoom();

                DeleteGrasp(index); //here so that the id can be used for the new spear, but deleted so the new spear can take the old spears place

                if (-1 != self.FreeHand())
                    self.SlugcatGrab(item.realizedObject, self.FreeHand());
            }
            #endregion
        }
    }
}
