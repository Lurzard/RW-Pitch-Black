﻿using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using MoreSlugcats;
using System.Collections.Generic;
using UnityEngine;
using Watcher;

namespace PitchBlack;
sealed class CitizenCritob : Critob
{
    internal CitizenCritob() : base(PBEnums.CreatureTemplateType.Citizen)
    {
        Icon = new SimpleIcon("Kill_Scavenger", mapCitizen);
        LoadedPerformanceCost = 100f; //probably for loading a lot of creatures
        ShelterDanger = 0;
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
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 1f),
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
        Relationships citzn = new Relationships(Type);
        //to others
        citzn.IsInPack(PBEnums.CreatureTemplateType.Citizen, 1f); //friend
        //basegame
        citzn.Ignores(CreatureTemplate.Type.Slugcat);
        citzn.Ignores(CreatureTemplate.Type.Vulture);
        citzn.Ignores(CreatureTemplate.Type.KingVulture);
        citzn.Ignores(CreatureTemplate.Type.Scavenger);
        citzn.Ignores(CreatureTemplate.Type.LanternMouse);
        citzn.Ignores(CreatureTemplate.Type.LizardTemplate);
        citzn.Ignores(CreatureTemplate.Type.Snail);
        citzn.Ignores(CreatureTemplate.Type.GarbageWorm);
        citzn.Ignores(CreatureTemplate.Type.DaddyLongLegs);
        citzn.Ignores(CreatureTemplate.Type.BrotherLongLegs);
        citzn.Ignores(CreatureTemplate.Type.Centipede);
        citzn.Ignores(CreatureTemplate.Type.RedCentipede);
        citzn.Ignores(CreatureTemplate.Type.SmallCentipede);
        citzn.Ignores(CreatureTemplate.Type.TentaclePlant);
        citzn.Ignores(CreatureTemplate.Type.PoleMimic);
        citzn.Ignores(CreatureTemplate.Type.MirosBird);
        citzn.Ignores(CreatureTemplate.Type.BigSpider);
        citzn.Ignores(CreatureTemplate.Type.BigNeedleWorm);
        citzn.Ignores(CreatureTemplate.Type.SmallNeedleWorm);
        citzn.Ignores(CreatureTemplate.Type.DropBug);
        citzn.Ignores(CreatureTemplate.Type.Overseer);
        //msc
        citzn.Ignores(MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy);
        citzn.Ignores(MoreSlugcatsEnums.CreatureTemplateType.FireBug);
        citzn.Ignores(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC);
        //dlcshared
        citzn.Ignores(DLCSharedEnums.CreatureTemplateType.Yeek);
        citzn.Ignores(DLCSharedEnums.CreatureTemplateType.MirosVulture);
        citzn.Ignores(DLCSharedEnums.CreatureTemplateType.Inspector);
        citzn.Ignores(DLCSharedEnums.CreatureTemplateType.TerrorLongLegs);
        //watcher
        citzn.Ignores(WatcherEnums.CreatureTemplateType.DrillCrab);
        citzn.Ignores(WatcherEnums.CreatureTemplateType.BigSandGrub);
        citzn.Ignores(WatcherEnums.CreatureTemplateType.FireSprite);
        //mods go down here later?

        //from others
        //basegame
        citzn.IgnoredBy(CreatureTemplate.Type.LizardTemplate);
        citzn.IgnoredBy(CreatureTemplate.Type.Vulture);
        citzn.IgnoredBy(CreatureTemplate.Type.Scavenger);
        citzn.IgnoredBy(CreatureTemplate.Type.BigSpider);
        //dlcshared
        citzn.IgnoredBy(DLCSharedEnums.CreatureTemplateType.MirosVulture);
        //watcher
        citzn.IgnoredBy(WatcherEnums.CreatureTemplateType.DrillCrab);
        citzn.IgnoredBy(WatcherEnums.CreatureTemplateType.Rattler);
        citzn.IgnoredBy(WatcherEnums.CreatureTemplateType.FireSprite);
        citzn.IgnoredBy(WatcherEnums.CreatureTemplateType.RotLoach);
        citzn.IgnoredBy(WatcherEnums.CreatureTemplateType.Frog);
        //mods go down here later?
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new ScavengerAI(acrit, acrit.world);

    public override AbstractCreatureAI CreateAbstractAI(AbstractCreature acrit) => new ScavengerAbstractAI(acrit.world, acrit);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Scavenger(acrit, acrit.world);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type ArenaFallback() => CreatureTemplate.Type.Scavenger;

    Color mapCitizen = new(0.9f, 0.9f, 0.9f);
}