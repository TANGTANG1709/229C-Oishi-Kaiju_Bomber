using UnityEngine;

/// <summary>
/// GunnerEnemy — stationary ground unit that fires projectiles upward at the Kaiju.
///
/// Physics topics used here:
///   C. Projectile Motion — bullet is launched at an angle toward the Kaiju
///      using computed vx/vy so it follows a ballistic arc.
///   D. Newton's Law of Universal Gravitation — bullet speed scales with
///      a simulated gravitational parameter to add gameplay variety.
/// </summary>
public class Gunner : MonoBehaviour
{
    [Header("Stats")]
    public int   hp             = 20;
    public float fireInterval   = 2.5f;
    public float bulletSpeed    = 5f;

    [Header("Projectile")]
    public GameObject bulletPrefab;

    [Header("Newton Gravity Scale (Topic D)")]
    [Tooltip("Simulated G factor — heavier planets shoot slower bullets")]
    public float gravitationalParam = 9.8f;   // G * M_planet (simplified)

    private Transform   kaijuTransform;
    private float       fireTimer;
    private bool        isDead = false;

    // event for GameManager / wave system
    public System.Action<Gunner> OnDied;

    void Start()
    {
        kaijuTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        fireTimer      = Random.Range(0f, fireInterval); // stagger initial shots
    }

    void Update()
    {
        if (isDead || kaijuTransform == null) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            fireTimer = fireInterval;
            FireAtKaiju();
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Topic C + D: compute launch velocity using projectile equations
    // and scale bullet speed via gravitational parameter.
    void FireAtKaiju()
    {
        if (bulletPrefab == null) return;

        Vector2 origin = transform.position;
        Vector2 target = kaijuTransform.position;
        Vector2 delta  = target - origin;

        // ── Topic D: Newton's Universal Gravitation ───────────────────────
        // We simulate: escape-like speed  v_eff = sqrt(G * M / r)
        // where r = distance to Kaiju, G*M = gravitationalParam.
        // This makes bullets slower when Kaiju is far, faster up close.
        float r       = Mathf.Max(delta.magnitude, 0.5f);
        float vEffect = Mathf.Sqrt(gravitationalParam / r);   // v ∝ 1/√r
        float speed   = Mathf.Clamp(bulletSpeed * vEffect, 2f, 14f);

        // ── Topic C: Projectile Motion ────────────────────────────────────
        // Decompose speed into vx, vy toward the target direction.
        // The bullet Rigidbody2D has gravity; Unity handles the arc.
        Vector2 dir      = delta.normalized;
        Vector2 velocity = dir * speed;

        GameObject bullet = Instantiate(bulletPrefab, origin + Vector2.up * 0.4f, Quaternion.identity);
        var rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = velocity;

        bullet.tag = "EnemyProjectile";
        Destroy(bullet, 4f);
    }

    // ── Topic A: Unity Physics 2D — damage via OnTriggerEnter2D ──────────
    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        if (hp <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        OnDied?.Invoke(this);
        Destroy(gameObject, 0.05f);
    }
}
