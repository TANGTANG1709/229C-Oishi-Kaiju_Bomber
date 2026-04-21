using UnityEngine;

/// <summary>
/// BombProjectile — the Kaiju's shard/bomb.
///
/// Physics topics used here:
///   C. Projectile Motion — initial horizontal velocity + gravity gives parabolic arc
///      Position: x(t) = x0 + vx*t
///                y(t) = y0 + vy*t - 0.5*g*t²   (handled by Rigidbody2D gravity)
///   A. Unity Physics 2D — OnTriggerEnter2D for AOE detection
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class BombProjectile : MonoBehaviour
{
    [Header("Projectile (Topic C)")]
    [Tooltip("Horizontal speed inherited from Kaiju movement or set manually")]
    public float initialVX   = 0f;   // set by spawner from Kaiju velocity
    public float gravityScale = 1.5f; // feels heavier than normal objects

    [Header("AOE")]
    public float    aoeRadius = 1.8f;
    public int      damage    = 10;
    public LayerMask targetLayers;
    public GameObject explosionVFXPrefab;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;

        // ── Topic C: Projectile Motion ────────────────────────────────────
        // Give the bomb horizontal velocity so it follows a parabolic path.
        // Vertical component starts at 0; gravity handles the downward arc.
        // v_x is passed from KaijuController (matches Kaiju's horizontal speed).
        rb.linearVelocity = new Vector2(initialVX, 0f);

        // Destroy if it survives too long (off-screen)
        Destroy(gameObject, 6f);
    }

    // ── Topic A: Unity Physics 2D — OnTriggerEnter2D ─────────────────────
    // The bomb's CircleCollider2D is set as Trigger.
    // When it enters a Building or Enemy collider → explode.
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground") || other.CompareTag("Building") ||
            other.CompareTag("Gunner") || other.CompareTag("Helicopter"))
        {
            Explode();
        }
    }

    void Explode()
    {
        // Spawn VFX
        if (explosionVFXPrefab)
            Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);

        // AOE — OverlapCircle collects everything in radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aoeRadius, targetLayers);
        foreach (var hit in hits)
        {
            // Buildings
            var building = hit.GetComponent<Building>();
            building?.TakeDamage(damage);

            // Gunner
            var gunner = hit.GetComponent<Gunner>();
            gunner?.TakeDamage(damage);

            // Helicopter (bomb must be above or overlapping)
            var heli = hit.GetComponent<HelicopterEnemy>();
            heli?.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

    // Helper called by KaijuController when spawning
    public void SetHorizontalVelocity(float vx) { initialVX = vx; }
}
