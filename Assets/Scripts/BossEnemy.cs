using UnityEngine;

public class BossEnemy : EnemyBase
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        maxHealth = 100.0f;
        visionDistance = 20f;
        visionAngle = 90f;
        hearingDistance = 20f;
    }

    protected override void Patrol()
    {

        if (!agent.pathPending && agent.remainingDistance < 0.2f)
        {
            if (currentPoint == patrolPoints.Length - 1)
            {
                currentPoint = -1;
                GoToNextPatrolPoint();
            }
            else
            {
                GoToNextPatrolPoint();

            }
        }


    }

    protected override void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        SetTargetPos(patrolPoints[++currentPoint].position);

        agent.SetDestination(targetPos);
    }
}
 