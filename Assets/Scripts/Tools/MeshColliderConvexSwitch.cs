#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class MeshColliderConvexSwitch : EditorWindow
{
    static bool Switch = true;
    [MenuItem("Tools/MeshCollider Convex Switch")]
    static void SetConvex()
    {
        // 获取当前选中的所有物体
        GameObject[] selected = Selection.gameObjects;
        if (selected.Length == 0)
        {
            Debug.LogWarning("请先在 Hierarchy 中选中你要处理的物体。");
            return;
        }

        int count = 0;

        foreach (GameObject go in selected)
        {
            // 遍历选中物体及其所有子物体上的 MeshCollider
            MeshCollider[] colliders = go.GetComponentsInChildren<MeshCollider>(true);
            foreach (MeshCollider mc in colliders)
            {
                    // 记录撤销操作，方便 Ctrl+Z 回退
                    Undo.RecordObject(mc, "Set MeshCollider Convex");
                    mc.convex = Switch;
                    EditorUtility.SetDirty(mc);
                    count++;
            }
        }

        Debug.Log($"MeshColliderConvexSwitch: 共将 {count} 个 MeshCollider设置为 {Switch} !");
        Switch = !Switch;
    }
}
#endif