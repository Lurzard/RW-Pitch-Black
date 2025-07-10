using Color = UnityEngine.Color;

namespace PitchBlack;

public static class Colors
{
    // #f02961
    public static readonly Color OverseerColor = new(240f/255f, 41f/255f, 97f/255f);
    
    // #1a1041
    public static readonly Color BeaconDefaultColor = new(26f/255f, 16f/255f, 65f/255f);
    public static readonly Color BeaconStarveColor = Color.Lerp(BeaconDefaultColor, Color.gray, 0.4f);
    // #3300ff
    public static readonly Color BeaconFullColor = new(.2f, 0f, 1f); 
    // Not readonly because it is assigned to the palette black color.
    public static Color playerPaletteBlack;
    public static readonly Color BeaconEyeColor = Color.Lerp(playerPaletteBlack, Color.white, .87f);
    
    // #d31c4a
    public static readonly Color NightmareColor = new (0.82745098039f, 0.10980392156f, 0.29019607843f);
    // #862e48
    public static readonly Color Rose = new(134f/255f, 46f/255f, 72f/255f);
    public static readonly Color SaturatedRose = Rose * 2f;
    public static readonly Color VisibleWhite = new(.9f, .9f, .9f);
    //public static readonly Color VisibleBlack = new(.003f, .003f, .003f);
}