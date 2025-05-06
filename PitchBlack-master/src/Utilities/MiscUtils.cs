using Random = UnityEngine.Random;
using static PitchBlack.Plugin;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using Watcher;
using System;
using System.Security.Cryptography.X509Certificates;
using IL.Menu.Remix.MixedUI;
using System.Globalization;

namespace PitchBlack;

public static class MiscUtils
{
    public static bool ValidTrackRoom(this Room room)
    {
        return room != null && !room.RoomIsAStartingCabinetsRoom() && !room.abstractRoom.shelter && !room.abstractRoom.gate;
    }
    public static bool RoomIsAStartingCabinetsRoom(this Room room)
    {
        string roomName = room.roomSettings.name;
        if (roomName == "SH_CABINETMERCHANT")
            return true;
        if (roomName == "SH_Long")
            return true;
        if (roomName == "SH_CabinetAlley")
            return true;
        if (roomName.StartsWith("RM_")) //ALSO DON'T TRACK IN THE ROT
            return true;

        for (int i = 1; i <= 5; i++)
        {
            //spinch: nt gets to track SH_CABINETS6, as a treat
            if (roomName == $"SH_CABINETS{i}")
                return true;
        }

        return false;
    }
    public static bool IsNightTerror(this CreatureTemplate creatureTemplate) => creatureTemplate.type == PBEnums.CreatureTemplateType.NightTerror;
    public static void SaveCollectionData()
    {
        string data = "";
        foreach (KeyValuePair<string, bool> keyValuePair in collectionSaveData)
        {
            data += keyValuePair.Key + ":" + (keyValuePair.Value ? "1" : "0") + "|";
        }
        File.WriteAllText(collectionSaveDataPath, data);
    }
    public static void TryReplaceCollectionMenuBackground(string data)
    {
        if (data != null && data != "")
        {
            File.WriteAllText(regionMenuDisplaySavePath, data);
        }
    }
    public static string GenerateRandomString(int shortestRange, int maxRange)
    {
        if (shortestRange > maxRange)
        {
            throw new System.Exception($"Noooo Moon why you do this make sure the stuff does the thiiiing {nameof(GenerateRandomString)}");
        }
        int range = Random.Range(shortestRange, maxRange);
        // I forgor why I named the variable this
        string URP = "ABCDEF01234567890123456789ABC";
        string retString = "";
        for (int i = 0; i < range; i++)
        {
            string char0 = URP[Random.Range(6, 7)].ToString();
            string char1 = URP[Random.Range(6, 11)].ToString();
            string char2 = URP[Random.Range(0, URP.Length)].ToString();
            string char3 = URP[Random.Range(0, URP.Length)].ToString();
            string s = char0 + char1 + char2 + char3;
            // Debug.Log($"Pitch Black: input unicode: {s}");
            // This is only kind of cursed to do I think (but it works!)
            char unicodeChar = (char)int.Parse(s, System.Globalization.NumberStyles.HexNumber);
            retString += unicodeChar;
            // Debug.Log($"Pitch Black: current return string, iteration {i} of {range-1}: {retString}");
        }
        //Debug.Log($"Pitch Black: {retString}"); Debug errors
        return retString;
    }
    #region Bacon or Photo checks
    public static bool IsBeaconOrPhoto(GameSession session)
    {
        return (session is StoryGameSession s) && IsBeaconOrPhoto(s.saveStateNumber);
    }
    public static bool IsBeaconOrPhoto(Creature crit)
    {
        return crit is Player player && IsBeaconOrPhoto(player.slugcatStats.name);
    }
    public static bool IsBeaconOrPhoto(SlugcatStats.Name slugName)
    {
        return null != slugName && (slugName == PBEnums.SlugcatStatsName.Beacon || slugName == PBEnums.SlugcatStatsName.Photomaniac);
    }
    #endregion
    #region Bacon Checks
    public static bool IsBeacon(GameSession session)
    {
        return (session is StoryGameSession s) && IsBeacon(s.saveStateNumber);
    }
    public static bool IsBeacon(Creature crit)
    {
        return (crit is Player player) && IsBeacon(player.slugcatStats.name);
    }
    public static bool IsBeacon(SlugcatStats.Name name)
    {
        return name != null && name == PBEnums.SlugcatStatsName.Beacon;
    }
    #endregion
    #region Photo Checks
    public static bool IsPhoto(GameSession session)
    {
        return (session is StoryGameSession s) && IsPhoto(s.saveStateNumber);
    }
    public static bool IsPhoto(Creature crit)
    {
        return (crit is Player player) && IsPhoto(player.slugcatStats.name);
    }
    public static bool IsPhoto(SlugcatStats.Name name)
    {
        return name != null && name == PBEnums.SlugcatStatsName.Photomaniac;
    }
    #endregion
    // This makes Beacon guaranteed close their eyes if true
    public static bool RegionBlindsBeacon(Room room)
    {
        string regionName = room.world.region.name;
        if (regionName == "VV")
        {
            return true;
        }
        // then add more conditions for the echo rooms later.
        return false;
    }
    // Identifying "Real" regions of the world
    public static bool IsRealscapeRegion(RoomCamera rCam)
    {
        string regionName = rCam.room.world.region.name;
        if (regionName == "SU" ||
        regionName == "HI" ||
        regionName == "SH" ||
        regionName == "CC" ||
        regionName == "LF")
        {
            return true;
        }
        return false;
    }
    // Identifying VV and the rooms that are meant to be treated as part of it
    public static bool IsDeamscapeRegion(RoomCamera rCam)
    {
        string regionName = rCam.room.world.region.name;
        if (regionName == "VV")
        {
            return true;
        }
        return false;
    }
    // Identifying "nightmare" regions of the world
    public static bool IsNightmarescapeRegion(RoomCamera rCam)
    {
        string regionName = rCam.room.world.region.name;
        if (regionName == "BSUR" ||
        regionName == "BDSR")
        {
            return true;
        }
        return false;
    }
    // Identifying DreamSpawn types
    public static bool IsDreamSpawn(VoidSpawn voidSpawn)
    {
        if (voidSpawn.variant == PBEnums.DreamSpawn.SpawnType.DreamSpawn ||
            voidSpawn.variant == PBEnums.DreamSpawn.SpawnType.DreamBiter ||
            voidSpawn.variant == PBEnums.DreamSpawn.SpawnType.DreamNoodle ||
            voidSpawn.variant == PBEnums.DreamSpawn.SpawnType.DreamAmoeba ||
            voidSpawn.variant == PBEnums.DreamSpawn.SpawnType.DreamJelly)
        {
            return true;
        }
        return false;
    }

