
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WaypointManager : MonoBehaviour
{
    public Transform[] waypoints;

    private void Awake()
    {
        List<Transform> waypointList = new List<Transform>();

        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Waypoint_"))
            {
                waypointList.Add(child);
            }
        }

        waypointList = waypointList
            .OrderBy(w => GetWaypointNumber(w.name))
            .ToList();

        waypoints = waypointList.ToArray();
    }

    private int GetWaypointNumber(string waypointName)
    {
        string numberPart = waypointName.Replace("Waypoint_", "");

        if (int.TryParse(numberPart, out int number))
        {
            return number;
        }

        return int.MaxValue;
    }

    public Transform GetWaypoint(int index)
    {
        if (index < 0 || index >= waypoints.Length)
        {
            Debug.LogError("Invalid waypoint index: " + index);
            return null;
        }

        return waypoints[index];
    }
}
