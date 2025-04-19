using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IL.Watcher;
using JetBrains.Annotations;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using DevInterface;

namespace PitchBlack;

#region Hooks
public class ElsehowViewHooks {
    public static void Apply() {
        On.Room.Loaded += Room_Loaded;
        On.Room.NowViewed += Room_NowViewed;
        On.DevInterface.RoomSettingsPage.DevEffectGetCategoryFromEffectType += RoomSettingsPage_DevEffectGetCategoryFromEffectType;
    }

    // Adds to PitchBlack dev effects catagory
    private static RoomSettingsPage.DevEffectsCategories RoomSettingsPage_DevEffectGetCategoryFromEffectType(On.DevInterface.RoomSettingsPage.orig_DevEffectGetCategoryFromEffectType orig, RoomSettingsPage self, RoomSettings.RoomEffect.Type type) {
        RoomSettingsPage.DevEffectsCategories res = orig(self,type);
        if (type == PBRoomEffectType.ElsehowView)
        {
            res = PBRoomEffectType.PitchBlack;
        }
        return res;
    }

    // Taken from other Watcher backgrounds, seems related to the building shader
    private static void Room_NowViewed(On.Room.orig_NowViewed orig, Room self) {
        orig(self);
        for (int i = 0; i < self.roomSettings.effects.Count; i++) {
            if (self.roomSettings.effects[i].type == PBRoomEffectType.ElsehowView) {
                Shader.SetGlobalFloat(RainWorld.ShadPropRimFix, 1f);
            }
        }
    }

    // Adds ElsehowView
    private static void Room_Loaded(On.Room.orig_Loaded orig, Room self) {
        orig(self);
        for (int num4 = 0; num4 < self.roomSettings.effects.Count; num4++) {
            if (self.roomSettings.effects[num4].type == PBRoomEffectType.ElsehowView) {
                self.AddObject(new ElsehowView(self, self.roomSettings.effects[num4]));
            }
        }
    }
}
#endregion

// ElsehowView
// (Uses borrowed implementation from AboveCloudsView and AnicentUrbanView.)
public class ElsehowView : BackgroundScene {
    public ElsehowView(Room room, RoomSettings.RoomEffect effect) : base(room) {
        this.effect = effect;
        UnityEngine.Random.State state = UnityEngine.Random.state;

        // I don't know what this does?
        UnityEngine.Random.InitState(0);

        // Sky illustration
        centensSky = new Simple2DBackgroundIllustration(this, "Centens_Sky", new Vector2(683f, 384f));

        // Tower element and position
        float screenWidth = 6500f;
        float offset = 300f;
        for (int i = 0; i < 400; i++) {
            float range = UnityEngine.Random.Range(1.2f, 8f);
            // Variable X and Y Position of BackgroundScene Towers
            Vector2 pos = new Vector2(PosFromDrawPosAtNeutralCamPos(new Vector2(UnityEngine.Random.Range(-screenWidth, screenWidth), 0f), 1f).x, floorLevel + offset * range + UnityEngine.Random.Range(-300f, 0f));
            AddElement(new Towers(this, "Centens_Tower_" + UnityEngine.Random.Range(0, 3).ToString(), pos, range, UnityEngine.Random.Range(0.75f, 1.75f), UnityEngine.Random.Range(-1.5f, 1.5f), 0.1f));
        }
        UnityEngine.Random.state = state;
    }

    public override void AddElement(BackgroundSceneElement element) {
        if (element is AboveCloudsView.Cloud)
        {
            elseclouds.Add(element as AboveCloudsView.Cloud);
        }
        base.AddElement(element);
    }

        public float AtmosphereColorAtDepth(float depth) {
        return Mathf.Clamp(depth / 8.2f, 0f, 1f);
    }

    // from implementation
    public override void Update(bool eu) {
        base.Update(eu);
    }
    public override void Destroy() {
        base.Destroy();
    }

    public RoomSettings.RoomEffect effect;
    private float floorLevel = -2000f;
    public Color atmosphereColor = new Color(0.451f, 0.8f, 1f);

    // References AncientUrbanView.Building
    private class Towers : BackgroundSceneElement {
        private ElsehowView vvScene {
            get {
                return scene as ElsehowView;
            }
        }

        public Towers(ElsehowView scene, string assetName, Vector2 pos, float depth, float scale, float rotation, float thickness) : base(scene, pos, depth) {
            this.assetName = assetName;
            this.scale = scale;
            this.pos = pos;
            this.depth = depth;
            this.scale = scale;
            this.rotation = rotation;
            this.thickness = thickness;
            scene.LoadGraphic(assetName, true, false);
        }

        private float getDepthForLayer(float layer) {
            return this.depth + layer * this.thickness;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
            sLeaser.sprites = new FSprite[3];
            for (int i = 0; i < sLeaser.sprites.Length; i++) {
                sLeaser.sprites[i] = new FSprite(assetName, true);
                sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["AncientUrbanBuilding"];
                sLeaser.sprites[i].anchorY = 0f;
                sLeaser.sprites[i].scale = scale * (1f / getDepthForLayer(1f - (float)i / 3f));
                sLeaser.sprites[i].rotation = rotation;
            }
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
            for (int i = 0; i < sLeaser.sprites.Length; i++) {
                float depthForLayer = getDepthForLayer(1f - (float)i / 3f);
                Vector2 vector = scene.DrawPos(pos, depthForLayer, camPos, rCam.hDisplace);
                sLeaser.sprites[i].x = vector.x;
                sLeaser.sprites[i].y = vector.y;
                sLeaser.sprites[i].color = new Color(vvScene.AtmosphereColorAtDepth(depthForLayer), 1f - (float)i / 3f, depthForLayer / 8.2f);
            }
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public string assetName;
        public float scale;
        public float rotation;
        public float thickness;
    }