    //Spawning DreamSpawn
    public static void MaterializeDreamSpawn(Room room, UnityEngine.Vector2 spawnPos, Room.RippleSpawnSource source)
    {
        float level = 3f;
        int amountToSpawn = 0;
        //int amountOfOscillations = 0;
        if (room.game.IsStorySession)
        {
            level = (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.rippleLevel;
        }
        if (source == PBEnums.DreamSpawn.SpawnSource.Death)
        {
            amountToSpawn = 1;
        }
        if (source == PBEnums.DreamSpawn.SpawnSource.Dreamer)
        {
            amountToSpawn = 50;
        }

        // NOTE: To be implemented later as part of Thanatosis, continuously spawning DeathSpawn each "oscillation tick" -Lur
        //if (source == PBExtEnums.DreamSpawnSource.Oscillation)
        //{
        //    amountToSpawn = numberOfOscillations;
        //}

        // Stopping spawning if the room has too many
        if (room.voidSpawns.Count >= amountToSpawn)
        {
            return;
        }
        // Determining type
        VoidSpawn.SpawnType spawnType = PBEnums.DreamSpawn.SpawnType.DreamSpawn;
        if (Random.Range(0, 10) >= 7)
        {
            spawnType = PBEnums.DreamSpawn.SpawnType.DreamJelly;
        }
        if (Random.Range(0, 10) >= 9)
        {
            spawnType = PBEnums.DreamSpawn.SpawnType.DreamNoodle;
        }
        if (Random.value <= 0.02f)
        {
            spawnType = PBEnums.DreamSpawn.SpawnType.DreamAmoeba;
        }
        // Spawning it
        VoidSpawn voidSpawn = new VoidSpawn(new AbstractPhysicalObject(room.world, PBAbstractObjectType.DreamSpawn, null, room.GetWorldCoordinate(spawnPos), room.game.GetNewID()), room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidMelt), VoidSpawnKeeper.DayLightMode(room), spawnType);
        if (IsDreamSpawn(voidSpawn))
        {
            // BezierSwarm for now, but later I want them to chase and kill anything they can
            voidSpawn.behavior = new VoidSpawn.BezierSwarm(voidSpawn, room);
            voidSpawn.timeUntilFadeout = Random.Range(400, 1200);
        }
        voidSpawn.PlaceInRoom(room);
        //voidSpawn.ChangeRippleLayer(0, true);
    }
    public static void SpawnOscillatingRipple(Player self, bool fromThanatosis)
    {
        float timer = 0;
        float timerLimit = 2f;
        bool secondOscillation = false;
        timer += Time.deltaTime;
        int life = Random.Range(80, 260);
        float intensity = fromThanatosis ? 1f : 0.5f;
        float speed = fromThanatosis ? Random.Range(0.7f, 1f) : Random.Range(0.35f, 0.5f);
        float vol = fromThanatosis ? 1f : 0.25f;
        float pitch = fromThanatosis ? Random.Range(0.6f, 0.9f) : Random.Range(0.3f, 0.45f);
        if (timer >= timerLimit && !secondOscillation)
        {
            secondOscillation = true;
            self.room.AddObject(new RippleRing(self.mainBodyChunk.pos, life, intensity, speed));
            self.room.PlaySound(WatcherEnums.WatcherSoundID.Warp_Point_Ripple_Hint, self.mainBodyChunk.pos, vol, pitch);
        }
        if (timer >= timerLimit + 0.04f && secondOscillation)
        {
            timerLimit = Random.Range(2f, 6f);
            timer = 0f;
            self.room.AddObject(new RippleRing(self.mainBodyChunk.pos, life, intensity, speed));
            secondOscillation = false;
        }
    }

