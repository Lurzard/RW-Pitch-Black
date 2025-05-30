using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using Fisobs.Sandbox;
using UnityEngine;
using DevInterface;
using MoreSlugcats;

namespace PitchBlack;

    sealed class ScholarScavCritob : Critob
{
    internal ScholarScavCritob() : base(PBEnums.CreatureTemplateType.UmbraScav)
    {
        Icon = new SimpleIcon("UmbraScav", Color.grey);
        RegisterUnlock(KillScore.Configurable(25), PBEnums.SandboxUnlockID.UmbraScav, MultiplayerUnlocks.SandboxUnlockID.Scavenger, 0);
        LoadedPerformanceCost = 100f; //probably for loading a lot of creatures
        SandboxPerformanceCost = new SandboxPerformanceCost(0.5f, 0.5f);
        ShelterDanger = 0;
    }

    public override int ExpeditionScore() => 25;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.white;

    public override string DevtoolsMapName(AbstractCreature acrit) => "schlr";

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[] { RoomAttractivenessPanel.Category.LikesInside };

    public override IEnumerable<string> WorldFileAliases() => new[] { "scholarscavenger" };

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate t = new CreatureFormula(CreatureTemplate.Type.Scavenger, PBEnums.CreatureTemplateType.UmbraScav, "UmbraScav")
        {
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 0.1f),
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Scavenger),
    }.IntoTemplate();
        t.dangerousToPlayer = 0;
        t.stowFoodInDen = true;
        t.shortcutColor = new Color(1f, 1f, 1f);


        //From: StaticWorld EliteScavenger check
        t.baseDamageResistance = 2.2f;
        t.baseStunResistance = 1.4f;
        t.instantDeathDamageLimit = 5f;
        t.offScreenSpeed = 0.75f;
        t.grasps = 4;
        t.AI = true;
        t.requireAImap = true;
        t.abstractedLaziness = 50;
        t.bodySize = 1.2f;
        t.doPreBakedPathing = false;
        t.preBakedPathingAncestor = t;
        t.stowFoodInDen = true;
        t.shortcutSegments = 3;
        t.visualRadius = 1200f;
        t.movementBasedVision = 0.3f;
        t.canSwim = true;
        t.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
        t.hibernateOffScreen = true;
        t.roamBetweenRoomsChance = 0.3f;
        t.roamInRoomChance = 0.7f;
        t.socialMemory = true;
        t.communityID = CreatureCommunities.CommunityID.Scavengers;
        t.communityInfluence = 1f;
        t.dangerousToPlayer = 0.3f;
        t.meatPoints = 4;
        t.usesNPCTransportation = true;
        t.usesRegionTransportation = false; //I assume we want to keep it in SL
        t.usesCreatureHoles = true;
        t.doesNotUseDens = false; //to spawn in den
        return t;
    }

    public override void EstablishRelationships()
    {
        //From: RW Wiki Elite Scavenger page UNFINISHED
        Relationships umbr = new Relationships(Type);
        umbr.Ignores(CreatureTemplate.Type.SmallCentipede);
        umbr.Ignores(CreatureTemplate.Type.SmallNeedleWorm);
        umbr.Attacks(CreatureTemplate.Type.Overseer, 1f);
        umbr.Attacks(DLCSharedEnums.CreatureTemplateType.Inspector, 1f);
        umbr.Attacks(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 1f);
        umbr.Attacks(DLCSharedEnums.CreatureTemplateType.Yeek, 0.1f);

        umbr.UncomfortableAround(CreatureTemplate.Type.GarbageWorm, 0.8f);
        umbr.UncomfortableAround(CreatureTemplate.Type.Snail, 0.6f);
        umbr.UncomfortableAround(CreatureTemplate.Type.BigNeedleWorm, 0.5f);
        umbr.UncomfortableAround(CreatureTemplate.Type.LanternMouse, 0.1f);

        umbr.IsInPack(CreatureTemplate.Type.Slugcat, 0.6f);
        umbr.IsInPack(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 0.6f);

        umbr.IsInPack(CreatureTemplate.Type.Scavenger, 1f);
        umbr.Attacks(DLCSharedEnums.CreatureTemplateType.ScavengerElite, 1f);
        umbr.Attacks(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, 1f);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new ScavengerAI(acrit, acrit.world);

    public override AbstractCreatureAI CreateAbstractAI(AbstractCreature acrit) => new ScavengerAbstractAI(acrit.world, acrit);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Scavenger(acrit, acrit.world);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type ArenaFallback() => CreatureTemplate.Type.Scavenger;
}