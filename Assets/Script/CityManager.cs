using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CityManager — tracks buildings and resets the city when all are destroyed.
/// Gives Kaiju +10 HP on full city reset.
/// </summary>
public class CityManager : MonoBehaviour
{
    [Header("Prefabs / Spawn Points")]
    public GameObject[]  buildingPrefabs;
    public Transform[]   buildingSpawnPoints;

    private List<Building> activeBuildings = new List<Building>();
    private KaijuController kaiju;
    private GameManager     gameManager;

    void Start()
    {
        kaiju       = FindFirstObjectByType<KaijuController>();
        gameManager = FindFirstObjectByType<GameManager>();
        SpawnAllBuildings();
    }

    void SpawnAllBuildings()
    {
        activeBuildings.Clear();
        foreach (var sp in buildingSpawnPoints)
        {
            var prefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
            var go     = Instantiate(prefab, sp.position, sp.rotation);
            var b      = go.GetComponent<Building>();
            if (b != null)
            {
                b.cityManager = this;
                activeBuildings.Add(b);
            }
        }
    }

    public void OnBuildingDestroyed(Building b, int pts)
    {
        activeBuildings.Remove(b);
        gameManager?.AddScore(pts);

        if (activeBuildings.Count == 0)
            ResetCity();
    }

    void ResetCity()
    {
        Debug.Log("[City] Fully destroyed — resetting!");
        kaiju?.HealHP(10);               // +10 HP on full reset
        SpawnAllBuildings();
    }
}
