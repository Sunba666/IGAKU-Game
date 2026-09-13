using UnityEngine;

[DefaultExecutionOrder(-500)]
[DisallowMultipleComponent]
public sealed class BattleHitStop : MonoBehaviour
{
    private static BattleHitStop instance;

    private bool hitStopActive;
    private float hitStopEndTime;
    private float timeScaleBeforeHitStop = 1f;

    public static bool IsActive =>
        instance != null &&
        instance.hitStopActive;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning(
                "场景中存在多个 BattleHitStop，" +
                "请只保留一个。",
                this
            );

            enabled = false;
            return;
        }

        instance = this;
    }

    private void Update()
    {
        if (!hitStopActive)
        {
            return;
        }

        if (Time.unscaledTime < hitStopEndTime)
        {
            return;
        }

        RestoreTimeScale();
    }

    public static void Request(float duration)
    {
        if (duration <= 0f)
        {
            return;
        }

        if (instance == null)
        {
            Debug.LogWarning(
                "场景中没有 BattleHitStop，" +
                "无法播放命中停顿。"
            );

            return;
        }

        instance.BeginOrExtendHitStop(duration);
    }

    private void BeginOrExtendHitStop(float duration)
    {
        float requestedEndTime =
            Time.unscaledTime + duration;

        if (hitStopActive)
        {
            hitStopEndTime = Mathf.Max(
                hitStopEndTime,
                requestedEndTime
            );

            return;
        }

        // 如果游戏已经被其他暂停系统暂停，
        // 不要擅自覆盖那个系统的暂停状态。
        if (Time.timeScale <= 0f)
        {
            return;
        }

        timeScaleBeforeHitStop =
            Time.timeScale;

        hitStopEndTime =
            requestedEndTime;

        hitStopActive = true;

        Time.timeScale = 0f;
    }

    private void RestoreTimeScale()
    {
        if (!hitStopActive)
        {
            return;
        }

        hitStopActive = false;
        hitStopEndTime = 0f;

        Time.timeScale =
            timeScaleBeforeHitStop;
    }

    private void OnDisable()
    {
        if (instance != this)
        {
            return;
        }

        RestoreTimeScale();
        instance = null;
    }
}