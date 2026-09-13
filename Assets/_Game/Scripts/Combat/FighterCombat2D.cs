using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterCombat2D : MonoBehaviour
{
    [Header("攻击点")]
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private LayerMask hurtboxLayer;

    [Header("轻攻击")]
    [SerializeField]
    private Vector2 lightAttackSize =
        new Vector2(1.5f, 1.2f);

    [SerializeField] private float lightAttackStartup = 0.12f;
    [SerializeField] private float lightAttackRecovery = 0.20f;
    [SerializeField] private int lightAttackDamage = 25;

    [Header("击退")]
    [SerializeField] private float knockbackX = 4f;
    [SerializeField] private float knockbackY = 2f;

    private FighterHealth ownerHealth;
    private Coroutine attackRoutine;

    public bool IsAttacking => attackRoutine != null;

    private void Awake()
    {
        ownerHealth = GetComponent<FighterHealth>();

        if (attackOrigin == null)
        {
            Transform visual = transform.Find("Visual");

            if (visual != null)
            {
                attackOrigin = visual.Find("AttackOrigin");
            }
        }
    }

    public void BeginLightAttack()
    {
        if (attackRoutine != null)
        {
            return;
        }

        if (ownerHealth != null && ownerHealth.IsKO)
        {
            return;
        }

        attackRoutine = StartCoroutine(LightAttackRoutine());
    }

    private IEnumerator LightAttackRoutine()
    {
        yield return new WaitForSeconds(lightAttackStartup);

        PerformLightAttackCheck();

        yield return new WaitForSeconds(lightAttackRecovery);

        attackRoutine = null;
    }

    private void PerformLightAttackCheck()
    {
        if (attackOrigin == null)
        {
            Debug.LogWarning(
                name + " 没有连接 AttackOrigin。"
            );

            return;
        }

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            attackOrigin.position,
            lightAttackSize,
            0f,
            hurtboxLayer
        );

        HashSet<FighterHealth> damagedTargets =
            new HashSet<FighterHealth>();

        foreach (Collider2D hit in hits)
        {
            FighterHealth target =
                hit.GetComponentInParent<FighterHealth>();

            if (target == null)
            {
                continue;
            }

            if (target == ownerHealth)
            {
                continue;
            }

            if (damagedTargets.Contains(target))
            {
                continue;
            }

            float direction =
                target.transform.position.x >= transform.position.x
                    ? 1f
                    : -1f;

            Vector2 knockback = new Vector2(
                direction * knockbackX,
                knockbackY
            );

            if (target.TakeHit(lightAttackDamage, knockback))
            {
                damagedTargets.Add(target);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackOrigin == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            attackOrigin.position,
            lightAttackSize
        );
    }
}