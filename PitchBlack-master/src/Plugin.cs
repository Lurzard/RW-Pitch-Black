using BepInEx;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using System.Security;
using Fisobs.Core;
using UnityEngine;
using BepInEx.Logging;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]
#pragma warning restore CS0618 // Type or member is obsolete

namespace PitchBlack;
[BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
class Plugin : BaseUnityPlugin
{
    public const string MOD_ID = "lurzard.pitchblack";
    public const string MOD_NAME = "Pitch Black";
    public const string MOD_VERSION = "0.1.0";

    public static readonly SlugcatStats.Name BeaconName = new("Beacon", false);
    public static readonly SlugcatStats.Name PhotoName = new("Photomaniac", false);
    private bool init = false;

    //public static ConditionalWeakTable<Player, BeaconCWT> bCon = new ConditionalWeakTable<Player, BeaconCWT>();
    //public static ConditionalWeakTable<Player, PhotoCWT> pCon = new ConditionalWeakTable<Player, PhotoCWT>();
    public static ConditionalWeakTable<Player, ScugCWT> scugCWT = new();

    internal static bool RotundWorldEnabled => _rotundWorldEnabled; //for a single check in BeaconHooks' Player.Update hook
    private static bool _rotundWorldEnabled;
    public static bool individualFoodEnabled = false;

    public static ManualLogSource logger;

    //public static List<string> currentDialog = new();
    //public static bool Speaking = false;
    //public static AbstractCreature PBOverseer;
    //public static int pbcooldown = 0;
    public void OnEnable()
    {
        logger = base.Logger;

        On.RainWorld.OnModsInit += OnModsInit;
        On.RainWorld.OnModsDisabled += DisableMod;
        On.RainWorld.PostModsInit += RainWorld_PostModsInit;

        Content.Register(new NightTerrorCritob());
        ScareEverything.Apply();

        ScugHooks.Apply();
        ScugGraphics.Apply();

        BeaconHooks.Apply();
        PhotoHooks.Apply();
        Crafting.Apply();
        
        PBOverseerGraphics.Apply();

        FlarebombHooks.Apply();

        RoomScripts.Apply();
        WorldChanges.Apply();

        JollyMenuHooks.Apply();

        DevCommOverride.Apply();
        OhNoMoonAndPebblesAreDeadGuys.Apply();
        
        // Make a reference to "...\steamapps\workshop\content\312520\2920439169\plugins\Pom.dll" in the csproj for these to work
            // Also to here "...\steamapps\common\Rain World\RainWorld_Data\Managed\UnityEngine.AssetBundleModule.dll"
        PBPOMSunrays.RegisterLightrays();
        PBPOMDarkness.RegisterDarkness();
        ReliableCreatureSpawner.RegisterSpawner();
        CreatureSpawnerHooks.Apply();

        //PBFrozenCycleTimer.Apply();

        //NightDay.Apply(); //unfinished
        PassageHooks.Apply();

        //On.Player.GraspsCanBeCrafted += Player_GraspsCanBeCrafted;
        //On.Player.SpitUpCraftedObject += Player_SpitUpCraftedObject;
        //On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
        //On.Player.CanBeSwallowed += Player_CanBeSwallowed;
        //On.Player.SwallowObject += Player_SwallowObject1;
        //On.Player.Grabability += GrabCoalescipedes;
    }
    public void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        MachineConnector.SetRegisteredOI("lurzard.pitchblack", PBOptions.Instance);
        if (!init) {
            Futile.atlasManager.LoadAtlas("atlases/photosplt");
            Futile.atlasManager.LoadAtlas("atlases/nightTerroratlas");
            Futile.atlasManager.LoadAtlas("atlases/PursuedAtlas");
            self.Shaders["Red"] = FShader.CreateShader("red", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath(path: "assetbundles/red")).LoadAsset<Shader>("Assets/red.shader"));
            self.Shaders["Sunrays"] = FShader.CreateShader("sunrays", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/sunrays")).LoadAsset<Shader>("Assets/sunrays.shader"));
            init = true;

            //I'M PRETTY SURE BEST PRACTICE IS TO PUT HOOKS HERE
            On.RainWorldGame.ctor += RainWorldGame_ctor;
            On.RainWorldGame.Update += RainWorldGame_Update;

        }
    }

    public static void DisableMod(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
    {
        orig(self, newlyDisabledMods);

        foreach (var mod in newlyDisabledMods)
        {
            if (mod.id == MOD_ID)
            {
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.NightTerror))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.NightTerror);

                CreatureTemplateType.UnregisterValues();
                SandboxUnlockID.UnregisterValues();
            }
        }
    }

    private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        orig(self);

        foreach (var mod in ModManager.ActiveMods)
        {
            if (mod.id == "willowwisp.bellyplus")
            {
                _rotundWorldEnabled = true;
            }
            else if (mod.id == "dressmyslugcat")
            {
                DMSPatch.AddSpritesToDMS();
            }
            else if (mod.id == "sprobgik.individualfoodbars")
            {
                individualFoodEnabled = true;
            }
        }
    }


    public static NTTracker myTracker;// IDRK WHAT TO DO WITH THIS

    private static void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        orig(self);
        //Debug.Log("UPDATEME");
        if (myTracker is not null)
            myTracker.Update();
    }

    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);

        if (self.IsStorySession)
        {
            myTracker = new NTTracker(self);
            Debug.Log("ADDING TRACKER");
        }

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