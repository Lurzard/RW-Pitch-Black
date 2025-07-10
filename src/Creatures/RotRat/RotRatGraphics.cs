using System;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;
using static PitchBlack.Plugin;

namespace PitchBlack;

internal class RotratGraphics : MouseGraphics
{
    public RotratGraphics(PhysicalObject ow) : base(ow)
    {
        rotRatData.Add(this,new RotData(10));
        tail.rad = 10f;
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        #region Base LanternMouse InitiateSprites
        sLeaser.sprites = new FSprite[this.TotalSprites];
        for (int i = 0; i < 3; i++)
        {
            sLeaser.sprites[this.RopeSprite(i)] = new CustomFSprite("pixel");
            sLeaser.sprites[this.RopeSprite(i)].anchorY = 0f;
        }
        for (int j = 3; j < this.TotalSprites; j++)
        {
            sLeaser.sprites[j] = new FSprite("pixel", true);
        }
        sLeaser.sprites[this.EyeASprite(1)].scaleX = -1f;
        sLeaser.sprites[this.EyeBSprite(1)].scaleX = -1f;
        for (int k = 0; k < 2; k++)
        {
            sLeaser.sprites[this.EyeBSprite(k)].color = new Color(0.2f, 0f, 0f);
            sLeaser.sprites[this.EyeASprite(k)].color = new Color(0.8f, 0.2f, 0.2f);
        }
        for (int l = 0; l < 2; l++)
        {
            for (int m = 0; m < 2; m++)
            {
                sLeaser.sprites[this.LimbSprite(l, m)].element = Futile.atlasManager.GetElementWithName("mouse" + ((l == 1) ? "Hind" : "Front") + "Leg");
                sLeaser.sprites[this.LimbSprite(l, m)].anchorY = 0.1f;
                sLeaser.sprites[this.BackSpotSprite(l, m)].element = Futile.atlasManager.GetElementWithName("mouseSpot");
                sLeaser.sprites[this.BackSpotSprite(l, m)].color = new Color(1f, 0f, 0f);
            }
        }
        sLeaser.sprites[this.BodySprite(0)].element = Futile.atlasManager.GetElementWithName("mouseBodyA");
        sLeaser.sprites[this.BodySprite(1)].element = Futile.atlasManager.GetElementWithName("mouseBodyB");
        sLeaser.sprites[this.BodySprite(1)].anchorY = 0f;
        sLeaser.sprites[this.HeadSprite].element = Futile.atlasManager.GetElementWithName("mouseHead0");
        #endregion

        if (rotRatData.TryGetValue(this, out RotData rotData))
        {
            rotData.startSprite = sLeaser.sprites.Length;
            Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + rotData.numOfSprites);
            for (int i = rotData.startSprite; i < rotData.startSprite + rotData.numOfSprites; i+=2)
            {
                sLeaser.sprites[i] = new FSprite("Futile_White")
                {
                    shader = rCam.room.game.rainWorld.Shaders["JaggedCircle"],
                    scale = 0.6f
                };
                sLeaser.sprites[i + 1] = new FSprite("mouseEyeB5")
                {
                    scale = 0.6f
                };
            }
        }
        AddToContainer(sLeaser, rCam, null);
    }
    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        base.AddToContainer(sLeaser, rCam, newContatiner);
        if (rotRatData.TryGetValue(this,out RotData rotData))
        {
            FContainer container = rCam.ReturnFContainer("Midground");
            for (int i = rotData.startSprite; i < rotData.startSprite + rotData.numOfSprites; i+=2)
            {
                Debug.Log(i + " " + sLeaser.sprites.Length + " " + (rotData.startSprite + rotData.numOfSprites));
                sLeaser.sprites[i].RemoveFromContainer();
                container.AddChild(sLeaser.sprites[i]);
                sLeaser.sprites[i + 1].RemoveFromContainer();
                container.AddChild(sLeaser.sprites[i + 1]);
            }
        }
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (rotRatData.TryGetValue(this, out RotData rotData))
        {
            Vector2 vector = Vector2.Lerp(this.mouse.bodyChunks[1].lastPos, this.mouse.bodyChunks[1].pos, timeStacker);
            Vector2 vector2 = Vector2.Lerp(this.mouse.bodyChunks[0].lastPos, this.mouse.bodyChunks[0].pos, timeStacker);
	        float rotation = Custom.AimFromOneVectorToAnother(vector, vector2);

            #region Eye Sprites
            sLeaser.sprites[EyeBSprite(0)].isVisible = false;
            sLeaser.sprites[EyeBSprite(1)].isVisible = false;
            sLeaser.sprites[EyeCSprite(0)].element = Futile.atlasManager.GetElementWithName("mouseEyeB5");
            sLeaser.sprites[EyeCSprite(1)].element = Futile.atlasManager.GetElementWithName("mouseEyeB5");
            #endregion

            for (int i = rotData.startSprite; i < rotData.startSprite + rotData.numOfSprites; i+=2)
            {
                float zRotation = Mathf.Max(Mathf.Abs(Mathf.Lerp(a: lastProfileFac, profileFac, timeStacker)), Mathf.InverseLerp(0.5f, 0.7f, Mathf.Lerp(lastBackToCam, backToCam, timeStacker)));
                Debug.Log($"Pitch Black: {zRotation}");
                
                if (zRotation < 0.1f) {
                    sLeaser.sprites[i].isVisible = false;
                    sLeaser.sprites[i+1].isVisible = false;
                }
                else {
                    sLeaser.sprites[i].isVisible = true;
                    sLeaser.sprites[i+1].isVisible = true;
                }

                // zRotation -= Vector2.Distance(sLeaser.sprites[BodySprite(0)].GetPosition(), sLeaser.sprites[i].GetPosition());
                sLeaser.sprites[i].rotation = rotation;
                sLeaser.sprites[i].color = DecalColor;
                sLeaser.sprites[i].scaleX = Mathf.Lerp(0, 0.6f, zRotation);
                sLeaser.sprites[i].SetPosition(sLeaser.sprites[BodySprite(0)].GetPosition() + rotData.bulbs[(i-rotData.startSprite)/2].position);
                sLeaser.sprites[i].MoveToFront();
                sLeaser.sprites[i + 1].rotation = rotation;
                sLeaser.sprites[i + 1].color = mouse.iVars.color.rgb;
                sLeaser.sprites[i + 1].scaleX = Mathf.Lerp(0, 0.6f, zRotation);
                sLeaser.sprites[i + 1].SetPosition(sLeaser.sprites[i].GetPosition());
                sLeaser.sprites[i + 1].MoveToFront();
            }
        }
    }
}