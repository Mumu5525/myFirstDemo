#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;          // NavMesh.GetAreaFromName
using Unity.AI.Navigation;     // NavMeshModifierVolume（之前缺这行）

public class AutoCreateNavMeshModifierVolumes : EditorWindow
{
    [MenuItem("Tools/Auto Create NavMeshModifierVolumes")]
    static void CreateVolumes()
    {
        GameObject[] selected = Selection.gameObjects;
        if (selected.Length == 0)
        {
            Debug.LogWarning("请先在 Hierarchy 中选中你要处理的山体（或它们的父物体）。");
            return;
        }

        // 预取 "Not Walkable" 的 area 索引，查一次即可，并做兜底判断
        int notWalkableArea = NavMesh.GetAreaFromName("Not Walkable");
        if (notWalkableArea < 0)
        {
            Debug.LogError("找不到 'Not Walkable' NavMesh Area，请检查 Navigation 设置。");
            return;
        }

        // 统一用一个空物体收纳所有 Volume，避免散落，也避免受山体 scale 影响
        GameObject container = new GameObject("NavMeshModifierVolumes");

        int count = 0;

        foreach (GameObject go in selected)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                Bounds bounds = r.bounds;   // 世界空间的轴对齐包围盒

                GameObject volumeObj = new GameObject($"NavVolume_{r.gameObject.name}");
                volumeObj.transform.position = bounds.center;      // 世界坐标定位
                volumeObj.transform.SetParent(container.transform);

                NavMeshModifierVolume volume = volumeObj.AddComponent<NavMeshModifierVolume>();
                volume.size = bounds.size;
                volume.center = Vector3.zero;
                volume.area = notWalkableArea;

                count++;
            }
        }

        Debug.Log($"批量创建完成，共生成 {count} 个 NavMeshModifierVolume。请重新 Bake 导航网格。");
    }
}
#endif
