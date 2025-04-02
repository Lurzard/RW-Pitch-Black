using Fisobs.Core;

namespace PitchBlack;

public class UmbraMaskAbstract : AbstractPhysicalObject
{
    public UmbraMaskAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, UmbraMaskFisob.UmbraMask, null, pos, ID)
    {
        scaleX = 1;
        scaleY = 1;
        saturation = 0.5f;
        hue = 1f;
    }
    public override void Realize()
    {
        base.Realize();
        if (realizedObject == null)
            realizedObject = new UmbraMask(this);
    }

    public float hue;
    public float saturation;
    public float scaleX;
    public float scaleY;
    public float damage;

    public int colorSeed;

    public override string ToString()
    {
        return this.SaveToString($"{hue};{saturation};{scaleX};{scaleY};{damage};{colorSeed}");
    }
}
