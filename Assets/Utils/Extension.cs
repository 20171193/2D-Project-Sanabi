using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extension
{
    // 확장 메서드
    
    // 레이어가 포함되어있는지 확인
    public static bool Contain(this LayerMask layerMask, int layer)
    {
        return ((1 << layer) & layerMask) != 0;
    }
}
