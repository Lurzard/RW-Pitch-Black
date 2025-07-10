namespace PitchBlack;

public static class MiscUtils
{
    #region Beacon Checks
    
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
        return name != null && name == Enums.SlugcatStatsName.Beacon;
    }
    
    #endregion
    
    public static bool IsNightTerror(this CreatureTemplate creatureTemplate) => creatureTemplate.type == Enums.CreatureTemplateType.NightTerror;
    
    public static bool ValidTrackRoom(this Room room)
    {
        return room != null && !room.abstractRoom.shelter && !room.abstractRoom.gate;
    }
    
    // Regions that make Beacon squint regardless of room darkness
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
}