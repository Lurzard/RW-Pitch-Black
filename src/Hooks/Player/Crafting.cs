using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using AbstractObjectType = AbstractPhysicalObject.AbstractObjectType;

namespace PitchBlack;

public static class Crafting
{
    /// <summary>
    /// Function that deletes object in grasp
    /// </summary>
    /// <param name="index">The hand an object is grabbed in</param>
    public static void DeleteGrasp(this Player self, int index)
    {
        AbstractPhysicalObject grasp = self.grasps[index].grabbed.abstractPhysicalObject;

        if (grasp == null)
        {
            //Debug.log($"Unable to delete null player grasp at index {index}");
            return;
        }

        //Debug.log($"Delete player grasp at index {index}");

        if (self.room.game.session is StoryGameSession story)
            story.RemovePersistentTracker(grasp);

        self.ReleaseGrasp(index);

        grasp.LoseAllStuckObjects();
        grasp.realizedObject.RemoveFromRoom();
        self.room.abstractRoom.RemoveEntity(grasp);
    }
    
    /// <summary>
    /// Actual crafting process for creating flares from rock objects
    /// [spinch]
    /// </summary>
    public static void BeaconCrafting(this Player self) {
        if (Plugin.scugCWT.TryGetValue(self, out ScugCWT scugCWT) && scugCWT is BeaconCWT beaconCWT) {
            // craft rocks into flarebombs instead of swallowing to do that
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
    
    public static void Apply()
    {
        IL.Player.GrabUpdate += Player_GrabUpdate;
        On.Player.GraspsCanBeCrafted += Player_GraspsCanBeCrafted;
        IL.Player.SpitUpCraftedObject += Player_SpitUpCraftedObject;
    }
    
    public static void Player_SpitUpCraftedObject(ILContext il)
    {
        ILCursor c = new(il);
        ILLabel label = il.DefineLabel();

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate((Player self) =>
        {
            return Enums.SlugcatStatsName.Photomaniac == self.slugcatStats.name;
        });
        c.Emit(OpCodes.Brfalse, label);

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate((Player self) =>
        {
            if (Enums.SlugcatStatsName.Beacon == self.slugcatStats.name)
            {
                self.BeaconCrafting();   
            }
        });
        c.Emit(OpCodes.Ret);

        c.MarkLabel(label);
    }

    /// <summary>
    /// Implements object crafting for Beacon and safeguards crafting from nuances
    /// [spinch]
    /// </summary>
    /// <param name="orig">MSC function for determining what slugcats/conditions allow crafting</param>
    /// <param name="self"></param>
    /// <returns>canCraft bool which conditionally greenlights the crafting code we have</returns>
    public static bool Player_GraspsCanBeCrafted(On.Player.orig_GraspsCanBeCrafted orig, Player self)
    {
        var val = orig(self);

        if (self.slugcatStats.name == Enums.SlugcatStatsName.Beacon)
        {
            if (self.grasps[0]?.grabbed != null
                && self.CanBeSwallowed(self.grasps[0].grabbed)
                && self.grasps[0].grabbed is not Rock)
            {
                // so you can swallow
                return false;
            }

            bool canCraft = false;

            for (int i = 0; i < 2; i++)
            {
                PhysicalObject grabbed = self.grasps[i]?.grabbed;

                if (null == grabbed)
                {
                    continue;
                }

                // if its food and youre NOT full, you CANT craft
                if (grabbed is IPlayerEdible && self.FoodInStomach < self.MaxFoodInStomach)
                {
                    return false;
                }

                if (grabbed is Rock or WaterNut or Lantern)
                {
                    // you have food, yippee you can craft
                    if (self.FoodInStomach > 0)
                    {
                        canCraft = true;
                    }

                    else if (self.swallowAndRegurgitateCounter > 10
                             && !(Plugin.individualFoodEnabled && ModManager.CoopAvailable))
                    {
                        // not enough food, show warning
                        FlareStorage.foodWarning = 20;
                    }
                }
            }

            return canCraft;
        }

        return val;
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
                return self.slugcatStats.name == Enums.SlugcatStatsName.Beacon;
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
}