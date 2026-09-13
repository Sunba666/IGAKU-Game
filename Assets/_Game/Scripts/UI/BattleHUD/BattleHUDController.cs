using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUDController : MonoBehaviour
{
    [Header("玩家生命值")]
    [SerializeField] private FighterHealth player1Health;
    [SerializeField] private FighterHealth player2Health;

    [Header("1P UI")]
    [SerializeField] private Image player1HealthFill;
    [SerializeField] private Image player1DamageFill;
    [SerializeField] private TMP_Text player1NameText;
    [SerializeField] private TMP_Text player1HealthText;

    [Header("2P UI")]
    [SerializeField] private Image player2HealthFill;
    [SerializeField] private Image player2DamageFill;
    [SerializeField] private TMP_Text player2NameText;
    [SerializeField] private TMP_Text player2HealthText;

    [Header("角色名字")]
    [SerializeField] private string player1Name = "MARCH 7TH";
    [SerializeField] private string player2Name = "TRAILBLAZER";

    [Header("延迟血条")]
    [SerializeField] private float damageBarCatchupSpeed = 0.6f;

    [Header("计时器")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private float timeLimit = 99f;
    [SerializeField] private bool startTimerAutomatically = true;

    private float remainingTime;
    private bool timerRunning;
    private bool timeUpMessageSent;

    public float RemainingTime => remainingTime;
    public bool IsTimeUp => remainingTime <= 0f;

    private void Start()
    {
        ResetHUD();
    }

    private void Update()
    {
        UpdateHealthBars();
        UpdateTimer();
    }

    public void ConfigurePlayers(
    FighterHealth newPlayer1Health,
    FighterHealth newPlayer2Health,
    string newPlayer1Name,
    string newPlayer2Name
)
    {
        player1Health = newPlayer1Health;
        player2Health = newPlayer2Health;

        if (!string.IsNullOrEmpty(newPlayer1Name))
        {
            player1Name = newPlayer1Name;
        }

        if (!string.IsNullOrEmpty(newPlayer2Name))
        {
            player2Name = newPlayer2Name;
        }

        if (player1NameText != null)
        {
            player1NameText.text = player1Name;
        }

        if (player2NameText != null)
        {
            player2NameText.text = player2Name;
        }

        SetHealthBarsImmediately();
    }

    public void ResetHUD()
    {
        remainingTime = Mathf.Max(0f, timeLimit);
        timerRunning = startTimerAutomatically;
        timeUpMessageSent = false;

        if (player1NameText != null)
        {
            player1NameText.text = player1Name;
        }

        if (player2NameText != null)
        {
            player2NameText.text = player2Name;
        }

        SetHealthBarsImmediately();
        UpdateTimerText();
    }

    public void StartTimer()
    {
        if (remainingTime > 0f)
        {
            timerRunning = true;
        }
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    private void UpdateHealthBars()
    {
        float player1Target = GetHealthRatio(player1Health);
        float player2Target = GetHealthRatio(player2Health);

        UpdateSingleHealthBar(
            player1Target,
            player1HealthFill,
            player1DamageFill
        );

        UpdateSingleHealthBar(
            player2Target,
            player2HealthFill,
            player2DamageFill
        );

        if (player1HealthText != null && player1Health != null)
        {
            player1HealthText.text =
                player1Health.CurrentHealth +
                " / " +
                player1Health.MaxHealth;
        }

        if (player2HealthText != null && player2Health != null)
        {
            player2HealthText.text =
                player2Health.CurrentHealth +
                " / " +
                player2Health.MaxHealth;
        }
    }

    private void UpdateSingleHealthBar(
        float target,
        Image healthFill,
        Image damageFill
    )
    {
        if (healthFill != null)
        {
            healthFill.fillAmount = target;
        }

        if (damageFill == null)
        {
            return;
        }

        if (target > damageFill.fillAmount)
        {
            damageFill.fillAmount = target;
        }
        else
        {
            damageFill.fillAmount = Mathf.MoveTowards(
                damageFill.fillAmount,
                target,
                damageBarCatchupSpeed * Time.deltaTime
            );
        }
    }

    private float GetHealthRatio(FighterHealth health)
    {
        if (health == null || health.MaxHealth <= 0)
        {
            return 0f;
        }

        return Mathf.Clamp01(
            (float)health.CurrentHealth / health.MaxHealth
        );
    }

    private void SetHealthBarsImmediately()
    {
        float player1Value = GetHealthRatio(player1Health);
        float player2Value = GetHealthRatio(player2Health);

        if (player1HealthFill != null)
        {
            player1HealthFill.fillAmount = player1Value;
        }

        if (player1DamageFill != null)
        {
            player1DamageFill.fillAmount = player1Value;
        }

        if (player2HealthFill != null)
        {
            player2HealthFill.fillAmount = player2Value;
        }

        if (player2DamageFill != null)
        {
            player2DamageFill.fillAmount = player2Value;
        }
    }

    private void UpdateTimer()
    {
        if (timerRunning && remainingTime > 0f)
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                timerRunning = false;

                if (!timeUpMessageSent)
                {
                    Debug.Log("TIME UP");
                    timeUpMessageSent = true;
                }
            }
        }

        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
        {
            return;
        }

        int displayTime = Mathf.CeilToInt(remainingTime);
        timerText.text = displayTime.ToString("00");
    }
}