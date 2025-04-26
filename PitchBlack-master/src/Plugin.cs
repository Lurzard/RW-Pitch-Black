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
    public static string regionMenuDisplaySavePath = "";
    public static readonly string rootSavePath = Application.persistentDataPath + Path.DirectorySeparatorChar.ToString();
    public static readonly string collectionSaveDataPath = rootSavePath + COLLECTION_SAVE_FOLDER_NAME + Path.DirectorySeparatorChar.ToString() + "PBcollectionsSaveData.txt";
    public static readonly Dictionary<string, bool> collectionSaveData = new Dictionary<string, bool>();
    public static ConditionalWeakTable<RainWorldGame, List<RiftWorldPrecence>> riftCWT = new();

    // I think these don't need to be registered because SlugBase handles it, we just need them to be used locally.
    public static readonly SlugcatStats.Name BeaconName = new("Beacon", false);
    public static readonly SlugcatStats.Name PhotoName = new("Photomaniac", false);
    // From Watcher, used for conditional Beacon world changes
    public static readonly SlugcatStats.Timeline BeaconTime = new("Beacon", false);

    private bool init = false;
    public static ManualLogSource logger;

    public static ConditionalWeakTable<Player, ScugCWT> scugCWT = new();
    public static ConditionalWeakTable<RainWorldGame, List<NTTracker>> NTTrackers = new ConditionalWeakTable<RainWorldGame, List<NTTracker>>();

    // Significantly used colors that would be fine here
    public static Color Rose = new Color(0.82745098039f, 0.10980392156f, 0.29019607843f); // #d31c4a
    public static Color PBAntiGold = new Color(0.355f, 0.31f, 0.87f); // #5b4fdd
    public static Color PBAnti_GoldRGB = new Color(0.20784313725f, 0.18039215686f, 0.52156862745f); // #352e85
    public static Color SaturatedRose = Rose * 2f;
    public static Color SaturatedAntiGold = PBAntiGold * 2f;
    public static Color PBRipple_Color = new Color(0.373f, 0.11f, 0.831f);
    public static Color SaturatedRipple = PBRipple_Color * 2f;

    // Thanatosis
    public static readonly Color beaconDefaultColor = new Color(0.10588235294f, 0.06666666666f, 0.25882352941f);
    public static readonly Color beaconFullColor = new Color(0.2f, 0f, 1f);
    public static readonly Color beaconEyeColor = Color.white;
    public static readonly Color flareColor = new Color(0.2f, 0f, 1f);
    // This is actually assigned in player applypalette
    public static Color beaconDeadColor; /*= new Color(0.05490196078f, 0.03921568627f, 0.10980392156f);*/ //#0e0a1c

    // "Save data" will be plugin variables for now, but should be moved to an actual savedata system that we can work with
    public static bool canIDoThanatosisYet = true; //after dev: false
    public static float qualiaLevel = 10f; //after dev: 0f

    // Rotund World stuffs
    internal static bool RotundWorldEnabled => _rotundWorldEnabled; //for a single check in BeaconHooks' Player.Update hook
    private static bool _rotundWorldEnabled;
    public static bool individualFoodEnabled = false;

    void FishobsNoWork()
    {
        // These caused problems on the update to 1.9.15, sanction them here
        try
        {
            //PBPOMSunrays.RegisterLightrays();
            //PBPOMDarkness.RegisterDarkness();
            //ReliableCreatureSpawner.RegisterSpawner();
            //CreatureSpawnerHooks.Apply();
            //BreathableWater.Register();
            //TeleportWater.Register();
            Content.Register(new RotRatCritob());
            Content.Register(new FireGrubCritob());
            Content.Register(new LMLLCritob());
            Content.Register(new NightTerrorCritob());
            Content.Register(new ScholarScavCritob());
            Content.Register(new UmbraMaskFisob());
            PBSoundID.RegisterValues();
            PBRoomEffectType.RegisterValues();
            PBEndGameID.RegisterValues();
            PBSceneID.RegisterValues();
        }
        catch (Exception err)
        {
            //Debug.LogError(err); Debug errors
            Logger.LogError(err);
        }
    }

    public void OnEnable()
    {
        logger = base.Logger;

        On.RainWorld.OnModsInit += OnModsInit;
        On.RainWorld.OnModsDisabled += DisableMod;
        On.RainWorld.PostModsInit += RainWorld_PostModsInit;
        On.RainWorldGame.ctor += RainWorldGame_ctor;
        On.RainWorldGame.Update += RainWorldGame_Update;
        On.Weapon.SetRandomSpin += Weapon_SetRandomSpin;
        On.RainWorld.UnloadResources += (orig, self) =>
        {
            orig(self);
            if (Futile.atlasManager.DoesContainAtlas("lmllspr"))
                Futile.atlasManager.UnloadAtlas("lmllspr");
        };
        DevHooks.Apply();
        MenuHooks.Apply();
        SyncMenuRegion.Apply();
        CreatureEdits.Apply();
        EchoHooks.Apply();
        JollyMenuHooks.Apply();
        ScugGraphics.Apply();
        MoonDialogue.Apply();
        OverseerGraphics.Apply();
        OverseerHooks.Apply();
        BeaconHooks.Apply();
        PhotoHooks.Apply();
        Crafting.Apply();
        FlarebombHooks.Apply();
        ScugHooks.Apply();
        DevCommOverride.Apply();
        PassageHooks.Apply();
        SpecialChanges.Apply();
        RoomScripts.Apply();
        WorldChanges.Apply();
        ScareEverything.Apply();
    }

    public void Update()
    {
        if (Input.anyKeyDown)
        {
            foreach (char c in Input.inputString)
            {
                InputChecker.AddInput(c);
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            InputChecker.AddInput('\u2190');
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            InputChecker.AddInput('\u2191');
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            InputChecker.AddInput('\u2192');
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            InputChecker.AddInput('\u2193');
        }
    }
    public void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        MachineConnector.SetRegisteredOI("lurzard.pitchblack", PBRemixMenu.Instance);
        if (!init)
        {

            regionMenuDisplaySavePath = ModManager.ActiveMods.First(x => x.id == MOD_ID).path + Path.DirectorySeparatorChar + "RegionMenuDisplay.txt";

            #region Load Collection data into readonly list
            try
            {
                // Creates a new directory and file if they do not exist (which they won't the first time the game is booted up), and fills it with default data.
                if (!Directory.Exists(rootSavePath + COLLECTION_SAVE_FOLDER_NAME))
                {
                    Directory.CreateDirectory(rootSavePath + COLLECTION_SAVE_FOLDER_NAME);
                }
                if (!File.Exists(collectionSaveDataPath))
                {
                    string defaultText = "";
                    foreach (var name in PBCollectionMenu.chatLogIDToButtonName.Keys)
                    {
                        defaultText += name + ":0|";
                    }
                    File.WriteAllText(collectionSaveDataPath, defaultText);
                }
                foreach (string text in File.ReadAllText(collectionSaveDataPath).Trim('|').Split('|'))
                {
                    // If the second part is a 1, it is unlocked, 0 (or anything else) is locked
                    bool unlocked = text.Split(':')[1] == "1";
                    collectionSaveData.Add(text.Split(':')[0], unlocked);
                }
            }
            catch (Exception err)
            {
                Debug.LogError($"Pitch Black Error with collection file read/write.\n{err}");
                throw err;
            }
            #endregion

            try
            {
                FishobsNoWork();
            }
            catch (Exception err)
            {
                Debug.Log($"Pitch Black error\n{err}");
                logger.LogDebug($"Pitch Black error\n{err}");
            }
            if (!Futile.atlasManager.DoesContainAtlas("lmllspr"))
                Futile.atlasManager.LoadAtlas("atlases/lmllspr");
            Futile.atlasManager.LoadAtlas("atlases/photosplt");
            Futile.atlasManager.LoadAtlas("atlases/nightTerroratlas");
            Futile.atlasManager.LoadAtlas("atlases/PursuedAtlas");
            Futile.atlasManager.LoadAtlas("atlases/pearlCursor");
            Futile.atlasManager.LoadAtlas("atlases/PBHat");
            Futile.atlasManager.LoadAtlas("atlases/UmbraScav");
            Futile.atlasManager.LoadAtlas("atlases/UmbraMask");
            Futile.atlasManager.LoadAtlas("atlases/icon_UmbraMask");
            //Futile.atlasManager.LoadAtlas("atlases/smallKarma10-10");
            //Futile.atlasManager.LoadAtlas("atlases/karma10-10");
            Futile.atlasManager.LoadAtlas("atlases/toriiEye");
            //Futile.atlasManager.LoadAtlas("atlases/FaceThanatosis");
            self.Shaders["PurpleEchoSkin"] = FShader.CreateShader("purpleechoskin", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/purpleecho")).LoadAsset<Shader>("Assets/shaders 1.9.03/PurpleEchoSkin.shader"));
            self.Shaders["Red"] = FShader.CreateShader("red", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath(path: "assetbundles/red")).LoadAsset<Shader>("Assets/red.shader"));
            self.Shaders["Sunrays"] = FShader.CreateShader("sunrays", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/sunrays")).LoadAsset<Shader>("Assets/sunrays.shader"));
            self.Shaders["DeathSpawnBody"] = FShader.CreateShader("deathspawnbody", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/deathspawnbody")).LoadAsset<Shader>("Assets/Shaders/DeathSpawnBody.shader"));
            self.Shaders["DeathGlow"] = FShader.CreateShader("deathglow", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/deathglow")).LoadAsset<Shader>("Assets/Shaders/DeathGlow.shader"));
            init = true;
            //RiftCosmetic.Register(self);
        }
    }
    private void Weapon_SetRandomSpin(On.Weapon.orig_SetRandomSpin orig, Weapon self)
    {
        if (self.room == null) { return; }
        try
        {
            orig(self);
        }
        catch (Exception err)
        {
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
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(PBSandboxUnlockID.NightTerror))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(PBSandboxUnlockID.NightTerror);

                if (MultiplayerUnlocks.CreatureUnlockList.Contains(PBSandboxUnlockID.LMiniLongLegs))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(PBSandboxUnlockID.LMiniLongLegs);

                PBCreatureTemplateType.UnregisterValues();
                PBSandboxUnlockID.UnregisterValues();
                PBSoundID.UnregisterValues();
                PBRoomEffectType.UnregisterValues();
                PBEndGameID.UnregisterValues();
                PBSceneID.UnregisterValues();
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
        //riftCWT.Add(self, new List<RiftWorldPrecence>());
        if ((IsBeacon(self.session) || PBRemixMenu.universalPursuer.Value) && NTTrackers.TryGetValue(self, out var trackers))
        {
            trackers.Add(new NTTracker(self));
            Debug.Log("ADDING TRACKER");
        }

    }
}