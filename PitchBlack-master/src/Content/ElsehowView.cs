using System.Collections.Generic;
using UnityEngine;
using DevInterface;
using RWCustom;

namespace PitchBlack;

// Uses borrowed implementation from AboveCloudsView and AnicentUrbanView.
public class ElsehowView : BackgroundScene
{
    public ElsehowView(Room room, RoomSettings.RoomEffect effect) : base(room)
    {
        this.effect = effect;
        Random.State state = Random.state;
        sceneOrigo = RoomToWorldPos(room.abstractRoom.size.ToVector2() * 5f);
        Shader.SetGlobalVector(RainWorld.ShadPropMultiplyColor, Color.white);
        Shader.SetGlobalVector(RainWorld.ShadPropAboveCloudsAtmosphereColor, atmosphereColor);
        Shader.SetGlobalVector(RainWorld.ShadPropSceneOrigoPosition, sceneOrigo);
        //Shader.SetGlobalVector(RainWorld.ShadPropMultiplyColor, Color.white);
        Random.InitState(1);
        // Sky illustration
        centensSky = new Simple2DBackgroundIllustration(this, "Centens_Sky", new Vector2(683f, 384f));
        int towers = 200;
        float screenWidth = 6500f;
        float offset = 0f;
        // Tower element and position
        for (int i = 0; i < towers; i++)
        {
            // Variable X and Y Position of Towers
            float depthRange = Random.Range(0.1f, 3f);
            float xplacementRange = Random.Range(-screenWidth, screenWidth);
            float ydepthRange = Random.Range(-350f, -150f);
            int visualVariation = Random.Range(0, 3);
            float scaleRange = Random.Range(0.75f, 1.25f);
            float towerLayerThickness = 0.1f;
            Vector2 pos = new Vector2(PosFromDrawPosAtNeutralCamPos(new Vector2(xplacementRange, 0f), depthRange).x, floorLevel + offset + ydepthRange);
            // Adding Towers
            AddElement(new Towers(this, "Centens_Tower_" + visualVariation.ToString(), pos, depthRange, scaleRange, 0f, towerLayerThickness));
        }
        if (room.world.region != null)
        {
            startAltitude = (room.world.region.regionParams.cloudsStart ?? startAltitude);
            endAltitude = (room.world.region.regionParams.cloudsEnd ?? endAltitude);
        }
        if (!room.game.IsArenaSession || effect.type != PBEnums.RoomEffectType.ElsehowView)
        {
            sceneOrigo = new Vector2(2514f, (startAltitude + endAltitude) / 2f);
        }
        //else
        //{
        //    Custom.Log(new string[]
        //    {
        //        "arena sky view is :",
        //        effect.amount.ToString()
        //    });
        //    float num = 10000f - effect.amount * 30000f;
        //    sceneOrigo = new Vector2(2514f, num);
        //    startAltitude = num - 5500f;
        //    endAltitude = num + 5500f;
        //}

        // Adding graphics
        elseClouds = new List<ElseCloud>();
        LoadGraphic("elsewhyClouds1", false, false);
        LoadGraphic("elsewhyClouds2", false, false);
        LoadGraphic("elsewhyClouds3", false, false);
        LoadGraphic("elsewhyFlyingClouds1", false, false);
        generalFog = new ElseFog(this);
        AddElement(generalFog);
        AddElement(centensSky);
        // Close clouds
        if (effect.type == PBEnums.RoomEffectType.ElsehowView)
        {
            int cloudCount = 7;
            for (int i = 0; i < cloudCount; i++)
            {
                float cloudDepth = (float)i / (float)(cloudCount - 1);
                AddElement(new CloseElseCloud(this, new Vector2(0f, 0f), cloudDepth, i));
            }
        }

        // NOTE: Distant clouds are from SIClouds, don't use, otherwise it just creates a big sky-colored box when too high up!!!
        //int distantCloudCount = 11;
        //for (int j = 0; j < distantCloudCount; j++)
        //{
        //    float num15 = (float)j / (float)(distantCloudCount - 1);
        //    AddElement(new DistantElseCloud(this, new Vector2(0f, -40f * cloudsEndDepth * (1f - num15)), num15, j));
        //}

        // Flying clouds
        AddElement(new FlyingElseCloud(this, PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 75f), 355f), 355f, 0, 0.35f, 0.5f, 0.9f));
        AddElement(new FlyingElseCloud(this, PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 43f), 920f), 920f, 0, 0.15f, 0.3f, 0.95f));
        // Needed in order to set the random state
        Random.state = state;
    }

    // Adding clouds
    public override void AddElement(BackgroundSceneElement element)
    {
        if (element is ElseCloud)
        {
            elseClouds.Add(element as ElseCloud);
        }
        base.AddElement(element);
    }
    // From AncientUrbanView
    public float AtmosphereColorAtDepth(float depth)
    {
        return Mathf.Clamp(depth / 8.2f, 0f, 1f);
    }
    // Mandatory functions
    public override void Update(bool eu)
    {
        base.Update(eu);
    }
    public override void Destroy()
    {
        base.Destroy();
    }

    public RoomSettings.RoomEffect effect;
    //floorlevel is from AncientUrbanView for the Buildings, this determines the exact x spacially that the towers are generated according to, yields weird results when y alters, needs to be checked out, and stuff that uses it to be altered. -Lur
    public Color atmosphereColor = new Color(0.451f, 0.8f, 1f);
    public float floorLevel = -2000f;
    public float yShift;

    #region ElseCloud
    public abstract class ElseCloud : BackgroundSceneElement
    {
        private ElsehowView vvScene
        {
            get
            {
                return scene as ElsehowView;
            }
        }
        public ElseCloud(ElsehowView vvScene, Vector2 pos, float depth, int index) : base(vvScene, pos, depth)
        {
            this.randomOffset = UnityEngine.Random.value;
            this.index = index;
        }
        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            this.skyColor = palette.skyColor;
        }

        public float randomOffset;
        public Color skyColor;
        public int index;
    }
    #endregion

    #region Towers
    // References AncientUrbanView.Building
    private class Towers : BackgroundSceneElement
    {
        private ElsehowView vvScene
        {
            get
            {
                return scene as ElsehowView;
            }
        }

        public Towers(ElsehowView scene, string assetName, Vector2 pos, float depth, float scale, float rotation, float atmosphericalDepthAdd) : base(scene, pos, depth)
        {
            this.assetName = assetName;
            this.atmosphericalDepthAdd = atmosphericalDepthAdd;
            this.scale = scale;
            this.pos = pos;
            this.depth = depth;
            this.rotation = rotation;
            scene.LoadGraphic(assetName, true, false);
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[3];
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i] = new FSprite(assetName, true);
                sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["AncientUrbanBuilding"];
                sLeaser.sprites[i].anchorY = 0f;
                sLeaser.sprites[i].scale = scale;
                sLeaser.sprites[i].rotation = rotation;
            }
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                Vector2 vector = base.DrawPos(new Vector2(camPos.x, camPos.y + vvScene.yShift), rCam.hDisplace);
                sLeaser.sprites[i].x = vector.x;
                sLeaser.sprites[i].y = vector.y;
                sLeaser.sprites[i].color = new Color(Mathf.Pow(Mathf.InverseLerp(0f, 600f, this.depth + this.atmosphericalDepthAdd), 0.3f) * 0.9f, 1f - (float)i / 3f, 0f);
            }
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public string assetName;
        public float scale;
        public float rotation;
        public float atmosphericalDepthAdd;
    }
    #endregion

    #region CloseElseCloud
    public class CloseElseCloud : ElseCloud
    {
        public ElsehowView vvScene
        {
            get
            {
                return scene as ElsehowView;
            }
        }

        public CloseElseCloud(ElsehowView vvScene, Vector2 pos, float depth, int index) : base(vvScene, pos, depth, index)
        {
            cloudDepth = depth;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FSprite("pixel", true);
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["Background"];
            sLeaser.sprites[0].anchorY = 0f;
            sLeaser.sprites[0].scaleX = 1400f;
            sLeaser.sprites[0].x = 683f;
            sLeaser.sprites[0].y = 0f;
            sLeaser.sprites[1] = new FSprite("elsewhyClouds" + (index % 3 + 1).ToString(), true);
            sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["Cloud"];
            sLeaser.sprites[1].anchorY = 1f;
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            float y = scene.RoomToWorldPos(rCam.room.cameraPositions[rCam.currentCameraPosition]).y;
            float altitude = Mathf.InverseLerp(vvScene.startAltitude, vvScene.endAltitude, y);
            float depth = cloudDepth;
            if (altitude > 0.5f)
            {
                depth = Mathf.Lerp(depth, 1f, Mathf.InverseLerp(0.5f, 1f, altitude) * 0.5f);
            }
            this.depth = Mathf.Lerp(vvScene.cloudsStartDepth, vvScene.cloudsEndDepth, depth);
            float num3 = Mathf.Lerp(10f, 2f, depth);
            float num4 = DrawPos(new Vector2(camPos.x, camPos.y + vvScene.yShift), rCam.hDisplace).y;
            num4 += Mathf.Lerp(Mathf.Pow(cloudDepth, 0.75f), Mathf.Sin(cloudDepth * 3.1415927f), 0.5f) * Mathf.InverseLerp(0.5f, 0f, altitude) * 600f;
            num4 -= Mathf.InverseLerp(0.18f, 0.1f, altitude) * Mathf.Pow(1f - cloudDepth, 3f) * 100f;
            float num5 = Mathf.Lerp(1f, Mathf.Lerp(0.75f, 0.25f, altitude), depth);
            sLeaser.sprites[0].scaleY = num4 - 150f * num3 * num5;
            sLeaser.sprites[1].scaleY = num5 * num3;
            sLeaser.sprites[1].scaleX = num3;
            sLeaser.sprites[1].color = new Color(depth * 0.75f, randomOffset, Mathf.Lerp(num5, 1f, 0.5f), 1f);
            sLeaser.sprites[1].x = 683f;
            sLeaser.sprites[1].y = num4 - 2f;
            sLeaser.sprites[0].color = Color.Lerp(skyColor, vvScene.atmosphereColor, depth * 0.75f);
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
        public float cloudDepth;
    }
    #endregion

    #region DistantElseCloud
    // Don't use, not what we want
    public class DistantElseCloud : ElseCloud
    {
        public ElsehowView vvScene
        {
            get
            {
                return scene as ElsehowView;
            }
        }

        public DistantElseCloud(ElsehowView vvScene, Vector2 pos, float depth, int index) : base(vvScene, pos, depth, index)
        {
            distantCloudDepth = depth;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FSprite("pixel", true);
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["Background"];
            sLeaser.sprites[0].anchorY = 0f;
            sLeaser.sprites[0].scaleX = 1400f;
            sLeaser.sprites[0].x = 683f;
            sLeaser.sprites[0].y = 0f;
            sLeaser.sprites[1] = new FSprite("elsewhyClouds" + (index % 3 + 1).ToString(), true);
            sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["CloudDistant"];
            sLeaser.sprites[1].anchorY = 1f;
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            float yPos = scene.RoomToWorldPos(rCam.room.cameraPositions[rCam.currentCameraPosition]).y + vvScene.yShift;
            if (Mathf.InverseLerp(vvScene.startAltitude, vvScene.endAltitude, yPos) < 0.33f)
            {
                sLeaser.sprites[1].isVisible = false;
                sLeaser.sprites[0].isVisible = false;
                return;
            }
            sLeaser.sprites[1].isVisible = true;
            sLeaser.sprites[0].isVisible = true;
            float num = 2f;
            float y = DrawPos(new Vector2(camPos.x, camPos.y + vvScene.yShift), rCam.hDisplace).y;
            float num2 = Mathf.Lerp(0.3f, 0.01f, distantCloudDepth);
            if (index == 8)
            {
                num2 *= 1.5f;
            }
            sLeaser.sprites[0].scaleY = yPos - 150f * num * num2;
            sLeaser.sprites[1].scaleY = num2 * num;
            sLeaser.sprites[1].scaleX = num;
            sLeaser.sprites[1].color = new Color(Mathf.Lerp(0.75f, 0.95f, distantCloudDepth), randomOffset, Mathf.Lerp(num2, 1f, 0.5f), 1f);
            sLeaser.sprites[1].x = 683f;
            sLeaser.sprites[1].y = yPos - 2f;
            sLeaser.sprites[0].color = Color.Lerp(skyColor, vvScene.atmosphereColor, Mathf.Lerp(0.75f, 0.95f, distantCloudDepth));
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
        private float distantCloudDepth;
    }
    #endregion

    #region FlyingElseCloud
    public class FlyingElseCloud : ElseCloud
    {
        public ElsehowView vvScene
        {
            get
            {
                return scene as ElsehowView;
            }
        }

        public FlyingElseCloud(ElsehowView vvScene, Vector2 pos, float depth, int index, float flattened, float alpha, float shaderInputColor) : base(vvScene, pos, depth, index)
        {
            this.flattened = flattened;
            this.alpha = alpha;
            this.shaderInputColor = shaderInputColor;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("elsewhyFlyingClouds1", true);
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["CloudDistant"];
            sLeaser.sprites[0].anchorY = 1f;
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            float yPos2 = scene.RoomToWorldPos(rCam.room.cameraPositions[rCam.currentCameraPosition]).y;
            if (Mathf.InverseLerp(vvScene.startAltitude, vvScene.endAltitude, yPos2) < 0.33f)
            {
                sLeaser.sprites[0].isVisible = false;
                return;
            }
            sLeaser.sprites[0].isVisible = true;
            float num = 2f;
            float drawPos = DrawPos(camPos, rCam.hDisplace).y;
            sLeaser.sprites[0].scaleY = flattened * num;
            sLeaser.sprites[0].scaleX = num;
            sLeaser.sprites[0].color = new Color(shaderInputColor, randomOffset, Mathf.Lerp(flattened, 1f, 0.5f), alpha);
            sLeaser.sprites[0].x = 683f;
            sLeaser.sprites[0].y = drawPos;
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
        private float flattened;
        private float alpha;
        private float shaderInputColor;
    }
    #endregion

    #region ElseFog
    public class ElseFog : FullScreenSingleColor
    {
        public ElsehowView vvScene
        {
            get
            {
                return scene as ElsehowView;
            }
        }

        public ElseFog(ElsehowView vvScene) : base(vvScene, default(Color), 1f, true, float.MaxValue)
        {
            depth = 0f;
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (!room.game.IsArenaSession)
            {
                float value = scene.RoomToWorldPos(camPos).y + vvScene.yShift;
                alpha = Mathf.InverseLerp(22000f, 18000f, value) * 0.6f;
            }
            else
            {
                alpha = 0f;
            }
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            color = palette.skyColor;
            base.ApplyPalette(sLeaser, rCam, palette);
        }
    }
    #endregion

    public Simple2DBackgroundIllustration centensSky;
    public ElseFog generalFog;
    public float startAltitude = 20000f;
    public float endAltitude = 31400f;
    public float cloudsStartDepth = 5f;
    public float cloudsEndDepth = 40f;
    public List<ElseCloud> elseClouds;
}
