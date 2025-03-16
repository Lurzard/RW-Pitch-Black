using BepInEx;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using System.Security;
using Fisobs.Core;
using UnityEngine;
using BepInEx.Logging;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using static PitchBlack.MiscUtils;

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
    private const string COLLECTION_SAVE_FOLDER_NAME = "PitchBlack";
#if PLAYTEST
    public static string regionMenuDisplaySavePath = "";
    public static readonly string rootSavePath = Application.persistentDataPath + Path.DirectorySeparatorChar.ToString(); 
    public static readonly string collectionSaveDataPath = rootSavePath + COLLECTION_SAVE_FOLDER_NAME + Path.DirectorySeparatorChar.ToString() + "PBcollectionsSaveData.txt";
    public static readonly Dictionary<string, bool> collectionSaveData = new Dictionary<string, bool>();
    public static ConditionalWeakTable<RainWorldGame, List<RiftWorldPrecence>> riftCWT = new();
#endif

    public static readonly SlugcatStats.Name BeaconName = new("Beacon", false);
    public static readonly SlugcatStats.Name PhotoName = new("Photomaniac", false);
    private bool init = false;

    //public static ConditionalWeakTable<Player, BeaconCWT> bCon = new ConditionalWeakTable<Player, BeaconCWT>();
    //public static ConditionalWeakTable<Player, PhotoCWT> pCon = new ConditionalWeakTable<Player, PhotoCWT>();
    public static ConditionalWeakTable<Player, ScugCWT> scugCWT = new();
    public static ConditionalWeakTable<RainWorldGame, List<NTTracker>> NTTrackers = new ConditionalWeakTable<RainWorldGame, List<NTTracker>>();


    internal static bool RotundWorldEnabled => _rotundWorldEnabled; //for a single check in BeaconHooks' Player.Update hook
    private static bool _rotundWorldEnabled;
    public static bool individualFoodEnabled = false;

    public static ManualLogSource logger;
    void FishobsNoWork() {
        // These caused problems on the update to 1.9.15, sanction them here
        try {
            PBPOMSunrays.RegisterLightrays();
            PBPOMDarkness.RegisterDarkness();
            ReliableCreatureSpawner.RegisterSpawner();
            CreatureSpawnerHooks.Apply();
            BreathableWater.Register();
#if PLAYTEST
            TeleportWater.Register();
            Content.Register(new RotRatCritob());
            Content.Register(new FireGrubCritob());
#endif
            Content.Register(new LMLLCritob());
            Content.Register(new NightTerrorCritob());
            Content.Register(new UmbraScavCritob());
        } catch (Exception err) {
            //Debug.LogError(err); Debug errors
            Logger.LogError(err);
        }
    }

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
        On.RainWorld.UnloadResources += (orig, self) =>
        {
            orig(self);
            if (Futile.atlasManager.DoesContainAtlas("lmllspr"))
                Futile.atlasManager.UnloadAtlas("lmllspr");
        };

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

        //NightDay.Apply(); //scrapped
        PassageHooks.Apply();

#if PLAYTEST
        //EchoMusic.Apply();
        //EchoGraphics.Apply();
        MenuHooks.Apply();
        SyncMenuRegion.Apply();
        PBFrozenCycleTimer.Apply();
        OverseerHooks.Apply();
        SpecialChanges.Apply();
