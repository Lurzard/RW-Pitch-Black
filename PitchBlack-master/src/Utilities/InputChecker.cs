using UnityEngine;

namespace PitchBlack;
#pragma warning disable IDE0300, IDE0090, IDE0044, IDE0028

static internal class InputChecker
{
    // The length of the longest sequence, needed to stop the input list from being super long and eating memory
    const int MAXCODELENGTH = 11;
    // The number of sequences, needed because all the sequences are stored in an Array
    const int NUMOFSEQUENCES = 3;
    // To add a new entry, increase NUMOFSEQUENCES by 1 and add a new entry.
    // Make sure MAXCODELENGTH matches the length of the longest list
    static string[] sequences = new string[NUMOFSEQUENCES]{
        "wwssadadbaE",
        "LURD",
        "ULURDR"
    };
    static string inputTracker = "";
    private static void ResetInputList() {
        inputTracker = "";
    }
    // Checks every sequence of inputs for a match
    internal static bool CheckInput() {
        foreach (string entry in sequences) {
            if (inputTracker.Length < entry.Length) {
                continue;
            }
            if (entry == inputTracker.Substring(inputTracker.Length - entry.Length, entry.Length)) {
                ResetInputList();
                return true;
            }
        }
        return false;
    }
    // Checks a specific sequence for the currently stored inputs
    internal static bool CheckInput(int entry) {
        if (inputTracker.Length < sequences[entry].Length) {
            return false;
        }
        bool ret = sequences[entry] == inputTracker.Substring(inputTracker.Length - sequences[entry].Length, sequences[entry].Length);
        if (ret) {
            ResetInputList();
        }
        return ret;
    }
    internal static void AddInput(char input) {
        // Enter/Return is replaced with E
        // Backspace is replaced with B
        // Left/Up/Right/Down arrows are replaced with L/U/R/D respectively
        inputTracker = input switch {
            '\r' or '\n' => inputTracker + "E",
            '\b' => inputTracker + "B",
            '\u2190' => inputTracker + "L",
            '\u2191' => inputTracker + "U",
            '\u2192' => inputTracker + "R",
            '\u2193' => inputTracker + "D",
            _ => inputTracker + input.ToString(),
        };
        if (inputTracker.Length > MAXCODELENGTH) {
            inputTracker = inputTracker.Substring(inputTracker.Length-MAXCODELENGTH, MAXCODELENGTH);
        }
    }
}