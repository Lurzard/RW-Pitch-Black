using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using static PitchBlack.Plugin;
using System.IO;
using System.Collections.Generic;

namespace PitchBlack;

public static class MiscUtils
{
    public static void SaveCollectionData() {
        string data = "";
        foreach (KeyValuePair<string, bool> keyValuePair in collectionSaveData) {
            data += keyValuePair.Key + ":" + (keyValuePair.Value? "1" : "0") + "|";
        }
        File.WriteAllText(collectionSaveDataPath, data);
    }
    public static void TryReplaceCollectionMenuBackground(string data) {
        if (data != null && data != "") {
            File.WriteAllText(regionMenuDisplaySavePath, data);
        }
    }
    public static string GenerateRandomString(int shortestRange, int maxRange) {
        if (shortestRange > maxRange) {
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
        Debug.Log($"Pitch Black: {retString}");
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
        return null != slugName && (slugName == BeaconName || slugName == PhotoName);
    }
    #endregion
    #region Bacon Checks
    public static bool IsBeacon(GameSession session) {
        return (session is StoryGameSession s) && IsBeacon(s.saveStateNumber);
    }
    public static bool IsBeacon(Creature crit) {
        return (crit is Player player) && IsBeacon(player.slugcatStats.name);
    }
    public static bool IsBeacon(SlugcatStats.Name name) {
        return name != null && name == BeaconName;
    }
    #endregion
    #region Photo Checks
    public static bool IsPhoto(GameSession session) {
        return (session is StoryGameSession s) && IsPhoto(s.saveStateNumber);
    }
    public static bool IsPhoto(Creature crit) {
        return (crit is Player player) && IsPhoto(player.slugcatStats.name);
    }
    public static bool IsPhoto(SlugcatStats.Name name) {
        return name != null && name == PhotoName;
    }
    #endregion
}