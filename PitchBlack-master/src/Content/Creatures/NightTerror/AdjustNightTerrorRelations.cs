using System;
using MonoMod.Utils;
#pragma warning disable CS8500

namespace PitchBlack;
public class ScareEverything {
    private static readonly CreatureTemplate.Relationship.Type newRelation = CreatureTemplate.Relationship.Type.Afraid;
    private static bool Condition(Creature crit) {
        return crit != null && crit.Template.type == PBEnums.CreatureTemplateType.NightTerror;
    }
    private delegate CreatureTemplate.Relationship Orig(ArtificialIntelligence self, RelationshipTracker.DynamicRelationship dRelation);
    unsafe private static CreatureTemplate.Relationship AdjustRelationship(Orig* orig, ArtificialIntelligence self, RelationshipTracker.DynamicRelationship dRelation) {
        CreatureTemplate.Relationship relationship = (*orig)(self, dRelation);
        Creature trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
        if (trackedCreature != null && trackedCreature.Template.type == PBEnums.CreatureTemplateType.NightTerror) {
            return new CreatureTemplate.Relationship(newRelation, 10);
        }
        return relationship;
    }
    public static void Apply()
    {
        On.ArtificialIntelligence.DynamicRelationship_CreatureRepresentation_AbstractCreature += (orig, self, rep, absCrit) => {
            Creature trackedCreature = null;
            if (rep != null) {
                trackedCreature = rep.representedCreature?.realizedCreature;
            }
            else if (absCrit != null) {
                trackedCreature = absCrit.realizedCreature;
            }
            if (Condition(trackedCreature)) {
                //self.preyTracker.prey.RemoveAll(c => c.critRep.representedCreature.realizedCreature is Player);
                return new CreatureTemplate.Relationship(newRelation, 10f);
            }
            return orig(self, rep, absCrit);
        };
        On.ArtificialIntelligence.StaticRelationship += (orig, self, otherCreature) => {
            return Condition(otherCreature.realizedCreature) ? new CreatureTemplate.Relationship(newRelation, 10f) : orig(self, otherCreature);
        };
        unsafe {
            On.BigNeedleWormAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => AdjustRelationship((Orig*)&orig, self, dRelation);
            On.BigSpiderAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => AdjustRelationship((Orig*)&orig, self, dRelation);
            On.CentipedeAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
                Creature trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
                if (trackedCreature != null && trackedCreature.Template.type == PBEnums.CreatureTemplateType.NightTerror && self.centipede.Template.type != PBEnums.CreatureTemplateType.NightTerror) {
                    // If the 'target' creature is a NightTerror and self is not, be afraid
                    return new CreatureTemplate.Relationship(newRelation, 10f);
                }
                if (trackedCreature != null && trackedCreature.Template.type == CreatureTemplate.Type.Slugcat && self.centipede.Template.type == PBEnums.CreatureTemplateType.NightTerror) {
                    // If the 'target' creature is slugcat and it is a nightterror, eat it 100%
                    return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f);
                }
                else if (trackedCreature != null && trackedCreature.Template.type == PBEnums.CreatureTemplateType.NightTerror && self.centipede.Template.type == PBEnums.CreatureTemplateType.NightTerror) {
                    // If the 'target' is a NightTerror, and self is a NightTerror, Ignore it
                    return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 1f);
                }
                // If somehow none of the above, return orig
                return orig(self, dRelation);
            };
            On.CicadaAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => AdjustRelationship((Orig*)&orig, self, dRelation);
            On.DropBugAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => AdjustRelationship((Orig*)&orig, self, dRelation);
            On.JetFishAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => AdjustRelationship((Orig*)&orig, self, dRelation);
            On.LizardAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => AdjustRelationship((Orig*)&orig, self, dRelation);
            On.MirosBirdAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => AdjustRelationship((Orig*)&orig, self, dRelation);
            On.ScavengerAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => AdjustRelationship((Orig*)&orig, self, dRelation);
            On.TempleGuardAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => AdjustRelationship((Orig*)&orig, self, dRelation);
            On.VultureAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => AdjustRelationship((Orig*)&orig, self, dRelation);
            On.MoreSlugcats.InspectorAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => AdjustRelationship((Orig*)&orig, self, dRelation);
            On.MoreSlugcats.SlugNPCAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => AdjustRelationship((Orig*)&orig, self, dRelation);
            On.MoreSlugcats.StowawayBugAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => AdjustRelationship((Orig*)&orig, self, dRelation);
            On.MoreSlugcats.YeekAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => AdjustRelationship((Orig*)&orig, self, dRelation);
        }
    }
}