    public class ElseCloseCloud : AboveCloudsView.Cloud {
        public ElsehowView vvScene {
            get {
                return scene as ElsehowView;
            }
        }

        public ElseCloseCloud(AboveCloudsView aboveCloudsScene, Vector2 pos, float depth, int index) : base(aboveCloudsScene, pos, depth, index) {
            cloudDepth = depth;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FSprite("pixel", true);
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["Background"];
            sLeaser.sprites[0].anchorY = 0f;
            sLeaser.sprites[0].scaleX = 1400f;
            sLeaser.sprites[0].x = 683f;
            sLeaser.sprites[0].y = 0f;
            sLeaser.sprites[1] = new FSprite("clouds" + (index % 3 + 1).ToString(), true);
            sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["Cloud"];
            sLeaser.sprites[1].anchorY = 1f;
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
            float y = scene.RoomToWorldPos(rCam.room.cameraPositions[rCam.currentCameraPosition]).y;
            float altitude = Mathf.InverseLerp(vvScene.startAltitude, vvScene.endAltitude, y);
            float depth = cloudDepth;
            if (altitude > 0.5f)
            {
                depth = Mathf.Lerp(depth, 1f, Mathf.InverseLerp(0.5f, 1f, altitude) * 0.5f);
            }
            this.depth = Mathf.Lerp(AboveCloudsScene.cloudsStartDepth, AboveCloudsScene.cloudsEndDepth, depth);
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

    public class ElseDistantCloud : AboveCloudsView.Cloud {
        public ElsehowView vvScene {
            get {
                return scene as ElsehowView;
            }
        }

        public ElseDistantCloud(AboveCloudsView aboveCloudsScene, Vector2 pos, float depth, int index) : base(aboveCloudsScene, pos, depth, index) {
            distantCloudDepth = depth;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FSprite("pixel", true);
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["Background"];
            sLeaser.sprites[0].anchorY = 0f;
            sLeaser.sprites[0].scaleX = 1400f;
            sLeaser.sprites[0].x = 683f;
            sLeaser.sprites[0].y = 0f;
            sLeaser.sprites[1] = new FSprite("clouds" + (index % 3 + 1).ToString(), true);
            sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["CloudDistant"];
            sLeaser.sprites[1].anchorY = 1f;
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
            float yPos = scene.RoomToWorldPos(rCam.room.cameraPositions[rCam.currentCameraPosition]).y + vvScene.yShift;
            if (Mathf.InverseLerp(vvScene.startAltitude, vvScene.endAltitude, yPos) < 0.33f) {
                sLeaser.sprites[1].isVisible = false;
                sLeaser.sprites[0].isVisible = false;
                return;
            }
            sLeaser.sprites[1].isVisible = true;
            sLeaser.sprites[0].isVisible = true;
            float num = 2f;
            float y = DrawPos(new Vector2(camPos.x, camPos.y + vvScene.yShift), rCam.hDisplace).y;
            float num2 = Mathf.Lerp(0.3f, 0.01f, distantCloudDepth);
            if (index == 8) {
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

    public class ElseFlyingCloud : AboveCloudsView.Cloud {
        public ElsehowView vvScene {
            get {
                return scene as ElsehowView;
            }
        }

        public ElseFlyingCloud(AboveCloudsView aboveCloudsScene, Vector2 pos, float depth, int index, float flattened, float alpha, float shaderInputColor) : base(aboveCloudsScene, pos, depth, index) {
            this.flattened = flattened;
            this.alpha = alpha;
            this.shaderInputColor = shaderInputColor;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("flyingClouds1", true);
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["CloudDistant"];
            sLeaser.sprites[0].anchorY = 1f;
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
            float yPos2 = scene.RoomToWorldPos(rCam.room.cameraPositions[rCam.currentCameraPosition]).y;
            if (Mathf.InverseLerp(vvScene.startAltitude, vvScene.endAltitude, yPos2) < 0.33f) {
                sLeaser.sprites[0].isVisible = false;
                return;
            }
            sLeaser.sprites[0].isVisible = true;
            float num = 2f;
            float drawPos = DrawPos(camPos, rCam.hDisplace).y;
            sLeaser.sprites[0].scaleY = this.flattened * num;
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

    public class ElseFog : FullScreenSingleColor {
        public ElsehowView vvScene {
            get {
                return scene as ElsehowView;
            }
        }

        public ElseFog(AboveCloudsView aboveCloudsScene) : base(aboveCloudsScene, default(Color), 1f, true, float.MaxValue) {
            depth = 0f;
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
            if (!room.game.IsArenaSession) {
                float value = scene.RoomToWorldPos(camPos).y + vvScene.yShift;
                alpha = Mathf.InverseLerp(22000f, 18000f, value) * 0.6f;
            }
            else  {
                alpha = 0f;
            }
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
            color = palette.skyColor;
            base.ApplyPalette(sLeaser, rCam, palette);
        }
    }

    public Simple2DBackgroundIllustration centensSky;
    public float startAltitude = 9000f;
    public float endAltitude = 26400f;
    public float yShift;
    //public float cloudsStartDepth = 5f;
    //public float cloudsEndDepth = 40f;
    public List<AboveCloudsView.Cloud> elseclouds;
}
