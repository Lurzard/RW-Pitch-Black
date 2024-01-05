using static Pom.Pom;
namespace PitchBlack;
public class BreathableWater {
    internal class BreathableWaterObject : UpdatableAndDeletable {
        public override void Update(bool eu) {
            base.Update(eu);
            foreach (Player player in room.PlayersInRoom) player.airInLungs = 1;
        }
    }
    internal static void Register() {
        RegisterFullyManagedObjectType(null, typeof(BreathableWaterObject), "BreathableWater", "Pitch-Black");
    }
}