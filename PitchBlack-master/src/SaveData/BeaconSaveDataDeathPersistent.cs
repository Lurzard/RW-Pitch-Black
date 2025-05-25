using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;

public class BeaconSaveDataDeathPersistent
{
    // NOTE: These all have to be properties in order to be json'd -Lur

    // Tutorials
    public bool hasStoredAFlare { get; set; } //MakeFlares tutorial
    public bool hasUsedThanatosis { get; set; } //Thanatosis tutorial
    public bool hasMovedInThanatosis { get; set; } //Oscillation tutorial
    public bool hasDiedInThanatosis { get; set; } //Drown tutorial
    public bool hasRevived { get; set; } //Revive Tutorial
    public bool hasSeenRot { get; set; } //Rot Tutorial

    // Thanatosis
    public int fakeDeaths { get; set; } //Thanatosis usage amount tracking
    public List<WorldCoordinate> fakeDeathPositions { get; set; } //Thanatosis usage position, for spawning DreamSpawn in that room

    // Dream (Karma)
    public float lowestDreamLevel { get; set; }
    public float highestDreamLevel { get; set; }
    public float dreamLevel { get; set; }
    public bool tempDream { get; set; } //Reinforcement

    // Dreamer
    public List<int> dreamerEncounters { get; set; }
    public bool dreamerNightmareEncounter { get; set; } //UD Dreamer

    // Story
    public bool unlockedThanatosis { get; set; } //Enabling Thanatosis mechanic
    public bool unlockedOscillation { get; set; } //Enabling Oscillation mechanic
    public bool unlockedRevive { get; set; } //Enabling Revive mechanic
    public bool unlockedNightTerror { get; set; } //Spawning Night Terror

    public void Reset()
    {
        hasStoredAFlare = false;
        hasUsedThanatosis = false;
        hasMovedInThanatosis = false;
        hasDiedInThanatosis = false;
        hasRevived = false;
        hasSeenRot = false;

        fakeDeaths = 0;
        fakeDeathPositions = new List<WorldCoordinate>();

        lowestDreamLevel = 0f;
        highestDreamLevel = 0f;
        dreamLevel = 0f;
        tempDream = false;

        dreamerEncounters = new List<int>();
        dreamerNightmareEncounter = false;

        unlockedThanatosis = false;
        unlockedOscillation = false;
        unlockedRevive = false;
        unlockedNightTerror = false;
    }
}
