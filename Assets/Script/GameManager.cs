using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// GameManager — top-level game state: timer, score, HP bar, pause, game-over.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("References")]
    public KaijuController kaiju;

    [Header("UI")]
    public Slider      hpSlider;
    public TMP_Text    hpText;
    public TMP_Text    timerText;
    public TMP_Text    scoreText;
    public Button      pauseButton;
    public GameObject  gameOverPanel;
    public TMP_Text    gameOverScoreText;
    public GameObject  goldenTimeVFX;    // overlay/aura for golden time

    [Header("Timer")]
    public float gameDuration = 120f;   // 2 minutes

    // ── runtime ──────────────────────────────────────────────────────────
    private float   timeLeft;
    private int     score    = 0;
    private bool    paused   = false;
    private bool    gameOver = false;

    void Start()
    {
        timeLeft = gameDuration;

        UpdateHPUI(kaiju.CurrentHP);
        UpdateTimerUI();
        UpdateScoreUI();

        kaiju.OnHPChanged       += UpdateHPUI;
        kaiju.OnGoldenTimeStart += HandleGoldenTime;
        kaiju.OnDeath           += TriggerGameOver;

        pauseButton.onClick.AddListener(TogglePause);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (goldenTimeVFX) goldenTimeVFX.SetActive(false);
    }

    void Update()
    {
        if (paused || gameOver) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            TriggerGameOver();
        }
        UpdateTimerUI();
    }

    // ── Public ────────────────────────────────────────────────────────────
    public void AddScore(int pts)
    {
        score += pts;
        UpdateScoreUI();
    }

    // ── UI helpers ────────────────────────────────────────────────────────
    void UpdateHPUI(int hp)
    {
        if (hpSlider) { hpSlider.maxValue = kaiju.maxHP; hpSlider.value = hp; }
        if (hpText)   hpText.text = $"{hp} / {kaiju.maxHP}";
    }

    void UpdateTimerUI()
    {
        int m = Mathf.FloorToInt(timeLeft / 60);
        int s = Mathf.FloorToInt(timeLeft % 60);
        if (timerText) timerText.text = $"{m}:{s:D2}";
    }

    void UpdateScoreUI()
    {
        if (scoreText) scoreText.text = $"{score} pts";
    }

    void HandleGoldenTime()
    {
        if (goldenTimeVFX) goldenTimeVFX.SetActive(true);
        // Optionally disable golden VFX after some time via coroutine
        StartCoroutine(EndGoldenTimeVFX(15f));
    }

    System.Collections.IEnumerator EndGoldenTimeVFX(float dur)
    {
        yield return new WaitForSeconds(dur);
        if (goldenTimeVFX) goldenTimeVFX.SetActive(false);
        kaiju.ResetGoldenTime();
    }

    void TogglePause()
    {
        paused       = !paused;
        Time.timeScale = paused ? 0f : 1f;
        pauseButton.GetComponentInChildren<TMP_Text>().text = paused ? "▶ Resume" : "⏸ Pause";
    }

    void TriggerGameOver()
    {
        if (gameOver) return;
        gameOver       = true;
        Time.timeScale = 0f;
        if (gameOverPanel)    gameOverPanel.SetActive(true);
        if (gameOverScoreText) gameOverScoreText.text = $"Score: {score}";
    }

    // Called by Restart button in GameOver panel
    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
