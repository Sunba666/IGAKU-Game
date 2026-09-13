using System.Collections;
using UnityEngine;

public class FighterHealth : MonoBehaviour
{
    [Header("生命值")]
    [SerializeField] private int maxHealth = 100;

    [Header("受击")]
    [SerializeField] private float invincibleTime = 0.12f;
    [SerializeField] private float hitStunTime = 0.25f;

    [Header("对象引用")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D body;

    private LocalFighterController2D controller;
    private Coroutine restoreControlRoutine;
    private float invincibleTimer;
    private int currentHealth;

    private static readonly int HitHash =
        Animator.StringToHash("Hit");

    private static readonly int KOHash =
        Animator.StringToHash("KO");

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsKO => currentHealth <= 0;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (body == null)
        {
            body = GetComponent<Rigidbody2D>();
        }

        controller = GetComponent<LocalFighterController2D>();

        if (animator != null)
        {
            animator.SetBool(KOHash, false);
        }
    }

    private void Update()
    {
        if (invincibleTimer > 0f)
        {
            invincibleTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (!IsKO || body == null)
        {
            return;
        }

#if UNITY_6000_0_OR_NEWER
    Vector2 currentVelocity = body.linearVelocity;
#else
        Vector2 currentVelocity = body.velocity;
#endif

        currentVelocity.x = 0f;

        // 如果角色正在向上飞，KO 后立即停止上升；
        // 如果正在下落，则保留向下速度，让角色正常落到地面。
        if (currentVelocity.y > 0f)
        {
            currentVelocity.y = 0f;
        }

#if UNITY_6000_0_OR_NEWER
    body.linearVelocity = currentVelocity;
#else
        body.velocity = currentVelocity;
#endif

        body.angularVelocity = 0f;
    }

    public bool TakeHit(int damage, Vector2 knockback)
    {
        if (IsKO || invincibleTimer > 0f)
        {
            return false;
        }

        damage = Mathf.Max(0, damage);
        currentHealth = Mathf.Max(0, currentHealth - damage);
        invincibleTimer = invincibleTime;

        Debug.Log(
            name + " 受到 " + damage +
            " 点伤害，当前生命值：" +
            currentHealth + "/" + maxHealth
        );

        if (currentHealth <= 0)
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
            StopCoroutine(restoreControlRoutine);
            restoreControlRoutine = null;
        }

        if (controller != null)
        {
            controller.enabled = false;
        }

        if (body != null)
        {
#if UNITY_6000_0_OR_NEWER
        Vector2 currentVelocity = body.linearVelocity;
#else
            Vector2 currentVelocity = body.velocity;
#endif

            currentVelocity.x = 0f;

            if (currentVelocity.y > 0f)
            {
                currentVelocity.y = 0f;
            }

#if UNITY_6000_0_OR_NEWER
        body.linearVelocity = currentVelocity;
#else
            body.velocity = currentVelocity;
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
            StopCoroutine(restoreControlRoutine);
        }

        controller.enabled = false;
        restoreControlRoutine =
            StartCoroutine(RestoreControlAfterDelay());
    }

    private IEnumerator RestoreControlAfterDelay()
    {
        yield return new WaitForSeconds(hitStunTime);

        if (!IsKO && controller != null)
        {
            controller.enabled = true;
        }

        restoreControlRoutine = null;
    }

    private void ApplyKnockback(Vector2 knockback)
    {
        if (body == null)
        {
            return;
        }

#if UNITY_6000_0_OR_NEWER
        body.linearVelocity = knockback;
#else
        body.velocity = knockback;
#endif
    }

    public void ResetHealth()
    {
        if (restoreControlRoutine != null)
        {
            StopCoroutine(restoreControlRoutine);
            restoreControlRoutine = null;
        }

        currentHealth = maxHealth;
        invincibleTimer = 0f;

        if (body != null)
        {
#if UNITY_6000_0_OR_NEWER
        body.linearVelocity = Vector2.zero;
#else
            body.velocity = Vector2.zero;
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
}