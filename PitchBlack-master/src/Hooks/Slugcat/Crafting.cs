using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using Colour = UnityEngine.Color;
using Random = UnityEngine.Random;
using MoreSlugcats;
using AbstractObjectType = AbstractPhysicalObject.AbstractObjectType;

namespace PitchBlack;

public static class Crafting
{
    //spinch: i am the lord of the crafting code
    enum WhatsThatSpear
    {
        None,
        Normal,
        Electric_NotFull,
        Electric_Full,
        Explosive
    }
    
    public static void Apply()
    {
        IL.Player.GrabUpdate += Player_GrabUpdate;
        On.Player.GraspsCanBeCrafted += Player_GraspsCanBeCrafted;
        IL.Player.SpitUpCraftedObject += Player_SpitUpCraftedObject;
    }

    public static void Player_GrabUpdate(ILContext il)
    {
        try
        {
            ILCursor c = new(il);
            ILLabel label = il.DefineLabel();

            if (!c.TryGotoNext(MoveType.After, i => i.MatchCallOrCallvirt<Player>("FreeHand"), i => i.MatchLdcI4(-1), i => i.Match(OpCodes.Beq_S)))
            {
                return;
            }

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<Player, bool>>(self =>
            {
                return self.slugcatStats.name == PBEnums.SlugcatStatsName.Photomaniac || PBEnums.SlugcatStatsName.Beacon == self.slugcatStats.name;
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

    public static bool Player_GraspsCanBeCrafted(On.Player.orig_GraspsCanBeCrafted orig, Player self)
    {
        bool val = orig(self);

        if (PBEnums.SlugcatStatsName.Photomaniac == self.slugcatStats.name || PBEnums.SlugcatStatsName.Beacon == self.slugcatStats.name)
        {
            //if (self.FoodInStomach <= 0) //cant craft on an empty stomach
            //    return false;

            if (PBEnums.SlugcatStatsName.Beacon == self.slugcatStats.name
                && self.grasps[0]?.grabbed != null && self.CanBeSwallowed(self.grasps[0].grabbed)
                && self.grasps[0].grabbed is not Rock)
            {
                //so you can swallow
                return false;
            }

            bool canCraft = false;

            for (int i = 0; i < 2; i++)
            {
                PhysicalObject grabbed = self.grasps[i]?.grabbed;

                if (null == grabbed)
                    continue;

                if (self.CanBeSwallowed(grabbed))
                {
                    //if you can swallow an item, you cant craft
                    if (PBEnums.SlugcatStatsName.Photomaniac == self.slugcatStats.name)
                        return false;
                }

                if (grabbed is IPlayerEdible && self.FoodInStomach < self.MaxFoodInStomach) //if its food and youre NOT full, you CANT craft
                    return false;

                if (PBEnums.SlugcatStatsName.Photomaniac == self.slugcatStats.name)
                {
                    if (grabbed.abstractPhysicalObject is AbstractSpear spearInHand)
                    {
                        if (spearInHand.hue > 0) //no fire spear crafting allowed
                            continue;
                        if (spearInHand.electric && spearInHand.electricCharge < 3) //ignore unfull elec spear
                            continue;
                        if (self.FoodInStomach > 0) //you have food, yippee you can craft
                            canCraft = true;
                        else if (self.swallowAndRegurgitateCounter > 10 && !(Plugin.individualFoodEnabled && ModManager.CoopAvailable)) //IF WE'VE BEEN HOLDING FOR A BIT
                            BeaconHooks.foodWarning = 20; //NOT ENOUGH FOOD! ALSO SHOW A HUD WARNING
                    }
                }
                else if (PBEnums.SlugcatStatsName.Beacon == self.slugcatStats.name)
                {
                    if (grabbed is Rock || grabbed is WaterNut || grabbed is Lantern )
                    {
                        if (self.FoodInStomach > 0) //you have food, yippee you can craft
                            canCraft = true;
                        else if (self.swallowAndRegurgitateCounter > 10 && !(Plugin.individualFoodEnabled && ModManager.CoopAvailable))
                            BeaconHooks.foodWarning = 20; //NOT ENOUGH FOOD! ALSO SHOW A HUD WARNING
                    }
                }
            }

            return canCraft;
        }

        return val;
    }

    public static void Player_SpitUpCraftedObject(ILContext il)
    {
        ILCursor c = new(il);
        ILLabel label = il.DefineLabel();

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate((Player self) =>
        {
            return PBEnums.SlugcatStatsName.Photomaniac == self.slugcatStats.name || PBEnums.SlugcatStatsName.Beacon == self.slugcatStats.name;
        });
        c.Emit(OpCodes.Brfalse, label);

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate((Player self) =>
        {
            if (PBEnums.SlugcatStatsName.Photomaniac == self.slugcatStats.name)
                self.PhotoCrafting();
            else if (PBEnums.SlugcatStatsName.Beacon == self.slugcatStats.name)
                self.BeaconCrafting();
        });
        c.Emit(OpCodes.Ret);

        c.MarkLabel(label);
    }

    public static void PhotoCrafting(this Player self)
    {
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
                else if (spear.hue <= 0) //NO fire spears allowed
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
            if (WhatsThatSpear.Normal == otherGrasp)
                SpawnElectricSpear(spearIndex);
            else if (WhatsThatSpear.Electric_Full == otherGrasp)
                SmallZap();
            else if (WhatsThatSpear.Explosive == otherGrasp)
                SmallDetonate(spearIndex);
            //nothing happens if (WhatsThatSpear.Electric_NotFull == otherGrasp)
        }
        void HasNormalSpear(int spearBeingChecked, int otherSpearIndex)
        {
            WhatsThatSpear otherGrasp = spearsInGrasps[otherSpearIndex];
            if (WhatsThatSpear.Normal == otherGrasp)
            {
                SpawnElectricSpear(0);
                SpawnElectricSpear(1);
            }
            else if (WhatsThatSpear.Electric_NotFull == otherGrasp)
            {
                //so 1 normal spear, 1 unfull elec spear
                //behaviour ignores the unfull elec spear and acts like its 1 normal spear and null grasp
                HasNoneGrasp(spearBeingChecked);
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
            //if (WhatsThatSpear.Electric_NotFull == otherGrasp)
            //{
            //    SpawnElectricSpear(0);
            //    SpawnElectricSpear(1);
            //} //nothing happens here
            if (WhatsThatSpear.Electric_Full == otherGrasp)
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
                    SpawnElectricSpear(spearBeingChecked);
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
            // Debug.log("Small zap");
            Flash();
            Stun();
        }
        void BigZap()
        {
            // Debug.log("Big zap");
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
                // Debug.log("Short circuit both grasps");
                spear1.ShortCircuit();
                spear2.ShortCircuit();
            }
            else
            {
                // Debug.log("Failed to short circuit both grasps! Resorting to short circuiting both grasps indivudually");
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
            // Debug.log("Small detonate");
            SmallDetonateEffects();
            self.room.PlaySound(SoundID.Fire_Spear_Explode, self.firstChunk.pos);
            self.room.InGameNoise(new Noise.InGameNoise(self.firstChunk.pos, 8000f, self, 1f));
            self.DeleteGrasp(index);
            StunnedFace();
        }
        void BigDetonate(int index)
        {
            if (self.grasps[index]?.grabbed is ExplosiveSpear spear)
            {
                DetonateEffects();
                // Debug.log($"Big detonate grasp at index {index}");
                spear.Explode();
            }
            // else
                // Debug.log($"Failed to detonate grasp! Player grasp at index {index} is not an explosive spear.");
            //dont even need to call self.Die() tbh
        }
        void BigDetonateBothGrasps()
        {
            //because calling BigDetonate twice only detonates 1
            if (self.grasps[0]?.grabbed is ExplosiveSpear spear1 && self.grasps[1]?.grabbed is ExplosiveSpear spear2)
            {
                DetonateEffects();
                // Debug.log("Big detonate both grasps");
                if (CanCraftTwoSpears)
                    spear2.Explode();
                spear1.Explode();
            }
            else
            {
                // Debug.log("Failed to detonate both grasps! Resorting to denoting both grasps indivudually");
                BigDetonate(0);
                BigDetonate(1); //if the previous works, this one fails because grasp is null now
            }
        }
        #endregion

        #region cosmetic effects (sort of for Stun)
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
        void SpawnElectricSpear(int index)
        {
            if (self.FoodInStomach <= 0)
                return;

            //for loop is an extra added cosmetic. its the same effect as electric spears occasionally sparking
            //feel free to delete the for loop if you don't like it
            for (int i = 0; i < 10; i++)
            {
                Vector2 a = RWCustom.Custom.RNV();
                self.room.AddObject(new Spark(self.firstChunk.pos + a * Random.value * 20f, a * Mathf.Lerp(4f, 10f, Random.value), Colour.white, null, 4, 18));
            }

            self.SubtractFood(1);
            self.room.PlaySound(SoundID.Zapper_Zap, self.firstChunk.pos, 1f, 1.5f + Random.value * 1.5f);
            self.room.AddObject(new Explosion.ExplosionLight(self.firstChunk.pos, 80f, 1f, 6, new Colour(0.7f, 1f, 1f)));

            // Debug.log("Spawning new electric spear");

            AbstractPhysicalObject item = new AbstractSpear(self.room.world, null, self.abstractPhysicalObject.pos, self.grasps[index].grabbed.abstractPhysicalObject.ID, false, true);
            self.room.abstractRoom.AddEntity(item);
            item.RealizeInRoom();

            self.DeleteGrasp(index); //here so that the id can be used for the new spear, but deleted so the new spear can take the old spears place

            if (-1 != self.FreeHand())
                self.SlugcatGrab(item.realizedObject, self.FreeHand());
        }
        #endregion
    }

    public static void BeaconCrafting(this Player self) {
        if (Plugin.scugCWT.TryGetValue(self, out ScugCWT scugCWT) && scugCWT is BeaconCWT beaconCWT) {
            //craft rocks into flarebombs instead of swallowing to do that
            for (int i = 0; i < self.grasps.Length; i++) {
                if (self.FoodInStomach <= 0) {
                    break;
                }

                if (self.grasps[i]?.grabbed is Rock) {
                    self.SubtractFood(1);
                    self.DeleteGrasp(i);

                    AbstractConsumable item = new(self.room.world, AbstractObjectType.FlareBomb, null, self.abstractCreature.pos, self.room.game.GetNewID(), -1, -1, null);
                    self.room.abstractRoom.AddEntity(item);
                    item.RealizeInRoom();
                    self.SlugcatGrab(item.realizedObject, i);

                    if (beaconCWT.storage.storedFlares.Count < beaconCWT.storage.capacity) {
                        beaconCWT.storage.FlarebombtoStorage(item.realizedObject as FlareBomb);
                        beaconCWT.heldCraft = true;
                    }
                    else if (self.FreeHand() != -1)
                        self.SlugcatGrab(item.realizedObject, self.FreeHand());
                }
            }
        }
    }

    public static void DeleteGrasp(this Player self, int index)
    {
        AbstractPhysicalObject grasp = self.grasps[index].grabbed.abstractPhysicalObject;

        if (grasp == null)
        {
            // Debug.log($"Unable to delete null player grasp at index {index}");
            return;
        }

        // Debug.log($"Delete player grasp at index {index}");

        if (self.room.game.session is StoryGameSession story)
            story.RemovePersistentTracker(grasp);

        self.ReleaseGrasp(index);

        grasp.LoseAllStuckObjects();
        grasp.realizedObject.RemoveFromRoom();
        self.room.abstractRoom.RemoveEntity(grasp);
    }
}
