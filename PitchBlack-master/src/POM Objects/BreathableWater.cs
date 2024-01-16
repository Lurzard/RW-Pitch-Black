using static Pom.Pom;
namespace PitchBlack;
public class BreathableWater {
    internal class BreathableWaterObject : UpdatableAndDeletable {
        public BreathableWaterObject(PlacedObject pObj, Room room) : base() {
            this.room = room;
        }
        public override void Update(bool eu) {
            base.Update(eu);
            foreach (Player player in room.PlayersInRoom) player.airInLungs = 1;
        }
    }
    internal static void Register() {
        RegisterFullyManagedObjectType(null, typeof(BreathableWaterObject), "BreathableWater", "Pitch-Black");
    }
}