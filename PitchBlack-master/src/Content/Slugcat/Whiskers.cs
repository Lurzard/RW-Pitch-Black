using System;
using UnityEngine;
using RWCustom;
using Colour = UnityEngine.Color;
using SlugBase.DataTypes;
using SlugBase.Features;
using SlugBase;

namespace PitchBlack;

public class Whiskers
{
    //whisker data is put into Photo's pre-existing CWT instead of making a new one

    public bool ready = false;
    public WeakReference<Player> playerRef;
    public string spriteName = "LizardScaleA0"; //just for changing out what sprite is used

    public int initialWhiskerIndex;
    public int endWhiskerIndex;
    public int initialLowerWhiskerIndex; //its just initialWhiskerIndex + headScales.Length / 2

    public Vector2[] headPositions = new Vector2[4];
    public PlayerGraphics.AxolotlScale[] headScales = new PlayerGraphics.AxolotlScale[4];
    //public Colour whiskerColour;

    public Whiskers(PlayerGraphics playerGraphics)
    {
        playerRef = new WeakReference<Player>(playerGraphics.player); //sets up a weak reference to the player

        for (int i = 0; i < headScales.Length; i++)
        {
            headScales[i] = new PlayerGraphics.AxolotlScale(playerGraphics);
            headPositions[i] = new Vector2(i < headScales.Length / 2 ? 0.7f : -0.7f, i == 1 ? 0.035f : 0.026f);
        }

        //SetupWhiskerColour(playerGraphics.player);

        //initialWhiskerIndex, endWhiskerIndex & initialLowerWhiskerIndex is done in PlayerGraphics.InitializeSprites;
    }
    //public int FaceWhiskerPos(int side, int pair)
    //{
    //    //it goes 14, 16, 15, 17, something like that
    //    //this falls apart with a larger array and starts to get previously-accessed indices
    //    return initialWhiskerIndex + side + pair + pair;
    //}

