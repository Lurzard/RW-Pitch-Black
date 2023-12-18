using System;
using static PitchBlack.Plugin;

namespace PitchBlack;

public class ScugCWT
{
    public WeakReference<Player> playerRef;

    public BeaconCWT Beacon; //remember to do a null or a IsBeacon/IsPhoto check before accessing these
    public PhotoCWT Photo;

    public readonly bool IsBeacon;
    public readonly bool IsPhoto;
    public readonly bool IsBeaconOrPhoto;

    public bool SpritesInitialized; //bc PlayerGraphics.InitializeSprites calls itself twice in a row gawd dam
    public Whiskers whiskers; //initialized in PlayerGraphics.ctor hook
    public int hatIndex; //index of the hat sprite

    public ScugCWT(Player player)
    {
        playerRef = new WeakReference<Player>(player);

        if (BeaconName == player.slugcatStats.name)
        {
            IsBeaconOrPhoto = true;
            IsBeacon = true;
            Beacon = new BeaconCWT(this);
        }
        else if (PhotoName == player.slugcatStats.name)
        {
            IsBeaconOrPhoto = true;
            IsPhoto = true;
            Photo = new PhotoCWT(this);
        }
        // else
        //     Debug.Log($"Pitch Black: How did {player.slugcatStats.name} player {player.playerState.playerNumber} get in the scug CWT?!");
    }
}