using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using Fisobs.Sandbox;
using static PathCost.Legality;
using UnityEngine;
using DevInterface;
using RWCustom;

namespace PitchBlack;

sealed class LMLLCritob : Critob
{
    internal LMLLCritob() : base(Enums.CreatureTemplateType.LMiniLongLegs)
    {
        Plugin.logger.LogDebug("Registering/loading LMLL from Pitch Black");
        Icon = new SimpleIcon("Kill_LMLL", Colors.NightmareColor);
        RegisterUnlock(KillScore.Configurable(5), Enums.SandboxUnlockID.LMiniLongLegs);
        SandboxPerformanceCost = new(1.5f, 1.25f);
        LoadedPerformanceCost = 100f;
        ShelterDanger = ShelterDanger.Hostile;
    }

    public override void ConnectionIsAllowed(AImap map, MovementConnection connection, ref bool? allow)
    {
        if (connection.type == MovementConnection.MovementType.ShortCut)
        {
            if (connection.startCoord.TileDefined && map.room.shortcutData(connection.StartTile).shortCutType == ShortcutData.Type.Normal)
                allow = true;
            if (connection.destinationCoord.TileDefined && map.room.shortcutData(connection.DestTile).shortCutType == ShortcutData.Type.Normal)
                allow = true;
        }
        else if (connection.type == MovementConnection.MovementType.BigCreatureShortCutSqueeze)
        {
            if (map.room.GetTile(connection.startCoord).Terrain == Room.Tile.TerrainType.ShortcutEntrance && map.room.shortcutData(connection.StartTile).shortCutType == ShortcutData.Type.Normal)
                allow = true;
            if (map.room.GetTile(connection.destinationCoord).Terrain == Room.Tile.TerrainType.ShortcutEntrance && map.room.shortcutData(connection.DestTile).shortCutType == ShortcutData.Type.Normal)
                allow = true;
        }
    }

    public override void TileIsAllowed(AImap map, IntVector2 tilePos, ref bool? allow) => allow = map.getTerrainProximity(tilePos) > 0;

    public override int ExpeditionScore() => 5;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.grey;

    public override string DevtoolsMapName(AbstractCreature acrit) => "mll";

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[] { RoomAttractivenessPanel.Category.LikesInside };

    public override IEnumerable<string> WorldFileAliases() => new[] { "lmindll", "lminilonglegs" };

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(CreatureTemplate.Type.DaddyLongLegs, Type, "LMiniLongLegs") 
        {
            TileResistances = new()
            {
                Air = new(1f, Allowed)
            },
            ConnectionResistances = new() 
            {
                Standard = new(1f, Allowed),
                ShortCut = new(1f, Allowed),
                BigCreatureShortCutSqueeze = new(10f, Allowed),
                OffScreenMovement = new(1f, Allowed),
                BetweenRooms = new(10f, Allowed)
            },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 0f),
            DamageResistances = new() { Base = 8f, Explosion = .2f },
            StunResistances = new() { Base = 8f, Explosion = .2f },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.DaddyLongLegs),
        }.IntoTemplate();
        t.dangerousToPlayer = .25f;
        t.meatPoints = 2;
        t.scaryness = .5f;
        t.wormGrassImmune = true;
        t.wormgrassTilesIgnored = true;
        t.shortcutSegments = 2;
        t.shortcutColor = Color.gray;
        t.bodySize = 1f;
        t.offScreenSpeed = .1f;
        t.roamBetweenRoomsChance = .5f;
        t.roamInRoomChance = .8f;
        t.abstractedLaziness = 35;
        return t;
    }

    public override void EstablishRelationships()
    {
        var daddy = new Relationships(Type);
        for (var i = 0; i < CreatureTemplate.Type.values.entries.Count; i++)
        {
            var v = new CreatureTemplate.Type(CreatureTemplate.Type.values.entries[i]);
            daddy.IgnoredBy(v);
            daddy.Ignores(v);
        }
        daddy.EatenBy(CreatureTemplate.Type.DaddyLongLegs, 1f);
        daddy.EatenBy(CreatureTemplate.Type.MirosBird, .5f);
        daddy.EatenBy(CreatureTemplate.Type.BigEel, .5f);
        daddy.Eats(CreatureTemplate.Type.Hazer, 1f);
        daddy.Eats(CreatureTemplate.Type.VultureGrub, 1f);
        daddy.Eats(CreatureTemplate.Type.SmallNeedleWorm, 1f);
        daddy.Eats(CreatureTemplate.Type.Snail, 1f);
        daddy.Eats(CreatureTemplate.Type.TubeWorm, 1f);
        daddy.Eats(CreatureTemplate.Type.SmallCentipede, 1f);
        daddy.FearedBy(CreatureTemplate.Type.SmallCentipede, 1f);
        daddy.FearedBy(CreatureTemplate.Type.Snail, 1f);
        daddy.FearedBy(CreatureTemplate.Type.Hazer, 1f);
        daddy.FearedBy(CreatureTemplate.Type.VultureGrub, 1f);
        daddy.FearedBy(CreatureTemplate.Type.SmallNeedleWorm, 1f);
        daddy.FearedBy(CreatureTemplate.Type.TubeWorm, 1f);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new DaddyAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new LittleLongLegs(acrit, acrit.world);

    public override CreatureState CreateState(AbstractCreature acrit) => new DaddyLongLegs.DaddyState(acrit);

    public override void LoadResources(RainWorld rainWorld) {}

    #nullable enable
    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.BrotherLongLegs;
}