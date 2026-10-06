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

    void Start()
    {
        currentHealth = maxHealth;
        /*if (animator == null)
        {
            animator = GetComponent<Animator>();
        }*/
    }

    void Update()
    {
        if (isDead) return;
        MoveAlongPath();
    }

    void MoveAlongPath()
    {
        if (wayPoints == null || wayPointIndex >= wayPoints.Count) return;

        Transform targetWaypoint = wayPoints[wayPointIndex];
        Vector3 targetPosition = targetWaypoint.position;
        Vector3 direction = targetPosition - transform.position;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            wayPointIndex++;
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        isDead = true;
        //animator.SetTrigger("Die");
        Destroy(gameObject, 2f);
    }
}