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
        if (Plugin.scugCWT.TryGetValue(self, out ScugCWT cwt) && cwt.IsBeacon)
        {
            if (cwt.Beacon.storage != null && obj is FlareBomb flare)
            {
                //foreach (FlareBomb storedFlare in cwt.Beacon.storage.storedFlares)
                //{
                //    if (storedFlare == flare)
                //    {
                //        return Player.ObjectGrabability.CantGrab;
                //    }
                //}
                if (cwt.Beacon.storage.storedFlares.Contains(flare))
                    return Player.ObjectGrabability.CantGrab;
            }
        }
        else if (self.slugcatStats.name == Plugin.PhotoName && obj is Weapon)
            return Player.ObjectGrabability.OneHand;
        return result;
    }
}
