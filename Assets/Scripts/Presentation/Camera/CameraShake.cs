using UnityEngine;

/// <summary>
/// 相机抖动。挂在 MainCamera 上（和 TopDownCamera 同一个物体）。
/// 必须在 TopDownCamera 之后执行，所以设置 Execution Order = 100。
/// </summary>
[DefaultExecutionOrder(100)]
public class CameraShake : MonoBehaviour
{
    [SerializeField] float defaultDuration = 0.15f;
    [SerializeField] float defaultMagnitude = 0.2f;
    [SerializeField] float rotationMultiplier = 6f;

    float duration;
    float magnitude;
    float timer;

    Vector3 basePosition;
    Quaternion baseRotation;

    public void Shake() => Shake(defaultMagnitude, defaultDuration);

    public void Shake(float mag, float dur)
    {
        magnitude = Mathf.Max(0f, mag);
        duration = Mathf.Max(0.01f, dur);
        timer = duration;
    }

    void LateUpdate()
    {
        if (timer <= 0f) return;

        // TopDownCamera 已在本帧 LateUpdate 中写好基准位姿
        basePosition = transform.position;
        baseRotation = transform.rotation;

        timer -= Time.deltaTime;

        float fade = Mathf.Clamp01(timer / duration);   // 1 → 0
        float amp = magnitude * fade;

        // 只在相机平面的 X/Y 方向抖，不前后推拉，避免穿模/焦点变化
        Vector2 jitter = Random.insideUnitCircle * amp;
        Vector3 localOffset = new Vector3(jitter.x, jitter.y, 0f);
        transform.position = basePosition + baseRotation * localOffset;

        float rotAmp = amp * rotationMultiplier;
        transform.rotation = baseRotation * Quaternion.Euler(
            Random.Range(-rotAmp, rotAmp),
            Random.Range(-rotAmp, rotAmp),
            Random.Range(-rotAmp, rotAmp));

        if (timer <= 0f)
        {
            transform.position = basePosition;
            transform.rotation = baseRotation;
        }
    }
}