    public void Update()
    {
        if (!playerRef.TryGetTarget(out Player player))
            return;

        PlayerGraphics playerGraphics = player.graphicsModule as PlayerGraphics;
        Vector2 pos = player.bodyChunks[0].pos;
        Vector2 pos2 = player.bodyChunks[1].pos;
        //int whiskerIndexedToZero = initialLowerWhiskerIndex - initialWhiskerIndex;

        for (int index = 0; index < headPositions.Length; index++)
        {
            float num = 0f;
            float num2 = 90f;
            int num3 = index % (headScales.Length / 2);
            float num4 = num2 / (headScales.Length / 2);

            //pos.x is the mf with a stronghold over whisker movement and rotation
            if (PBEnums.SlugcatStatsName.Beacon == player.slugcatStats.name)
            {
                //beacon works the same if i didnt reverse it like this
                //i just didnt want to mess with rotation math AGAIN just to have an identical end result
                //since i originally thought beacon had to be reversed like this
                if ((index + 2) % 2 != 0)
                {
                    pos.x -= 4f;
                }
                else
                {
                    pos.x += 2f;
                    //if this pos.x Abs value is lower than the above pos x's Abs (2f < 4f)
                    //then the right whiskers will not get shaved when beacon jump-walks to the right
                }
            }
            else
            {
                if ((index + 2) % 2 != 0) //dont divide by zero exception on me
                {
                    pos.x += 8f;
                }
                else
                    pos.x -= 8f;
            }

            Vector2 a = Custom.rotateVectorDeg(Custom.DegToVec(0f), num3 * num4 - num2 / 2f + num + 90f);
            float f = Custom.VecToDeg(playerGraphics.lookDirection);
            Vector2 vector = Custom.rotateVectorDeg(Custom.DegToVec(0f), num3 * num4 - num2 / 2f + num);
            Vector2 a2 = Vector2.Lerp(vector, Custom.DirVec(pos2, pos), Mathf.Abs(f));

            if (headPositions[index].y < 0.2f)
            {
                a2 -= a * Mathf.Pow(Mathf.InverseLerp(0.2f, 0f, headPositions[index].y), 2f) * 2f;
            }
            a2 = Vector2.Lerp(a2, vector, Mathf.Pow(0.0875f, 1f)).normalized;
            Vector2 vector2 = pos + a2 * headScales.Length;
            if (!Custom.DistLess(headScales[index].pos, vector2, headScales[index].length / 2f))
            {
                Vector2 a3 = Custom.DirVec(headScales[index].pos, vector2);
                float num5 = Vector2.Distance(headScales[index].pos, vector2);
                float num6 = headScales[index].length / 2f;
                headScales[index].pos += a3 * (num5 - num6);
                headScales[index].vel += a3 * (num5 - num6);
            }
            headScales[index].vel += Vector2.ClampMagnitude(vector2 - headScales[index].pos, 10f) / Mathf.Lerp(5f, 1.5f, 0.5873646f);
            headScales[index].vel *= Mathf.Lerp(1f, 0.8f, 0.5873646f);
            headScales[index].ConnectToPoint(pos, headScales[index].length, true, 0f, Vector2.zero, 0f, 0f);
            headScales[index].Update();
        }
    }
    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser)
    {
        if (!playerRef.TryGetTarget(out Player player))
            return;

        float whiskerHeight = 10f / Futile.atlasManager.GetElementWithName(spriteName).sourcePixelSize.y;
        if (PBEnums.SlugcatStatsName.Beacon == player.slugcatStats.name)
            whiskerHeight *= 1.5f;

        for (int i = initialWhiskerIndex; i < endWhiskerIndex; i++)
        {
            sLeaser.sprites[i] = new(spriteName)
            {
                scaleY = whiskerHeight,
                anchorY = 0.1f
            };
        }
    }
    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        for (int i = initialWhiskerIndex; i < endWhiskerIndex; i++)
        {
            rCam.ReturnFContainer("Foreground").RemoveChild(sLeaser.sprites[i]);
            rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[i]);
            sLeaser.sprites[i].MoveInFrontOfOtherNode(sLeaser.sprites[3]);
        }
    }
    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, float timeStacker, Vector2 camPos)
    {
        if (!playerRef.TryGetTarget(out Player player))
            return;

        for (int i = initialWhiskerIndex; i < endWhiskerIndex; i++)
        {
            Vector2 vector = new(sLeaser.sprites[9].x + camPos.x, sLeaser.sprites[9].y + camPos.y);

            float rotationAngle; //+ve goes down, -ve goes up (for left whiskers, opposite for right)

            if (PBEnums.SlugcatStatsName.Beacon == player.slugcatStats.name)
                rotationAngle = -135f;
            else
                rotationAngle = 180f;

            if (i >= initialLowerWhiskerIndex)
            {
                //makes the lower whiskers go down so it wont overlap with the upper whiskers
                //this might be the upper whiskers actually. i dont know
                rotationAngle -= 40f;
            }
            else if (PBEnums.SlugcatStatsName.Beacon == player.slugcatStats.name && i % 2 != 0)
            {
                //put the top left whiskers a little more down
                rotationAngle -= 10f;
            }

            if (i % 2 != 0)
            {
                //left whiskers
                if (PBEnums.SlugcatStatsName.Photomaniac == player.slugcatStats.name)
                    rotationAngle -= 4f;
                else
                    rotationAngle += 2f;
                rotationAngle *= -1;
                vector.x -= 5f;
                sLeaser.sprites[i].scaleX = -0.4f;
                //sLeaser.sprites[i].color = Colour.green;
            }
            else
            {
                //right whiskers
                rotationAngle += 5f;
                vector.x += 5f;
                sLeaser.sprites[i].scaleX = 0.4f;
                //sLeaser.sprites[i].color = Colour.red;
            }

            int index = i - initialWhiskerIndex;

            sLeaser.sprites[i].x = vector.x - camPos.x;
            sLeaser.sprites[i].y = vector.y - camPos.y;
            sLeaser.sprites[i].rotation = Custom.AimFromOneVectorToAnother(vector, Vector2.Lerp(headScales[index].lastPos, headScales[index].pos, timeStacker)) + rotationAngle;
            //sLeaser.sprites[i].color = whiskerColour;
            sLeaser.sprites[i].color = sLeaser.sprites[0].color; //[0] is body sprite
        }
    }

    public void ApplyPalette(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser)
    {
        //Colour newColour = whiskerColour;
        Colour newColour = sLeaser.sprites[0].color;
        if (self.malnourished > 0f)
        {
            float num = self.player.Malnourished ? self.malnourished : Mathf.Max(0f, self.malnourished - 0.005f);
            newColour = Colour.Lerp(newColour, Colour.gray, 0.4f * num);
        }
        newColour = self.HypothermiaColorBlend(newColour);

        for (int i = initialWhiskerIndex; i < endWhiskerIndex; i++)
        {
            sLeaser.sprites[i].color = newColour;
        }
    }
}