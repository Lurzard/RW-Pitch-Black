using System.Runtime.CompilerServices;
using RWCustom;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace PitchBlack;

public class UmbraMask : PlayerCarryableItem, IDrawable
{
    public static readonly VultureMask.MaskType UMBRA = new VultureMask.MaskType("UMBRA", true);

    public bool UmbraScav
    {
        get
        {
            return maskType == UMBRA;
        }
    }

    public UmbraMaskAbstract AbstrUmbraMsk
    {
        get
        {
            return abstractPhysicalObject as UmbraMaskAbstract;
        }
    }

    public UmbraMask(UmbraMaskAbstract abstractPhysicalObject) : base(abstractPhysicalObject)
    {
        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.14f);
        bodyChunkConnections = new PhysicalObject.BodyChunkConnection[0];
        airFriction = 0.999f;
        gravity = 0.9f;
        bounce = 0.4f;
        surfaceFriction = 0.3f;
        collisionLayer = 2;
        waterFriction = 0.98f;
        buoyancy = 0.6f;
        GenerateColor(AbstrUmbraMsk.colorSeed);
    }

    public void GenerateColor(int colorSeed)
    {
            ColorA = new HSLColor(1f, 1f, 1f);
            ColorB = new HSLColor(1f, 1f, 1f);
            return;
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
        onGroundPos = Random.Range(0, 3) - 1;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        lastRotationA = rotationA;
        lastRotationB = rotationB;
        lastDonned = donned;
        lastViewFromSide = viewFromSide;
        float to = 0f;
        float to2 = 0f;
        rotationA = Custom.DegToVec(Custom.VecToDeg(rotationA) + rotVel.x);
        rotationB = Custom.DegToVec(Custom.VecToDeg(rotationB) + rotVel.y);
        rotVel = Vector2.ClampMagnitude(rotVel, 50f);
        rotVel *= Custom.LerpMap(rotVel.magnitude, 5f, 50f, 1f, 0.8f);
        fallOffVultureMode = Mathf.Max(0f, fallOffVultureMode - 0.00625f);
        CollideWithTerrain = grabbedBy.Count == 0;
        CollideWithObjects = grabbedBy.Count == 0;
        if (grabbedBy.Count > 0)
        {
            Vector2 vector = Custom.PerpendicularVector(firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos);
            if (grabbedBy[0].grabber is Player player)
            {
                vector *= Mathf.Sign(Custom.DistanceToLine(firstChunk.pos, player.bodyChunks[0].pos, player.bodyChunks[1].pos));
                if (player.graphicsModule != null && player.standing && (player.bodyMode != Player.BodyModeIndex.ClimbingOnBeam || player.animation == Player.AnimationIndex.StandOnBeam) && player.bodyMode != Player.BodyModeIndex.Swimming && (grabbedBy[0].graspUsed == 1 || player.grasps[1] == null || player.grasps[1].grabbed.abstractPhysicalObject.type != AbstractPhysicalObject.AbstractObjectType.VultureMask))
                {
                    to = Mathf.InverseLerp(15f, 10f, Vector2.Distance((player.graphicsModule as PlayerGraphics).hands[grabbedBy[0].graspUsed].pos, player.mainBodyChunk.pos));
                    if (player.input[0].x != 0 && Mathf.Abs(player.bodyChunks[1].lastPos.x - player.bodyChunks[1].pos.x) > 2f)
                    {
                        to2 = player.input[0].x;
                    }
                }
            }
            rotationA = Vector3.Slerp(rotationA, vector, 0.5f);
            rotationB = new Vector2(0f, 1f);
        }
        else if (firstChunk.ContactPoint.y < 0)
        {
            Vector2 b;
            Vector2 b2;
            if (onGroundPos == 0)
            {
                b = new Vector2(0f, 1f);
                b2 = new Vector2(0f, -1f);
            }
            else
            {
                b = Custom.DegToVec(15f * (float)onGroundPos);
                b2 = Custom.DegToVec(120f * (float)onGroundPos);
            }
            rotationA = Vector2.Lerp(rotationA, b, Random.value);
            rotationB = Vector2.Lerp(rotationB, b2, Random.value);
            rotVel *= Random.value;
        }
        else if (Vector2.Distance(firstChunk.lastPos, firstChunk.pos) > 5f && rotVel.magnitude < 7f)
        {
            rotVel += Custom.RNV() * (Mathf.Lerp(7f, 25f, Random.value) + firstChunk.vel.magnitude * 2f);
            onGroundPos = Random.Range(0, 3) - 1;
        }
        donned = Custom.LerpAndTick(donned, to, 0.11f, 0.033333335f);
        viewFromSide = Custom.LerpAndTick(viewFromSide, to2, 0.11f, 0.033333335f);
    }

    public override void PickedUp(Creature upPicker)
    {
        room.PlaySound(SoundID.Vulture_Mask_Pick_Up, firstChunk);
    }

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        base.TerrainImpact(chunk, direction, speed, firstContact);
        if (grabbedBy.Count == 0 && speed > 4f && firstContact)
        {
            room.PlaySound(SoundID.Vulture_Mask_Terrain_Impact, firstChunk, false, Custom.LerpMap(speed, 4f, 9f, 0.2f, 1f), 1f);
        }
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        newContatiner ??= rCam.ReturnFContainer("Items");
        for (int i = 0; i < 4; i++)
        {
            sLeaser.sprites[i].RemoveFromContainer();
        }
        newContatiner.AddChild(sLeaser.sprites[0]);
        newContatiner.AddChild(sLeaser.sprites[3]);
        newContatiner.AddChild(sLeaser.sprites[4]);
        newContatiner.AddChild(sLeaser.sprites[0]);
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        blackColor = palette.blackColor;
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 vector = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        float num = Mathf.Lerp(lastDonned, donned, timeStacker);
        Vector2 vector2 = Vector3.Slerp(lastRotationA, rotationA, timeStacker);
        Vector2 vector3 = Vector3.Slerp(lastRotationB, rotationB, timeStacker);
        if (num > 0f && grabbedBy.Count > 0 && grabbedBy[0].grabber is Player player)
        {
            if (player.graphicsModule is PlayerGraphics playerGraphics)
            {
                float num22 = Mathf.Lerp(lastViewFromSide, viewFromSide, timeStacker);
                Vector2 vector4 = Custom.DirVec(Vector2.Lerp(playerGraphics.drawPositions[1, 1], playerGraphics.drawPositions[1, 0], timeStacker), Vector2.Lerp(playerGraphics.drawPositions[0, 1], playerGraphics.drawPositions[0, 0], timeStacker));
                Vector2 vector5 = Vector2.Lerp(playerGraphics.drawPositions[0, 1], playerGraphics.drawPositions[0, 0], timeStacker) + vector4 * 3f;
                vector5 = Vector2.Lerp(vector5, Vector2.Lerp(playerGraphics.head.lastPos, playerGraphics.head.pos, timeStacker) + vector4 * 3f, 0.5f);
                vector5 += Vector2.Lerp(playerGraphics.lastLookDir, playerGraphics.lookDirection, timeStacker) * 1.5f;
                vector2 = Vector3.Slerp(vector2, vector4, num);
                if ((playerGraphics.owner as Player).eatCounter < 35)
                {
                    vector3 = Vector3.Slerp(vector3, new Vector2(0f, -1f), num);
                    vector5 += vector4 * Mathf.InverseLerp(35f, 15f, (playerGraphics.owner as Player).eatCounter) * 7f;
                }
                else
                {
                    vector3 = Vector3.Slerp(vector3, new Vector2(0f, 1f), num);
                }
                if (num22 != 0f)
                {
                    vector2 = Custom.DegToVec(Custom.VecToDeg(vector2) - 20f * num22);
                    vector3 = Vector3.Slerp(vector3, Custom.DegToVec(-50f * num22), Mathf.Abs(num22));
                    vector5 += vector4 * 2f * Mathf.Abs(num22);
                    vector5 -= Custom.PerpendicularVector(vector4) * 4f * num22;
                }
                vector = Vector2.Lerp(vector, vector5, num);
            }
        }

        float num5 = rCam.room.Darkness(vector) * (1f - rCam.room.LightSourceExposure(vector)) * 0.8f * (1f - fallOffVultureMode);
        float num6 = Custom.VecToDeg(vector3);
        int spriteIndexByRotation = Custom.IntClamp(Mathf.RoundToInt(Mathf.Abs(num6 / 180f) * 8f), 0, 8);
        float num4 = 1.15f;

        // float num2 = Custom.VecToDeg(vector2);
        // int num3 = Custom.IntClamp(Mathf.RoundToInt(Mathf.Abs(num2 / 180f) * 8f), 0, 8);

        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {

            string element = "UmbraMask";
            sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName(element + spriteIndexByRotation.ToString());
            sLeaser.sprites[i].scaleX = Mathf.Sign(num6) * num4;
            sLeaser.sprites[i].anchorY = Custom.LerpMap(Mathf.Abs(num6), 0f, 100f, 0.5f, 0.675f, 2.1f);
            sLeaser.sprites[i].anchorX = 0.5f - vector3.x * 0.1f * Mathf.Sign(num6);
            sLeaser.sprites[i].rotation = Custom.VecToDeg(vector2);
            sLeaser.sprites[i].x = vector.x - camPos.x;
            sLeaser.sprites[i].y = vector.y - camPos.y;
            sLeaser.sprites[i].color = Color.Lerp(Color.Lerp(Color.Lerp(HSLColor.Lerp(ColorA, ColorB, 0.8f - 0.3f * fallOffVultureMode).rgb, blackColor, 0.53f), Color.Lerp(ColorA.rgb, new Color(1f, 1f, 1f), 0.35f), 0.1f), blackColor, 0.6f * num5);
        }
        sLeaser.sprites[1].scaleX *= 0.85f * num4;
        sLeaser.sprites[1].scaleY = 0.9f * num4;
        sLeaser.sprites[2].scaleY = 1.1f * num4;
        sLeaser.sprites[2].anchorY += 0.015f;

        if (blink > 0 && Random.value < 0.5f)
        {
            for (int j = 0; j < 4; j++)
            {
                sLeaser.sprites[j].color = new Color(1f, 1f, 1f);
            }
            return;
        }
        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
        sLeaser.sprites = new FSprite[5];
        sLeaser.sprites[0] = new FSprite("pixel", true);
        sLeaser.sprites[1] = new FSprite("pixel", true);
        sLeaser.sprites[2] = new FSprite("pixel", true);
        sLeaser.sprites[3] = new FSprite("pixel", true);
        sLeaser.sprites[4] = new FSprite("pixel", true);
        for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].scale = 1.15f;
            }
            AddToContainer(sLeaser, rCam, null);
        }


    public VultureMask.MaskType maskType;
    public int firstSprite;

    public Vector2 rotationA;
    public Vector2 lastRotationA;
    public Vector2 rotationB;
    public Vector2 lastRotationB;
    public Vector2 rotVel;
    public int onGroundPos;
    public float donned;
    public float lastDonned;
    public float viewFromSide;
    public float lastViewFromSide;
    public float fallOffVultureMode;
    public Color blackColor;
    public HSLColor ColorA;
    public HSLColor ColorB;
}
