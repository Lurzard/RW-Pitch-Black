namespace PitchBlack;

public class LittleLongLegs(AbstractCreature abstractCreature, World world)
    : DaddyLongLegs(abstractCreature, world), IPlayerEdible
{
    internal const int TooMuchFoodToBeCarried = 8;
    const int SplitSizeChange = 6;
    public int splitCounter = 0;
    public int BitesLeft {get; set;} = 3;

    public int FoodPoints {get; set;} = 2;

    public bool Edible => !State.dead || (State.dead && FoodPoints < TooMuchFoodToBeCarried);

    public bool AutomaticPickUp => false;

    public void BitByPlayer(Grasp grasp, bool eu)
    {
        BitesLeft--;
        room.PlaySound(BitesLeft == 0 ? SoundID.Slugcat_Eat_Centipede : SoundID.Slugcat_Bite_Centipede, mainBodyChunk.pos);
        firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
        if (BitesLeft < 1)
        {
            (grasp.grabber as Player).ObjectEaten(this);
            grasp.Release();
            Destroy();
        }
    }

    public void ThrowByPlayer()
    {
        Stun(30);
    }

    public void SizeChange(int increase) {
        foreach(BodyChunk chunk in bodyChunks) {
            chunk.rad += increase;
        }
        foreach(var connection in bodyChunkConnections) {
            connection.distance += increase;
        }
    }

    public void LittleLongLegsSplit() {
        splitCounter = 0;
        int diff = FoodPoints - SplitSizeChange;
        FoodPoints = diff;
        State.meatLeft = diff;
        SizeChange(-SplitSizeChange);
        AbstractCreature creature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(Enums.CreatureTemplateType.LMiniLongLegs), null, room.GetWorldCoordinate(mainBodyChunk.pos), room.game.GetNewID());
        room.abstractRoom.AddEntity(creature);
        creature.RealizeInRoom();
        (creature.realizedCreature as LittleLongLegs).FoodPoints = SplitSizeChange;
        (creature.realizedCreature as LittleLongLegs).SizeChange(SplitSizeChange-2);
        creature.realizedCreature.Stun(55);
    }
}