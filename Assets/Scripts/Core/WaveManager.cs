using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WaveManager : MonoBehaviour
{
    public GameObject enemyPrefab;      // 敌人预制体
    [Header("生成")]
    [Tooltip("生成中心（默认为玩家）")]
    public Transform Candidate;            // 玩家（生成中心）
    [Tooltip("生成范围半径")]
    public float spawnRadius = 40f;     // 生成范围半径
    [Tooltip("波次间隔")]
    public float waveInterval = 5f;    // 波次间隔
    [Tooltip("基础数量")]
    public int baseEnemyCount = 5;      // 基础数量
    [Tooltip("成长倍数")]
    public int enemyGrowth = 2;         // 每波增长（与波次成正比）
    [Tooltip("上限")]
    public int maxCount = 40;

    [Header("检测")]
    [Tooltip("山体/洞穴层")]
    public LayerMask obstacleMask;   // 山体/洞穴层
    [Tooltip("地面层")]
    public LayerMask terrainMask;    // 地面层

    int waveNumber;
    List<Enemy> aliveEnemies = new();
    ObjectPool<Enemy> enemyPool;

    void Awake()
    {
        Transform parent = new GameObject("Enemies").transform;
        enemyPool = new ObjectPool<Enemy>(enemyPrefab.GetComponent<Enemy>(), baseEnemyCount, parent);
    }

    void Start() => StartCoroutine(WaveLoop());

    IEnumerator WaveLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(waveInterval);   // 波次间隔 5s
            yield return StartCoroutine(RunWave(++waveNumber));
        }
    }

    IEnumerator RunWave(int wave)
    {
        // int hascount = EnemyCount > maxCount ? maxCount
        // for (int i = 0; i < count; i++) SpawnEnemy();
        baseEnemyCount = baseEnemyCount * enemyGrowth;
        yield return StartCoroutine(SpawnEnemy(baseEnemyCount));
        float timer = 0;
        float waveTimeLimit = wave * 15f;
        while (aliveEnemies.Count > 0 && timer < waveTimeLimit)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        KillAllRemaining();   // 超时清场（全打死时列表已经空了，这步无操作）
    }

    IEnumerator SpawnEnemy(int EnemyCount)
    {
        int count = EnemyCount;
        do
        {
            if (aliveEnemies.Count < maxCount)
            {

                Enemy enemy = enemyPool.Get();
                enemy.GetComponent<EnemyDeath>().OnExpire = () => enemyPool.Return(enemy);
                enemy.ReSpawn(GetSpawnPosition());
                aliveEnemies.Add(enemy);
                count--;
                System.Action onDeath = null;
                onDeath = () => { aliveEnemies.Remove(enemy); enemy.GetComponent<Health>().OnDeath -= onDeath; };   // 自退订，防止复用时累积
                enemy.GetComponent<Health>().OnDeath += onDeath;
            }
            else
            {
                yield return null;
            }
        } while (count > 0);
    }

    Vector3 GetSpawnPosition()
    {
        for (int i = 0; i < 30; i++)   // 最多试 30 次
        {
            Vector2 random = Random.insideUnitCircle * spawnRadius;
            Vector3 candidate = Candidate.position + new Vector3(random.x, 0, random.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, spawnRadius, NavMesh.AllAreas)
                && IsOutsideCamera(hit.position)
                && IsSpawnable(hit.position))
            {
                Debug.Log($"候选={candidate}  吸附={hit.position}  可生成={IsSpawnable(hit.position)}");
                return hit.position;
            }
                
        }
        return Candidate.position;   // 兜底（理论上到不了这里）
    }


    bool IsSpawnable(Vector3 point)
    {
        Vector3 origin = point + Vector3.up * 100f;   // 从上方 100m 往下
        float remaining = 200f;
        int crossings = 0;

        while (remaining > 0)
        {
            // 打所有层（要同时检测地面和山体）
            if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, remaining))
                break;

            int layer = hit.collider.gameObject.layer;

            // 打中地面 → 停止，判断奇偶
            if ((terrainMask.value & (1 << layer)) != 0)
                return crossings % 2 == 0;   // 偶数 = 山体外 = 可生成

            // 打中山体 → 穿过一次
            if ((obstacleMask.value & (1 << layer)) != 0)
                crossings++;

            // 从命中点再往下挪一点，继续打下一层
            origin = hit.point + Vector3.down * 0.01f;
            remaining -= hit.distance + 0.01f;
        }

        return false;   // 没打中地面，异常
    }

    bool IsOutsideCamera(Vector3 worldPos)
    {
        Vector3 vp = Camera.main.WorldToViewportPoint(worldPos);
        float margin = 0.1f;
        return vp.z < 0 || vp.x < -margin || vp.x > 1 + margin
                        || vp.y < -margin || vp.y > 1 + margin;
    }

    void KillAllRemaining()
    {
        foreach (var enemy in aliveEnemies.ToArray())
            enemy.GetComponent<Health>().TakeDamage(99999f);   // 直接打死
        aliveEnemies.Clear();
    }
}
