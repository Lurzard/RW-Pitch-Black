namespace PitchBlack;

public class MakeFlares : UpdatableAndDeletable
{
    public MakeFlares(Room room)
    {
        this.room = room;
        if (room.game.session.Players[0].pos.room != room.abstractRoom.index)
        {
            Destroy();
        }
    }
    public override void Update(bool eu)
    {
        base.Update(eu);
        if (room.game.session.Players[0].realizedCreature != null &&
        room.game.cameras[0].hud != null &&
        room.game.cameras[0].hud.textPrompt != null &&
        room.game.cameras[0].hud.textPrompt.messages.Count < 1)
        {
            int num = message;
            switch (message)
            {
                case 0:
                    room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("Hold a rock and GRAB to create flares"), 120, 160, false, true);
                    message++;
                    return;
                case 1:
                    room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("Then hold GRAB with a flare to store it in your collar"), 120, 160, false, true);
                    message++;
                    return;
                case 2:
                    Destroy();
                    break;
                default:
                    return;
            }
        }
    }
    public int message;
}
public class Thanatosis : UpdatableAndDeletable
{
    public Thanatosis(Room room)
    {
        this.room = room;
        if (room.game.session.Players[0].pos.room != room.abstractRoom.index)
        {
            Destroy();
        }
    }
    public override void Update(bool eu)
    {
        base.Update(eu);
        if (room.game.session.Players[0].realizedCreature != null &&
       room.game.cameras[0].hud != null &&
       room.game.cameras[0].hud.textPrompt != null &&
       room.game.cameras[0].hud.textPrompt.messages.Count < 1)
        {
            int num = message;
            switch (message)
            {
                case 0:
                    //room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("Hold SPECIAL to kill yourself"), 120, 160, false, true);
                    room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("Hold SPECIAL to enter and exit a state of thanatosis"), 120, 160, false, true);
                    message++;
                    return;
                case 1:
                    room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("Walking this fringe comes with its price"), 120, 160, false, true);
                    message++;
                    return;
                case 2:
                    room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("How long can you oscillate..."), 120, 160, false, true);
                    message++;
                    return;
                case 3:
                    Destroy();
                    break;
                default:
                    return;
            }
        }
    }
    public int message;
}
