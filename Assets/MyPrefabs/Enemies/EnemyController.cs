using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Componentes")]
    public Animator animator;

    [Header("Vida")]
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

    [Header("Movimiento")]
    public float moveSpeed = 0.1f;
    public float rotationSpeed = 10f;

    [Header("Waypoints")]
    public List<Transform> wayPoints = new List<Transform>();
    private int wayPointIndex = 0;

    [Header("Ataque al Castillo")]
    public CastleController castle;
    public float castleDamage = 10f;
    public float castleAttackInterval = 2f;

    private bool hasReachedCastle = false;
    private float attackTimer = 0f;

    void Start()
    {
        currentHealth = maxHealth;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogWarning(
                "No se encontró un Animator en el enemigo: " +
                gameObject.name
            );
        }
    }

    void Update()
    {
        if (isDead)
            return;

        if (hasReachedCastle)
        {
            AttackCastle();
            return;
        }

        MoveAlongPath();
    }

    void MoveAlongPath()
    {
        if (wayPoints == null || wayPoints.Count == 0)
        {
            SetMoving(false);
            return;
        }

        if (wayPointIndex >= wayPoints.Count)
        {
            ReachCastle();
            return;
        }

        Transform targetWaypoint = wayPoints[wayPointIndex];

        if (targetWaypoint == null)
        {
            wayPointIndex++;

            if (wayPointIndex >= wayPoints.Count)
            {
                ReachCastle();
            }

            return;
        }

        SetMoving(true);

        Vector3 targetPosition = targetWaypoint.position;
        Vector3 direction = targetPosition - transform.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            transform.position = targetPosition;
            wayPointIndex++;

            if (wayPointIndex >= wayPoints.Count)
            {
                ReachCastle();
            }
        }
    }

    void ReachCastle()
    {
        if (hasReachedCastle)
            return;

        hasReachedCastle = true;
        attackTimer = 0f;

        SetMoving(false);
    }

    void AttackCastle()
    {
        if (castle == null || castle.IsDestroyed)
            return;

        attackTimer += Time.deltaTime;

        float interval = Mathf.Max(0.05f, castleAttackInterval);

        if (attackTimer >= interval)
        {
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            castle.TakeDamage(castleDamage);

            attackTimer = 0f;
        }
    }

    void SetMoving(bool moving)
    {
        if (animator != null)
        {
            animator.SetBool("IsMoving", moving);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead)
            return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Die");
        }

        Destroy(gameObject, 2f);
    }
}