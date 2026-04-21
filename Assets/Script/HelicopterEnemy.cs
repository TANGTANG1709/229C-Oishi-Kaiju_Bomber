using UnityEngine;

/// <summary>
/// HelicopterEnemy — airborne unit that hovers near the Kaiju and fires straight missiles.
///
/// Physics topics used here:
///   F. Rotational Motion / Magnus Effect — missile spins on its axis (angular velocity)
///      and Vector3.Cross is used to add a small lateral Magnus deflection force,
///      making missiles slightly curve toward the Kaiju.
///   E. Air Resistance — helicopter body experiences drag F = -k*v keeping it from
///      drifting too far from its patrol position.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class HelicopterEnemy : MonoBehaviour
{
    [Header("Stats")]
    public int   hp             = 30;
    public float patrolSpeed    = 2.5f;
    public float fireInterval   = 3f;
    public float hoverDistance  = 3f;   // preferred distance from Kaiju

    [Header("Missile")]
    public GameObject missilePrefab;
    public float missileSpeed = 7f;

    [Header("Air Resistance (Topic E)")]
    public float dragK = 1.2f;

    private Rigidbody2D rb;
    private Transform   kaijuTransform;
    private float       fireTimer;
    private bool        isDead = false;

    public System.Action<HelicopterEnemy> OnDied;

    void Start()
    {
        rb             = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        kaijuTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        fireTimer      = Random.Range(0f, fireInterval);
    }

    void Update()
    {
        if (isDead || kaijuTransform == null) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f) { fireTimer = fireInterval; FireMissile(); }
    }

    void FixedUpdate()
    {
        if (isDead || kaijuTransform == null) return;

        // Patrol: keep hoverDistance away from Kaiju on the side
        Vector2 toKaiju = (Vector2)kaijuTransform.position - (Vector2)transform.position;
        float   dist    = toKaiju.magnitude;
        Vector2 desired = dist > hoverDistance
            ? toKaiju.normalized * patrolSpeed
            : -toKaiju.normalized * patrolSpeed * 0.5f;

        rb.linearVelocity = desired;

        // ── Topic E: Air Resistance on helicopter body ────────────────────
        Vector2 drag = -dragK * rb.linearVelocity;
        rb.AddForce(drag);  // F_drag = -k * v
    }

    // ─────────────────────────────────────────────────────────────────────
    void FireMissile()
    {
        if (missilePrefab == null) return;

        Vector3 dir3D = (kaijuTransform.position - transform.position).normalized;
        GameObject missile = Instantiate(missilePrefab, transform.position, Quaternion.identity);
        missile.tag = "EnemyProjectile";

        var m = missile.GetComponent<MagnasMissile>();
        if (m != null)
        {
            m.direction = dir3D;
            m.speed     = missileSpeed;
        }

        Destroy(missile, 5f);
    }

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

// ── Attached to the missile prefab ───────────────────────────────────────────
/// <summary>
/// MagnasMissile — straight-firing missile with Magnus Effect spin deflection.
///
/// Topic F: Rotational Motion / Magnus Effect
///   The missile rotates (angularVelocity ω).
///   Magnus lift: F_magnus = k_m * (ω_vec × v_vec)
///   Computed via Vector3.Cross, then applied as a 2D lateral force,
///   causing a slight homing curve effect.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MagnasMissile : MonoBehaviour
{
    [HideInInspector] public Vector3 direction;
    [HideInInspector] public float   speed = 7f;
    public int damage = 15;

    [Header("Magnus Effect (Topic F)")]
    public float angularVelocityDeg = 360f;  // spin rate (°/s)
    public float magnusCoeff        = 0.4f;  // k_m

    private Rigidbody2D rb;
    private float       spinAngle = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(direction.x, direction.y) * speed;
    }

    void FixedUpdate()
    {
        // ── Topic F: Rotational Motion ────────────────────────────────────
        // ω increases spin angle
        spinAngle += angularVelocityDeg * Time.fixedDeltaTime;

        // ω as a 3-D vector (rotation about Z axis → out of screen)
        float omegaRad = angularVelocityDeg * Mathf.Deg2Rad;
        Vector3 omega  = new Vector3(0f, 0f, omegaRad);

        // Current velocity in 3D
        Vector3 vel3   = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, 0f);

        // F_magnus = k_m * (ω × v)  — Vector3.Cross gives lateral force
        Vector3 magnus = magnusCoeff * Vector3.Cross(omega, vel3);

        // Apply as 2D force  (F = ma → AddForce handles /m internally)
        rb.AddForce(new Vector2(magnus.x, magnus.y));

        // Rotate sprite to match flight direction (visual only)
        float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<KaijuController>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
