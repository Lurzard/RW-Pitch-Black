﻿using HUD;
using On.RWCustom;
using UnityEngine;

namespace PitchBlack;

public class KarmaBoosting
{
    // halfassed karmaflower karma boost tracking!!
    private static bool karmaBoost = false;
    private static bool doubledKarmaBoost = false;
    private static bool moreThanMaxKarma = false;

    // Keeping the code here for now to reference later -Lur

    public static void Apply()
    {
        //Reinforced Karma
        //On.HUD.KarmaMeter.Update += KarmaMeter_Update;
        //On.HUD.KarmaMeter.KarmaSymbolSprite += KarmaMeter_KarmaSymbolSprite1;

        //these apparently change Watcher's Ripple Sprites color, so maybe baseColor is only used for Ripple
        //On.HUD.KarmaMeter.UpdateGraphic += KarmaMeter_UpdateGraphic;
        //On.HUD.KarmaMeter.ctor += KarmaMeter_ctor;
    }

    private static string KarmaMeter_KarmaSymbolSprite1(On.HUD.KarmaMeter.orig_KarmaSymbolSprite orig, bool small, RWCustom.IntVector2 k)
    {
        int min = 0;
        if (ModManager.MSC && small)
        {
            min = -1;
        }
        if (k.x < 5)
        {
            return (small ? "smallKarma" : "karma") + Mathf.Clamp(k.x, min, 4).ToString();
        }
        return (small ? "smallKarma" : "karma") + Mathf.Clamp(k.x, 5, 10).ToString() + "-" + Mathf.Clamp(k.y, k.x, 10).ToString();
    }

    private static void KarmaMeter_Update(On.HUD.KarmaMeter.orig_Update orig, KarmaMeter self)
    {
        orig(self);
        // if owner != null, player != null, room != null, Beacon, and region != null | OTHERWISE BREAKS EVERYTHING!!!
        if (self.hud.owner != null && self.hud.owner is Player && (self.hud.owner as Player).room != null && (self.hud.owner as Player).SlugCatClass == Plugin.BeaconName && (self.hud.owner as Player).room.world.region != null)
        {
            int karma = ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma;
            int karmaCap = ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap;

            if (self.showAsReinforced)
            {
                //any karma except max or 4
                if (!karmaBoost && karma < 9 && karma != 4)
                {
                    ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma++;
                    (self.hud.owner as Player).room.game.cameras[0].hud.karmaMeter.UpdateGraphic();
                    karmaBoost = true;
                }
                //same implementation as the first echo giving you 2 extra
                else if (!doubledKarmaBoost && karmaCap == 4 && !karmaBoost)
                {
                    ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma++;
                    ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap = 6;
                    (self.hud.owner as Player).room.game.cameras[0].hud.karmaMeter.UpdateGraphic();
                    doubledKarmaBoost = true;
                }
                //secret karma
                else if (!moreThanMaxKarma && !karmaBoost && !doubledKarmaBoost && karma == 9)
                {
                    ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma = 10;
                    (self.hud.owner as Player).room.game.cameras[0].hud.karmaMeter.UpdateGraphic();
                    moreThanMaxKarma = true;
                }
            }

            if (!self.showAsReinforced && (karmaBoost || doubledKarmaBoost || moreThanMaxKarma))
            {
                if (karmaBoost)
                {
                    ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma--;
                    (self.hud.owner as Player).room.game.cameras[0].hud.karmaMeter.UpdateGraphic();
                    karmaBoost = false;
                }

                if (doubledKarmaBoost)
                {
                    ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma = 4;
                    ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap = 4;
                    (self.hud.owner as Player).room.game.cameras[0].hud.karmaMeter.UpdateGraphic();
                    doubledKarmaBoost = false;
                }

                if (moreThanMaxKarma)
                {
                    ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma = 9;
                    ((self.hud.owner as Player).abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap = 9;
                    (self.hud.owner as Player).room.game.cameras[0].hud.karmaMeter.UpdateGraphic();
                    moreThanMaxKarma = false;
                }
            }
        }
    }
}
