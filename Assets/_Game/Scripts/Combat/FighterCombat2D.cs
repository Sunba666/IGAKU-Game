using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterCombat2D : MonoBehaviour
{
    public enum AttackPhase
    {
        None,
        Startup,
        Active,
        Recovery
    }

    [Header("攻击点")]
    [SerializeField]
    private Transform attackOrigin;

    [SerializeField]
    private LayerMask hurtboxLayer;

    [Header("轻攻击判定")]
    [SerializeField]
    private Vector2 lightAttackSize =
        new Vector2(1.5f, 1.2f);

    [SerializeField]
    private int lightAttackDamage = 25;

    [Header("轻攻击帧阶段")]
    [SerializeField, Min(0f)]
    private float lightAttackStartup = 0.10f;

    [SerializeField, Min(0.01f)]
    private float lightAttackActiveDuration = 0.10f;

    [SerializeField, Min(0f)]
    private float lightAttackRecovery = 0.23f;

    [Header("击退")]
    [SerializeField]
    private float knockbackX = 4f;

    [SerializeField]
    private float knockbackY = 2f;

    [Header("普通命中反馈")]
    [SerializeField, Range(0f, 0.2f)]
    private float hitStopDuration = 0.055f;

    [SerializeField, Range(0f, 0.5f)]
    private float cameraShakeDuration = 0.12f;

    [SerializeField, Range(0f, 0.5f)]
    private float cameraShakeStrength = 0.08f;

    [SerializeField]
    private Color hitSparkColor =
        new Color(1f, 0.35f, 0.75f, 1f);

    [Header("KO命中强化")]
    [SerializeField, Min(1f)]
    private float koFeedbackMultiplier = 1.8f;

    [Header("可选战斗音效")]
    [SerializeField]
    private AudioSource combatAudioSource;

    [SerializeField]
    private AudioClip attackSwingSound;

    [SerializeField]
    private AudioClip lightHitSound;

    [SerializeField]
    private AudioClip koHitSound;

    [SerializeField, Range(0f, 1f)]
    private float swingVolume = 0.8f;

    [SerializeField, Range(0f, 1f)]
    private float hitVolume = 1f;

    [Header("运行时调试")]
    [SerializeField]
    private AttackPhase currentPhase =
        AttackPhase.None;

    private FighterHealth ownerHealth;
    private TwoFighterCamera2D battleCamera;
    private Coroutine attackRoutine;

    public bool IsAttacking =>
        attackRoutine != null;

    public bool IsHitboxActive =>
        currentPhase == AttackPhase.Active;

    public AttackPhase CurrentPhase =>
        currentPhase;

    private void Awake()
    {
        ownerHealth =
            GetComponent<FighterHealth>();

        if (attackOrigin == null)
        {
            Transform visual =
                transform.Find("Visual");

            if (visual != null)
            {
                attackOrigin =
                    visual.Find("AttackOrigin");
            }
        }

        if (combatAudioSource == null)
        {
            combatAudioSource =
                GetComponent<AudioSource>();
        }

        ResolveBattleCamera();
    }

    public bool BeginLightAttack()
    {
        if (attackRoutine != null)
        {
            return false;
        }

        if (ownerHealth != null &&
            ownerHealth.IsKO)
        {
            return false;
        }

        PlaySound(
            attackSwingSound,
            swingVolume
        );

        attackRoutine =
            StartCoroutine(
                LightAttackRoutine()
            );

        return true;
    }

    public void CancelCurrentAttack()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        currentPhase =
            AttackPhase.None;
    }

    private IEnumerator LightAttackRoutine()
    {
        HashSet<FighterHealth> damagedTargets =
            new HashSet<FighterHealth>();

        // 第一阶段：前摇。
        // 角色已经播放攻击动画，
        // 但攻击判定还没有生效。
        currentPhase =
            AttackPhase.Startup;

        if (lightAttackStartup > 0f)
        {
            yield return new WaitForSeconds(
                lightAttackStartup
            );
        }

        // 第二阶段：有效帧。
        // 在这一段时间内，每帧检查 Hurtbox。
        currentPhase =
            AttackPhase.Active;

        float activeElapsedTime = 0f;

        while (activeElapsedTime <
               lightAttackActiveDuration)
        {
            PerformLightAttackCheck(
                damagedTargets
            );

            yield return null;

            activeElapsedTime +=
                Time.deltaTime;
        }

        // 第三阶段：后摇。
        // 攻击判定已经关闭，
        // 但角色还没有恢复行动。
        currentPhase =
            AttackPhase.Recovery;

        if (lightAttackRecovery > 0f)
        {
            yield return new WaitForSeconds(
                lightAttackRecovery
            );
        }

        currentPhase =
            AttackPhase.None;

        attackRoutine = null;
    }

    private void PerformLightAttackCheck(
        HashSet<FighterHealth> damagedTargets
    )
    {
        if (attackOrigin == null)
        {
            Debug.LogWarning(
                name +
                " 没有连接 AttackOrigin。"
            );

            return;
        }

        Collider2D[] hits =
            Physics2D.OverlapBoxAll(
                attackOrigin.position,
                lightAttackSize,
                0f,
                hurtboxLayer
            );

        foreach (Collider2D hit in hits)
        {
            FighterHealth target =
                hit.GetComponentInParent<
                    FighterHealth
                >();

            if (target == null)
            {
                continue;
            }

            if (target == ownerHealth)
            {
                continue;
            }

            // 整个攻击期间已经伤害过这个目标，
            // 后续有效帧不再重复扣血。
            if (damagedTargets.Contains(target))
            {
                continue;
            }

            float direction =
                target.transform.position.x >=
                transform.position.x
                    ? 1f
                    : -1f;

            Vector2 knockback =
                new Vector2(
                    direction * knockbackX,
                    knockbackY
                );

            bool attackConnected =
                target.TakeHit(
                    lightAttackDamage,
                    knockback
                );

            if (!attackConnected)
            {
                continue;
            }

            damagedTargets.Add(target);

            bool isKOHit =
                target.IsKO;

            Vector3 hitPosition =
                CalculateHitPosition(hit);

            PlayHitFeedback(
                hitPosition,
                isKOHit
            );
        }
    }

    private Vector3 CalculateHitPosition(
        Collider2D hitCollider
    )
    {
        if (hitCollider == null)
        {
            return attackOrigin.position;
        }

        Vector3 attackPosition =
            attackOrigin.position;

        Vector3 hitPosition;

        if (hitCollider.bounds.Contains(
            attackPosition
        ))
        {
            hitPosition =
                hitCollider.bounds.center;
        }
        else
        {
            hitPosition =
                hitCollider.ClosestPoint(
                    attackPosition
                );
        }

        hitPosition.z = 0f;

        return hitPosition;
    }

    private void PlayHitFeedback(
        Vector3 hitPosition,
        bool isKOHit
    )
    {
        float feedbackMultiplier =
            isKOHit
                ? koFeedbackMultiplier
                : 1f;

        BattleHitStop.Request(
            hitStopDuration *
            feedbackMultiplier
        );

        ResolveBattleCamera();

        if (battleCamera != null)
        {
            battleCamera.RequestShake(
                cameraShakeDuration *
                feedbackMultiplier,

                cameraShakeStrength *
                feedbackMultiplier
            );
        }

        HitSpark2D.Spawn(
            hitPosition,
            hitSparkColor,
            isKOHit
        );

        PlaySound(
            isKOHit
                ? koHitSound
                : lightHitSound,

            hitVolume
        );
    }

    private void ResolveBattleCamera()
    {
        if (battleCamera != null)
        {
            return;
        }

        Camera mainCamera =
            Camera.main;

        if (mainCamera == null)
        {
            return;
        }

        battleCamera =
            mainCamera.GetComponent<
                TwoFighterCamera2D
            >();
    }

    private void PlaySound(
        AudioClip clip,
        float volume
    )
    {
        if (combatAudioSource == null ||
            clip == null)
        {
            return;
        }

        combatAudioSource.PlayOneShot(
            clip,
            volume
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (attackOrigin == null)
        {
            return;
        }

        if (currentPhase ==
            AttackPhase.Active)
        {
            Gizmos.color =
                Color.red;
        }
        else if (currentPhase ==
                 AttackPhase.Startup)
        {
            Gizmos.color =
                Color.yellow;
        }
        else if (currentPhase ==
                 AttackPhase.Recovery)
        {
            Gizmos.color =
                Color.cyan;
        }
        else
        {
            Gizmos.color =
                new Color(
                    1f,
                    1f,
                    1f,
                    0.35f
                );
        }

        Gizmos.DrawWireCube(
            attackOrigin.position,
            lightAttackSize
        );
    }

    private void OnDisable()
    {
        CancelCurrentAttack();
    }

    private void OnValidate()
    {
        lightAttackStartup =
            Mathf.Max(
                0f,
                lightAttackStartup
            );

        lightAttackActiveDuration =
            Mathf.Max(
                0.01f,
                lightAttackActiveDuration
            );

        lightAttackRecovery =
            Mathf.Max(
                0f,
                lightAttackRecovery
            );

        koFeedbackMultiplier =
            Mathf.Max(
                1f,
                koFeedbackMultiplier
            );
    }
}