using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WaveManager : MonoBehaviour
{
    public GameObject enemyPrefab;      // 敌人预制体
    public Transform player;            // 玩家（生成中心）
    public float spawnRadius = 40f;     // 生成范围半径
    public float waveInterval = 5f;    // 波次间隔
    public int baseEnemyCount = 5;      // 基础数量
    public int enemyGrowth = 2;         // 每波增长（与波次成正比）
    public int maxCount = 40;

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

    public LayerMask obstacleMask;
    Vector3 GetSpawnPosition()
    {
        for (int i = 0; i < 30; i++)   // 最多试 30 次
        {
            Vector2 random = Random.insideUnitCircle * spawnRadius;
            Vector3 candidate = player.position + new Vector3(random.x, 0, random.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, spawnRadius, NavMesh.AllAreas)
                && IsOutsideCamera(hit.position)
                && !Physics.CheckSphere(hit.position, 1f, obstacleMask))
                return hit.position;
        }
        return player.position;   // 兜底（理论上到不了这里）
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
