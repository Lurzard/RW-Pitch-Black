using NightTerror;

public class ScareEverything {
    public static CreatureTemplate.Relationship.Type newRelation = CreatureTemplate.Relationship.Type.Afraid;
    public static bool Condition(Creature? crit) {
        if (crit != null && crit.Template.type == CreatureTemplateType.NightTerror) {
            return true;
        }
        else {
            return false;
        }
    }
    public static void Apply() {
        On.ArtificialIntelligence.DynamicRelationship_CreatureRepresentation_AbstractCreature += (orig, self, rep, absCrit) => {
            //Debug.Log("Yippe updating");
            Creature? trackedCreature = null;
            if (rep != null) {
                trackedCreature = rep.representedCreature?.realizedCreature;
            }
            else if (absCrit != null) {
                trackedCreature = absCrit.realizedCreature;
            }
            //Debug.Log($"trackedCreature is: {trackedCreature} Player? {trackedCreature is Player}");
            if (Condition(trackedCreature)) {
                //Debug.Log($"Returned afraid\n");
                //self.preyTracker.prey.RemoveAll(c => c.critRep.representedCreature.realizedCreature is Player);
                return new CreatureTemplate.Relationship(newRelation, 10f);
            }
            //Debug.Log($"Skipped muh code. self.creature: {self.creature}. realized?: {self.creature.realizedCreature} template: {self.creature.realizedCreature.Template.type}\n");
            return orig(self, rep, absCrit);
        };

        On.ArtificialIntelligence.StaticRelationship += (orig, self, otherCreature) => {
            if (Condition(otherCreature.realizedCreature)) {
                //Debug.Log($"Returning Afraid\n");
                return new CreatureTemplate.Relationship(newRelation, 10f);
            }
            //Debug.Log($"Did not become afraid\n");
            return orig(self, otherCreature);
        };

        On.BigNeedleWormAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
            CreatureTemplate.Relationship relationship = orig(self, dRelation);
            Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
            if (Condition(trackedCreature)) {
                //Debug.Log("Made it to changing relationship");
                return new CreatureTemplate.Relationship(newRelation, 10f);
            }
            return relationship;
        };

        On.BigSpiderAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
            CreatureTemplate.Relationship relationship = orig(self, dRelation);
            Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
            if (Condition(trackedCreature)) {
                //Debug.Log("Made it to changing relationship");
                return new CreatureTemplate.Relationship(newRelation, 10f);
            }
            return relationship;
        };

        On.CentipedeAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
            CreatureTemplate.Relationship relationship = orig(self, dRelation);
            Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
            if (Condition(trackedCreature)) {
                //Debug.Log("Made it to changing relationship");
                return new CreatureTemplate.Relationship(newRelation, 10f);
            }
            return relationship;
        };

        On.CicadaAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
            CreatureTemplate.Relationship relationship = orig(self, dRelation);
            Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
            if (Condition(trackedCreature)) {
                //Debug.Log("Made it to changing relationship");
                return new CreatureTemplate.Relationship(newRelation, 10f);
            }
            return relationship;
        };

        On.DropBugAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
            CreatureTemplate.Relationship relationship = orig(self, dRelation);
            Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
            if (Condition(trackedCreature)) {
                //Debug.Log("Made it to changing relationship");
                return new CreatureTemplate.Relationship(newRelation, 10f);
            }
            return relationship;
        };

        On.JetFishAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
            CreatureTemplate.Relationship relationship = orig(self, dRelation);
            Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
            if (Condition(trackedCreature)) {
                //Debug.Log("Made it to changing relationship");
                return new CreatureTemplate.Relationship(newRelation, 10f);
            }
            return relationship;
        };

        On.LizardAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
            CreatureTemplate.Relationship relationship = orig(self, dRelation);
            Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
            if (Condition(trackedCreature)) {
                //Debug.Log("Made it to changing relationship");
                return new CreatureTemplate.Relationship(newRelation, 10f);
            }
            return relationship;
        };

        On.MirosBirdAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
            CreatureTemplate.Relationship relationship = orig(self, dRelation);
            Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
            if (Condition(trackedCreature)) {
                //Debug.Log("Made it to changing relationship");
                return new CreatureTemplate.Relationship(newRelation, 10f);
            }
            return relationship;
        };

        On.ScavengerAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
            CreatureTemplate.Relationship relationship = orig(self, dRelation);
            Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
            if (Condition(trackedCreature)) {
                //Debug.Log("Made it to changing relationship");
                return new CreatureTemplate.Relationship(newRelation, 10f);
            }
            return relationship;
        };

        On.TempleGuardAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
            CreatureTemplate.Relationship relationship = orig(self, dRelation);
            Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
            if (Condition(trackedCreature)) {
                //Debug.Log("Made it to changing relationship");
                return new CreatureTemplate.Relationship(newRelation, 10f);
            }
            return relationship;
        };

        On.VultureAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
            CreatureTemplate.Relationship relationship = orig(self, dRelation);
            Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
            if (Condition(trackedCreature)) {
                //Debug.Log("Made it to changing relationship");
                return new CreatureTemplate.Relationship(newRelation, 10f);
            }
            return relationship;
        };

        On.MoreSlugcats.InspectorAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
            CreatureTemplate.Relationship relationship = orig(self, dRelation);
            Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
            if (Condition(trackedCreature)) {
                //Debug.Log("Made it to changing relationship");
                return new CreatureTemplate.Relationship(newRelation, 10f);
            }
            return relationship;
        };

        On.MoreSlugcats.SlugNPCAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
            CreatureTemplate.Relationship relationship = orig(self, dRelation);
            Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
            if (Condition(trackedCreature)) {
                //Debug.Log("Made it to changing relationship");
                return new CreatureTemplate.Relationship(newRelation, 10f);
            }
            return relationship;
        };

        On.MoreSlugcats.StowawayBugAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
            CreatureTemplate.Relationship relationship = orig(self, dRelation);
            Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
            if (Condition(trackedCreature)) {
                //Debug.Log("Made it to changing relationship");
                return new CreatureTemplate.Relationship(newRelation, 10f);
            }
            return relationship;
        };

        On.MoreSlugcats.YeekAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
            CreatureTemplate.Relationship relationship = orig(self, dRelation);
            Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
            if (Condition(trackedCreature)) {
                //Debug.Log("Made it to changing relationship");
                return new CreatureTemplate.Relationship(newRelation, 10f);
            }
            return relationship;
        };
    }
}