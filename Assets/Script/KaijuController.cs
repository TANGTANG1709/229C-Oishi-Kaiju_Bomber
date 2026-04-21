using UnityEngine;

/// <summary>
/// KaijuController — controls the flying Kaiju (player).
///
/// Physics topics used here:
///   A. Unity Physics 2D — OnTriggerEnter2D detects bomb pickup / damage zones
///   C. Projectile Motion — bombs are launched with initial velocity + gravity
///   E. Air Resistance — drag force F_drag = -k * v applied per frame to the Kaiju
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class KaijuController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed      = 6f;
    public float goldenMoveSpeed = 10f;   // faster in golden time

    [Header("Air Resistance (Topic E)")]
    [Tooltip("Drag coefficient k in F_drag = -k * v")]
    public float dragCoefficient = 0.8f;  // tunable per-project

    [Header("Bomb")]
    public GameObject bombPrefab;
    public float bombCooldown       = 1.0f;
    public float goldenBombCooldown = 0.1f;

    [Header("HP")]
    public int maxHP = 100;

    // ── runtime ──────────────────────────────────────────────────────────
    private Rigidbody2D  rb;
    private float        bombTimer   = 0f;
    private bool         goldenTime  = false;
    private int          currentHP;
    private int          killCount   = 0;          // for golden time trigger
    private const int    GOLDEN_KILLS = 10;
    private float moveInput;

    public int   CurrentHP  => currentHP;
    public bool  GoldenTime => goldenTime;
    public int   KillCount  => killCount;

    // ── events ────────────────────────────────────────────────────────────
    public System.Action<int> OnHPChanged;
    public System.Action      OnGoldenTimeStart;
    public System.Action      OnDeath;
    SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Awake()
    {
        rb        = GetComponent<Rigidbody2D>();
        currentHP = maxHP;

        // Physics 2D setup — gravity should be OFF for a flying Kaiju
        rb.gravityScale = 0f;
    }

    void Update()
    {
        moveInput = Input.GetAxis("Horizontal");

        if (moveInput != 0)
        {
            sr.flipX = (moveInput < 0);
        }
        if (bombTimer > 0f)
            bombTimer -= Time.deltaTime;


        HandleInput();
    }

    void FixedUpdate()
    {
        // ── Topic E: Air Resistance ───────────────────────────────────────
        // F_drag = -k * v   (opposes velocity, proportional to speed)
        // F = ma  →  a = F/m  →  apply via AddForce
        Vector2 dragForce = -dragCoefficient * rb.linearVelocity;
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        rb.AddForce(dragForce);   // F = ma applied through Rigidbody2D
    }

    // ─────────────────────────────────────────────────────────────────────
    void HandleInput()
    {
        float speed = goldenTime ? goldenMoveSpeed : moveSpeed;

        // Horizontal / vertical movement
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(h, v).normalized;
        rb.linearVelocity = dir * speed;   // direct velocity control (drag will dampen)

        // Drop bomb on left-click
        if (Input.GetMouseButtonDown(0))
            TryDropBomb();
    }

    void TryDropBomb()
    {
        float cd = goldenTime ? goldenBombCooldown : bombCooldown;
        if (bombTimer > 0f) return;

        bombTimer = cd;
        // Instantiate bomb at Kaiju position, slightly below
        Vector3 spawnPos = transform.position + Vector3.down * 0.3f;
        Instantiate(bombPrefab, spawnPos, Quaternion.identity);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Topic A: Unity Physics 2D — OnTriggerEnter2D
    // Projectiles / missiles enter a trigger collider on the Kaiju
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyProjectile"))
        {
            TakeDamage(other.GetComponent<ProjectileBase>()?.damage ?? 10);
            Destroy(other.gameObject);
        }
    }

    public void TakeDamage(int dmg)
    {
        currentHP = Mathf.Max(0, currentHP - dmg);
        OnHPChanged?.Invoke(currentHP);
        if (currentHP == 0) OnDeath?.Invoke();
    }

    public void HealHP(int amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        OnHPChanged?.Invoke(currentHP);
    }

    // Called by GameManager when an enemy is killed
    public void RegisterKill()
    {
        killCount++;
        if (!goldenTime && killCount >= GOLDEN_KILLS)
            ActivateGoldenTime();
    }

    void ActivateGoldenTime()
    {
        goldenTime = true;
        HealHP(30);
        OnGoldenTimeStart?.Invoke();
        Debug.Log("[Kaiju] GOLDEN TIME activated!");
    }

    // GameManager resets this after golden time window ends (optional)
    public void ResetGoldenTime() { goldenTime = false; killCount = 0; }
}
