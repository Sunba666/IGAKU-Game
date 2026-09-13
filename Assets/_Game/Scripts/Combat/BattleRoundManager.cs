using System.Collections;
using TMPro;
using UnityEngine;

public class BattleRoundManager : MonoBehaviour
{
    private enum RoundResult
    {
        None,
        Player1,
        Player2,
        Draw
    }

    [Header("玩家")]
    [SerializeField] private FighterHealth player1Health;
    [SerializeField] private FighterHealth player2Health;

    [Header("出生点")]
    [SerializeField] private Transform player1Spawn;
    [SerializeField] private Transform player2Spawn;

    [Header("HUD")]
    [SerializeField] private BattleHUDController battleHUD;
    [SerializeField] private TMP_Text centerMessageText;
    [SerializeField] private TMP_Text player1RoundText;
    [SerializeField] private TMP_Text player2RoundText;

    [Header("比赛规则")]
    [SerializeField] private int roundsToWin = 2;

    [Header("显示时间")]
    [SerializeField] private float readyDuration = 1f;
    [SerializeField] private float fightMessageDuration = 0.6f;
    [SerializeField] private float endTitleDuration = 0.7f;
    [SerializeField] private float roundResultDuration = 2f;

    private LocalFighterController2D player1Controller;
    private LocalFighterController2D player2Controller;

    private Rigidbody2D player1Body;
    private Rigidbody2D player2Body;

    private int player1Wins;
    private int player2Wins;

    private bool matchFinished;
    private Coroutine matchRoutine;

    private void Awake()
    {
        CachePlayerComponents();
    }

    public void ConfigurePlayers(
    FighterHealth newPlayer1Health,
    FighterHealth newPlayer2Health
)
    {
        player1Health = newPlayer1Health;
        player2Health = newPlayer2Health;

        CachePlayerComponents();
    }

    private void CachePlayerComponents()
    {
        if (player1Health != null)
        {
            player1Controller =
                player1Health.GetComponent<LocalFighterController2D>();

            player1Body =
                player1Health.GetComponent<Rigidbody2D>();
        }
        else
        {
            player1Controller = null;
            player1Body = null;
        }

        if (player2Health != null)
        {
            player2Controller =
                player2Health.GetComponent<LocalFighterController2D>();

            player2Body =
                player2Health.GetComponent<Rigidbody2D>();
        }
        else
        {
            player2Controller = null;
            player2Body = null;
        }
    }
    private void Start()
    {
        StartNewMatch();
    }

    private void Update()
    {
        if (matchFinished && Input.GetKeyDown(KeyCode.R))
        {
            StartNewMatch();
        }
    }

    public void StartNewMatch()
    {
        if (matchRoutine != null)
        {
            StopCoroutine(matchRoutine);
        }

        player1Wins = 0;
        player2Wins = 0;
        matchFinished = false;

        UpdateRoundScore();

        matchRoutine = StartCoroutine(MatchRoutine());
    }

    private IEnumerator MatchRoutine()
    {
        if (!ReferencesAreValid())
        {
            yield break;
        }

        while (
            player1Wins < roundsToWin &&
            player2Wins < roundsToWin
        )
        {
            yield return StartCoroutine(PrepareRound());

            bool endedByTime = false;

            while (
                !player1Health.IsKO &&
                !player2Health.IsKO &&
                !battleHUD.IsTimeUp
            )
            {
                yield return null;
            }

            if (
                battleHUD.IsTimeUp &&
                !player1Health.IsKO &&
                !player2Health.IsKO
            )
            {
                endedByTime = true;
            }

            SetPlayerControls(false);
            battleHUD.StopTimer();

            RoundResult result = DetermineRoundResult();

            yield return StartCoroutine(
                ShowRoundResult(result, endedByTime)
            );
        }

        ShowMatchResult();
        matchRoutine = null;
    }

    private IEnumerator PrepareRound()
    {
        SetCenterMessage("");

        ResetFighter(
            player1Health,
            player1Controller,
            player1Body,
            player1Spawn,
            true
        );

        ResetFighter(
            player2Health,
            player2Controller,
            player2Body,
            player2Spawn,
            false
        );

        SetPlayerControls(false);

        battleHUD.ResetHUD();
        battleHUD.StopTimer();

        SetCenterMessage("READY");

        yield return new WaitForSeconds(readyDuration);

        SetCenterMessage("FIGHT!");

        SetPlayerControls(true);
        battleHUD.StartTimer();

        yield return new WaitForSeconds(fightMessageDuration);

        SetCenterMessage("");
    }

