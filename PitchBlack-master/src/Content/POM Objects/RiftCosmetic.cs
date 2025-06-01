using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pom.Pom;
using UnityEngine;
using RWCustom;
using BepInEx.Logging;
using System.IO;

namespace PitchBlack
{
    public class RiftCosmetic : UpdatableAndDeletable, IDrawable
    {
        private readonly PlacedObject placedObject;

        public RiftCosmetic(PlacedObject pObj, Room room)
        {
            this.room = room;
            this.placedObject = pObj;
        }

        public static Texture2D noise;

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            rCam.ReturnFContainer("HUD").AddChild(sLeaser.sprites[0]);

            rCam.ReturnFContainer("HUD2").AddChild(sLeaser.sprites[1]);
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            // i LOVE this function :steamhappy:
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[0].SetPosition((placedObject.pos + (((ManagedData)placedObject.data).GetValue<Vector2>("rect") / 2)) - rCam.pos);
            sLeaser.sprites[0].scaleX = ((ManagedData)placedObject.data).GetValue<Vector2>("rect").x / sLeaser.sprites[0].element.sourceSize.x;
            sLeaser.sprites[0].scaleY = ((ManagedData)placedObject.data).GetValue<Vector2>("rect").y / sLeaser.sprites[0].element.sourceSize.x;

            sLeaser.sprites[0].rotation = Custom.VecToDeg(((ManagedData)placedObject.data).GetValue<Vector2>("dir"));

            sLeaser.sprites[1].SetPosition((placedObject.pos + (((ManagedData)placedObject.data).GetValue<Vector2>("rect") / 2)) - rCam.pos);
            sLeaser.sprites[1].scale = (((ManagedData)placedObject.data).GetValue<Vector2>("rect").x + 100) / sLeaser.sprites[1].element.sourceSize.x;

            sLeaser.sprites[1].rotation = Custom.VecToDeg(((ManagedData)placedObject.data).GetValue<Vector2>("dir"));
            sLeaser.sprites[1].alpha = ((ManagedData)placedObject.data).GetValue<float>("glow alpha");

            if ((sLeaser.sprites[0]._renderLayer._material != null) && sLeaser.sprites[0]._renderLayer._gameObject.GetComponent<Renderer>().material != null)
            {
                Material flameMat = sLeaser.sprites[0]._renderLayer._gameObject.GetComponent<Renderer>().material;
                Debug.Log(flameMat);

                flameMat.SetTexture("_Noise", noise);
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FSprite($"rift{((ManagedData)placedObject.data).GetValue<int>("sprite index")}");

            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["Rift"];

            sLeaser.sprites[1] = new FSprite("Futile_White");
            sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["FlatWaterLight"];
            sLeaser.sprites[1].color = new Color(0.6f, 0.2f, 1f);

            AddToContainer(sLeaser, rCam, null);
        }

        public static void Register(RainWorld rainWorld)
        {
            try
            {
                var bundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles" + Path.DirectorySeparatorChar + "rifts"));
                rainWorld.Shaders["Rift"] = FShader.CreateShader("Rift", bundle.LoadAsset<Shader>("Assets/shaders/Rift.shader"));

                if (!Futile.atlasManager.DoesContainAtlas("rift0"))
                {
                    Futile.atlasManager.LoadAtlasFromTexture("rift0", bundle.LoadAsset<Texture2D>("Assets/rift sprites/rift.png"), false);
                    Futile.atlasManager.LoadAtlasFromTexture("rift1", bundle.LoadAsset<Texture2D>("Assets/rift sprites/rift2.png"), false);
                    Futile.atlasManager.LoadAtlasFromTexture("rift2", bundle.LoadAsset<Texture2D>("Assets/rift sprites/rift3.png"), false);
                    Futile.atlasManager.LoadAtlasFromTexture("rift3", bundle.LoadAsset<Texture2D>("Assets/rift sprites/rift4.png"), false);
                    Futile.atlasManager.LoadAtlasFromTexture("rift4", bundle.LoadAsset<Texture2D>("Assets/rift sprites/rift5.png"), false);
                    Futile.atlasManager.LoadAtlasFromTexture("rift5", bundle.LoadAsset<Texture2D>("Assets/rift sprites/rift6.png"), false);
                    Futile.atlasManager.LoadAtlasFromTexture("rift6", bundle.LoadAsset<Texture2D>("Assets/rift sprites/rift7.png"), false);
                }

                RiftCosmetic.noise = bundle.LoadAsset<Texture2D>("Assets/rift sprites/noise.png");

                List<ManagedField> fields = new List<ManagedField>
                {
                    new IntegerField("sprite index", 0, 6, 0),
                    new FloatField("glow alpha", 0f, 1f, 0.5f, 0.01f),

                    new Vector2Field("dir", Vector2.one, Vector2Field.VectorReprType.line),
                    new Vector2Field("rect", Vector2.one, Vector2Field.VectorReprType.rect),
                };
                RegisterFullyManagedObjectType(fields.ToArray(), typeof(RiftCosmetic), "RiftCosmetic", "Pitch-Black");
            }
            catch(Exception e)
            {
                Debug.Log(e);
            }
        }
    }
}