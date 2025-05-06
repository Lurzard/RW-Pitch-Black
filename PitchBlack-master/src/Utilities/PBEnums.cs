using System.Diagnostics.CodeAnalysis;
using DevInterface;

namespace PitchBlack;

public static class PBEnums
{
    // Likely that SlugBase handles registering, so these are unregistered
    public static class SlugcatStatsName
    {
        public static readonly SlugcatStats.Name Beacon = new(nameof(Beacon), false);
        public static readonly SlugcatStats.Name Photomaniac = new(nameof(Photomaniac), false);
    }
    public static class Timeline
    {
        public static readonly SlugcatStats.Timeline Beacon = new(nameof(Beacon), false);
    }
    public static class DreamSpawn
    {
        public static class SpawnSource
        {
            public static Room.RippleSpawnSource Death = new Room.RippleSpawnSource(nameof(Death), true);
            public static Room.RippleSpawnSource Oscillation = new Room.RippleSpawnSource(nameof(Oscillation), true);
            public static Room.RippleSpawnSource Dreamer = new Room.RippleSpawnSource(nameof(Dreamer), true);
        }
        public static class SpawnType
        {
            public static VoidSpawn.SpawnType DreamSpawn = new(nameof(DreamSpawn), true);
            public static VoidSpawn.SpawnType DreamJelly = new(nameof(DreamJelly), true);
            public static VoidSpawn.SpawnType DreamAmoeba = new(nameof(DreamAmoeba), true);
            public static VoidSpawn.SpawnType DreamNoodle = new(nameof(DreamNoodle), true);
            public static VoidSpawn.SpawnType DreamBiter = new(nameof(DreamBiter), true);
        }
    }
    public static class Tutorial
    {
        public static DeathPersistentSaveData.Tutorial MakeFlares = new(nameof(MakeFlares), true);
        public static DeathPersistentSaveData.Tutorial Thanatosis = new(nameof(Thanatosis), true);
    }
    public static class CreatureTemplateType
    {
        [AllowNull] public static CreatureTemplate.Type NightTerror = new(nameof(NightTerror), true);
        [AllowNull] public static CreatureTemplate.Type LMiniLongLegs = new(nameof(LMiniLongLegs), true);
        [AllowNull] public static CreatureTemplate.Type Rotrat = new(nameof(Rotrat), true);
        [AllowNull] public static CreatureTemplate.Type UmbraScav = new(nameof(UmbraScav), true);
        [AllowNull] public static CreatureTemplate.Type FireGrub = new(nameof(FireGrub), true);
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
    public class DevEffectsCatagories
    {
        public static RoomSettingsPage.DevEffectsCategories PitchBlack = new RoomSettingsPage.DevEffectsCategories(nameof(PitchBlack), true);
        public static void UnregisterValues()
        {
            if (PitchBlack != null)
            {
                PitchBlack.Unregister();
                PitchBlack = null;
            }
        }
    }
    public class RoomEffectType
    {
        // Actual effects
        public static RoomSettings.RoomEffect.Type ElsehowView = new RoomSettings.RoomEffect.Type(nameof(ElsehowView), true);
        public static RoomSettings.RoomEffect.Type RippleSpawn = new RoomSettings.RoomEffect.Type(nameof(RippleSpawn), true);
        public static RoomSettings.RoomEffect.Type RippleMelt = new RoomSettings.RoomEffect.Type(nameof(RippleMelt), true);
        public static void UnregisterValues()
        {
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
        }
    }
    public class PlacedObjectType
    {
        public static PlacedObject.Type DreamerSpot;
        public static void RegisterValues()
        {
            DreamerSpot = new PlacedObject.Type(nameof(DreamerSpot), true);
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
    public class AbstractObjectType
    {
        public static AbstractPhysicalObject.AbstractObjectType DreamSpawn;
        public static void RegisterValues()
        {
            DreamSpawn = new AbstractPhysicalObject.AbstractObjectType(nameof(DreamSpawn), true);
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
    public class GhostID
    {
        public static GhostWorldPresence.GhostID Dreamer;
        public static void RegisterValues()
        {
            Dreamer = new GhostWorldPresence.GhostID(nameof(Dreamer), true);
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
    public class EndGameID
    {
        public static WinState.EndgameID Hunted = new WinState.EndgameID(nameof(Hunted), true);
        public static void UnregisterValues()
        {
            if (Hunted != null)
            {
                Hunted.Unregister();
                Hunted = null;
            }
        }
    }

    public class SceneID
    {
        public static Menu.MenuScene.SceneID Endgame_Hunted = new Menu.MenuScene.SceneID(nameof(Endgame_Hunted), true);
        public static void UnregisterValues()
        {
            if (Endgame_Hunted != null)
            {
                Endgame_Hunted.Unregister();
                Endgame_Hunted = null;
            }
        }
    }
    public static class SoundID
    {
        public static global::SoundID Player_Activated_Thanatosis;
        public static global::SoundID Player_Deactivated_Thanatosis;
        public static global::SoundID Player_Deactivated_Thanatosis_From_Stun;
        public static global::SoundID Player_Died_From_Thanatosis;
        public static global::SoundID Player_Revived;
        public static global::SoundID Thanatosis_Drowning_LOOP;
        // These apparently HAVE to be registered to play ingame. -Lur
        public static void RegisterValues()
        {
            // Keeping as strings, because they're specifically used in sounds.txt
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
}