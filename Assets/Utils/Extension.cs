using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extension
{
    // Ȯ�� �޼���
    
    // ���̾ ���ԵǾ��ִ��� Ȯ��
    public static bool Contain(this LayerMask layerMask, int layer)
    {
        return ((1 << layer) & layerMask) != 0;
    }
}
