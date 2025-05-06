using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace PitchBlack;

// Spawned creature from VoidSpawn effect (which use the type RippleSpawn here), edited to use RippleMelt as well.
public class RippleSpawnKeeper : UpdatableAndDeletable
{
    public int IdealSpawnNumber
    {
        get
        {
            return Math.Min((int)((float)(room.TileWidth * room.TileHeight) * effect.amount), 4000) / 40;
        }
    }
    public bool CurrentlyViewed
    {
        get
        {
            // We don't need the void sea scene check
            return room.BeingViewed;
        }
    }
    public static bool DayLightMode(Room room)
    {
        // NOTE: Room.RoomSettings.Palette and FadePalette is how we can access the palette of the room -Lur
        // region not null, region not SL, palette not 10, palette not 8, fadepalette not null, fadepalette not 10, fadepalette not 8
        return room.world.region != null /*&& !(room.world.region.name != "SL")*/ && room.roomSettings.Palette != 10 && room.roomSettings.Palette != 8 && (room.roomSettings.fadePalette == null || (room.roomSettings.fadePalette.palette != 10 && room.roomSettings.fadePalette.palette != 8));
    }
    public RippleSpawnKeeper(Room room, RoomSettings.RoomEffect effect)
    {
        this.room = room;
        this.effect = effect;
        daylightMode = DayLightMode(room);
        if (room.world.voidSpawnWorldAI == null)
        {
            // All variants of VoidSpawn use VoidSpawnAI
            room.world.AddWorldProcess(new VoidSpawnWorldAI(room.world));
        }
        worldAI = room.world.voidSpawnWorldAI;
        spawn = new List<VoidSpawn>();
        // will be changed to RippleMelt
        rippleMeltInRoom = room.roomSettings.GetEffectAmount(PBEnums.RoomEffectType.RippleMelt);
        fromRooms = new int[0];
    }
    public override void Update(bool eu)
    {
        base.Update(eu);
        if (worldAI.directionFinder == null || !worldAI.directionFinder.done)
        {
            return;
        }
        if (!initiated)
        {
            initiated = true;
            Initiate();
            return;
        }
        if (CurrentlyViewed)
        {
            if (!lastViewed)
            {
                this.ScatterSpawn();
            }
            for (int i = spawn.Count - 1; i >= 0; i--)
            {
                if (spawn[i].slatedForDeletetion)
                {
                    spawn.RemoveAt(i);
                }
            }
            if (spawn.Count < IdealSpawnNumber && UnityEngine.Random.value < 0.2f)
            {
                this.AddOneSpawn();
            }
        }
        else if (lastViewed)
        {
            for (int j = spawn.Count - 1; j >= 0; j--)
            {
                spawn[j].Destroy();
            }
            spawn.Clear();
        }
        lastViewed = CurrentlyViewed;
    }
    private void Initiate()
    {
        float maxVal = float.MaxValue;
        List<int> list = new List<int>();
        for (int i = 0; i < room.abstractRoom.connections.Length; i++)
        {
            if (room.abstractRoom.connections[i] > -1)
            {
                WorldCoordinate worldCoord = new WorldCoordinate(room.abstractRoom.index, -1, -1, i);
                WorldCoordinate worldCoord2 = new WorldCoordinate(room.abstractRoom.connections[i], -1, -1, room.world.GetAbstractRoom(room.abstractRoom.connections[i]).ExitIndex(room.abstractRoom.index));
                float distance = worldAI.directionFinder.DistanceToDestination(worldCoord2);
                if (distance > -1f)
                {
                    if (distance < maxVal)
                    {
                        maxVal = distance;
                        toRoom = room.abstractRoom.connections[i];
                    }
                    if (worldAI.directionFinder.DistanceToDestination(worldCoord) > -1f && distance > worldAI.directionFinder.DistanceToDestination(worldCoord))
                    {
                        list.Add(room.abstractRoom.connections[i]);
                    }
                }
            }
        }
        list.Remove(toRoom);
        fromRooms = list.ToArray();
        Custom.Log(toRoom.ToString());
        Custom.Log("::TO ROOM:", toRoom.ToString(), (toRoom == -1) ? "NULL" : room.world.GetAbstractRoom(toRoom).name);
        for (int j = 0; j < fromRooms.Length; j++)
        {
            Custom.Log("From room:", room.world.GetAbstractRoom(fromRooms[j]).name);
        }
        if (toRoom == -1)
        {
            Destroy();
        }
    }
    private void ScatterSpawn()
    {
        if (toRoom == -1)
        {
            return;
        }
        int num = 0;
        while (num < IdealSpawnNumber * 0.7f)
        {
            Vector2 randomPos = room.RandomPos();
            VoidSpawn rippleSpawn = new VoidSpawn(new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.VoidSpawn, null, room.GetWorldCoordinate(randomPos), room.game.GetNewID()), rippleMeltInRoom, daylightMode, VoidSpawn.SpawnType.RippleSpawn);
            if (room.abstractRoom.name == "SB_L01" || (ModManager.MSC && (room.abstractRoom.name == "SB_DO6")))
            {
                rippleSpawn.behavior = new VoidSpawn.VoidSeaDive(rippleSpawn, room);
                float value = UnityEngine.Random.value;
                rippleSpawn.abstractPhysicalObject.pos = room.GetWorldCoordinate(Vector2.Lerp(new Vector2(160f, 2110f), Custom.RandomPointInRect(VoidSpawn.MillAround.MillRectInRoom("SB_L01")), value) + Custom.RNV() * UnityEngine.Random.value * Mathf.Lerp(500f, 50f, value));
            }
            else if (room.abstractRoom.name == "SH_D02" || room.abstractRoom.name == "SH_E02")
            {
                rippleSpawn.behavior = new VoidSpawn.MillAround(rippleSpawn, room);
                if (UnityEngine.Random.value < 0.7f)
                {
                    rippleSpawn.abstractPhysicalObject.pos = room.GetWorldCoordinate(Custom.RandomPointInRect((rippleSpawn.behavior as VoidSpawn.MillAround).rect));
                }
            }
            else
            {
                rippleSpawn.behavior = new VoidSpawn.PassThrough(rippleSpawn, toRoom, room);
                (rippleSpawn.behavior as VoidSpawn.PassThrough).pnt = room.MiddleOfTile(rippleSpawn.abstractPhysicalObject.pos);
            }
            rippleSpawn.PlaceInRoom(room);
            spawn.Add(rippleSpawn);
            num++;
        }
    }
    private void AddOneSpawn()
    {
        if (toRoom == -1)
        {
            return;
        }
        Vector2 vector;
        if (fromRooms.Length != 0)
        {
            int num = fromRooms[UnityEngine.Random.Range(0, fromRooms.Length)];
            vector = room.world.RoomToWorldPos(new Vector2((float)room.world.GetAbstractRoom(num).size.x * UnityEngine.Random.value * 20f, (float)room.world.GetAbstractRoom(num).size.y * UnityEngine.Random.value * 20f), num);
            vector -= room.world.RoomToWorldPos(new Vector2(0f, 0f), room.abstractRoom.index);
            vector += Custom.DirVec(room.RandomPos(), vector) * 2000f;
            vector = Custom.RectCollision(room.RandomPos(), vector, room.RoomRect.Grow(200f)).GetCorner(FloatRect.CornerLabel.D);
        }
        else
        {
            vector = room.RandomPos() + Custom.RNV() * 10000f;
            vector = Custom.RectCollision(room.RandomPos(), vector, room.RoomRect.Grow(200f)).GetCorner(FloatRect.CornerLabel.D);
        }
        VoidSpawn rippleSpawn = new VoidSpawn(new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.VoidSpawn, null, room.GetWorldCoordinate(vector), room.game.GetNewID()), rippleMeltInRoom, daylightMode, VoidSpawn.SpawnType.RippleSpawn);
        rippleSpawn.PlaceInRoom(room);
        if (room.abstractRoom.name == "SB_L01")
        {
            rippleSpawn.behavior = new VoidSpawn.VoidSeaDive(rippleSpawn, room);
        }
        else if (room.abstractRoom.name == "SH_D02" || room.abstractRoom.name == "SH_E02")
        {
            rippleSpawn.behavior = new VoidSpawn.MillAround(rippleSpawn, room);
        }
        else
        {
            rippleSpawn.behavior = new VoidSpawn.PassThrough(rippleSpawn, toRoom, room);
        }
        spawn.Add(rippleSpawn);
    }
    private RoomSettings.RoomEffect effect;
    public bool daylightMode;
    private VoidSpawnWorldAI worldAI;
    public int toRoom = -1;
    public int[] fromRooms;
    public bool initiated;
    public List<VoidSpawn> spawn;
    private bool lastViewed;
    public float rippleMeltInRoom;
}