    public static void ThanatosisDeathIntensity(Player self)
    {
        if (scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT beaconCWT)
        {
            if (beaconCWT.isDead)
            {
                //function made by MaxDubstep <3 -Lur
                float timeCounter = beaconCWT.thanatosisCounter; //x
                float minKarmaSafeTime = 12 * 40f; //tc
                float maxKarmaSafeTime = 40 * 40f; // Tc
                float beginningIntensity = 0.4f; //l
                float endIntensity = 0.45f; //m
                float windUpTime = 3 * 40f; //wc
                float rampUpTime = 3 * 40f; //Wc
                float maxIntensity = 0.9f; //u
                float plateauDuration = (qualiaLevel - 1) * (maxKarmaSafeTime - (windUpTime + rampUpTime) * 2) / 4 + minKarmaSafeTime - windUpTime - rampUpTime; //c
                // Starting plateau
                if (timeCounter < windUpTime)
                {
                    self.rippleDeathIntensity = Mathf.Sqrt(timeCounter) * beginningIntensity / Mathf.Sqrt(windUpTime);
                }
                // Middle of plateau
                if ((timeCounter < windUpTime + plateauDuration) && timeCounter >= windUpTime)
                {
                    self.rippleDeathIntensity = (timeCounter - windUpTime) * (endIntensity - beginningIntensity) / plateauDuration + beginningIntensity;
                }
                // Ending ramp up
                //if ((timeCounter >= windUpTime + plateauDuration) && (timeCounter <= rampUpTime + windUpTime + plateauDuration))
                //{
                // self.rippleDeathIntensity = Mathf.Pow(timeCounter - plateauDuration - windUpTime, 2) * (maxIntensity - endIntensity) / rampUpTime * rampUpTime + endIntensity;
                //}
                // This MIGHT work -Lur
                if (timeCounter >= windUpTime + plateauDuration + (rampUpTime / 2))
                {
                    float increment = 0.008f;
                    int mult = 4;
                    self.rippleDeathIntensity += increment;
                    increment += 0.008f * mult;
                    mult += 4;
                }
            }
            if ((beaconCWT.diedInThanatosis || self.dead) && self.rippleDeathIntensity < 0.12f)
            {
                self.rippleDeathIntensity += 0.004f;
            }
            if (self.rippleDeathIntensity > 0 && !beaconCWT.isDead)
            {
                self.rippleDeathIntensity -= 0.002f;
            }
        }
    }
    public static string QualiaSymbolSprite(bool small, float level)
    {
        double num = Math.Round((double)(level * 2f), MidpointRounding.AwayFromZero) / 2.0;
        return (small ? "smallQualia" : "qualia") + num.ToString("#.0", CultureInfo.InvariantCulture);
    }

    public static string SidewaysSymbolSprite(bool small, float level)
    {
        double num = Math.Round((double)(level * 2f), MidpointRounding.AwayFromZero) / 2.0;
        return (small ? "smallSideways" : "sideways") + num.ToString("#.0", CultureInfo.InvariantCulture);
    }

    public static bool Dreamer(Ghost ghost)
    {
        return ghost.worldGhost.ghostID == PBGhostID.Dreamer;
    }
}