#endif


        //On.Player.GraspsCanBeCrafted += Player_GraspsCanBeCrafted;
        //On.Player.SpitUpCraftedObject += Player_SpitUpCraftedObject;
        //On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
        //On.Player.CanBeSwallowed += Player_CanBeSwallowed;
        //On.Player.SwallowObject += Player_SwallowObject1;
        //On.Player.Grabability += GrabCoalescipedes;
    }

    public void Update() {
        if (Input.anyKeyDown) {
            foreach (char c in Input.inputString) {
                InputChecker.AddInput(c);
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            InputChecker.AddInput('\u2190');
        }
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            InputChecker.AddInput('\u2191');
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            InputChecker.AddInput('\u2192');
        }
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            InputChecker.AddInput('\u2193');
        }
    }
    public void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        MachineConnector.SetRegisteredOI("lurzard.pitchblack", PBOptions.Instance);
        if (!init) {

#if PLAYTEST
            regionMenuDisplaySavePath = ModManager.ActiveMods.First(x => x.id == MOD_ID).path + Path.DirectorySeparatorChar + "RegionMenuDisplay.txt";

            #region Load Collection data into readonly list
            try {
                // Creates a new directory and file if they do not exist (which they won't the first time the game is booted up), and fills it with default data.
                if (!Directory.Exists(rootSavePath + COLLECTION_SAVE_FOLDER_NAME)) {
                    Directory.CreateDirectory(rootSavePath + COLLECTION_SAVE_FOLDER_NAME);
                }
                if (!File.Exists(collectionSaveDataPath)) {
                    string defaultText = "";
                    foreach (var name in PitchBlackCollectionMenu.chatLogIDToButtonName.Keys) {
                        defaultText += name + ":0|";
                    }
                    File.WriteAllText(collectionSaveDataPath, defaultText);
                }
                foreach (string text in File.ReadAllText(collectionSaveDataPath).Trim('|').Split('|')) {
                    // If the second part is a 1, it is unlocked, 0 (or anything else) is locked
                    bool unlocked = text.Split(':')[1] == "1";
                    collectionSaveData.Add(text.Split(':')[0], unlocked);
                }
            } catch (Exception err) {
                Debug.LogError($"Pitch Black Error with collection file read/write.\n{err}");
                throw err;
            }
            #endregion
            try {
                FishobsNoWork();
            } catch (Exception err) {
                Debug.Log($"Pitch Black error\n{err}");
                logger.LogDebug($"Pitch Black error\n{err}");
            }
#endif

            if (!Futile.atlasManager.DoesContainAtlas("lmllspr"))
                Futile.atlasManager.LoadAtlas("atlases/lmllspr");
            Futile.atlasManager.LoadAtlas("atlases/photosplt");
            Futile.atlasManager.LoadAtlas("atlases/nightTerroratlas");
            Futile.atlasManager.LoadAtlas("atlases/PursuedAtlas");
#if PLAYTEST
            Futile.atlasManager.LoadAtlas("atlases/pearlCursor");
            Futile.atlasManager.LoadAtlas("atlases/PBHat");
            Futile.atlasManager.LoadAtlas("atlases/UmbraScav");
            Futile.atlasManager.LoadAtlas("atlases/UmbraMask");
            Futile.atlasManager.LoadAtlas("atlases/icon_UmbraMask");
            self.Shaders["PurpleEchoSkin"] = FShader.CreateShader("purpleechoskin", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/purpleecho")).LoadAsset<Shader>("Assets/shaders 1.9.03/PurpleEchoSkin.shader"));
#endif
            self.Shaders["Red"] = FShader.CreateShader("red", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath(path: "assetbundles/red")).LoadAsset<Shader>("Assets/red.shader"));
            self.Shaders["Sunrays"] = FShader.CreateShader("sunrays", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/sunrays")).LoadAsset<Shader>("Assets/sunrays.shader"));
            init = true;

            //I'M PRETTY SURE BEST PRACTICE IS TO PUT HOOKS HERE
            On.RainWorldGame.ctor += RainWorldGame_ctor;
            On.RainWorldGame.Update += RainWorldGame_Update;
            On.Weapon.SetRandomSpin += Weapon_SetRandomSpin;

            RiftCosmetic.Register(self);
        }
    }
    private void Weapon_SetRandomSpin(On.Weapon.orig_SetRandomSpin orig, Weapon self)
    {
        if (self.room == null) { return; }
        try {
            orig(self);
        } catch (Exception err) {
            Debug.LogError($"Pitch Black, Caught exception in {nameof(Weapon.SetRandomSpin)}.\n{err}");
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

                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.LMiniLongLegs))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.LMiniLongLegs);

                CreatureTemplateType.UnregisterValues();
                SandboxUnlockID.UnregisterValues();

                break;
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
    private static void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        orig(self);
        if (NTTrackers.TryGetValue(self, out List<NTTracker> trackers)) foreach (NTTracker tracker in trackers) tracker.Update();
    }
    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);
        NTTrackers.Add(self, new List<NTTracker>());
#if PLAYETST
        riftCWT.Add(self, new List<RiftWorldPrecence>());
#endif
        if ((IsBeacon(self.session) || PBOptions.universalPursuer.Value) && NTTrackers.TryGetValue(self, out var trackers))
        {
            trackers.Add(new NTTracker(self));
            Debug.Log("ADDING TRACKER");
        }

    }
}