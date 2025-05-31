using RWCustom;
using UnityEngine;
using static Pom.Pom;

namespace PitchBlack;

public class BreathableWater
{
    internal class BreathableWaterObject : UpdatableAndDeletable
    {
        readonly PlacedObject pObj;
        public BreathableWaterObject(PlacedObject pObj, Room room) : base()
        {
            this.pObj = pObj;
            this.room = room;
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            foreach (Player player in room.PlayersInRoom) {
                if (player.submerged && Vector2.Distance(player.mainBodyChunk.pos, pObj.pos) <= Vector2.Distance(Vector2.zero, (pObj.data as ManagedData).GetValue<Vector2>("Area"))) {
                    player.airInLungs = 1f;
                }
            }
        }
    }
    internal static void Register()
    {
        ManagedField[] fields = new ManagedField[] {
            new Vector2Field("Area", Vector2.one, Vector2Field.VectorReprType.circle, "Area")
        };
        RegisterFullyManagedObjectType(fields, typeof(BreathableWaterObject), "BreathableWater", "Pitch-Black");
    }
}