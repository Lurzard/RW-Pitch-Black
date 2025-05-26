using System.Collections.Generic;

namespace PitchBlack;

public class ScugHooks {
    public static void Apply() {
        //hooks that bacon and photo share
        On.Player.ctor += Player_ctor;
        On.Player.Grabability += BeaconDontWantToTouchCollar;
    }

    public static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world) {
        orig(self, abstractCreature, world);

        if (MiscUtils.IsBeaconOrPhoto(self.slugcatStats.name)) {
            if (self.slugcatStats.name == PBEnums.SlugcatStatsName.Photomaniac) {
                self.playerState.isPup = true;
            }

            if (!Plugin.scugCWT.TryGetValue(self, out _)) {
                Plugin.scugCWT.Add(self, MiscUtils.IsBeacon(self)? new BeaconCWT(self) : new PhotoCWT());
            }
            
            if (self.slugcatStats.name == PBEnums.SlugcatStatsName.Beacon &&
                self.room.abstractRoom.shelter && Plugin.scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT cwt) {
                foreach (List<PhysicalObject> thingQuar in self.room.physicalObjects) {
                    foreach (PhysicalObject item in thingQuar) {
                        if (item is FlareBomb flashbang && cwt.storage.storedFlares.Count < cwt.storage.capacity) {
                            foreach (var player in self.room.PlayersInRoom) {
                                if (player != null && Plugin.scugCWT.TryGetValue(player, out var op) && op is BeaconCWT otherPlayer && otherPlayer.storage.storedFlares.Contains(flashbang)) {
                                    goto SkipAddingFlare;
                                }
                            }
                            cwt.storage.FlarebombtoStorage(flashbang);
                            SkipAddingFlare:;
                        }
                    }
                }
            }
        }
    }
    public static Player.ObjectGrabability BeaconDontWantToTouchCollar(On.Player.orig_Grabability orig, Player self, PhysicalObject obj) {
        Player.ObjectGrabability result = orig(self, obj);

        if (self.slugcatStats.name == PBEnums.SlugcatStatsName.Photomaniac && obj is Spear) {
            return Player.ObjectGrabability.OneHand;
        }

        if (obj is FlareBomb flarebomb && obj.room != null) {
            foreach (AbstractCreature abstrCrit in flarebomb.room.game.Players) {
                if (abstrCrit.realizedCreature == null) {
                    continue;
                }
                if (Plugin.scugCWT.TryGetValue(abstrCrit.realizedCreature as Player, out ScugCWT cwt) && cwt is BeaconCWT beaconCWT) {
                    if (beaconCWT.storage.storedFlares.Contains(flarebomb))
                        return Player.ObjectGrabability.CantGrab;
                }
            }
        }

        return result;
    }
}
