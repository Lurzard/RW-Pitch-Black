using System.Collections.Generic;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using UnityEngine;

namespace PitchBlack;

public class RotRatCritob : Critob
{
    public RotRatCritob() : base(PBEnums.CreatureTemplateType.Rotrat)
    {
        Icon = new SimpleIcon("Kill_Mouse", Plugin.NightmareColor);
        LoadedPerformanceCost = 100f;
        SandboxPerformanceCost = new SandboxPerformanceCost(0.5f, 0.5f);
        RegisterUnlock(KillScore.Configurable(3), PBEnums.SandboxUnlockID.Rotrat);
    }
    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit)
    {
        return new MouseAI(acrit, acrit.world);
    }

    public override Creature CreateRealizedCreature(AbstractCreature acrit)
    {
        return new LanternMouse(acrit, acrit.world);
    }
    public override CreatureState CreateState(AbstractCreature acrit)
    {
        return new MouseState(acrit);
    }

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate t = new CreatureFormula(CreatureTemplate.Type.LanternMouse, PBEnums.CreatureTemplateType.Rotrat, "Rotrat").IntoTemplate();
        t.dangerousToPlayer = 1;
        t.grasps = 1;
        t.stowFoodInDen = true;
        t.shortcutColor = new Color(1f, 0.4f, 0f);
        return t;
    }
    public override IEnumerable<string> WorldFileAliases()
    {
        return new[] { "rotrat" };
    }
    public override void EstablishRelationships()
    {
        Relationships rels = new Relationships(PBEnums.CreatureTemplateType.Rotrat);
        rels.HasDynamicRelationship(CreatureTemplate.Type.Slugcat, 1f);
        rels.Eats(CreatureTemplate.Type.JetFish,1f);
        rels.Eats(CreatureTemplate.Type.Scavenger, 1f);
        rels.Eats(CreatureTemplate.Type.EggBug, 1f);
        rels.Eats(CreatureTemplate.Type.SmallNeedleWorm, 1f);
        rels.Eats(CreatureTemplate.Type.SmallCentipede, 1f);
        rels.Eats(CreatureTemplate.Type.Hazer, 1f);
        rels.Eats(CreatureTemplate.Type.Spider, 1f);
        rels.Eats(CreatureTemplate.Type.VultureGrub, 1f);
        rels.Eats(CreatureTemplate.Type.TubeWorm, 1f);
        rels.Eats(CreatureTemplate.Type.Fly, 1f);
        rels.Eats(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 1f);
        rels.Eats(DLCSharedEnums.CreatureTemplateType.Yeek, 1f);
    }
}