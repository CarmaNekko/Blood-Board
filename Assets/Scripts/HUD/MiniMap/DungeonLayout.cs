using System;
using System.Collections.Generic;
using UnityEngine;

public enum DungeonRoomType
{
    Start,
    Normal,
    Final
}

[Serializable]
public class MapConnection
{
    public int fromRoomId;
    public int toRoomId;

    public bool Matches(int firstRoomId, int secondRoomId)
    {
        return (fromRoomId == firstRoomId && toRoomId == secondRoomId) ||
               (fromRoomId == secondRoomId && toRoomId == firstRoomId);
    }
}

[Serializable]
public class MapNode
{
    public int id;
    public DungeonRoomType roomType;
    public Vector2Int gridPosition;
    public Vector3 worldPosition;
    public bool discovered;
    public List<int> neighbors = new List<int>();

    [NonSerialized] public RoomInstance roomInstance;
}

public class DungeonLayout
{
    public event Action LayoutChanged;

    private readonly List<MapNode> rooms = new List<MapNode>();
    private readonly List<MapConnection> connections = new List<MapConnection>();

    public IReadOnlyList<MapNode> Rooms => rooms;
    public IReadOnlyList<MapConnection> Connections => connections;
    public int CurrentRoomId { get; private set; } = -1;

    public int AddRoom(DungeonRoomType roomType, Vector2Int gridPosition, Vector3 worldPosition)
    {
        MapNode room = new MapNode
        {
            id = rooms.Count,
            roomType = roomType,
            gridPosition = gridPosition,
            worldPosition = worldPosition
        };

        rooms.Add(room);
        LayoutChanged?.Invoke();
        return room.id;
    }

    public void BindRoomInstance(int roomId, RoomInstance roomInstance)
    {
        if (!TryGetRoom(roomId, out MapNode room))
        {
            return;
        }

        room.roomInstance = roomInstance;
        room.worldPosition = roomInstance.transform.position;
        LayoutChanged?.Invoke();
    }

    public void ConnectRooms(int firstRoomId, int secondRoomId)
    {
        if (!TryGetRoom(firstRoomId, out MapNode firstRoom) ||
            !TryGetRoom(secondRoomId, out MapNode secondRoom) ||
            firstRoomId == secondRoomId)
        {
            return;
        }

        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i].Matches(firstRoomId, secondRoomId))
            {
                return;
            }
        }

        connections.Add(new MapConnection
        {
            fromRoomId = firstRoomId,
            toRoomId = secondRoomId
        });

        AddNeighbor(firstRoom, secondRoomId);
        AddNeighbor(secondRoom, firstRoomId);
        LayoutChanged?.Invoke();
    }

    public bool DiscoverRoom(int roomId)
    {
        if (!TryGetRoom(roomId, out MapNode room) || room.discovered)
        {
            return false;
        }

        room.discovered = true;
        LayoutChanged?.Invoke();
        return true;
    }

    public bool SetCurrentRoom(int roomId)
    {
        if (!TryGetRoom(roomId, out _))
        {
            return false;
        }

        if (CurrentRoomId == roomId)
        {
            return false;
        }

        CurrentRoomId = roomId;
        LayoutChanged?.Invoke();
        return true;
    }

    public bool IsDiscovered(int roomId)
    {
        return TryGetRoom(roomId, out MapNode room) && room.discovered;
    }

    public bool TryGetRoom(int roomId, out MapNode room)
    {
        if (roomId >= 0 && roomId < rooms.Count)
        {
            room = rooms[roomId];
            return true;
        }

        room = null;
        return false;
    }

    private static void AddNeighbor(MapNode room, int neighborId)
    {
        if (!room.neighbors.Contains(neighborId))
        {
            room.neighbors.Add(neighborId);
        }
    }
}
