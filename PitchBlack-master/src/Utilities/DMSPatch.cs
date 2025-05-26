using DressMySlugcat;
using static PitchBlack.Plugin;

namespace PitchBlack;

internal class DMSPatch
{
    //DMS v1.6.6
    public static void AddSpritesToDMS()
    {
        SpriteDefinitions.AddSprite(new()
        {
            Name = "WHISKERS", //name at the top when clicking on the description box
            Description = "Whiskers", //description on dms menu
            GallerySprite = "LizardScaleA0",
            RequiredSprites = new() { "LizardScaleA0" },
            Slugcats = new() {PBEnums.SlugcatStatsName.Beacon.value, PBEnums.SlugcatStatsName.Photomaniac.value }
        });

        SpriteDefinitions.AddSprite(new()
        {
            Name = "SPLATTER",
            Description = "Splatter",
            GallerySprite = "PhotoSplatter",
            RequiredSprites = new() { "PhotoSplatter" },
            Slugcats = new() { PBEnums.SlugcatStatsName.Photomaniac.value },
        });
    }
}
