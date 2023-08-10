using Vector2 = UnityEngine.Vector2;
using Debug = UnityEngine.Debug;
using static PitchBlack.RoomScripts;

namespace PitchBlack;

public class RoomScripts
{
    internal static bool EveryoneStopDyingRightNowInTheIntroScript = false;
    public static void Apply()
    {
        On.Player.Die += Player_Die;
        On.RoomSpecificScript.AddRoomSpecificScript += RoomSpecificScript_AddRoomSpecificScript;
    }

    private static void Player_Die(On.Player.orig_Die orig, Player self)
    {
        if (EveryoneStopDyingRightNowInTheIntroScript)
            return;

        orig(self);
    }

    private static void RoomSpecificScript_AddRoomSpecificScript(On.RoomSpecificScript.orig_AddRoomSpecificScript orig, Room room)
    {
        orig(room);

        if (room.game.session is StoryGameSession story
            && MiscUtils.IsBeaconOrPhoto(story.game.StoryCharacter)
            && story.saveState.denPosition == "SH_CABINETS1"
            && room.abstractRoom.name == "SH_CABINETS1")
        {
            room.AddObject(new SH_CABINETS1_IntroScript(room));
        }
    }
}

public class SH_CABINETS1_IntroScript : UpdatableAndDeletable
{
    int counter;
    const int COUNTER_MAX = 10 * 40; //when to force the script to stop and give players their controllers back
    bool hitWater;
    bool StayInAir => room.game.manager.FadeDelayInProgress || !room.fullyLoaded || !room.BeingViewed;
    Player RealizedPlayer => room.game.Players.Count > 0 ? room.game.Players[0].realizedCreature as Player : null;

    public SH_CABINETS1_IntroScript(Room room)
    {
        this.room = room;
        counter = 0;
        EveryoneStopDyingRightNowInTheIntroScript = true;
        Debug.Log($"Pitch Black: Created new {nameof(SH_CABINETS1_IntroScript)} room script in room {room.abstractRoom.name}");
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (null == RealizedPlayer)
        {
            return;
        }

        counter++;

        if (StayInAir)
        {
            foreach (var abstrCrit in room.game.session.Players)
            {
                if (abstrCrit == null || abstrCrit.realizedCreature == null)
                    continue;

                var player = abstrCrit.realizedCreature as Player;

                player.controller ??= new Player.NullController(); //??= (null coalescing operator) is if null, then assign new thing

                if (RealizedPlayer != player)
                    player.SuperHardSetPosition(new Vector2(923, 294)); //co-op players start in the water
            }

            RealizedPlayer.SuperHardSetPosition(new Vector2(923, 2373)); //player 1 starts at the top and falls down
            return;
        }

        if (RealizedPlayer.Submersion > 0f)
        {
            hitWater = true;
        }

        if (hitWater || COUNTER_MAX == counter) //has a counter check to forcibly give players back their controller, even if the script fucks up
        {
            EveryoneStopDyingRightNowInTheIntroScript = false;
            foreach (var abstrCrit in room.game.session.Players)
            {
                if (abstrCrit == null || abstrCrit.realizedCreature == null)
                    continue;

                (abstrCrit.realizedCreature as Player).controller = null;
            }
            return;
        }
    }
}
