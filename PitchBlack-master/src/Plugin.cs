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

    private bool init = false;
    public static ManualLogSource logger;

    public static ConditionalWeakTable<Player, ScugCWT> scugCWT = new();
    public static ConditionalWeakTable<RainWorldGame, List<NTTracker>> NTTrackers = new ConditionalWeakTable<RainWorldGame, List<NTTracker>>();

    // Significantly used colors that would be fine here
    public static readonly Color Rose = new Color(0.82745098039f, 0.10980392156f, 0.29019607843f); // #d31c4a
    public static readonly Color PBAntiGold = new Color(0.355f, 0.31f, 0.87f); // #5b4fdd
    public static readonly Color PBAnti_GoldRGB = new Color(0.20784313725f, 0.18039215686f, 0.52156862745f); // #352e85
    public static readonly Color SaturatedRose = Rose * 2f;
    public static readonly Color SaturatedAntiGold = PBAntiGold * 2f;
    public static readonly Color PBRipple_Color = RainWorld.RippleColor;
    public static readonly Color SaturatedRipple = PBRipple_Color * 2f;
    public static readonly Color beaconDefaultColor = new Color(0.10588235294f, 0.06666666666f, 0.25882352941f);
    public static readonly Color beaconFullColor = new Color(0.2f, 0f, 1f);
    public static readonly Color beaconEyeColor = Color.white;
    public static readonly Color flareColor = new Color(0.2f, 0f, 1f);
    // This is actually assigned in BeaconHooks to the palette black color.
    public static Color beaconDeadColor;

    // Save data
    // NOTE: indev, mess with values for testing
    public static bool canIDoThanatosisYet = true;
    public static float qualiaLevel = 1f;

    // Rotund World stuff
    internal static bool RotundWorldEnabled => _rotundWorldEnabled; //for a single check in BeaconHooks' Player.Update hook
    private static bool _rotundWorldEnabled;
    public static bool individualFoodEnabled = false;

    void FishobsNoWork()
    {
        // These caused problems on the update to 1.9.15, sanction them here
        try
        {
            // NOTE: the POM objects need testing to confirm they work as intended, for now they are commented out to prevent issues -Lur
            //PBPOMSunrays.RegisterLightrays();
            //PBPOMDarkness.RegisterDarkness();
            //ReliableCreatureSpawner.RegisterSpawner();
            //CreatureSpawnerHooks.Apply();
            //BreathableWater.Register();
            //TeleportWater.Register();

            // These error if any namespace uses PitchBlack.Content!!
            Content.Register(new RotRatCritob());
            Content.Register(new FireGrubCritob());
            Content.Register(new LMLLCritob());
            Content.Register(new NightTerrorCritob());
            Content.Register(new ScholarScavCritob());
            Content.Register(new UmbraMaskFisob());
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
        On.DeathPersistentSaveData.ctor += DeathPersistentSaveData_ctor;
        On.AbstractPhysicalObject.Realize += AbstractPhysicalObject_Realize;

        DevHooks.Apply();
        MenuHooks.Apply();
        SyncMenuRegion.Apply();
        CreatureEdits.Apply();
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

    private void AbstractPhysicalObject_Realize(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
    {
        orig(self);
        if (self.type == PBEnums.AbstractObjectType.DreamSpawn)
        {
            self.realizedObject = new VoidSpawn(self,
                (self.Room.realizedRoom != null) ? self.Room.realizedRoom.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidMelt) : 0f,
                self.Room.realizedRoom != null && VoidSpawnKeeper.DayLightMode(self.Room.realizedRoom),
                PBEnums.DreamSpawn.SpawnType.DreamSpawn);
            return;
        }
    }

    private void DeathPersistentSaveData_ctor(On.DeathPersistentSaveData.orig_ctor orig, DeathPersistentSaveData self, SlugcatStats.Name slugcat)
    {
        orig(self, slugcat);
        if (slugcat == PBEnums.SlugcatStatsName.Beacon)
        {
            self.rippleLevel = 1f;
            self.minimumRippleLevel = 1f;
            self.maximumRippleLevel = 5f;
        }
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
            // Register non-sanctioned PBEnums
            PBEnums.SoundID.RegisterValues();
            PBEnums.AbstractObjectType.RegisterValues();
            PBEnums.GhostID.RegisterValues();
            //PBEnums.PlacedObjectType.RegisterValues();

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
            Futile.atlasManager.LoadAtlas("atlases/QualiaSymbols");
            Futile.atlasManager.LoadAtlas("atlases/SidewaysSymbols");
            self.Shaders["Red"] = FShader.CreateShader("red", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath(path: "assetbundles/red")).LoadAsset<Shader>("Assets/red.shader"));
            self.Shaders["Sunrays"] = FShader.CreateShader("sunrays", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/sunrays")).LoadAsset<Shader>("Assets/sunrays.shader"));
            self.Shaders["DreamSpawnBody"] = FShader.CreateShader("dreamspawnbody", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/dreamspawnbody")).LoadAsset<Shader>("Assets/Shaders/DreamSpawnBody.shader"));
            self.Shaders["BlackGlow"] = FShader.CreateShader("blackglow", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/blackglow")).LoadAsset<Shader>("Assets/Shaders/BlackGlow.shader"));
            self.Shaders["DreamerSkin"] = FShader.CreateShader("dreamerskin", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/dreamerskin")).LoadAsset<Shader>("Assets/Shaders/DreamerSkin.shader"));
            self.Shaders["DreamerRag"] = FShader.CreateShader("dreamerrags", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/dreamerrags")).LoadAsset<Shader>("Assets/Shaders/DreamerRag.shader"));
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
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(PBEnums.SandboxUnlockID.NightTerror))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(PBEnums.SandboxUnlockID.NightTerror);

                if (MultiplayerUnlocks.CreatureUnlockList.Contains(PBEnums.SandboxUnlockID.LMiniLongLegs))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(PBEnums.SandboxUnlockID.LMiniLongLegs);

                PBEnums.CreatureTemplateType.UnregisterValues();
                PBEnums.SandboxUnlockID.UnregisterValues();
                PBEnums.SoundID.UnregisterValues();
                PBRoomEffectType.UnregisterValues();
                PBEnums.EndGameID.UnregisterValues();
                PBEnums.SceneID.UnregisterValues();
                PBEnums.AbstractObjectType.UnregisterValues();
                PBEnums.GhostID.UnregisterValues();
                PBEnums.PlacedObjectType.UnregisterValues();
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