using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;
public class BeaconImplementation {
    public static void Apply() {
        //WIP in-code Beacon instead of Slugbase (does not run in plugin on purpose)
        
        //Menu
        On.Menu.CharacterSelectPage.GetSlugcatPortrait += CharacterSelectPage_GetSlugcatPortrait; //Default colored character portrait
    }

    private static Menu.MenuIllustration CharacterSelectPage_GetSlugcatPortrait(On.Menu.CharacterSelectPage.orig_GetSlugcatPortrait orig, Menu.CharacterSelectPage self, SlugcatStats.Name slugcat, UnityEngine.Vector2 pos)
    {
        orig(self, slugcat, pos);
        string folderName = "illustrations";
        string fileName = "";
        if (slugcat == PBSlugcatStatsName.Beacon) fileName = "multiplayerportrait41-Beacon";
        return new Menu.MenuIllustration(self.menu, self, folderName, fileName, pos, true, true);
    }
}
