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
    [Tooltip("波次持续时间")]
    public float waveTimeLimit = 60f;
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

    void OnEnable()
    {
        EventBus<OnUnitDied>.Subscribe(OnEnemyDied);
        EventBus<OnEnemyRemoved>.Subscribe(OnEnemyExpired);
    }

    void OnDisable()
    {
        EventBus<OnUnitDied>.Unsubscribe(OnEnemyDied);
        EventBus<OnEnemyRemoved>.Unsubscribe(OnEnemyExpired);
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
        baseEnemyCount = baseEnemyCount * enemyGrowth;
        yield return StartCoroutine(SpawnEnemy(baseEnemyCount));
        float timer = 0;
        while (aliveEnemies.Count > 0 && timer < waveTimeLimit)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        KillAllRemaining();   // 超时清场（全打死时列表已经空了，这步无操作）

        EventBus<OnWaveFinished>.Publish(new OnWaveFinished { waveNumber = wave });
    }

    IEnumerator SpawnEnemy(int EnemyCount)
    {
        int count = EnemyCount;
        Transform player = PlayerLocator.Find();

        do
        {
            if (aliveEnemies.Count < maxCount)
            {
                Enemy enemy = enemyPool.Get();
                enemy.ReSpawn(GetSpawnPosition(), player);
                aliveEnemies.Add(enemy);
                count--;
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


            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, spawnRadius, NavMesh.AllAreas))
            {
                Debug.Log($"候选={candidate}  吸附={hit.position}  可生成={IsSpawnable(hit.position)}");
                if (IsOutsideCamera(hit.position))
                {
                    if(IsSpawnable(hit.position))
                    {
                        return hit.position;
                    }   
                }
            }
                
        }
        return Candidate.position;   // 兜底（理论上到不了这里）
    }


    bool IsSpawnable(Vector3 point)
    {
        Ray ray = new Ray(point + Vector3.up * 100f, Vector3.down);
        return !Physics.Raycast(ray, 200f, obstacleMask);
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
            enemy.GetComponent<Health>().TakeDamage(99999f);
        // aliveEnemies 由 OnEnemyDied handler 逐个移除，无需手动 Clear
    }

    // ─── EventBus handlers ───

    bool OnEnemyDied(OnUnitDied e)
    {
        var enemy = e.victim.GetComponent<Enemy>();
        if (enemy == null) return false;
        aliveEnemies.Remove(enemy);
        return false;
    }

    bool OnEnemyExpired(OnEnemyRemoved e)
    {
        var enemy = e.enemy.GetComponent<Enemy>();
        if (enemy == null) return false;
        enemyPool.Return(enemy);
        return false;
    }
}
