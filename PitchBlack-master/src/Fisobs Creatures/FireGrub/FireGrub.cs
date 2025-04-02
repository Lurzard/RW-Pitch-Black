using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PitchBlack;

public class FireGrub : TubeWorm, IProvideWarmth
{
    public HSLColor hslColor;
    public float charge;
    public int heldCounter;
    public FireGrub(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        charge = dead ? 0f : 1f;
        heldCounter = 0;

        Random.State seed = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        hslColor = new HSLColor(Custom.WrappedRandomVariation(0.1f, 0.05f, 0.6f), 1f, Custom.ClampedRandomVariation(0.5f, 0.15f, 0.1f));
        Random.state = seed;
    }

    public Room loadedRoom => room;

    public float warmth => 0.2f;

    public float range => 50;

    public Vector2 Position()
    {
        return mainBodyChunk.pos;
    }

    public override void InitiateGraphicsModule()
    {
        graphicsModule ??= new FireGrubGraphics(this);
        graphicsModule.Reset();
    }

    public override void Update(bool eu) {
        float lastLungs = lungs;
        base.Update(eu);
        if (lungs < lastLungs)
        {
            // Speed up drowning
            lungs += (lungs - lastLungs) * 0.3f;
        }
        else
        {
            // Speed up air recovery
            lungs += lungs + (lungs - lastLungs) * 2f;
        }
        lungs = Mathf.Clamp01(lungs);

        if(dead)
        {
            // Drain charge in 20 seconds
            charge = Mathf.Clamp01(charge - 1f / 20f / 40f);
        }

        if (grabbedBy.Count > 0) {
            heldCounter++;
        }
        else {
            heldCounter = 0;
        }
    }
}