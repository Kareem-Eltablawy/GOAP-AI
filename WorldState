using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

// World state representation using enums for better performance and clarity
public enum WorldStateKey
{
    SeeThief,
    AtWaypoint,
    ThiefCaught,
    PatrolComplete
}

public class WorldState
{
    public Dictionary<WorldStateKey, bool> States { get; private set; }

    public WorldState()
    {
        States = new Dictionary<WorldStateKey, bool>();
        // Initialize all states to false
        foreach (WorldStateKey key in Enum.GetValues(typeof(WorldStateKey)))
        {
            States.Add(key, false);
        }
    }

    public bool GetState(WorldStateKey key)
    {
        return States[key];
    }

    public void SetState(WorldStateKey key, bool value)
    {
        States[key] = value;
    }

    public WorldState Clone()
    {
        WorldState clone = new WorldState();
        foreach (var kvp in States)
        {
            clone.States[kvp.Key] = kvp.Value;
        }
        return clone;
    }
}
