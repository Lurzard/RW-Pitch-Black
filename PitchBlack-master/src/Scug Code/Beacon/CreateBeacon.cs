#if false
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Menu;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace PitchBlack;
public class CreateBeacon {

    public delegate void orig_SetSlugcatColorOrder(SlugcatSelectMenu self);
    public static void PB_SetSlugcatColorOrder_set(orig_SetSlugcatColorOrder orig, SlugcatSelectMenu self) {
        orig(self);
        self.slugcatColorOrder.Add(PBSlugcatStatsName.Beacon);
    }

    public static void Apply() {
        //WIP in-code Beacon instead of Slugbase (does not run in plugin on purpose)

        //****
        //MENU
        //****
        //SlugcatMenu.SlugcatColorOrder
        On.Menu.SlugcatSelectMenu.SlugcatPageNewGame.ctor += SlugcatPageNewGame_ctor; //Character select screen text
        new Hook(typeof(CreateBeacon).GetProperty("SetSlugcatColorOrder", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(),
            typeof(CreateBeacon).GetMethod("PB_SetSlugcatColorOrder_set", BindingFlags.Public | BindingFlags.Static));
        On.Menu.CharacterSelectPage.GetSlugcatPortrait += CharacterSelectPage_GetSlugcatPortrait; //Default colored character portrait
        //On.Menu.CharacterSelectPage.UpdateSelectedSlugcat
        //BuildBeaconScene()
        //BuildBeaconSleepScreen()
    }

    private static void SlugcatPageNewGame_ctor(On.Menu.SlugcatSelectMenu.SlugcatPageNewGame.orig_ctor orig, SlugcatSelectMenu.SlugcatPageNewGame self, Menu.Menu menu, MenuObject owner, int pageIndex, SlugcatStats.Name slugcatNumber) {
        orig(self, menu, owner, pageIndex, slugcatNumber);
        string name = "";
        string desc = "";
        float num2 = 0f;
        if (self.slugcatNumber == PBSlugcatStatsName.Beacon) {
            name = menu.Translate("THE BEACON");
            desc = menu.Translate("beacon-character-description");
        }
        self.difficultyLabel = new MenuLabel(menu, self, name, new Vector2(-1000f, self.imagePos.y - 249f + num2), new Vector2(200f, 30f), true, null);
        self.difficultyLabel.label.alignment = FLabelAlignment.Center;
        self.subObjects.Add(self.difficultyLabel);
        self.infoLabel = new MenuLabel(menu, self, desc, new Vector2(-1000f, self.imagePos.y - 249f - 60f + num2 / 2f), new Vector2(400f, 60f), true, null);
        self.infoLabel.label.alignment = FLabelAlignment.Center;
        self.subObjects.Add(self.infoLabel);
    }


    private static MenuIllustration CharacterSelectPage_GetSlugcatPortrait(On.Menu.CharacterSelectPage.orig_GetSlugcatPortrait orig, CharacterSelectPage self, SlugcatStats.Name slugcat, Vector2 pos) {
        orig(self, slugcat, pos);
        string folderName = "illustrations";
        string fileName = "";
        if (slugcat == PBSlugcatStatsName.Beacon) fileName = "multiplayerportrait41-Beacon";
        return new MenuIllustration(self.menu, self, folderName, fileName, pos, true, true);
    }
}
#endif