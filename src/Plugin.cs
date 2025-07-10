using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
using Fisobs.Core;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;

// Allows access to private members
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]
#pragma warning restore CS0618 // Type or member is obsolete

namespace PitchBlack;
[BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]

class  Plugin : BaseUnityPlugin
{
    public const string MOD_ID = "lurzard.pitchblack";
    public const string MOD_NAME = "Pitch Black";
    public const string MOD_VERSION = "0.1.0";

    private bool init = false;
    public static ManualLogSource logger;
    
    // CWTs
    public static readonly ConditionalWeakTable<Player, ScugCWT> scugCWT = new();
    public static readonly ConditionalWeakTable<AbstractCreature, NightTerror> NTAbstractCWT = new();
    public static readonly ConditionalWeakTable<AbstractCreature, StrongBox<int>> KILLIT = new();
    public static readonly ConditionalWeakTable<RainWorldGame, List<NTTracker>> pursuerTracker = new();
    public static readonly ConditionalWeakTable<MouseGraphics, RotData> rotRatData = new();

    
    // Colors moved to Colors.cs after I saw Alduris set up his codespace that way -Lur 
    
    /// <summary>
    /// SlugBase Features for PB:
    /// - Names MUST match in both code and .json in order to work, otherwise SlugBase throws a fit.
    /// - These fields MUST be in Plugin (according to SlimeCubed).
    /// - Implemented in Hooks\Player\PBSlugBaseFeatures.cs
    /// [Lur]
    /// </summary>
    public static readonly PlayerFeature<float> FlipBoost = PlayerFloat("pb/flip_boost");
    
    // Rotund World stuff -WW
    internal static bool RotundWorldEnabled => _rotundWorldEnabled; // For a single check in BeaconHooks' Player.Update hook
    private static bool _rotundWorldEnabled;
    public static bool individualFoodEnabled;
    
    /// <summary>
    /// Applies all hooks.
    /// </summary>
    public void OnEnable()
    {
        logger = Logger;
        
        logger.LogDebug("Applying PitchBlack hooks...");
        On.RainWorld.OnModsInit += OnModsInit;
        On.RainWorld.OnModsDisabled += DisableMod;
        On.RainWorld.PostModsInit += RainWorld_PostModsInit;
        On.RainWorldGame.ctor += RainWorldGame_ctor;
        On.RainWorldGame.Update += RainWorldGame_Update;
        On.RainWorld.UnloadResources += (orig, self) =>
        {
            orig(self);
            if (Futile.atlasManager.DoesContainAtlas("lmllspr"))
                Futile.atlasManager.UnloadAtlas("lmllspr");
        };
        
        WorldHooks.Apply();
        DevToolsHooks.Apply();
        PBSlugBaseFeatures.Apply();
        ScugHooks.Apply();
        ScugGraphics.Apply();
        FlareStorage.Apply();
        Crafting.Apply();
        FlareBombHooks.Apply();
        
        logger.LogDebug("PitchBlack's hooks successfully applied!");
    }
    
    /// <summary>
    /// Load any resources
    /// Enum Registering
    /// </summary>
    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        
        MachineConnector.SetRegisteredOI(MOD_ID, ModOptions.Instance);
        if (!init)
        {
            try
            {
                Content.Register(new LMLLCritob());
                Content.Register(new NTCritob());
                Content.Register(new RotRatCritob());
                Content.Register(new CitizenCritob());
                
                LMLLHooks.Apply();
                NTHooks.Apply();
                ScareEverything.Apply();
                RotRatHooks.Apply();
                CitizenHooks.Apply();
                
                // Add creatures to CreatureUnlockList
                if (!MultiplayerUnlocks.CreatureUnlockList.Contains(Enums.SandboxUnlockID.LMiniLongLegs))
                {
                    MultiplayerUnlocks.CreatureUnlockList.Add(Enums.SandboxUnlockID.LMiniLongLegs);
                }
                // if (!MultiplayerUnlocks.CreatureUnlockList.Contains(Enums.SandboxUnlockID.NightTerror))
                // {
                //     MultiplayerUnlocks.CreatureUnlockList.Add(Enums.SandboxUnlockID.NightTerror);
                // }
                if (!MultiplayerUnlocks.CreatureUnlockList.Contains(Enums.SandboxUnlockID.RotRat))
                {
                    MultiplayerUnlocks.CreatureUnlockList.Add(Enums.SandboxUnlockID.RotRat);
                }
            }
            catch (Exception err)
            {
                logger.LogDebug($"PitchBlack error\n{err}");
            }
            
            Futile.atlasManager.LoadAtlas("atlases/PBHat");
            if (!Futile.atlasManager.DoesContainAtlas("lmllspr"))
                Futile.atlasManager.LoadAtlas("atlases/lmllspr");
            Futile.atlasManager.LoadAtlas("atlases/nightTerroratlas");
            
            init = true;
        }
    }

    /// <summary>
    /// Unload any resources
    /// Enum Unregistering
    /// </summary>
    private void DisableMod(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
    {
        orig(self, newlyDisabledMods);

        foreach (var mod in newlyDisabledMods)
        {
            if (mod.id == MOD_ID)
            {
                Enums.CreatureTemplateType.UnregisterValues();
                Enums.SandboxUnlockID.UnregisterValues();
                Enums.RoomEffectType.UnregisterValues();
                
                // Remove creatures from CreatureUnlockList
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(Enums.SandboxUnlockID.LMiniLongLegs))
                {
                    MultiplayerUnlocks.CreatureUnlockList.Remove(Enums.SandboxUnlockID.LMiniLongLegs);
                }
                // if (MultiplayerUnlocks.CreatureUnlockList.Contains(Enums.SandboxUnlockID.NightTerror))
                // {
                //     MultiplayerUnlocks.CreatureUnlockList.Remove(Enums.SandboxUnlockID.NightTerror);
                // }
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(Enums.SandboxUnlockID.RotRat))
                {
                    MultiplayerUnlocks.CreatureUnlockList.Remove(Enums.SandboxUnlockID.RotRat);
                }
                
                break;
            }
        }
    }
    
    /// <summary>
    /// Utilized for conditional code when other mods are enabled.
    /// </summary>
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
                //DMSPatch.AddSpritesToDMS();
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
        if (pursuerTracker.TryGetValue(self, out List<NTTracker> trackers)) foreach (NTTracker tracker in trackers) tracker.Update();
    }
    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);
        pursuerTracker.Add(self, new List<NTTracker>());
        //riftCWT.Add(self, new List<RiftWorldPrecence>());
        if ((MiscUtils.IsBeacon(self.session) || ModOptions.universalPursuer.Value) && pursuerTracker.TryGetValue(self, out var trackers))
        {
            trackers.Add(new NTTracker(self));
            logger.LogDebug("ADDING NT TRACKER");
        }

    }
}