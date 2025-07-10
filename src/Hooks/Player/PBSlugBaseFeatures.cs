using UnityEngine;
using static PitchBlack.Plugin;

namespace PitchBlack;

public static class PBSlugBaseFeatures
{

    public static void Apply()
    {
        On.Player.Jump += Player_Jump;
    }

    /// <summary>
    /// FlipBoost
    /// Injects FlipBoost into a jump height calculation.
    /// </summary>
    private static void Player_Jump(On.Player.orig_Jump orig, Player self)
    {
        orig(self);

        if (FlipBoost.TryGet(self, out var power))
        {
            if (Player.AnimationIndex.Flip == self.animation)
            {
                self.jumpBoost *= 1f + power;
            }
            else
            {
                self.jumpBoost *= 1f + 0.1f;
            }
        }
    }
}