using System.Collections;
using UnityEngine;

/// <summary>
/// WaveManager — spawns Gunners and Helicopters in escalating waves.
/// Wave size grows gradually but stays moderate.
/// </summary>
public class WaveManager : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject gunnerPrefab;
    public GameObject helicopterPrefab;

    [Header("Spawn Positions")]
    public Transform[] groundSpawnPoints;   // for Gunners
    public Transform[] airSpawnPoints;      // for Helicopters

    [Header("Wave Settings")]
    public float firstWaveDelay  = 5f;
    public float waveCooldown    = 12f;
    [Tooltip("Additional enemies added each wave (capped at maxPerWave)")]
    public int   escalationStep  = 1;
    public int   maxPerWave      = 6;

    private GameManager gameManager;
    private int         waveNumber = 0;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        StartCoroutine(WaveLoop());
    }

    IEnumerator WaveLoop()
    {
        yield return new WaitForSeconds(firstWaveDelay);

        while (true)
        {
            waveNumber++;
            int total    = Mathf.Min(waveNumber * escalationStep + 1, maxPerWave);
            int gunners  = Mathf.CeilToInt(total * 0.6f);
            int helis    = total - gunners;

            SpawnWave(gunners, helis);
            yield return new WaitForSeconds(waveCooldown);
        }
    }

    void SpawnWave(int gunnerCount, int heliCount)
    {
        for (int i = 0; i < gunnerCount; i++)
        {
            var sp = groundSpawnPoints[Random.Range(0, groundSpawnPoints.Length)];
            var go = Instantiate(gunnerPrefab, sp.position, Quaternion.identity);
            RegisterEnemy(go);
        }
        for (int i = 0; i < heliCount; i++)
        {
            var sp = airSpawnPoints[Random.Range(0, airSpawnPoints.Length)];
            var go = Instantiate(helicopterPrefab, sp.position, Quaternion.identity);
            RegisterEnemy(go);
        }
    }

    void RegisterEnemy(GameObject go)
    {
        var g = go.GetComponent<Gunner>();
        if (g != null) g.OnDied += _ => OnEnemyKilled();

        var h = go.GetComponent<HelicopterEnemy>();
        if (h != null) h.OnDied += _ => OnEnemyKilled();
    }

    void OnEnemyKilled()
    {
        gameManager?.GetComponent<KaijuController>()?.RegisterKill();
    }
}
