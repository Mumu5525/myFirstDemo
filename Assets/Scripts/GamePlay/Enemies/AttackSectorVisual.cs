using UnityEngine;

// 在 Game 视图的地面上画出攻击扇形：浅红底色 + 亮红充能填充
public class AttackSectorVisual : MonoBehaviour
{
    const int MaxSegments = 48;
    const float FillHeight = 0.002f;   // 填充层比底色略高一点，避免共面闪烁

    static readonly Color RangeColor = new Color(1f, 0.25f, 0.25f, 0.12f);
    static readonly Color FillColor = new Color(1f, 0.1f, 0.1f, 0.5f);

    float radius;
    float angle;
    Mesh fillMesh;
    float lastFill = -1f;

    public static AttackSectorVisual Create(Transform owner, float radius, float angle)
    {
        GameObject go = new GameObject("AttackSector");
        go.transform.SetParent(owner, false);
        go.transform.localPosition = new Vector3(0f, -0.5f, 0f);

        AttackSectorVisual visual = go.AddComponent<AttackSectorVisual>();
        visual.radius = radius;
        visual.angle = angle;

        // 底色：完整扇形
        Mesh rangeMesh = new Mesh { name = "AttackRange" };
        BuildFan(rangeMesh, radius, -angle * 0.5f, angle * 0.5f);
        CreateLayer(go.transform, "Range", rangeMesh, RangeColor);

        // 填充：亮红扇形，随充能角度变大
        visual.fillMesh = new Mesh { name = "AttackFill" };
        Transform fill = CreateLayer(go.transform, "Fill", visual.fillMesh, FillColor);
        fill.localPosition = new Vector3(0f, FillHeight, 0f);

        visual.SetVisible(false);   // 只有攻击时才显示
        return visual;
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    // progress: 0 = 只显示底色，1 = 填满整个扇形（从左边沿扫到右边沿）
    public void SetFill(float progress)
    {
        progress = Mathf.Clamp01(progress);
        if (Mathf.Abs(progress - lastFill) < 0.001f) return;
        lastFill = progress;

        if (progress <= 0f)
        {
            fillMesh.Clear();
            return;
        }

        float start = -angle * 0.5f;
        BuildFan(fillMesh, radius, start, Mathf.Lerp(start, angle * 0.5f, progress));
    }

    static Transform CreateLayer(Transform parent, string name, Mesh mesh, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;

        MeshRenderer renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = new Material(Shader.Find("Sprites/Default")) { color = color };
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return go.transform;
    }

    // 在 XZ 平面上生成以本地原点为圆心、+Z 为 0° 的扇形网格
    static void BuildFan(Mesh mesh, float radius, float startAngle, float endAngle)
    {
        int segments = Mathf.Max(1, Mathf.CeilToInt(MaxSegments * Mathf.Abs(endAngle - startAngle) / 360f));
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i <= segments; i++)
        {
            float a = Mathf.Lerp(startAngle, endAngle, i / (float)segments) * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(Mathf.Sin(a) * radius, 0f, Mathf.Cos(a) * radius);
        }
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }
}