namespace PitchBlack;

public class LittleLongLegs : DaddyLongLegs, IPlayerEdible {
    public LittleLongLegs(AbstractCreature abstractCreature, World world) : base(abstractCreature, world) {
        BitesLeft = 3;
    }
    public int BitesLeft {get; set;}

    public int FoodPoints => 2;

    public bool Edible => true;

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
}