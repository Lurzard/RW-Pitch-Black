using Fisobs.Core;
using Fisobs.Creatures;
using System.Collections.Generic;
using UnityEngine;
using DevInterface;

namespace PitchBlack;
    sealed class CitizenCritob : Critob
{
    internal CitizenCritob() : base (PBEnums.CreatureTemplateType.Citizen)
    {
        Icon = new SimpleIcon("Kill_Scavenger", mapCitizen);
        LoadedPerformanceCost = 100f; //probably for loading a lot of creatures
        ShelterDanger = 0;
        CitizenHooks.Apply();
    }
    public override int ExpeditionScore() => 0;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => mapCitizen;

    public override string DevtoolsMapName(AbstractCreature acrit) => "ctzn";

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction()
    {
        return new[] { RoomAttractivenessPanel.Category.LikesInside };
    }

    public override IEnumerable<string> WorldFileAliases()
    {
        return new string[] { "Citizen" };
    }
    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate t = new CreatureFormula(CreatureTemplate.Type.Scavenger, PBEnums.CreatureTemplateType.Citizen, "Citizen")
        {
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 0.1f),
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Scavenger),
        }.IntoTemplate();
        t.dangerousToPlayer = 0;
        t.stowFoodInDen = false;
        t.shortcutColor = mapCitizen;

        //got a lot of this from Umbrascav and edited, don't know what a lot of this does tbh
        t.offScreenSpeed = 1f;
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
        t.roamBetweenRoomsChance = 0.5f;
        t.roamInRoomChance = 0.7f;
        t.socialMemory = true;
        t.communityID = CreatureCommunities.CommunityID.Scavengers;
        t.communityInfluence = 1f;
        t.dangerousToPlayer = 0f;
        t.meatPoints = 0;
        if (ModManager.MSC)
        {
            t.usesNPCTransportation = true;
            t.usesRegionTransportation = false;
            t.usesCreatureHoles = true; 
            t.doesNotUseDens = false;
        }
        return t;
    }

    public override void EstablishRelationships()
    {
        //From: RW Wiki Elite Scavenger page UNFINISHED
        Relationships Citzn = new Relationships(Type);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new ScavengerAI(acrit, acrit.world);

    public override AbstractCreatureAI CreateAbstractAI(AbstractCreature acrit) => new ScavengerAbstractAI(acrit.world, acrit);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Scavenger(acrit, acrit.world);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type ArenaFallback() => CreatureTemplate.Type.Scavenger;

    Color mapCitizen = new(0.9f, 0.9f, 0.9f);
}