    private void ResetFighter(
        FighterHealth health,
        LocalFighterController2D controller,
        Rigidbody2D body,
        Transform spawn,
        bool faceRight
    )
    {
        if (health == null || spawn == null)
        {
            return;
        }

        Transform fighterTransform = health.transform;

        fighterTransform.position = spawn.position;
        fighterTransform.rotation = Quaternion.identity;

        if (body != null)
        {
#if UNITY_6000_0_OR_NEWER
            body.linearVelocity = Vector2.zero;
#else
            body.velocity = Vector2.zero;
#endif

            body.angularVelocity = 0f;
            body.simulated = true;
        }

        health.ResetHealth();

        if (controller != null)
        {
            controller.enabled = true;
            controller.ResetForRound(faceRight);
            controller.enabled = false;
        }
    }

    private RoundResult DetermineRoundResult()
    {
        if (player1Health.IsKO && player2Health.IsKO)
        {
            return RoundResult.Draw;
        }

        if (player1Health.IsKO)
        {
            return RoundResult.Player2;
        }

        if (player2Health.IsKO)
        {
            return RoundResult.Player1;
        }

        if (
            player1Health.CurrentHealth >
            player2Health.CurrentHealth
        )
        {
            return RoundResult.Player1;
        }

        if (
            player2Health.CurrentHealth >
            player1Health.CurrentHealth
        )
        {
            return RoundResult.Player2;
        }

        return RoundResult.Draw;
    }

    private IEnumerator ShowRoundResult(
        RoundResult result,
        bool endedByTime
    )
    {
        if (endedByTime)
        {
            SetCenterMessage("TIME UP");
        }
        else
        {
            SetCenterMessage("K.O.");
        }

        yield return new WaitForSeconds(endTitleDuration);

        switch (result)
        {
            case RoundResult.Player1:
                player1Wins++;
                SetCenterMessage("PLAYER 1 WINS");
                break;

            case RoundResult.Player2:
                player2Wins++;
                SetCenterMessage("PLAYER 2 WINS");
                break;

            case RoundResult.Draw:
                SetCenterMessage("DRAW");
                break;
        }

        UpdateRoundScore();

        yield return new WaitForSeconds(roundResultDuration);
    }

    private void ShowMatchResult()
    {
        matchFinished = true;
        SetPlayerControls(false);
        battleHUD.StopTimer();

        if (player1Wins >= roundsToWin)
        {
            SetCenterMessage(
                "PLAYER 1 MATCH WIN\nPRESS R TO RESTART"
            );
        }
        else
        {
            SetCenterMessage(
                "PLAYER 2 MATCH WIN\nPRESS R TO RESTART"
            );
        }
    }

    private void SetPlayerControls(bool enabled)
    {
        if (player1Controller != null)
        {
            player1Controller.enabled = enabled;
        }

        if (player2Controller != null)
        {
            player2Controller.enabled = enabled;
        }
    }

    private void SetCenterMessage(string message)
    {
        if (centerMessageText != null)
        {
            centerMessageText.text = message;
        }
    }

    private void UpdateRoundScore()
    {
        if (player1RoundText != null)
        {
            player1RoundText.text =
                "P1 WINS: " + player1Wins;
        }

        if (player2RoundText != null)
        {
            player2RoundText.text =
                "P2 WINS: " + player2Wins;
        }
    }

    private bool ReferencesAreValid()
    {
        bool valid = true;

        if (player1Health == null)
        {
            Debug.LogError(
                "BattleRoundManager 没有连接 Player1Health。"
            );

            valid = false;
        }

        if (player2Health == null)
        {
            Debug.LogError(
                "BattleRoundManager 没有连接 Player2Health。"
            );

            valid = false;
        }

        if (player1Spawn == null)
        {
            Debug.LogError(
                "BattleRoundManager 没有连接 Player1Spawn。"
            );

            valid = false;
        }

        if (player2Spawn == null)
        {
            Debug.LogError(
                "BattleRoundManager 没有连接 Player2Spawn。"
            );

            valid = false;
        }

        if (battleHUD == null)
        {
            Debug.LogError(
                "BattleRoundManager 没有连接 BattleHUD。"
            );

            valid = false;
        }

        return valid;
    }
}