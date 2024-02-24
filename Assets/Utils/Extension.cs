using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Extension method
public static class Extension
{
    // Check if the layer is included in the layermask
    public static bool Contain(this LayerMask layerMask, int layer)
    {
        return ((1 << layer) & layerMask) != 0;
    }

    // Return the zAngle from the agent position(3D) to the target point(2D)
    public static float GetAngleToTarget2D(this Vector3 agentPosition, Vector2 targetPosition)
    {
        Vector3 distance = new Vector3(targetPosition.x - agentPosition.x, targetPosition.y - agentPosition.y, 0);
        return Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;
    }

    // Return the direction from the agent position(3D) to the target point(2D)
    public static Vector3 GetDirectionToTarget2D(this Vector3 agentPosition, Vector2 targetPoint)
    {
        Vector3 direction = new Vector3(targetPoint.x - agentPosition.x, targetPoint.y - agentPosition.y, 0).normalized;
        return direction;
    }
}
