using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// Extension method
public static class Extension
{
    // 레이어마스크가 해당 레이어를 포함하고 있는지 체크
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
    public static Vector3 GetDirectionTo2DTarget(this Vector3 agentPosition, Vector2 targetPoint)
    {
        Vector3 direction = new Vector3(targetPoint.x - agentPosition.x, targetPoint.y - agentPosition.y, 0).normalized;
        return direction;
    }
    
    // convert Vector3 to Vector2 (z = 0) 
    public static Vector2 ConvertToVector2(this Vector3 origin)
    {
        Vector2 ret = new Vector2(origin.x, origin.y);
        return ret;
    }

    // convert Vector2 to Vector3 (z = param)
    public static Vector3 ConvertToVector3(this Vector2 origin, float zPos)
    {
        Vector3 ret = new Vector3(origin.x, origin.y, zPos);
        return ret;
    }
    // Overload convert Vector2 to Vector3 (z = 0)
    public static Vector3 ConvertToVector3(this Vector2 origin)
    {
        Vector3 ret = new Vector3(origin.x, origin.y, 0f);
        return ret;
    }

    public static IEnumerator DelayRoutine(float delayTime, UnityAction nextAction)
    {
        yield return new WaitForSeconds(delayTime);
        nextAction?.Invoke();
    }
}
