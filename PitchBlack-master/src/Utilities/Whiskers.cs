using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using PitchBlack;
using BepInEx;
using SlugBase;

namespace PitchBlack
{
    internal class Whiskers
    {
        public static void Hooks()
        {
            On.PlayerGraphics.ctor += PlayerGraphics_ctor;
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.PlayerGraphics.Update += PlayerGraphics_Update;
        }

        public static ConditionalWeakTable<Player, Whiskerdata> tailwhiskerstorage = new ConditionalWeakTable<Player, Whiskerdata>();
        public class Whiskerdata
        {
            public bool ready = false;
            public int initialtailwhiskerloc; //initial location for each sprite!!
            public int initialfacewhiskerloc;
            public string sprite = "LizardScaleA0"; //just for changing out what sprite is used
            public string facesprite = "LizardScaleA0";
            public WeakReference<Player> playerref;
            public Whiskerdata(Player player) //sets up a weak reference to the player.
            {
                playerref = new WeakReference<Player>(player);
            }
            public Scale[] tailScales = new Scale[6]; //each scale
            public Vector2[] headpositions = new Vector2[4]; // since lost has tail and head whiskers
            public Scale[] headScales = new Scale[4]; // theres two pairs of scale + position arrays!
            //scales are literaly stolen from rivulet's gill scales :]
            public class Scale : BodyPart
            {
                public Scale(GraphicsModule cosmetics) : base(cosmetics)
                {

                }
                public override void Update()
                {
                    base.Update();
                    if (this.owner.owner.room.PointSubmerged(this.pos))
                    {
                        this.vel *= 0.5f;
                    }
                    else
                    {
                        this.vel *= 0.9f;
                    }
                    this.lastPos = this.pos;
                    this.pos += this.vel;
                }
                public float length = 10f;
            }
            public int facewhiskersprite(int side, int pair)
            {
                return initialfacewhiskerloc + side + pair + pair;
            }
        }

        private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);

