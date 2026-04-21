using UnityEngine;

/// <summary>
/// Building — destructible city building.
/// Topic A: OnTriggerEnter2D with bomb's AOE collider deals damage.
/// </summary>
public class Building : MonoBehaviour
{
    public int hp = 30;
    public int pointsOnDestroy = 10;
    public GameObject rubblePrefab;

    // Reference set by CityManager
    [HideInInspector] public CityManager cityManager;

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        if (hp <= 0) Collapse();
    }

    void Collapse()
    {
        if (rubblePrefab)
            Instantiate(rubblePrefab, transform.position, Quaternion.identity);

        cityManager?.OnBuildingDestroyed(this, pointsOnDestroy);
        Destroy(gameObject);
    }
}
