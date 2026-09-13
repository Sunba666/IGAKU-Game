using System.Collections;
using UnityEngine;

public class FighterHealth : MonoBehaviour
{
    [Header("生命值")]
    [SerializeField]
    private int maxHealth = 100;

    [Header("受击")]
    [SerializeField]
    private float invincibleTime = 0.12f;

    [SerializeField]
    private float hitStunTime = 0.25f;

    [Header("对象引用")]
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private Rigidbody2D body;

    [Header("受击闪色")]
    [SerializeField]
    private SpriteRenderer[] flashRenderers;

    [SerializeField]
    private Color hitFlashColor =
        new Color(1f, 0.25f, 0.25f, 1f);

    [SerializeField, Min(0.01f)]
    private float hitFlashDuration = 0.09f;

    [SerializeField, Min(0.01f)]
    private float koFlashDuration = 0.18f;

    private LocalFighterController2D controller;
    private FighterCombat2D combat;
    private Coroutine restoreControlRoutine;
    private Coroutine hitFlashRoutine;

    private Color[] originalRendererColors;

    private float invincibleTimer;
    private int currentHealth;

    private static readonly int HitHash =
        Animator.StringToHash("Hit");

    private static readonly int KOHash =
        Animator.StringToHash("KO");

    public int CurrentHealth =>
        currentHealth;

    public int MaxHealth =>
        maxHealth;

    public bool IsKO =>
        currentHealth <= 0;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (animator == null)
        {
            animator =
                GetComponentInChildren<Animator>();
        }

        if (body == null)
        {
            body =
                GetComponent<Rigidbody2D>();
        }

        controller =
            GetComponent<LocalFighterController2D>();

        combat =
            GetComponent<FighterCombat2D>();

        CacheFlashRenderers();

