using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using static PathCost.Legality;

namespace PitchBlack;
sealed class FireGrubCritob : Critob
{
    internal FireGrubCritob() : base(PBEnums.CreatureTemplateType.FireGrub)
    {
        Icon = new SimpleIcon("Kill_Tubeworm", Custom.HSL2RGB(0.1f, 1f, 0.5f));
        RegisterUnlock(KillScore.Configurable(5), PBEnums.SandboxUnlockID.FireGrub);
        SandboxPerformanceCost = new (3f, 1.5f);
        LoadedPerformanceCost = 100f;
        ShelterDanger = ShelterDanger.Safe;
        // FireGrubHooks.Apply();
    }

    public override void ConnectionIsAllowed(AImap map, MovementConnection connection, ref bool? allow)
    {
        base.ConnectionIsAllowed(map, connection, ref allow);
    }

    public override int ExpeditionScore() => 3;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.red;

    public override string DevtoolsMapName(AbstractCreature acrit) => "frgb";

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[] { RoomAttractivenessPanel.Category.LikesInside, RoomAttractivenessPanel.Category.Dark, RoomAttractivenessPanel.Category.LikesOutside };

    public override IEnumerable<string> WorldFileAliases() => new[] { "firegrub" };

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(CreatureTemplate.Type.TubeWorm, Type, "FireGrub")
        {
            TileResistances = new ()
            {
                OffScreen = new (1f, Allowed),
                Floor = new (1f, Allowed),
                Corridor = new (1f, Allowed)
            },
            ConnectionResistances = new ()
            {
                Standard = new PathCost(1f, Allowed),
                OpenDiagonal = new PathCost(3f, Allowed),
                ReachOverGap = new PathCost(3f, Allowed),
                ReachUp = new PathCost(2, Allowed),
                DoubleReachUp = new PathCost(2, Allowed),
                ReachDown = new PathCost(2, Allowed),
                SemiDiagonalReach = new PathCost(2f, Allowed),
                DropToFloor = new PathCost(1f, Allowed),
                DropToWater = new PathCost(1, Allowed),
                ShortCut = new PathCost(1.5f, Allowed),
                NPCTransportation = new PathCost(25f, Allowed),
                OffScreenMovement = new PathCost(1f, Allowed),
                BetweenRooms = new PathCost(10f, Allowed),
                Slope = new PathCost(1.5f, Allowed)
            }
        }.IntoTemplate();
        t.baseDamageResistance = 0.3f;
        t.baseStunResistance = 0.3f;
        t.instantDeathDamageLimit = 1.2f;
        t.AI = true;
        t.requireAImap = true;
        t.doPreBakedPathing = false;
        t.offScreenSpeed = 0f;
        t.bodySize = 0.25f;
        t.grasps = 0;
        t.visualRadius = 100f;
        t.communityInfluence = 0.15f;
        t.countsAsAKill = 1;
        t.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.GreenLizard);
        t.waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
        t.meatPoints = 1;

        return t;
    }

    public override void EstablishRelationships()
    {
        var frgrub = new Relationships(Type);
        for (var i = 0; i < CreatureTemplate.Type.values.entries.Count; i++)
        {
            var v = new CreatureTemplate.Type(CreatureTemplate.Type.values.entries[i]);
            frgrub.IgnoredBy(v);
            frgrub.IntimidatedBy(v, 1f);
        }
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new TubeWormAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new FireGrub(acrit, acrit.world);

    public override void LoadResources(RainWorld rainWorld) {}

    public override CreatureTemplate.Type ArenaFallback() => CreatureTemplate.Type.TubeWorm;
}