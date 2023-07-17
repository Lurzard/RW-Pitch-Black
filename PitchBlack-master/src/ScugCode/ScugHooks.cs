namespace PitchBlack;

public class ScugHooks
{
    public static void Apply()
    {
        //hooks that bacon and photo share
        On.Player.ctor += Player_ctor;
        On.Player.Grabability += BeaconDontWantToTouchCollar;
    }

    public static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);

        if (MiscUtils.IsBeaconOrPhoto(self.slugcatStats.name))
        {
            if (self.slugcatStats.name == Plugin.PhotoName)
            {
                self.playerState.isPup = true;
            }

            if (!Plugin.scugCWT.TryGetValue(self, out _))
                Plugin.scugCWT.Add(self, new ScugCWT(self));
        }
    }
    public static Player.ObjectGrabability BeaconDontWantToTouchCollar(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        Player.ObjectGrabability result = orig(self, obj);

        if (self.slugcatStats.name == Plugin.PhotoName && obj is Spear)
            return Player.ObjectGrabability.OneHand;

        if (obj is FlareBomb flarebomb && obj.room != null)
        {
            foreach (AbstractCreature abstrCrit in flarebomb.room.game.Players)
            {
                if (abstrCrit.realizedCreature == null)
                    continue;

                if (Plugin.scugCWT.TryGetValue(abstrCrit.realizedCreature as Player, out var cwt) && cwt.IsBeacon)
                {
                    if (cwt.Beacon.storage.storedFlares.Contains(flarebomb))
                        return Player.ObjectGrabability.CantGrab;
                }
            }
        }

        return result;
    }
}
