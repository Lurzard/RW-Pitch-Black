using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;

public class MoonDialogue {
    public static void Apply() {
        On.SLOracleBehaviorHasMark.CreatureJokeDialog += SLOracleBehaviorHasMark_CreatureJokeDialog;
    }

    private static void SLOracleBehaviorHasMark_CreatureJokeDialog(On.SLOracleBehaviorHasMark.orig_CreatureJokeDialog orig, SLOracleBehaviorHasMark self) {
        orig(self);
        CreatureTemplate.Type randomCreatue = self.CheckStrayCreatureInRoom();

        //or NT wing or mini terror
        if (randomCreatue == PBCreatureTemplateType.NightTerror) {
            int num = UnityEngine.Random.Range(0, 3);

            if (num == 0)
            {
                self.dialogBox.NewMessage(self.Translate("This is bad."), 10);
            }
            if (num == 1)
            {
                self.dialogBox.NewMessage(self.Translate("What a nightmare."), 10);
            }
            if (num == 2)
            {
                self.dialogBox.NewMessage(self.Translate("What is that thing?"), 10);
            }
            return;
        }

        if (randomCreatue == PBCreatureTemplateType.LMiniLongLegs)
        {
            self.dialogBox.NewMessage(self.Translate("Oh no."), 10);
            return;
        }

        if (randomCreatue == PBCreatureTemplateType.Rotrat)
        {
            self.dialogBox.NewMessage(self.Translate("No. Get it out!"), 10);
            return;
        }

        if (randomCreatue == PBCreatureTemplateType.FireGrub)
        {
            self.dialogBox.NewMessage(self.Translate("What- How?"), 10);
            return;
        }
    }
}
