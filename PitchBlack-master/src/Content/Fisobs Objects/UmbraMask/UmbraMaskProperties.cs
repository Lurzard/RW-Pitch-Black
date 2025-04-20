using Fisobs.Properties;
using System.Linq;

namespace PitchBlack;
sealed class UmbraMaskProperties : ItemProperties
{
    public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
    {
        if (player.grasps.Any(g => g?.grabbed is UmbraMask))
        {
            grabability = Player.ObjectGrabability.OneHand;
        }
    }
}