        if (animator != null)
        {
            animator.SetBool(
                KOHash,
                false
            );
        }
    }

    private void Update()
    {
        if (invincibleTimer > 0f)
        {
            invincibleTimer -=
                Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (!IsKO || body == null)
        {
            return;
        }

#if UNITY_6000_0_OR_NEWER
        Vector2 currentVelocity =
            body.linearVelocity;
#else
        Vector2 currentVelocity =
            body.velocity;
#endif

        currentVelocity.x = 0f;

        if (currentVelocity.y > 0f)
        {
            currentVelocity.y = 0f;
        }

#if UNITY_6000_0_OR_NEWER
        body.linearVelocity =
            currentVelocity;
#else
        body.velocity =
            currentVelocity;
#endif

        body.angularVelocity = 0f;
    }

    public bool TakeHit(
        int damage,
        Vector2 knockback
    )
    {
        if (IsKO || invincibleTimer > 0f)
        {
            return false;
        }

        if (combat != null)
        {
            combat.CancelCurrentAttack();
        }

        damage =
            Mathf.Max(0, damage);

        currentHealth =
            Mathf.Max(
                0,
                currentHealth - damage
            );

        invincibleTimer =
            invincibleTime;

        bool becameKO =
            currentHealth <= 0;

        PlayHitFlash(becameKO);

        Debug.Log(
            name +
            " 受到 " +
            damage +
            " 点伤害，当前生命值：" +
            currentHealth +
            "/" +
            maxHealth
        );

        if (becameKO)
        {
            EnterKO();
        }
        else
        {
            ApplyKnockback(knockback);
            EnterHit();
        }

        return true;
    }

    private void EnterHit()
    {
        if (animator != null)
        {
            animator.ResetTrigger(HitHash);
            animator.SetTrigger(HitHash);
        }

        LockControlTemporarily();
    }

    private void EnterKO()
    {
        if (restoreControlRoutine != null)
        {
            StopCoroutine(
                restoreControlRoutine
            );

            restoreControlRoutine = null;
        }

        if (controller != null)
        {
            controller.enabled = false;
        }

        if (body != null)
        {
#if UNITY_6000_0_OR_NEWER
            Vector2 currentVelocity =
                body.linearVelocity;
#else
            Vector2 currentVelocity =
                body.velocity;
#endif

            currentVelocity.x = 0f;

            if (currentVelocity.y > 0f)
            {
                currentVelocity.y = 0f;
            }

#if UNITY_6000_0_OR_NEWER
            body.linearVelocity =
                currentVelocity;
#else
            body.velocity =
                currentVelocity;
#endif

            body.angularVelocity = 0f;
        }

        if (animator != null)
        {
            animator.ResetTrigger(HitHash);
            animator.SetBool(KOHash, true);
        }
    }

    private void LockControlTemporarily()
    {
        if (controller == null)
        {
            return;
        }

        if (restoreControlRoutine != null)
        {
            StopCoroutine(
                restoreControlRoutine
            );
        }

        controller.enabled = false;

        restoreControlRoutine =
            StartCoroutine(
                RestoreControlAfterDelay()
            );
    }

    private IEnumerator RestoreControlAfterDelay()
    {
        yield return new WaitForSeconds(
            hitStunTime
        );

        if (!IsKO && controller != null)
        {
            controller.enabled = true;
        }

        restoreControlRoutine = null;
    }

    private void ApplyKnockback(
        Vector2 knockback
    )
    {
        if (body == null)
        {
            return;
        }

#if UNITY_6000_0_OR_NEWER
        body.linearVelocity =
            knockback;
#else
        body.velocity =
            knockback;
#endif
    }

    private void CacheFlashRenderers()
    {
        if (flashRenderers == null ||
            flashRenderers.Length == 0)
        {
            flashRenderers =
                GetComponentsInChildren<
                    SpriteRenderer
                >(true);
        }

        originalRendererColors =
            new Color[flashRenderers.Length];

        for (int index = 0;
             index < flashRenderers.Length;
             index++)
        {
            SpriteRenderer renderer =
                flashRenderers[index];

            originalRendererColors[index] =
                renderer != null
                    ? renderer.color
                    : Color.white;
        }
    }

    private void PlayHitFlash(
        bool isKOHit
    )
    {
        StopHitFlashAndRestore();

        if (combat != null)
        {
            combat.CancelCurrentAttack();
        }

        if (flashRenderers == null ||
            flashRenderers.Length == 0)
        {
            return;
        }

        hitFlashRoutine =
            StartCoroutine(
                HitFlashRoutine(isKOHit)
            );
    }

    private IEnumerator HitFlashRoutine(
        bool isKOHit
    )
    {
        if (!isKOHit)
        {
            SetFlashColor();

            yield return
                new WaitForSecondsRealtime(
                    hitFlashDuration
                );

            RestoreFlashColors();
            hitFlashRoutine = null;
            yield break;
        }

        float firstFlashTime =
            koFlashDuration * 0.35f;

        float gapTime =
            koFlashDuration * 0.15f;

        float secondFlashTime =
            koFlashDuration * 0.50f;

        SetFlashColor();

        yield return
            new WaitForSecondsRealtime(
                firstFlashTime
            );

        RestoreFlashColors();

        yield return
            new WaitForSecondsRealtime(
                gapTime
            );

        SetFlashColor();

        yield return
            new WaitForSecondsRealtime(
                secondFlashTime
            );

        RestoreFlashColors();
        hitFlashRoutine = null;
    }

    private void SetFlashColor()
    {
        for (int index = 0;
             index < flashRenderers.Length;
             index++)
        {
            SpriteRenderer renderer =
                flashRenderers[index];

            if (renderer == null)
            {
                continue;
            }

            Color flashColor =
                hitFlashColor;

            if (originalRendererColors != null &&
                index <
                originalRendererColors.Length)
            {
                flashColor.a =
                    originalRendererColors[index].a;
            }

            renderer.color =
                flashColor;
        }
    }

    private void RestoreFlashColors()
    {
        if (flashRenderers == null ||
            originalRendererColors == null)
        {
            return;
        }

        int count = Mathf.Min(
            flashRenderers.Length,
            originalRendererColors.Length
        );

        for (int index = 0;
             index < count;
             index++)
        {
            if (flashRenderers[index] != null)
            {
                flashRenderers[index].color =
                    originalRendererColors[index];
            }
        }
    }

    private void StopHitFlashAndRestore()
    {
        if (hitFlashRoutine != null)
        {
            StopCoroutine(hitFlashRoutine);
            hitFlashRoutine = null;
        }

        RestoreFlashColors();
    }

    public void ResetHealth()
    {
        if (restoreControlRoutine != null)
        {
            StopCoroutine(
                restoreControlRoutine
            );

            restoreControlRoutine = null;
        }

        StopHitFlashAndRestore();

        currentHealth = maxHealth;
        invincibleTimer = 0f;

        if (body != null)
        {
#if UNITY_6000_0_OR_NEWER
            body.linearVelocity =
                Vector2.zero;
#else
            body.velocity =
                Vector2.zero;
#endif

            body.angularVelocity = 0f;
        }

        if (animator != null)
        {
            animator.ResetTrigger(HitHash);
            animator.SetBool(KOHash, false);
            animator.Play("Idle", 0, 0f);
            animator.Update(0f);
        }

        if (controller != null)
        {
            controller.enabled = true;
        }
    }

    private void OnDisable()
    {
        StopHitFlashAndRestore();
    }

    private void OnValidate()
    {
        hitFlashDuration =
            Mathf.Max(
                0.01f,
                hitFlashDuration
            );

        koFlashDuration =
            Mathf.Max(
                0.01f,
                koFlashDuration
            );
    }
}