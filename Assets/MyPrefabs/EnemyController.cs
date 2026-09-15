using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Componentes")]
    public Animator animator;

    [Header("Estadisticas")]
    public float health = 100f;
    public float moveSpeed;
    public float rotationSpeed;

    [Header("Waypoints")]
    public List<Transform> wayPoints = new List<Transform>();
    public int wayPointIndex;
    public int distance;

    [Header("Estados")]
    public bool moving;
    private bool isDead = false;
    private bool isAttacking = false;

    void Start()
    {
        StartMoving();
    }

    public void StartMoving()
    {
        animator.SetBool("Walking", true);
        wayPointIndex = 0;
        moving = true;
    }

    void Update()
    {
        if (isDead || isAttacking) return;
        
        MoveAlongPath();
    }

    private void MoveAlongPath()
    {
        if (!moving || wayPoints == null || wayPoints.Count == 0)
        {
            return;
        }

        if (wayPointIndex < wayPoints.Count)
        {
            transform.position = Vector3.MoveTowards(transform.position, wayPoints[wayPointIndex].position, moveSpeed * Time.deltaTime);

            var direction = transform.position - wayPoints[wayPointIndex].position;
            var targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            var distance = Vector3.Distance(transform.position, wayPoints[wayPointIndex].position);
            if (distance <= 0.1f)
            {
                wayPointIndex++;
            }

        }
        else
        {
            moving = false;
            StartAttack();
        }
    }

    public void StartAttack()
    {
        isAttacking = true;
        animator.SetBool("Walking", false);
        animator.SetTrigger("Attack");
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        animator.SetTrigger("Die");

        Destroy(gameObject, 1f);
    }
}