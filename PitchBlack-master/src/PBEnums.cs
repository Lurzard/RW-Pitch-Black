using System.Diagnostics.CodeAnalysis;
using DevInterface;

namespace PitchBlack;

public static class PBEnums
{
    public static class SlugcatStatsName
    {
        public static readonly SlugcatStats.Name Beacon = new("Beacon", false);
        public static readonly SlugcatStats.Name Photomaniac = new("Photomaniac", false);
    }
    public static class Timeline
    {
        public static readonly SlugcatStats.Timeline Beacon = new("Beacon", true);
    }
    public static class VoidSpawn
    {
        public static class SpawnSource
        {
            public static readonly Room.RippleSpawnSource Death = new("Death", true);
            public static readonly Room.RippleSpawnSource Oscillation = new("Oscillation", true);
            public static readonly  Room.RippleSpawnSource Dreamer = new("Dreamer", true);
        }
        public static class SpawnType
        {
            public static readonly global::VoidSpawn.SpawnType StarSpawn = new("StarSpawn", true);
            public static readonly global::VoidSpawn.SpawnType DreamSpawn = new("DreamSpawn", true);
            public static readonly global::VoidSpawn.SpawnType DreamJelly = new("DreamJelly", true);
            public static readonly global::VoidSpawn.SpawnType DreamAmoeba = new("DreamAmoeba", true);
            public static readonly global::VoidSpawn.SpawnType DreamNoodle = new("DreamNoodle", true);
            public static readonly global::VoidSpawn.SpawnType DreamBiter = new("DreamBiter", true);
            // Plan: ID is passed then it instead spawns the Night Terror creature.
            public static readonly global::VoidSpawn.SpawnType DreamChimera = new("DreamChimera", true);

        }
    }
    public static class Tutorial
    {
        public static DeathPersistentSaveData.Tutorial MakeFlares = new("MakeFlares", true);
        public static DeathPersistentSaveData.Tutorial Thanatosis = new("Thanatosis", true);
        public static DeathPersistentSaveData.Tutorial Oscillation = new("Oscillation", true);
        public static DeathPersistentSaveData.Tutorial Drown = new("Drown", true);
        public static DeathPersistentSaveData.Tutorial Revive = new("Revive", true);
        public static DeathPersistentSaveData.Tutorial Rot = new("Rot", true);
    }
    public static class CreatureTemplateType
    {
        [AllowNull] public static CreatureTemplate.Type NightTerror = new(nameof(NightTerror), true);
        [AllowNull] public static CreatureTemplate.Type LMiniLongLegs = new(nameof(LMiniLongLegs), true);
        [AllowNull] public static CreatureTemplate.Type Rotrat = new(nameof(Rotrat), true);
        [AllowNull] public static CreatureTemplate.Type UmbraScav = new(nameof(UmbraScav), true);
        [AllowNull] public static CreatureTemplate.Type FireGrub = new(nameof(FireGrub), true);
        [AllowNull] public static CreatureTemplate.Type Citizen = new(nameof(Citizen), true);
        public static void UnregisterValues()
        {
            if (NightTerror != null)
            {
                NightTerror.Unregister();
                NightTerror = null;
            }
            if (LMiniLongLegs != null)
            {
                LMiniLongLegs.Unregister();
                LMiniLongLegs = null;
            }
            if (Rotrat != null)
            {
                Rotrat.Unregister();
                Rotrat = null;
            }
            if (FireGrub != null)
            {
                FireGrub.Unregister();
                FireGrub = null;
            }
            if (UmbraScav != null)
            {
                UmbraScav.Unregister();
                UmbraScav = null;
            }
            if (Citizen != null)
            {
                Citizen.Unregister();
                Citizen = null;
            }
        }
    }
    public static class SandboxUnlockID
    {
        [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID NightTerror = new(nameof(NightTerror), true);
        [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID LMiniLongLegs = new(nameof(LMiniLongLegs), true);
        [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID Rotrat = new(nameof(Rotrat), true);
        [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID FireGrub = new(nameof(FireGrub), true);
        [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID UmbraScav = new(nameof(UmbraScav), true);
        public static void UnregisterValues()
        {
            if (NightTerror != null)
            {
                NightTerror.Unregister();
                NightTerror = null;
            }
            if (LMiniLongLegs != null)
            {
                LMiniLongLegs.Unregister();
                LMiniLongLegs = null;
            }
            if (Rotrat != null)
            {
                Rotrat.Unregister();
                Rotrat = null;
            }
            if (FireGrub != null)
            {
                FireGrub.Unregister();
                FireGrub = null;
            }
            if (UmbraScav != null)
            {
                UmbraScav.Unregister();
                UmbraScav = null;
            }
        }
    }
    public static class PlacedObjectType
    {
        public static PlacedObject.Type DreamerSpot;
        public static void RegisterValues()
        {
            DreamerSpot = new PlacedObject.Type("DreamerSpot", true);
        }
        public static void UnregisterValues()
        {
            if (DreamerSpot != null)
            {
                DreamerSpot.Unregister();
                DreamerSpot = null;
            }
        }
    }
    public static class AbstractObjectType
    {
        public static AbstractPhysicalObject.AbstractObjectType DreamSpawn;
        public static void RegisterValues()
        {
            DreamSpawn = new AbstractPhysicalObject.AbstractObjectType("DreamSpawn");
        }
        public static void UnregisterValues()
        {
            if (DreamSpawn != null)
            {
                DreamSpawn.Unregister();
                DreamSpawn = null;
            }
        }
    }
    public static class GhostID
    {
        public static GhostWorldPresence.GhostID Dreamer;
        public static void RegisterValues()
        {
            Dreamer = new GhostWorldPresence.GhostID("Dreamer", true);
        }
        public static void UnregisterValues()
        {
            if (Dreamer != null)
            {
                Dreamer.Unregister();
                Dreamer = null;
            }
        }
    }
    public static class EndGameID
    {
        public static WinState.EndgameID Hunted = new("Hunted", true);
        public static void UnregisterValues()
        {
            if (Hunted != null)
            {
                Hunted.Unregister();
                Hunted = null;
            }
        }
    }
    public static class SceneID
    {
        public static Menu.MenuScene.SceneID Endgame_Hunted = new("Engame_Hunted", true);
        public static void UnregisterValues()
        {
            if (Endgame_Hunted != null)
            {
                Endgame_Hunted.Unregister();
                Endgame_Hunted = null;
            }
        }
    }
    // These apparently HAVE to be registered to play ingame. -Lur
    public static class SoundID
    {
        public static global::SoundID Player_Activated_Thanatosis;
        public static global::SoundID Player_Deactivated_Thanatosis;
        public static global::SoundID Player_Deactivated_Thanatosis_From_Stun;
        public static global::SoundID Player_Died_From_Thanatosis;
        public static global::SoundID Player_Revived;
        public static global::SoundID Thanatosis_Drowning_LOOP;
        public static void RegisterValues()
        {
            Player_Activated_Thanatosis = new global::SoundID("Player_Activated_Thanatosis", true);
            Player_Deactivated_Thanatosis = new global::SoundID("Player_Deactivated_Thanatosis", true);
            Player_Deactivated_Thanatosis_From_Stun = new global::SoundID("Player_Deactivated_Thanatosis_From_Stun", true);
            Player_Died_From_Thanatosis = new global::SoundID("Player_Died_From_Thanatosis", true);
            Player_Revived = new global::SoundID("Player_Revived", true);
            Thanatosis_Drowning_LOOP = new global::SoundID("Drowning_Thanatosis_LOOP", true);
        }
        public static void UnregisterValues()
        {
            if (Player_Activated_Thanatosis != null)
            {
                Player_Activated_Thanatosis.Unregister();
            }
            if (Player_Deactivated_Thanatosis != null)
            {
                Player_Deactivated_Thanatosis.Unregister();
            }
            if (Player_Deactivated_Thanatosis_From_Stun != null)
            {
                Player_Deactivated_Thanatosis_From_Stun.Unregister();
            }
            if (Player_Died_From_Thanatosis != null)
            {
                Player_Died_From_Thanatosis.Unregister();
            }
            if (Player_Revived != null)
            {
                Player_Revived.Unregister();
            }
            if (Thanatosis_Drowning_LOOP != null)
            {
                Thanatosis_Drowning_LOOP.Unregister();
            }
        }
    }
    public static class RoomEffectType
    {
        // I just threw this in here, it's used with the others.
        public static RoomSettingsPage.DevEffectsCategories PitchBlackCatagory = new RoomSettingsPage.DevEffectsCategories("Pitch-Black", true);
        // Actual effects
        public static RoomSettings.RoomEffect.Type ElsehowView = new("ElsehowView", true);
        public static RoomSettings.RoomEffect.Type RippleSpawn = new("RippleSpawn", true);
        public static RoomSettings.RoomEffect.Type RippleMelt = new("RippleMelt", true);
        public static RoomSettings.RoomEffect.Type RoseSky = new("RoseSky", true);
        public static void UnregisterValues()
        {
            if (PitchBlackCatagory != null)
            {
                PitchBlackCatagory.Unregister();
                PitchBlackCatagory = null;
            }
            if (ElsehowView != null)
            {
                ElsehowView.Unregister();
                ElsehowView = null;
            }
            if (RippleSpawn != null)
            {
                RippleSpawn.Unregister();
                RippleSpawn = null;
            }
            if (RippleMelt != null)
            {
                RippleMelt.Unregister();
                RippleMelt = null;
            }
            if (RoseSky != null)
            {
                RoseSky.Unregister();
                RoseSky = null;
            }
        }
    }
}