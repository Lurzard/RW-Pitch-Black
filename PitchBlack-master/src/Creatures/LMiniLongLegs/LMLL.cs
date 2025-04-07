namespace PitchBlack;

public class LittleLongLegs : DaddyLongLegs, IPlayerEdible {
    internal const int TooMuchFoodToBeCarried = 8;
    const int SplitSizeChange = 6;
    public LittleLongLegs(AbstractCreature abstractCreature, World world) : base(abstractCreature, world) {
        BitesLeft = 3;
        FoodPoints = 2;
        splitCounter = 0;
    }
    public int splitCounter;
    public int BitesLeft {get; set;}

    public int FoodPoints {get; set;}

    public bool Edible => !State.dead || (State.dead && FoodPoints < TooMuchFoodToBeCarried);

    public bool AutomaticPickUp => false;

    public void BitByPlayer(Grasp grasp, bool eu)
    {
        this.BitesLeft--;
        this.room.PlaySound((this.BitesLeft == 0) ? SoundID.Slugcat_Eat_Centipede : SoundID.Slugcat_Bite_Centipede, base.mainBodyChunk.pos);
        base.firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
        if (this.BitesLeft < 1)
        {
            (grasp.grabber as Player).ObjectEaten(this);
            grasp.Release();
            this.Destroy();
        }
    }

    public void ThrowByPlayer()
    {
        Stun(30);
    }

    public void LittleLongLegsSizeChange(int increase) {
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
        LittleLongLegsSizeChange(-SplitSizeChange);
        AbstractCreature abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(PBCreatureTemplateType.LMiniLongLegs), null, room.GetWorldCoordinate(mainBodyChunk.pos), room.game.GetNewID());
        room.abstractRoom.AddEntity(abstractCreature);
        abstractCreature.RealizeInRoom();
        (abstractCreature.realizedCreature as LittleLongLegs).FoodPoints = SplitSizeChange;
        (abstractCreature.realizedCreature as LittleLongLegs).LittleLongLegsSizeChange(SplitSizeChange-2);
        abstractCreature.realizedCreature.Stun(55);
    }
}