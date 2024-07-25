using UnityEngine;

namespace PitchBlack;
#pragma warning disable IDE0300, IDE0090, IDE0044, IDE0028

static internal class InputChecker
{
    // The length of the longest sequence, needed to stop the input list from being super long and eating memory
    const int MAXCODELENGTH = 11;
    // The number of sequences, needed because all the sequences are stored in an Array
    const int NUMOFSEQUENCES = 1;
    // To add a new entry, increase NUMOFSEQUENCES by 1 and add a new entry.
    // Make sure MAXCODELENGTH matches the length of the longest list
    static string[] sequences = new string[NUMOFSEQUENCES]{
        "wwssadadbaE"
    };
    static string inputTracker = "";
    // Checks every sequence of inputs for a match
    internal static bool CheckInput() {
        foreach (string entry in sequences) {
            if (entry == inputTracker.Substring(0, entry.Length)) {
                return true;
            }
        }
        return false;
    }
    // Checks a specific sequence for the currently stored inputs
    internal static bool CheckInput(int entry) {
        return sequences[entry] == inputTracker.Substring(0, sequences[entry].Length);
    }
    internal static void AddInput(char input) {
        // Enter/Return is replaced with E
        // Backspace is replaced with B
        // Left/Up/Right/Down arrows are replaced with L/U/R/D respectively
        switch (input) {
            case '\r':
            case '\n': 
                inputTracker = "E" + inputTracker;
                break;
            case '\b': 
                inputTracker = "B" + inputTracker;
                break;
            case '\u2190':
                inputTracker = "L" + inputTracker;
                break;
            case '\u2191':
                inputTracker = "U" + inputTracker;
                break;
            case '\u2192':
                inputTracker = "R" + inputTracker;
                break;
            case '\u2193':
                inputTracker = "D" + inputTracker;
                break;
            default:
                inputTracker = input.ToString() + inputTracker;
                break;
        }
        while (inputTracker.Length > MAXCODELENGTH) {
            inputTracker = inputTracker.Substring(0, MAXCODELENGTH-1);
        }
        Debug.Log("Current string log is: " + inputTracker);
    }
}