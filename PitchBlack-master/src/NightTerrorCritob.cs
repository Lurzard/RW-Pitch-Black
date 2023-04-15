using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using static PathCost.Legality;

namespace NightTerror
{
    sealed class RedCentipedeCritob : Critob
    {
        internal RedCentipedeCritob() : base(CreatureTemplateType.NightTerror)
        {
            Icon = new SimpleIcon("Night_Terror", Color.clear);
            RegisterUnlock(KillScore.Configurable(25), SandboxUnlockID.NightTerror);
            SandboxPerformanceCost = new(3f, 1.5f);
            LoadedPerformanceCost = 200f;
            ShelterDanger = ShelterDanger.Hostile;
            Hooks.Apply();
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

        public override void TileIsAllowed(AImap map, IntVector2 tilePos, ref bool? allow) => allow = ((map.getAItile(tilePos).terrainProximity > 1f && map.getAItile(tilePos).terrainProximity < 3f) || (map.getAItile(tilePos).narrowSpace) || map.getAItile(tilePos).walkable) && map.getAItile(tilePos).terrainProximity != 0;

        public override int ExpeditionScore() => 25;

        public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.blue;

        public override string DevtoolsMapName(AbstractCreature acrit) => "ntr";

        public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[] { RoomAttractivenessPanel.Category.LikesInside };

        public override IEnumerable<string> WorldFileAliases() => new[] { "nightterror" };

        public override CreatureTemplate CreateTemplate()
        {
            var t = new CreatureFormula(CreatureTemplate.Type.RedCentipede, Type, "NightTerror")
            {
                TileResistances = new()
                {
                    Air = new(1f, Allowed)
                },
                ConnectionResistances = new()
                {
                    Standard = new(1f, Allowed),
                    ShortCut = new(1f, Allowed),
                    BigCreatureShortCutSqueeze = new(1f, Allowed),
                    OffScreenMovement = new(1f, Allowed),
                    BetweenRooms = new(1f, Allowed)
                },
                DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 1f),
                DamageResistances = new() { Base = 200f, Explosion = .03f },
                StunResistances = new() { Base = 200f },
                HasAI = true,
                Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.RedCentipede),
            }.IntoTemplate();
            return t;
        }

        public override void EstablishRelationships()
        {
            var daddy = new Relationships(Type);
            daddy.Eats(CreatureTemplate.Type.Slugcat, 1f);
        }

        public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new CentipedeAI(acrit, acrit.world);

        public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Centipede(acrit, acrit.world);

        public override CreatureState CreateState(AbstractCreature acrit) => new Centipede.CentipedeState(acrit);

        public override void LoadResources(RainWorld rainWorld) { }

        public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.RedCentipede;
    }
}