            if (self.player.slugcatStats.name == Plugin.BeaconName || self.player.slugcatStats.name == Plugin.PhotoName)
            {
                tailwhiskerstorage.Add(self.player, new Whiskerdata(self.player)); //setup the CWT
                tailwhiskerstorage.TryGetValue(self.player, out Whiskerdata data); // really im stupid this could just have been setup before adding it to the cwt!
                for (int i = 0; i < data.tailScales.Length; i++) //some loops for setting up the arrays
                {

                }
                for (int i = 0; i < data.headScales.Length; i++)
                {
                    data.headScales[i] = new Whiskerdata.Scale(self);
                    data.headpositions[i] = new Vector2((i < data.headScales.Length / 2 ? 0.7f : -0.7f), i == 1 ? 0.035f : 0.026f);
                }
            }
        }

        private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);
            if (self.player.slugcatStats.name == Plugin.BeaconName || self.player.slugcatStats.name == Plugin.PhotoName)
            {

                tailwhiskerstorage.TryGetValue(self.player, out var thedata); //get out the data from thw CWT

                thedata.initialfacewhiskerloc = sLeaser.sprites.Length; //add on 6 more bc theres 6 tail sprites
                Debug.Log($"Whiskers -> sLeaser length: {sLeaser.sprites.Length}");
                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 4); //add on more space for our sprites
                // 6 for tail sprites, 4 for face sprites.
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        sLeaser.sprites[thedata.facewhiskersprite(i, j)] = new FSprite(thedata.facesprite);

                        sLeaser.sprites[thedata.facewhiskersprite(i, j)].scaleY = 10f / Futile.atlasManager.GetElementWithName(thedata.sprite).sourcePixelSize.y;
                        sLeaser.sprites[thedata.facewhiskersprite(i, j)].anchorY = 0.1f;
                    }
                }
                thedata.ready = true; //say that we're ready to add these to the container!
                self.AddToContainer(sLeaser, rCam, null); //then add em!  // do not enable this it is cursed!
            }
        }

        private static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);

            if ((self.player.slugcatStats.name == Plugin.BeaconName || self.player.slugcatStats.name == Plugin.PhotoName) && tailwhiskerstorage.TryGetValue(self.player, out Whiskerdata data) && data.ready) //make sure to check that we're ready
            {
                
                for (int i = 0; i < 2; i++) //same thing as before but for the head sprites
                {
                    for (int j = 0; j < 2; j++)
                    {
                        FSprite whisker = sLeaser.sprites[data.facewhiskersprite(i, j)];
                        rCam.ReturnFContainer("Foreground").RemoveChild(whisker);
                        rCam.ReturnFContainer("Midground").AddChild(whisker);
                        whisker.MoveInFrontOfOtherNode(sLeaser.sprites[3]);
                        //Debug.Log("Please work (It will not)");   //I FIXED IT
                    }
                }
                data.ready = false; //set ready to false for next time.
            }
        }

        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (MiscUtils.SlugIsInMod(self.player.slugcatStats.name) && tailwhiskerstorage.TryGetValue(self.player, out Whiskerdata data))
            {
                //oh god i need to rewrite all of this into one for loop. <-- past me is right, you probably want to do this better. its a mess
                int index = 0;
                for (int i = 0; i < 2; i++) //as i said before, basically just rivy's code.
                {
                    for (int j = 0; j < 2; j++)
                    {
                        Vector2 vector = new Vector2(sLeaser.sprites[9].x + camPos.x, sLeaser.sprites[9].y + camPos.y);
                        float rotationAngle /*= 0f*/;
                        if (i == 0)
                        {
                            //left whiskers
                            rotationAngle = -45f;
                            vector.x -= 5f;
                        }
                        else
                        {
                            //right whiskers
                            rotationAngle = 180f;
                            vector.x += 5f;
                        }

                        //if (data.facewhiskersprite(i, j) % 2 != 0)
                        //{
                        //    //the left whiskers
                        //    rotationAngle -= 80f;
                        //}
                        //else
                        //{
                        //    rotationAngle += 80f;
                        //}

                        sLeaser.sprites[data.facewhiskersprite(i, j)].x = vector.x - camPos.x;
                        sLeaser.sprites[data.facewhiskersprite(i, j)].y = vector.y - camPos.y;
                        sLeaser.sprites[data.facewhiskersprite(i, j)].rotation = Custom.AimFromOneVectorToAnother(vector, Vector2.Lerp(data.headScales[index].lastPos, data.headScales[index].pos, timeStacker)) + rotationAngle;
                        if (i == 1)
                        {
                            sLeaser.sprites[data.facewhiskersprite(i, j)].scaleX = 0.4f;
                        }
                        else
                        {
                            sLeaser.sprites[data.facewhiskersprite(i, j)].scaleX = -0.4f;
                        }
                        sLeaser.sprites[data.facewhiskersprite(i, j)].color = sLeaser.sprites[1].color;
                        //Color col = Color.white;
                        // Jolly can go perish (idk what we're using for the initial whisker sprite in the CWT)
                        /*if (self.useJollyColor) {
                            sLeaser.sprites[something.initialFinSprite + i + j].color = PlayerGraphics.JollyColor(self.player.playerState.playerNumber, 0);
                        }*/
                        //if (!PlayerGraphics.CustomColorsEnabled()) {
                        //    SlugBaseCharacter.TryGet(SlugBaseCharacter.Registry.Keys.Where(name => name == Plugin.PhotoName).ToList()[0], out SlugBaseCharacter chara);
                        //    SlugBase.Features.PlayerFeatures.CustomColors.TryGet(chara, out SlugBase.DataTypes.ColorSlot[] colors);
                        //    col = colors[0].GetColor(self.player.playerState.playerNumber);
                        //}
                        //else if (PlayerGraphics.CustomColorsEnabled()) {
                        //    col = PlayerGraphics.CustomColorSafety(0);
                        //}
                        //Debug.Log($"Color is: {col}. I swear if this is the problem...");
                        //sLeaser.sprites[data.facewhiskersprite(i, j)].color = col;
                        //Debug.Log("Please move behind it and fix my whisker");
                        index++;
                    }
                }
            }
        }
        private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);
            if (MiscUtils.SlugIsInMod(self.player.slugcatStats.name) && tailwhiskerstorage.TryGetValue(self.player, out Whiskerdata data))
            {
                int index = 0; // once again we are in horrid loop hell. ew.
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        Vector2 pos = self.owner.bodyChunks[0].pos; //the lost got a whisker transplant from rivvy... now rivvy has no whiskers so sad....
                        Vector2 pos2 = self.owner.bodyChunks[1].pos;
                        float num = 0f;
                        float num2 = 90f;
                        int num3 = index % (data.headScales.Length / 2);
                        float num4 = num2 / (float)(data.headScales.Length / 2);
                        if (i == 1)
                        {
                            num = 0f;
                            pos.x += 5f;
                        }
                        else
                        {
                            pos.x -= 5f;
                        }
                        Vector2 a = Custom.rotateVectorDeg(Custom.DegToVec(0f), (float)num3 * num4 - num2 / 2f + num + 90f);
                        float f = Custom.VecToDeg(self.lookDirection);
                        Vector2 vector = Custom.rotateVectorDeg(Custom.DegToVec(0f), (float)num3 * num4 - num2 / 2f + num);
                        Vector2 a2 = Vector2.Lerp(vector, Custom.DirVec(pos2, pos), Mathf.Abs(f));
                        if (data.headpositions[index].y < 0.2f)
                        {
                            a2 -= a * Mathf.Pow(Mathf.InverseLerp(0.2f, 0f, data.headpositions[index].y), 2f) * 2f;
                        }
                        a2 = Vector2.Lerp(a2, vector, Mathf.Pow(0.0875f, 1f)).normalized;
                        Vector2 vector2 = pos + a2 * data.headScales.Length;
                        if (!Custom.DistLess(data.headScales[index].pos, vector2, data.headScales[index].length / 2f))
                        {
                            Vector2 a3 = Custom.DirVec(data.headScales[index].pos, vector2);
                            float num5 = Vector2.Distance(data.headScales[index].pos, vector2);
                            float num6 = data.headScales[index].length / 2f;
                            data.headScales[index].pos += a3 * (num5 - num6);
                            data.headScales[index].vel += a3 * (num5 - num6);
                        }
                        data.headScales[index].vel += Vector2.ClampMagnitude(vector2 - data.headScales[index].pos, 10f) / Mathf.Lerp(5f, 1.5f, 0.5873646f);
                        data.headScales[index].vel *= Mathf.Lerp(1f, 0.8f, 0.5873646f);
                        data.headScales[index].ConnectToPoint(pos, data.headScales[index].length, true, 0f, new Vector2(0f, 0f), 0f, 0f);
                        data.headScales[index].Update();
                        index++;
                    }
                }
            }
        }
    }
}
