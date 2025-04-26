using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RWCustom;
using UnityEngine;

namespace PitchBlack;
public class DeathSpawn : PhysicalObject
{
    // Used to access the list of deathspawn in a room
    public class DeathSpawnCWT
    {
        public List<DeathSpawn> deathSpawns;
    }
    public ConditionalWeakTable<Room, DeathSpawnCWT> deathSpawnCWT;

    public DeathSpawn(AbstractPhysicalObject abstractPhysicalObject, float voidMeltInRoom, bool dayLightMode, SpawnType variant) : base(abstractPhysicalObject)
    {
        this.variant = variant;
        GenerateBody();
        this.voidMeltInRoom = voidMeltInRoom;
        this.dayLightMode = dayLightMode;
        airFriction = 1f;
        gravity = 0f;
        bounce = 0f;
        surfaceFriction = 0.4f;
        collisionLayer = 0;
        waterFriction = 1f;
        buoyancy = 0f;
        swimCycle = UnityEngine.Random.value;
        CollideWithTerrain = false;
        CollideWithObjects = false;
        canBeHitByWeapons = false;
    }
    // Spawns DeathSpawn
    public DeathSpawn(AbstractPhysicalObject abstractPhysicalObject, float voidMeltInRoom, bool dayLightMode) : this(abstractPhysicalObject, voidMeltInRoom, dayLightMode, SpawnType.DeathSpawn)
    {
    }
    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        Vector2 vector = placeRoom.MiddleOfTile(abstractPhysicalObject.pos);
        Vector2 a = Custom.RNV();
        if (behavior != null)
        {
            a = Custom.DirVec(vector, behavior.SwimTowards);
        }
        for (int i = 0; i < bodyChunks.Length; i++)
        {
            bodyChunks[i].HardSetPosition(vector + a * 2f);
        }
        if (graphicsModule != null)
        {
            graphicsModule.Reset();
        }
    }
    private void GenerateBody()
    {
        int length = UnityEngine.Random.Range(3, UnityEngine.Random.Range(3, 16));
        int index = 0;
        List<BodyChunk> list = new List<BodyChunk>();
        List<BodyChunkConnection> list2 = new List<BodyChunkConnection>();
        float sizeMultiplier = 1f;
        float num4 = Mathf.Lerp(3f, 8f, UnityEngine.Random.value);
        // RippleAmoeba
        if (variant == SpawnType.DeathAmoeba)
        {
            sizeMultiplier = 2f;
            length = UnityEngine.Random.Range(5, 8);
            num4 = Mathf.Lerp(8f, 12f, UnityEngine.Random.value);
        }
        // RippleJelly
        else if (variant == SpawnType.DeathJelly)
        {
            length = UnityEngine.Random.Range(3, 4);
        }
        // RippleNoodle
        else if (variant == SpawnType.DeathNoodle)
        {
            sizeMultiplier = 0.5f;
            length = UnityEngine.Random.Range(6, 10);
            num4 = Mathf.Lerp(1f, 6f, UnityEngine.Random.value);
        }
        // New variant, they have Amoeba dominance while being very tiny
        else if (variant == SpawnType.DeathBiter)
        {
            sizeMultiplier = 0.5f;
            length = UnityEngine.Random.Range(4, 5);
            num4 = Mathf.Lerp(1f, 6f, UnityEngine.Random.value);
        }
        float num5 = Mathf.Lerp(Mathf.Lerp(0.5f, 4f, UnityEngine.Random.value), num4 / 2f, UnityEngine.Random.value);
        float p = Mathf.Lerp(0.1f, 0.7f, UnityEngine.Random.value);
        sizeFac = Mathf.Lerp(0.5f, 1.2f, UnityEngine.Random.value) * sizeMultiplier;
        swimSpeed = Mathf.Lerp(0.5f, 1f, UnityEngine.Random.value);
        dominance = Mathf.InverseLerp(0f, 2.4f, sizeFac);
        dominance *= Mathf.InverseLerp(3f, 8f, (float)length);
        for (int i = 0; i < length; i++)
        {
            float num6 = (float)i / (float)(length - 1);
            float num7 = Mathf.Lerp(Mathf.Lerp(num4, num5, num6), Mathf.Lerp(num5, num4, Mathf.Sin(Mathf.Pow(num6, p) * 3.1415927f)), 0.5f) * sizeFac;
            list.Add(new BodyChunk(this, index, default(Vector2), num7, num7 * 0.1f));
            if (i > 0)
            {
                list2.Add(new BodyChunkConnection(list[i - 1], list[i], Mathf.Lerp((list[i - 1].rad + list[i].rad) * 1.25f, Mathf.Max(list[i - 1].rad, list[i].rad), 0.5f), BodyChunkConnection.Type.Normal, 1f, -1f));
            }
            index++;
        }
        mainBody = list.ToArray();
        bodyChunks = list.ToArray();
        bodyChunkConnections = list2.ToArray();
    }
    public override void InitiateGraphicsModule()
    {
        if (graphicsModule == null)
        {
            // Changing this to DeathSpawnGraphics, so I can change the shaders
            graphicsModule = new DeathSpawnGraphics(this);
        }
    }
    public override void Update(bool eu)
    {
        base.Update(eu);
        tick++;
        lastFade = fade;
        if ((!dayLightMode && room.PointSubmerged(mainBody[0].pos)) || startFadeOut || (rippleLayer == 1 && room.game.ActiveRippleLayer != 1))
        {
            // Whats used for rippleSpawn in num originally
            float rNum = 0.008333334f;
            if (variant == SpawnType.DeathJelly || variant == SpawnType.DeathBiter)
            {
                rNum = 0.05f;
            }
            fade = Mathf.Max(0f, fade - rNum);
            if (fade == 0f)
            {
                Destroy();
            }
        }
        else
        {
            fade = Mathf.Min(1f, fade + 0.025f);
        }
        if (timeUntilFadeout > 0)
        {
            timeUntilFadeout--;
            if (timeUntilFadeout == 0)
            {
                startFadeOut = true;
            }
        }
        if (inEggMode > 0f)
        {
            inEggMode = Mathf.Max(0f, inEggMode - 0.016666668f);
            if (inEggMode <= 0f)
            {
                egg = null;
            }
        }
        for (int i = 0; i < bodyChunks.Length; i++)
        {
            mainBody[i].vel *= Custom.LerpMap(mainBody[i].vel.magnitude, 0.2f * sizeFac, 6f * sizeFac, 1f, 0.7f);
        }
        if (consious)
        {
            float multSwimSpeed = swimSpeed;
            float velSpeed = -1f;
            // RippleAmoeba
            if (variant == SpawnType.DeathAmoeba && behavior != null)
            {
                float t = Mathf.InverseLerp(25f, 300f, Vector2.Distance(mainBody[0].pos, behavior.SwimTowards)) * Mathf.Abs(Mathf.Sin((float)tick * 1f / 18.849556f));
                velSpeed = Mathf.Lerp(0.5f, 10f, t);
                multSwimSpeed *= Mathf.Lerp(0.2f, 1.25f, t);
                if (proximityCounter > 0)
                {
                    velSpeed = 0.1f;
                    multSwimSpeed *= 0.1f;
                }
            }
            // RippleJelly
            if (variant == SpawnType.DeathJelly && behavior != null)
            {
                multSwimSpeed /= 2f;
            }
            swimCycle += 0.2f * multSwimSpeed;
            Vector2 p = mainBody[mainBody.Length - 1].pos + Custom.DirVec(mainBody[0].pos, mainBody[mainBody.Length - 1].pos) * 100f;
            if (behavior != null)
            {
                p = behavior.SwimTowards;
            }
            Vector2 a = Custom.DirVec(mainBody[0].pos, p);
            for (int j = 0; j < mainBody.Length; j++)
            {
                float num4 = (float)j / ((float)mainBody.Length - 1f);
                mainBody[j].vel += a * Mathf.Lerp(1f, -1f, Mathf.InverseLerp(0f, 0.5f, num4)) * Mathf.InverseLerp(0.5f, 0f, num4) * 0.06f * sizeFac * multSwimSpeed;
                if (j < mainBody.Length - 1)
                {
                    Vector2 vector = Custom.DirVec(mainBody[j + 1].pos, mainBody[j].pos);
                    mainBody[j].vel += (vector + Custom.PerpendicularVector(vector) * Mathf.Sin(swimCycle - (float)j * 1.2f) * 0.8f * Mathf.Pow(num4, 0.3f)).normalized * 0.2f * sizeFac * multSwimSpeed;
                }
                if (velSpeed >= 0f && mainBody[j].vel.x > velSpeed)
                {
                    BodyChunk bodyChunk = mainBody[j];
                    bodyChunk.vel.x = bodyChunk.vel.x * 0.8f;
                }
                if (velSpeed >= 0f && mainBody[j].vel.y > velSpeed)
                {
                    BodyChunk bodyChunk2 = mainBody[j];
                    bodyChunk2.vel.y = bodyChunk2.vel.y * 0.8f;
                }
            }
            // dominance affecting speed vel
            float d = 0.02f;
            // RippleAmoeba
            if (variant == SpawnType.DeathAmoeba)
            {
                d = 0.2f;
            }
            mainBody[0].vel += a * d * sizeFac * Mathf.Pow(multSwimSpeed, 2f);
        }
        for (int k = 2; k < mainBody.Length; k++)
        {
            WeightedPush(k - 2, k, Custom.DirVec(mainBody[k].pos, mainBody[k - 2].pos), 0.1f * sizeFac);
        }
        if (!room.RoomRect.Vector2Inside(mainBody[0].pos) && !room.ViewedByAnyCamera(mainBody[0].pos, 200f))
        {
            if (canBeDestroyed)
            {
                Destroy();
            }
        }
        else
        {
            canBeDestroyed = true;
        }
        // Directly involves DeathSpawnGraphics, which may need hooking, or a reimplementation for DeathSpawn
        if (graphicsModule is DeathSpawnGraphics)
        {
            culled = !room.ViewedByAnyCamera(mainBody[0].pos, 300f) || !(graphicsModule as DeathSpawnGraphics).VisibleAtGlowDist(mainBody[0].pos, (graphicsModule as DeathSpawnGraphics).glowPos, 100f);
            if (!culled && lastCulled)
            {
                (graphicsModule as DeathSpawnGraphics).Reset();
            }
            lastCulled = culled;
        }
        proximityCounter--;
    }
    public BodyChunk[] mainBody;
    public float swimCycle;
    public float sizeFac;
    public float swimSpeed;
    public Behavior behavior;
    public bool canBeDestroyed;
    public bool culled;
    public bool lastCulled;
    public float fade;
    public float lastFade;
    public float inEggMode;
    public bool consious = true;
    public VoidSpawnEgg egg;
    public float voidMeltInRoom;
    public bool dayLightMode;
    public int rippleLayer;
    public int timeUntilFadeout;
    public bool startFadeOut;
    public SpawnType variant;
    public int proximityCounter;
    private int tick;
    public float dominance;
    public class SpawnType : ExtEnum<SpawnType>
    {
        public SpawnType(string value, bool register = false) : base(value, register)
        {
        }
        // All from og code
        public static readonly SpawnType DeathSpawn = new SpawnType("DeathSpawn", true);
        public static readonly SpawnType DeathJelly = new SpawnType("DeathJelly", true);
        public static readonly SpawnType DeathAmoeba = new SpawnType("DeathAmoeba", true);
        public static readonly SpawnType DeathNoodle = new SpawnType("DeathNoodle", true);
        // New guy
        public static readonly SpawnType DeathBiter = new SpawnType("DeathBiter", true);
    }
    public abstract class Behavior
    {
        public virtual Vector2 SwimTowards
        {
            get
            {
                return owner.mainBody[0].pos;
            }
        }
        public Behavior(DeathSpawn owner)
        {
            this.owner = owner;
        }
        public DeathSpawn owner;
    }
    public class PassThrough : Behavior
    {
        public override Vector2 SwimTowards
        {
            get
            {
                return Vector2.Lerp(pnt, finalDest, Mathf.InverseLerp(500f, 10f, Mathf.Abs(Custom.DistanceToLine(owner.mainBody[0].pos, pnt, finalDest))));
            }
        }
        public PassThrough(DeathSpawn owner, int toRoom, Room room) : base(owner)
        {
            this.toRoom = toRoom;
            pnt = room.RandomPos();
            Vector2 p = room.world.RoomToWorldPos(owner.mainBody[0].pos, room.abstractRoom.index);
            Vector2 vector = room.world.RoomToWorldPos(new Vector2((float)room.world.GetAbstractRoom(toRoom).size.x * UnityEngine.Random.value * 20f, (float)room.world.GetAbstractRoom(toRoom).size.y * UnityEngine.Random.value * 20f), toRoom);
            finalDest = vector - room.world.RoomToWorldPos(new Vector2(0f, 0f), room.abstractRoom.index);
            finalDest += Custom.DirVec(p, vector) * 2000f;
        }
        public int toRoom;
        public Vector2 pnt;
        public Vector2 finalDest;
    }
    public class MillAround : Behavior
    {
        public override Vector2 SwimTowards
        {
            get
            {
                if (Custom.DistLess(dest, owner.mainBody[0].pos, 100f))
                {
                    NewDest();
                }
                return dest;
            }
        }
        public MillAround(DeathSpawn owner, Room room) : base(owner)
        {
            this.room = room;
            rect = MillRectInRoom(room.abstractRoom.name);
            NewDest();
        }
        public void NewDest()
        {
            destShifts++;
            if (destShifts > UnityEngine.Random.Range(4, 8))
            {
                dest = room.RandomPos() + Custom.RNV() * 50000f;
                dest = Custom.RectCollision(room.RandomPos(), dest, room.RoomRect.Grow(700f)).GetCorner(FloatRect.CornerLabel.D);
                return;
            }
            dest = Custom.RandomPointInRect(rect);
        }
        public static FloatRect MillRectInRoom(string roomName)
        {
            if (roomName != null)
            {
                if (roomName == "SH_D02")
                {
                    return new FloatRect(900f, 260f, 3800f, 360f);
                }
                if (roomName == "SH_E02")
                {
                    return new FloatRect(380f, 260f, 2460f, 360f);
                }
            }
            return default(FloatRect);
        }
        public Room room;
        public Vector2 dest;
        public FloatRect rect;
        public int destShifts;
    }
    public class VoidSeaDive : Behavior
    {
        public override Vector2 SwimTowards
        {
            get
            {
                return Vector2.Lerp(pnt, finalDest, Mathf.InverseLerp(700f, 100f, Mathf.Abs(Custom.DistanceToLine(owner.mainBody[0].pos, pnt, finalDest))));
            }
        }
        public VoidSeaDive(DeathSpawn owner, Room room) : base(owner)
        {
            finalDest = new Vector2(2500f, 360f);
            pnt = Custom.RandomPointInRect(new FloatRect(2000f, 1170f, 3500f, 1300f));
        }
        public Vector2 pnt;
        public Vector2 finalDest;
    }
    // We won't need Eggs for DeathSpawn
    // We will need SwimDown but pointing up maybe?
    public class SwimUp : Behavior
    {
        public SwimUp(DeathSpawn owner, Room room) : base(owner)
        {
        }
        public override Vector2 SwimTowards
        {
            get
            {
                // swapped - to +, which should point upward
                return new Vector2(owner.mainBody[0].pos.x, owner.mainBody[1].pos.y + 100f);
            }
        }
    }
    public class ChasePlayer : Behavior
    {
        public ChasePlayer(DeathSpawn owner, Room room) : base(owner)
        {
        }
        public override Vector2 SwimTowards
        {
            get
            {
                Player player = null;
                float num = 0f;
                foreach (AbstractCreature abstractCreature in owner.room.game.Players)
                {
                    Player player2 = null;
                    if (abstractCreature.realizedCreature != null)
                    {
                        player2 = abstractCreature.realizedCreature as Player;
                    }
                    if (player2 != null && player2.room != null && player2.room.abstractRoom.index == owner.room.abstractRoom.index)
                    {
                        float num2 = Vector2.Distance(owner.firstChunk.pos, player2.mainBodyChunk.pos);
                        if (player == null || num2 < num)
                        {
                            player = player2;
                            num = num2;
                        }
                        // Swaps behavior automatically if the type isn't DeathAmoeba
                        if (owner.variant != SpawnType.DeathAmoeba && num2 < 400f)
                        {
                            owner.behavior = new CircleSwarm(owner, owner.room);
                        }
                    }
                }
                if (player == null)
                {
                    return new Vector2(owner.mainBody[0].pos.x, owner.mainBody[1].pos.y);
                }
                if (player.standingInWarpPointProtectionTime > 0 || player.warpPointCooldown > 0)
                {
                    return owner.mainBody[0].pos + Custom.DirVec(player.mainBodyChunk.pos, owner.mainBody[0].pos) * 400f;
                }
                return player.mainBodyChunk.pos;
            }
        }
    }
    // Intended for RippleJelly
    public class JellyBehavior : Behavior
    {
        public JellyBehavior(DeathSpawn owner, Room room) : base(owner)
        {
            direction = UnityEngine.Random.value < 0.5f;
            normalSwimSpeed = owner.swimSpeed;
        }
        public override Vector2 SwimTowards
        {
            get
            {
                velocityLoss += goingDown ? -0.025f : 0.05f;
                if (velocityLoss >= 1f || velocityLoss <= 0f)
                {
                    goingDown = !goingDown;
                }
                owner.swimSpeed = Mathf.Lerp(0f, normalSwimSpeed, velocityLoss);
                return owner.mainBody[0].pos + new Vector2(80f * (float)(direction ? -1 : 1) + UnityEngine.Random.Range(50f, 50f), 80f);
            }
        }
        private bool direction;
        public float velocityLoss;
        public bool goingDown;
        public float normalSwimSpeed;
    }
    public class Flee : Behavior
    {
        public Flee(DeathSpawn owner, Room room) : base(owner)
        {
        }
        public override Vector2 SwimTowards
        {
            get
            {
                Player player = null;
                float num = 0f;
                Vector2 vector = default(Vector2);
                foreach (AbstractCreature abstractCreature in owner.room.game.Players)
                {
                    Player player2 = null;
                    if (abstractCreature.realizedCreature != null)
                    {
                        player2 = abstractCreature.realizedCreature as Player;
                    }
                    if (player2 != null && player2.room != null && player2.room.abstractRoom.index == owner.room.abstractRoom.index)
                    {
                        float num2 = Vector2.Distance(owner.firstChunk.pos, player2.mainBodyChunk.pos);
                        if (player == null || num2 < num)
                        {
                            player = player2;
                            num = num2;
                        }
                    }
                }
                boredom += 0.02f;
                if (UnityEngine.Random.value < boredom)
                {
                    owner.behavior = new BezierSwarm(owner, owner.room);
                }
                if (player != null && num < 900f)
                {
                    vector = Custom.DirVec(player.mainBodyChunk.pos, owner.firstChunk.pos);
                    bool flag = vector.y > 0f;
                    return new Vector2((owner.firstChunk.pos + vector.normalized * 5f).x, owner.firstChunk.pos.y + (flag ? 1f : -1f) * 5f);
                }
                return new Vector2(owner.mainBody[0].pos.x, owner.mainBody[1].pos.y);
            }
        }
        private float boredom;
    }
    public class Jitter : Behavior
    {
        public Jitter(DeathSpawn owner, Room room) : base(owner)
        {
            spot = owner.mainBody[0].pos + Custom.IntVector2ToVector2(Custom.eightDirections[UnityEngine.Random.Range(0, Custom.eightDirections.Length - 1)]) * 20f;
        }
        public override Vector2 SwimTowards
        {
            get
            {
                if (jitterTime <= 0)
                {
                    jitterTime = 800;
                    spot = owner.mainBody[0].pos + Custom.IntVector2ToVector2(Custom.eightDirections[UnityEngine.Random.Range(0, Custom.eightDirections.Length - 1)]) * 10f;
                }
                else
                {
                    jitterTime--;
                }
                return spot;
            }
        }
        public Vector2 spot;
        private int jitterTime = 500;
    }
    public class Swarm : Behavior
    {
        public Swarm(DeathSpawn owner, Room room) : base(owner)
        {
        }
        public override Vector2 SwimTowards
        {
            get
            {
                if (checkForNewLeader == 0 || swarmLeader == null)
                {
                    GetNewLeader();
                }
                else
                {
                    checkForNewLeader--;
                }
                if (Custom.Dist(owner.firstChunk.pos, dest) < 50f + 200f * Mathf.InverseLerp(1f, 0f, owner.dominance))
                {
                    GetNewDest();
                }
                return dest;
            }
        }
        public void GetNewDest()
        {
            if (swarmLeader != null)
            {
                Vector2 a = Custom.DirVec(swarmLeader.bodyChunks[1].pos, swarmLeader.bodyChunks[0].pos) + Custom.DegToVec(45f - 45f * Mathf.InverseLerp(0f, 1f, owner.dominance)) * ((UnityEngine.Random.value < 0.5f) ? 1f : -1f);
                dest = swarmLeader.firstChunk.pos + a * (5f - Mathf.InverseLerp(0f, 1f, owner.dominance));
            }
        }
        public void GetNewLeader()
        {
            owner.deathSpawnCWT.TryGetValue(owner.room, out DeathSpawnCWT cwt);
            foreach (DeathSpawn deathSpawn in cwt.deathSpawns)
            {
                if ((swarmLeader == null || swarmLeader.dominance < deathSpawn.dominance) && deathSpawn.dominance > owner.dominance)
                {
                    swarmLeader = deathSpawn;
                }
            }
            checkForNewLeaderAdditionalCD += 500;
            checkForNewLeader = 1000 + checkForNewLeaderAdditionalCD;
            GetNewDest();
        }
        public DeathSpawn swarmLeader;
        public float swarmSpot;
        private int checkForNewLeader = 250;
        private int checkForNewLeaderAdditionalCD;
        private Vector2 dest;
    }
    public class CircleSwarm : Behavior
    {
        public Vector2 positionPlusVariance
        {
            get
            {
                float d = 1f;
                if (Mathf.Abs(Custom.eightDirections[currentDirection].x) == 1 && Mathf.Abs(Custom.eightDirections[currentDirection].y) == 1)
                {
                    d = 0.75f;
                }
                return variance + Custom.IntVector2ToVector2(Custom.eightDirections[currentDirection]) * 165f * d;
            }
        }
        public CircleSwarm(DeathSpawn owner, Room room) : base(owner)
        {
            currentDirection = UnityEngine.Random.Range(0, 7);
            GetNewVariance();
        }
        public void GetNewVariance()
        {
            variance = new Vector2(UnityEngine.Random.Range(-45f, 45f), UnityEngine.Random.Range(-45f, 45f));
        }
        public override Vector2 SwimTowards
        {
            get
            {
                boredom += 0.002f;
                boredomTickCheck--;
                if (boredomTickCheck <= 0)
                {
                    boredomTickCheck = 90;
                    if (UnityEngine.Random.value < boredom)
                    {
                        owner.behavior = new Flee(owner, owner.room);
                    }
                }
                using (List<AbstractCreature>.Enumerator enumerator = owner.room.game.Players.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        AbstractCreature abstractCreature = enumerator.Current;
                        Player player = null;
                        if (abstractCreature.realizedCreature != null)
                        {
                            player = abstractCreature.realizedCreature as Player;
                        }
                        if (player != null && player.room != null && player.room.abstractRoom.index == owner.room.abstractRoom.index)
                        {
                            if (Custom.DistLess(owner.firstChunk.pos, player.mainBodyChunk.pos + positionPlusVariance, 45f))
                            {
                                currentDirection++;
                                if (currentDirection >= Custom.eightDirections.Length)
                                {
                                    currentDirection = 0;
                                }
                                GetNewVariance();
                            }
                            return player.mainBodyChunk.pos + positionPlusVariance;
                        }
                        owner.behavior = new BezierSwarm(owner, owner.room);
                        return owner.mainBody[0].pos;
                    }
                }
                return Vector2.zero;
            }
        }
        public void GetNewCenter()
        {
            float value = UnityEngine.Random.value;
        }
        public float currentDegree;
        public Vector2 currentCenter;
        private int currentDirection;
        public Vector2 variance;
        public float boredom;
        public int boredomTickCheck = 90;
    }
    public class BezierSwarm : Behavior
    {
        public BezierSwarm(DeathSpawn owner, Room room) : base(owner)
        {
            variance = new Vector2((float)UnityEngine.Random.Range(-150, 150), (float)UnityEngine.Random.Range(-150, 150));
        }
        public void GetNewBezier()
        {
            if (followingAmount < 0)
            {
                if (leader != null)
                {
                    BezierPoints = leader.BezierPoints;
                    return;
                }
                owner.deathSpawnCWT.TryGetValue(owner.room, out DeathSpawnCWT cwt);
                foreach (DeathSpawn deathSpawn in cwt.deathSpawns)
                {
                    if (deathSpawn.behavior is BezierSwarm)
                    {
                        BezierSwarm bezierSwarm = deathSpawn.behavior as BezierSwarm;
                        if (bezierSwarm.hasBezierPoints && !bezierSwarm.isFollowing && bezierSwarm.followingAmount < 10 && (BezierPoints == null || (BezierPoints != null && bezierSwarm.BezierPoints[0] != BezierPoints[0])))
                        {
                            leader = bezierSwarm;
                            isFollowing = true;
                            bezierSwarm.followingAmount++;
                            BezierPoints = new Vector2[4];
                            BezierPoints = bezierSwarm.BezierPoints;
                            hasBezierPoints = true;
                            currentT = 0f;
                            return;
                        }
                    }
                }
            }
            if (owner.mainBody[0].pos == Vector2.zero)
            {
                return;
            }
            foreach (AbstractCreature abstractCreature in owner.room.game.Players)
            {
                Player player = null;
                if (abstractCreature.realizedCreature != null)
                {
                    player = abstractCreature.realizedCreature as Player;
                }
                if (player != null && player.room != null && player.room.abstractRoom.index == owner.room.abstractRoom.index && !Custom.DistLess(player.firstChunk.pos, owner.mainBody[0].pos, 75f))
                {
                    currentT = 0f;
                    BezierPoints = new Vector2[4];
                    BezierPoints[0] = owner.mainBody[0].pos;
                    BezierPoints[1] = owner.mainBody[0].pos + new Vector2(UnityEngine.Random.Range(-1000f, 1000f), UnityEngine.Random.Range(-200f, 1000f));
                    BezierPoints[2] = player.firstChunk.pos + new Vector2(UnityEngine.Random.Range(-350f, 350f), UnityEngine.Random.Range(-200f, 600f));
                    BezierPoints[3] = player.firstChunk.pos;
                    hasBezierPoints = true;
                    break;
                }
                bool flag = owner.room.PixelWidth / 2f < owner.mainBody[0].pos.x;
                currentT = 0f;
                BezierPoints = new Vector2[4];
                BezierPoints[0] = owner.mainBody[0].pos;
                BezierPoints[1] = owner.mainBody[0].pos + new Vector2(UnityEngine.Random.Range(-1000f, 1000f), UnityEngine.Random.Range(-200f, 1000f));
                if (flag)
                {
                    Vector2 vector = new Vector2(UnityEngine.Random.Range(0f, owner.room.PixelWidth / 2f), owner.room.PixelHeight / 2f);
                    BezierPoints[2] = new Vector2(Mathf.Clamp(vector.x + UnityEngine.Random.Range(0f, 1000f), 0f, owner.room.PixelWidth), Mathf.Clamp(vector.y + UnityEngine.Random.Range(-500f, 500f), 0f, owner.room.PixelHeight));
                    BezierPoints[3] = vector;
                }
                else
                {
                    Vector2 vector2 = new Vector2(UnityEngine.Random.Range(owner.room.PixelWidth / 2f, owner.room.PixelWidth), owner.room.PixelHeight / 2f);
                    BezierPoints[2] = new Vector2(Mathf.Clamp(vector2.x + UnityEngine.Random.Range(-1000f, 0f), 0f, owner.room.PixelWidth), Mathf.Clamp(vector2.y + UnityEngine.Random.Range(-500f, 500f), 0f, owner.room.PixelHeight));
                    BezierPoints[3] = vector2;
                }
            }
            isFollowing = false;
        }
        public override Vector2 SwimTowards
        {
            get
            {
                if (BezierPoints == null || !hasBezierPoints)
                {
                    GetNewBezier();
                    return owner.firstChunk.pos;
                }
                Vector2 a = Custom.Bezier(BezierPoints[0], BezierPoints[1], BezierPoints[2], BezierPoints[3], currentT);
                foreach (AbstractCreature abstractCreature in owner.room.game.Players)
                {
                    Player player = null;
                    if (abstractCreature.realizedCreature != null)
                    {
                        player = (abstractCreature.realizedCreature as Player);
                    }
                    if (player != null && Custom.DistLess(player.firstChunk.pos, owner.mainBody[0].pos, 150f))
                    {
                        interestInPlayer = Mathf.Clamp01(interestInPlayer + 0.01f);
                        if (UnityEngine.Random.value < interestInPlayer)
                        {
                            owner.behavior = new ChasePlayer(owner, owner.room);
                            return owner.mainBody[0].pos;
                        }
                    }
                    else
                    {
                        interestInPlayer = Mathf.Clamp01(interestInPlayer - 0.005f);
                    }
                }
                if (Custom.DistLess(owner.firstChunk.pos, a + variance, 35f))
                {
                    if (currentT < 1f)
                    {
                        variance = new Vector2((float)UnityEngine.Random.Range(-100, 100), (float)UnityEngine.Random.Range(-100, 100));
                        currentT += 0.1f;
                    }
                    else
                    {
                        GetNewBezier();
                    }
                }
                return a + variance;
            }
        }
        public Vector2[] BezierPoints = new Vector2[4];
        public float currentT;
        public bool hasBezierPoints;
        public int followingAmount;
        public bool isFollowing;
        public Vector2 variance;
        public BezierSwarm leader;
        public float interestInPlayer;
    }
}