using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace PitchBlack;

public class DeathSpawnGraphics : ComplexGraphicsModule
{
    public DeathSpawn spawn
    {
        get
        {
            return owner as DeathSpawn;
        }
    }
    public bool dayLightMode
    {
        get
        {
            return spawn.dayLightMode;
        }
    }
    public int BodyMeshSprite
    {
        get
        {
            return 0;
        }
    }
    public int GlowSprite
    {
        get
        {
            return 1;
        }
    }
    public int EffectSprite
    {
        get
        {
            return 2;
        }
    }
    // We may not need this
    public bool hasOwnGoldEffect
    {
        get
        {
            return spawn.voidMeltInRoom == 0f;
        }
    }
    public DeathSpawnGraphics(PhysicalObject owner) : base(owner, false)
    {
        playersGlowVision = new float[owner.abstractPhysicalObject.world.game.Players.Count, 2];
        totalSprites = hasOwnGoldEffect ? 3 : 2;
        antennae = new List<Antenna>();
        float num = Mathf.Lerp(spawn.sizeFac, 0.5f + 0.5f * UnityEngine.Random.value, UnityEngine.Random.value);
        int num2;
        float num3;
        int num4;
        float num7;
        float forceDirection;
        float num9;
        //cause im bored
        int chance = 1;
        if (variant == DeathSpawn.SpawnType.DeathAmoeba || (UnityEngine.Random.Range(0, 2) == chance && variant == DeathSpawn.SpawnType.DeathBiter))
        {
            antennae.Add(new TailAntenna(this, totalSprites, UnityEngine.Random.Range(4, 8), 12f * num, spawn.mainBody[spawn.mainBody.Length - 1].rad, 0f, 0.1f * num, 2, 2.2f));
            AddSubModule(antennae[antennae.Count - 1]);
        }
        else if (variant == DeathSpawn.SpawnType.DeathJelly)
        {
            num2 = UnityEngine.Random.Range(6, 8);
            num3 = Mathf.Lerp(Mathf.Lerp(8f, 24f, UnityEngine.Random.value) * (float)num2, Mathf.Lerp(16f, 45f, UnityEngine.Random.value), UnityEngine.Random.value);
            num4 = UnityEngine.Random.Range(4, 14);
            float a = Mathf.Lerp(0.2f, 1f, UnityEngine.Random.value);
            for (int i = 0; i < num2; i++)
            {
                float num5 = (float)i / (float)(num2 - 1);
                antennae.Add(new TailAntenna(this, totalSprites, num4, 12f * num * Mathf.Lerp(a, 1f, Mathf.Sin(num5 * 3.1415927f)), spawn.mainBody[spawn.mainBody.Length - 1].rad, Mathf.Lerp(-num3, num3, num5), 0.1f * num, 2, 2.2f));
                AddSubModule(antennae[antennae.Count - 1]);
            }
        }
        else
        {
            switch (UnityEngine.Random.Range(0, 3))
            {
                case 0:
                    antennae.Add(new TailAntenna(this, totalSprites, UnityEngine.Random.Range(3, 18), 12f * num, spawn.mainBody[spawn.mainBody.Length - 1].rad, 0f, 0.1f * num, 2, 2.2f));
                    AddSubModule(antennae[antennae.Count - 1]);
                    break;
                case 1:
                    {
                        num2 = UnityEngine.Random.Range(2, UnityEngine.Random.Range(2, 5));
                        num3 = Mathf.Lerp(Mathf.Lerp(2f, 15f, UnityEngine.Random.value) * (float)num2, Mathf.Lerp(8f, 70f, UnityEngine.Random.value), UnityEngine.Random.value);
                        num4 = UnityEngine.Random.Range(3, 18);
                        float a = Mathf.Lerp(0.2f, 1f, UnityEngine.Random.value);
                        for (int j = 0; j < num2; j++)
                        {
                            float num6 = (float)j / (float)(num2 - 1);
                            antennae.Add(new TailAntenna(this, totalSprites, num4, 12f * num * Mathf.Lerp(a, 1f, Mathf.Sin(num6 * 3.1415927f)), spawn.mainBody[spawn.mainBody.Length - 1].rad, Mathf.Lerp(-num3, num3, num6), 0.1f * num, 2, 2.2f));
                            AddSubModule(antennae[antennae.Count - 1]);
                        }
                        break;
                    }
                case 2:
                    {
                        num2 = UnityEngine.Random.Range(2, 6);
                        num7 = Mathf.Lerp(0.1f, 1.8f, UnityEngine.Random.value);
                        num3 = Mathf.Lerp(Mathf.Lerp(2f, 15f, UnityEngine.Random.value) * (float)num2, Mathf.Lerp(8f, 70f, UnityEngine.Random.value), UnityEngine.Random.value);
                        num4 = UnityEngine.Random.Range(3, UnityEngine.Random.Range(5, 8));
                        int num8 = UnityEngine.Random.Range(1, num4 + 1);
                        forceDirection = Mathf.Lerp(1.5f, 7f, UnityEngine.Random.value) * num7 / Mathf.Lerp(1f, (float)num8, 0.5f);
                        num9 = Mathf.Lerp(4f, 12f, UnityEngine.Random.value) * num;
                        float a = Mathf.Lerp(0.2f, 1f, UnityEngine.Random.value);
                        for (int k = 0; k < num2; k++)
                        {
                            float num10 = (float)k / (float)(num2 - 1);
                            antennae.Add(new TailAntenna(this, totalSprites, num4, num9 * Mathf.Lerp(a, 1f, Mathf.Sin(num10 * 3.1415927f)), 2f, Mathf.Lerp(-num3, num3, num10), num7, num8, forceDirection));
                            AddSubModule(antennae[antennae.Count - 1]);
                        }
                        break;
                    }
            }
        }
        num2 = UnityEngine.Random.Range(2, UnityEngine.Random.Range(2, 7));
        num4 = UnityEngine.Random.Range(2, UnityEngine.Random.Range(4, (int)Custom.LerpMap((float)spawn.mainBody.Length, 3f, 16f, 6f, 12f, 0.5f)));
        int num11 = num4;
        if (UnityEngine.Random.value < 0.5f)
        {
            num11 = UnityEngine.Random.Range(1, num4 + 1);
        }
        num9 = Mathf.Lerp(3f, 8f, UnityEngine.Random.value);
        num3 = Mathf.Lerp(12f, 50f, Mathf.Pow(UnityEngine.Random.value, 1.5f));
        forceDirection = Mathf.Lerp(2f, 7f, UnityEngine.Random.value) / Mathf.Lerp(1f, (float)num11, 0.5f);
        num7 = Mathf.Lerp(0.4f, 2.2f, UnityEngine.Random.value);
        if (variant != DeathSpawn.SpawnType.DeathJelly)
        {
            for (int l = 0; l < num2; l++)
            {
                float t = (float)l / (float)(num2 - 1);
                antennae.Add(new FrontAntenna(this, totalSprites, num4, num9, 2f * num, Mathf.Lerp(-num3, num3, t), num7, num11, forceDirection));
                AddSubModule(antennae[antennae.Count - 1]);
            }
        }
        Reset();
    }
    public override void Update()
    {
        if (!spawn.culled)
        {
            Update();
        }
        for (int i = 0; i < owner.room.game.Players.Count; i++)
        {
            playersGlowVision[i, 1] = playersGlowVision[i, 0];
            float num = 0f;
            if (owner.room.game.Players[i].realizedCreature != null && (owner.room.game.setupValues.playerGlowing || (owner.room.game.session is StoryGameSession && (owner.room.game.session as StoryGameSession).saveState.CanSeeVoidSpawn)) && !owner.room.game.Players[i].realizedCreature.inShortcut)
            {
                num = 1f;
            }
            if (playersGlowVision[i, 0] < num)
            {
                playersGlowVision[i, 0] = Custom.LerpAndTick(playersGlowVision[i, 0], num, 0.025f, 0.016666668f);
            }
            else
            {
                playersGlowVision[i, 0] = Custom.LerpAndTick(playersGlowVision[i, 0], num, 0.1f, 0.33333334f);
            }
        }
    }
    private FShader BodyShader
    {
        get
        {
            // VoidSpawnBody edited to be black
            return Custom.rainWorld.Shaders["DeathSpawnBody"];
        }
    }
    private FShader GlowShader
    {
        get
        {
            // GoldenGlow edited to be black
            return Custom.rainWorld.Shaders["DeathGlow"];
        }
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[totalSprites];
        base.InitiateSprites(sLeaser, rCam);
        sLeaser.sprites[BodyMeshSprite] = TriangleMesh.MakeLongMesh(spawn.mainBody.Length, false, true);
        sLeaser.sprites[BodyMeshSprite].shader = BodyShader;
        sLeaser.sprites[GlowSprite] = new FSprite("Futile_White", true);
        sLeaser.sprites[GlowSprite].shader = rCam.game.rainWorld.Shaders["FlatWaterLightBothSides"];
        if (hasOwnGoldEffect)
        {
            sLeaser.sprites[EffectSprite] = new FSprite("Futile_White", true);
            sLeaser.sprites[EffectSprite].shader = GlowShader;
        }
        AddToContainer(sLeaser, rCam, null);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (rCam.followAbstractCreature.realizedCreature != null && rCam.followAbstractCreature.realizedCreature is Player)
        {
            glowPos = Vector2.Lerp(rCam.followAbstractCreature.realizedCreature.mainBodyChunk.lastPos, rCam.followAbstractCreature.realizedCreature.mainBodyChunk.pos, timeStacker);
            playerGlowVision = Mathf.Lerp(playersGlowVision[(rCam.followAbstractCreature.realizedCreature as Player).playerState.playerNumber, 1], playersGlowVision[(rCam.followAbstractCreature.realizedCreature as Player).playerState.playerNumber, 0], timeStacker) * Mathf.Lerp(spawn.lastFade, spawn.fade, timeStacker);
        }
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        Vector2 vector = Vector2.Lerp(spawn.mainBody[0].lastPos, spawn.mainBody[0].pos, timeStacker);
        if (!spawn.culled)
        {
            sLeaser.sprites[BodyMeshSprite].shader = BodyShader;
            if (hasOwnGoldEffect)
            {
                sLeaser.sprites[EffectSprite].shader = GlowShader;
            }
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].isVisible = true;
            }
            Vector2 vector2 = vector;
            if (spawn.inEggMode > 0f && spawn.egg != null)
            {
                vector2 = Vector2.Lerp(vector2, Vector2.Lerp(spawn.egg.lastPos, spawn.egg.pos, timeStacker), spawn.inEggMode);
            }
            if (dayLightMode)
            {
                meshColor = new Color(1f - AlphaFromGlowDist(vector2, glowPos) * 0.5f, 0.5f, 0f);
            }
            else
            {
                meshColor = new Color(0.7f + 0.3f * Mathf.InverseLerp(0.1f, 0.9f, darkness), Mathf.Lerp(Mathf.Lerp(Custom.LerpMap(darkness, 0.1f, 0.9f, 0.1f, 0.075f, 0.5f), 0.24f, spawn.inEggMode), 0.8f, spawn.voidMeltInRoom), 0f);
            }
            UpdateGlowSpriteColor(sLeaser);
            sLeaser.sprites[GlowSprite].x = vector2.x - camPos.x;
            sLeaser.sprites[GlowSprite].y = vector2.y - camPos.y;
            if (spawn.inEggMode > 0.9f && spawn.egg != null)
            {
                sLeaser.sprites[GlowSprite].scale = Mathf.Lerp(2.8f * spawn.TotalMass * Mathf.Lerp(0.6f, 1f, darkness), spawn.egg.rad / 4f, Mathf.Lerp(0.9f, 1f, spawn.inEggMode));
            }
            else
            {
                sLeaser.sprites[GlowSprite].scale = 2.8f * spawn.TotalMass * Mathf.Lerp(0.6f, 1f, darkness);
            }
            if (dayLightMode)
            {
                sLeaser.sprites[GlowSprite].alpha = Mathf.Pow(AlphaFromGlowDist(vector2, glowPos), 0.5f) * Mathf.Lerp(0.5f, 1f, spawn.inEggMode);
            }
            else
            {
                sLeaser.sprites[GlowSprite].alpha = AlphaFromGlowDist(vector2, glowPos) * Mathf.Lerp(Mathf.Lerp(0.1f, 0.4f, Mathf.Pow(darkness, 2f)), 1f, spawn.voidMeltInRoom * 0.5f);
            }
            sLeaser.sprites[GlowSprite].shader = rCam.game.rainWorld.Shaders["FlatWaterLightBothSides"];
            if (hasOwnGoldEffect)
            {
                sLeaser.sprites[EffectSprite].x = vector2.x - camPos.x;
                sLeaser.sprites[EffectSprite].y = vector2.y - camPos.y;
                sLeaser.sprites[EffectSprite].scale = 6f * spawn.TotalMass;
                if (dayLightMode)
                {
                    sLeaser.sprites[EffectSprite].alpha = Mathf.Pow(AlphaFromGlowDist(vector2, glowPos), 0.5f) * 0.5f;
                }
                else
                {
                    sLeaser.sprites[EffectSprite].alpha = Mathf.Pow(AlphaFromGlowDist(vector2, glowPos), 0.6f) * Custom.LerpMap(darkness, 0.1f, 0.9f, 0.3f, 1f, 1.5f);
                }
            }
            vector += Custom.DirVec(Vector2.Lerp(spawn.mainBody[1].lastPos, spawn.mainBody[1].pos, timeStacker), vector) * spawn.mainBody[0].rad;
            float num = spawn.mainBody[0].rad / 2f;
            for (int j = 0; j < spawn.mainBody.Length; j++)
            {
                Vector2 vector3 = Vector2.Lerp(spawn.mainBody[j].lastPos, spawn.mainBody[j].pos, timeStacker);
                Vector2 normalized = (vector3 - vector).normalized;
                Vector2 a = Custom.PerpendicularVector(normalized);
                float d = Vector2.Distance(vector3, vector) / 5f;
                float rad = spawn.mainBody[j].rad;
                (sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(j * 4, vector - a * (num + rad) * 0.5f + normalized * d - camPos);
                (sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(j * 4 + 1, vector + a * (num + rad) * 0.5f + normalized * d - camPos);
                (sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(j * 4 + 2, vector3 - a * rad - normalized * d - camPos);
                (sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(j * 4 + 3, vector3 + a * rad - normalized * d - camPos);
                vector = vector3;
                num = rad;
            }
            for (int k = 0; k < (sLeaser.sprites[BodyMeshSprite] as TriangleMesh).verticeColors.Length; k++)
            {
                (sLeaser.sprites[BodyMeshSprite] as TriangleMesh).verticeColors[k] = new Color(meshColor.r, meshColor.g, meshColor.b, AlphaFromGlowDist((sLeaser.sprites[BodyMeshSprite] as TriangleMesh).vertices[k], glowPos - camPos));
            }
        }
        else
        {
            for (int l = 0; l < sLeaser.sprites.Length; l++)
            {
                sLeaser.sprites[l].isVisible = false;
            }
        }
        // From source, neat but not what we have to include lol
        //MapExporter.DeathSpawnGraphics_DrawSprites(this, sLeaser);
    }
    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
        darkness = palette.darkness;
        UpdateGlowSpriteColor(sLeaser);
    }
    private void UpdateGlowSpriteColor(RoomCamera.SpriteLeaser sLeaser)
    {
        // Intentionally hide them with solid black (which has 0 opacity)
        if (dayLightMode)
        {
            sLeaser.sprites[GlowSprite].color = Color.black;
            
            return;
        }
        sLeaser.sprites[GlowSprite].color = Color.Lerp(Color.black, Plugin.beaconDeadColor, Mathf.InverseLerp(0.2f, 0.8f, darkness));
    }
    public bool VisibleAtGlowDist(Vector2 A, Vector2 B, float margin)
    {
        if (playerGlowVision == 0f)
        {
            return false;
        }
        float num = 1.25f;
        return Custom.DistLess(A, B, Mathf.Lerp(100f, 400f * num, playerGlowVision) + margin / 1.5f);
    }
    public float AlphaFromGlowDist(Vector2 A, Vector2 B)
    {
        float num = Vector2.Distance(A, B);
        float num2 = 1.3f;
        //  if (spawn.rippleSpawn && num < (100f + 400f * num2) / 2f)
        //    {
        //      return this.playerGlowVision;
        //  }
        if (spawn.inEggMode > 0f)
        {
            return Mathf.Lerp(Mathf.Sin(Mathf.InverseLerp(Mathf.Lerp(100f, 400f * num2, playerGlowVision), 50f, num) * 3.1415927f), Mathf.InverseLerp(Mathf.Lerp(100f, 400f * num2, playerGlowVision), 50f, num), spawn.inEggMode) * playerGlowVision;
        }
        return Mathf.Sin(Mathf.InverseLerp(Mathf.Lerp(100f, 400f * num2, playerGlowVision), 50f, num) * 3.1415927f) * playerGlowVision;
    }
    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        if (newContatiner == null)
        {
            newContatiner = rCam.ReturnFContainer("GrabShaders");
        }
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].RemoveFromContainer();
            if (i == GlowSprite)
            {
                rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[i]);
            }
            else if (i == EffectSprite && hasOwnGoldEffect)
            {
                rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[i]);
            }
            else
            {
                newContatiner.AddChild(sLeaser.sprites[i]);
            }
        }
    }
    public DeathSpawn.SpawnType variant;
    public Vector2 glowPos;
    public float playerGlowVision;
    public float darkness;
    public List<Antenna> antennae;
    public float[,] playersGlowVision;
    public Color meshColor;

    public class Antenna : GraphicsSubModule
    {
        public DeathSpawnGraphics dsGraphics
        {
            get
            {
                return owner as DeathSpawnGraphics;
            }
        }
        public virtual Vector2 ResetPos
        {
            get
            {
                return dsGraphics.spawn.mainBody[0].pos;
            }
        }
        public virtual Vector2 ResetDir
        {
            get
            {
                return Custom.RNV();
            }
        }
        public Antenna(ComplexGraphicsModule owner, int firstSprite, int segs, float conRad, float thickness, float ang, float rigid, int rigidSegments, float forceDirection) : base(owner, firstSprite)
        {
            this.conRad = conRad;
            this.thickness = thickness;
            this.rigid = rigid;
            this.rigidSegments = rigidSegments;
            this.forceDirection = forceDirection;
            this.ang = ang;
            totalSprites = 1;
            segments = new Vector2[segs, 3];
        }
        public override void Update()
        {
            base.Update();
            for (int i = 0; i < segments.GetLength(0); i++)
            {
                segments[i, 1] = segments[i, 0];
                segments[i, 0] += segments[i, 2];
                segments[i, 2] *= Custom.LerpMap(segments[i, 2].magnitude, 0.2f * dsGraphics.spawn.sizeFac, 6f * dsGraphics.spawn.sizeFac, 1f, 0.7f);
            }
            for (int j = 1; j < segments.GetLength(0); j++)
            {
                Vector2 a = Custom.DirVec(segments[j, 0], segments[j - 1, 0]);
                float num = Vector2.Distance(segments[j, 0], segments[j - 1, 0]);
                segments[j, 0] -= (conRad - num) * a * 0.5f;
                segments[j, 2] -= (conRad - num) * a * 0.5f;
                segments[j - 1, 0] += (conRad - num) * a * 0.5f;
                segments[j - 1, 2] += (conRad - num) * a * 0.5f;
            }
            for (int k = 2; k < segments.GetLength(0); k++)
            {
                Vector2 a2 = Custom.DirVec(segments[k, 0], segments[k - 2, 0]);
                segments[k, 2] -= a2 * rigid;
                segments[k - 2, 2] += a2 * rigid;
            }
        }
        public override void Reset()
        {
            base.Reset();
            Vector2 resetPos = ResetPos;
            Vector2 resetDir = ResetDir;
            for (int i = 0; i < segments.GetLength(0); i++)
            {
                segments[i, 0] = resetPos + resetDir * conRad;
                segments[i, 1] = segments[i, 0];
                segments[i, 2] *= 0f;
            }
        }
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
            sLeaser.sprites[firstSprite] = TriangleMesh.MakeLongMesh(segments.GetLength(0), false, true);
            sLeaser.sprites[firstSprite].shader = dsGraphics.BodyShader;
        }
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            Vector2 vector = Vector2.Lerp(segments[0, 1], segments[0, 0], timeStacker);
            vector += Custom.DirVec(Vector2.Lerp(segments[1, 1], segments[1, 0], timeStacker), vector) * conRad * 0.3f;
            float num = 1f;
            for (int i = 0; i < segments.GetLength(0); i++)
            {
                float f = (float)i / (float)(segments.GetLength(0) - 1);
                Vector2 vector2 = Vector2.Lerp(segments[i, 1], segments[i, 0], timeStacker);
                Vector2 normalized = (vector2 - vector).normalized;
                Vector2 a = Custom.PerpendicularVector(normalized);
                float d = Vector2.Distance(vector2, vector) / 5f;
                float num2 = Mathf.Lerp(thickness, 0.5f, Mathf.Pow(f, 0.2f));
                (sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4, vector - a * (num + num2) * 0.5f + normalized * d - camPos);
                (sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector + a * (num + num2) * 0.5f + normalized * d - camPos);
                (sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - a * num2 - normalized * d - camPos);
                (sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + a * num2 - normalized * d - camPos);
                vector = vector2;
                num = num2;
            }
            for (int j = 0; j < (sLeaser.sprites[firstSprite] as TriangleMesh).verticeColors.Length; j++)
            {
                (sLeaser.sprites[firstSprite] as TriangleMesh).verticeColors[j] = new Color(dsGraphics.meshColor.r, dsGraphics.meshColor.g, dsGraphics.meshColor.b, dsGraphics.AlphaFromGlowDist((sLeaser.sprites[firstSprite] as TriangleMesh).vertices[j], dsGraphics.glowPos - camPos));
            }
            sLeaser.sprites[firstSprite].shader = dsGraphics.BodyShader;
        }
        public Vector2[,] segments;
        public float conRad;
        public float thickness;
        public float rigid;
        public float forceDirection;
        public float ang;
        public int rigidSegments;
    }
    public class FrontAntenna : Antenna
    {
        public FrontAntenna(ComplexGraphicsModule owner, int firstSprite, int segs, float conRad, float thickness, float ang, float rigid, int rigidSegments, float forceDirection) : base(owner, firstSprite, segs, conRad, thickness, ang, rigid, rigidSegments, forceDirection)
        {
        }
        public override void Update()
        {
            base.Update();
            segments[0, 0] = dsGraphics.spawn.mainBody[0].pos;
            segments[0, 2] *= 0f;
            Vector2 a = Custom.DegToVec(Custom.AimFromOneVectorToAnother(dsGraphics.spawn.mainBody[1].pos, dsGraphics.spawn.mainBody[0].pos) + ang);
            int num = 1;
            while (num < segments.GetLength(0) && num < rigidSegments)
            {
                segments[num, 2] += a * forceDirection * Mathf.InverseLerp((float)rigidSegments, 1f, (float)num);
                num++;
            }
        }
        public override Vector2 ResetPos
        {
            get
            {
                return dsGraphics.spawn.mainBody[0].pos;
            }
        }
        public override Vector2 ResetDir
        {
            get
            {
                return Custom.DegToVec(Custom.AimFromOneVectorToAnother(dsGraphics.spawn.mainBody[1].pos, dsGraphics.spawn.mainBody[0].pos) + ang);
            }
        }
    }
    public class TailAntenna : Antenna
    {
        public TailAntenna(ComplexGraphicsModule owner, int firstSprite, int segs, float conRad, float thickness, float ang, float rigid, int rigidSegments, float forceDirection) : base(owner, firstSprite, segs, conRad, thickness, ang, rigid, rigidSegments, forceDirection)
        {
        }
        public override void Update()
        {
            base.Update();
            segments[0, 0] = dsGraphics.spawn.mainBody[dsGraphics.spawn.mainBody.Length - 1].pos;
            segments[0, 2] *= 0f;
            Vector2 a = Custom.DegToVec(Custom.AimFromOneVectorToAnother(dsGraphics.spawn.mainBody[dsGraphics.spawn.mainBody.Length - 2].pos, dsGraphics.spawn.mainBody[dsGraphics.spawn.mainBody.Length - 1].pos) + ang);
            int num = 1;
            while (num < segments.GetLength(0) && num < rigidSegments)
            {
                segments[num, 2] += a * forceDirection * Mathf.InverseLerp((float)rigidSegments, 1f, (float)num);
                num++;
            }
        }
        public override Vector2 ResetPos
        {
            get
            {
                return dsGraphics.spawn.mainBody[dsGraphics.spawn.mainBody.Length - 1].pos;
            }
        }
        public override Vector2 ResetDir
        {
            get
            {
                return Custom.DegToVec(Custom.AimFromOneVectorToAnother(dsGraphics.spawn.mainBody[dsGraphics.spawn.mainBody.Length - 2].pos, dsGraphics.spawn.mainBody[dsGraphics.spawn.mainBody.Length - 1].pos) + this.ang);
            }
        }
    }
}