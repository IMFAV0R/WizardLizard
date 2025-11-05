using JetBrains.Annotations;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public abstract class EnemyBase : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float visionRange = 10.0f;
    public float visionAngle = 60.0f;
    public float visionDistance = 8.0f;
    public float loseDistance = 12.0f;
    public float moveSpeed = 2.0f;
    public float chaseSpeed = 4.0f;
    public float loseSightTime = 2.0f;
    public float maxChaseTime = 2.0f;
    public float hearingDistance = 0.0f;
    public float waitTime = 2.0f;
    public Animator animator;

    public float maxHealth = 10.0f;
    private float currentHealth = 0.0f;

    protected NavMeshAgent agent;
    protected Transform player;
    protected int currentPoint = 0;

    protected bool isBack = false;
    protected bool isChasing = false;
    protected bool CanSeePlayer = false;
    protected float timeSinceLastSeen = 0f;
    protected float chaseTimer = 0f;

    // === Investigate ===
    protected bool isGoingToInvestigating = false;
    protected bool isInvestigating = false;
    protected bool isTurningToInvestigate = false;
    protected float investigateDelay = 2.0f;
    protected float investigateTimer = 0f;
    public float turnSpeed = 5f;
    protected float lookDelay = 1.0f;
    protected float lookTimer = 0f;

    protected Vector3 targetPos;

    protected bool canBeInterrupt = true;
    protected float idleTimer = 0.0f;

    private bool isDead = false;

    public PowerManager powerManager;

    protected enum State
    {
        Patrol, 
        Chase, 
        Investigate, 
        Idle
    }

    protected enum IdleType
    {
        IdleP, 
        Tired,
        BeControlled
    }

    protected State state = State.Patrol;
    protected IdleType idle = IdleType.IdleP ;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = false;

        currentHealth = maxHealth;

        player = GameObject.FindGameObjectWithTag("Player").transform;
        GoToNextPatrolPoint();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        canBeInterrupt = true;

        switch (state)
        {
            case State.Patrol:
                Patrol();
                break;

            case State.Chase:
                Chase();
                break;

            case State.Investigate:
                Investigate();
                break;
            case State.Idle:
                Idle();
                break;
        }

        DetectPlayer();

    }


    protected virtual void DetectPlayer()
    {
        if(player == null) return;

        if(!canBeInterrupt) return;

        Vector3 dirToPlayer = player.position - transform.position;
        float distance = dirToPlayer.magnitude;

        if (distance < visionDistance)
        {
            float angle = Vector3.Angle(transform.forward, dirToPlayer);

            if (angle < visionAngle / 2f)
            {
                if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dirToPlayer.normalized, out RaycastHit hit, visionDistance))
                {
                    if (hit.collider.CompareTag("Player") && !powerManager.isPlayerHidding())
                    {
                        CanSeePlayer = true;
                        agent.isStopped = false;
                        timeSinceLastSeen = 0;
                        StartChase();
                        return;
                    }
                }
            }
        }

        timeSinceLastSeen += Time.deltaTime;
        CanSeePlayer = false;

    }

    private void OnDrawGizmosSelected()
    {
        if (agent == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 1f, visionDistance);

        Vector3 forward = transform.forward;
        float halfAngle = visionAngle / 2f;
        Vector3 leftBoundary = Quaternion.Euler(0, -halfAngle, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, halfAngle, 0) * forward;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + Vector3.up * 1f, transform.position + Vector3.up * 1f + leftBoundary * visionDistance);
        Gizmos.DrawLine(transform.position + Vector3.up * 1f, transform.position + Vector3.up * 1f + rightBoundary * visionDistance);
    }



    // ***************************
    //         PATROL LOGIC
    // ***************************

    protected virtual void Patrol()
    {

        if (!agent.pathPending && agent.remainingDistance < 0.2f)
        {
            if (currentPoint == 0 || currentPoint == patrolPoints.Length - 1)
            {
                isBack = !isBack;
                Idle(waitTime, IdleType.IdleP);
            }
            else
            {
                GoToNextPatrolPoint();

            }
        }


    }

    private void BackToPatrol()
    {
        agent.isStopped = false;
        state = State.Patrol;
        GoToNextPatrolPoint() ;
    }

    protected virtual void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        if (isBack && currentPoint != 0)
        {
            SetTargetPos(patrolPoints[--currentPoint].position);

        }else if (!isBack && currentPoint != patrolPoints.Length - 1)
        {
            SetTargetPos(patrolPoints[++currentPoint].position);
        }

        agent.SetDestination(targetPos);
    }

    protected void SetTargetPos(Vector3 nextTarget)
    {
        targetPos = nextTarget;
    }

    // *********************
    //      CHASE LOGIC
    // *********************
    protected virtual void StartChase()
    {
        if(!isChasing)
        {
            chaseTimer = maxChaseTime;
            state = State.Chase;
            isInvestigating = false;
            isChasing = true;
            agent.speed = chaseSpeed; 
        }
    }

    protected virtual void GiveUpChasing()
    {
        canBeInterrupt = false;
        agent.isStopped = false;
        isChasing = false;
        agent.speed = moveSpeed;
        BackToPatrol();
    }

    protected virtual void Chase()
    {
        if (player == null) return;

        if(isChasing)
        {
            chaseTimer -= Time.deltaTime;
            agent.SetDestination(player.position);

            if (!CanSeePlayer && timeSinceLastSeen > loseSightTime)
            {
                GiveUpChasing();
            }else if (chaseTimer <= 0.0f)
            {

                Idle(2.0f, IdleType.Tired);
            }
        }
    }
     
    protected virtual void Investigate()
    {

        if (isTurningToInvestigate)
        {

            float angle = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(LookAtDir(targetPos, turnSpeed)));
            if (angle < 5f)
            {
                lookTimer += Time.deltaTime;
                if (lookTimer >= lookDelay)
                {
                    isTurningToInvestigate = false;
                    agent.isStopped = false;
                    agent.SetDestination(targetPos);
                }
            }
            return;
        }


        if (!agent.pathPending && agent.remainingDistance < 0.2f && !isInvestigating)
        {
            StartInvestigating();
        }

        if (isInvestigating)
        {
            investigateTimer -= Time.deltaTime;

            if (investigateTimer <= 0.0f)
            {
                GiveUpInvestigating();

            }
        }
    }

    private Vector3 LookAtDir(Vector3 target, float speed)
    {
        Vector3 dir = (target - transform.position).normalized;
        dir.y = 0f;
        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRot, speed * 100f * Time.deltaTime);
        }

        return dir;
    }

    public virtual void HearSound(Vector3 soundPos)
    {
        if(!isChasing)
        {
            state = State.Investigate;
            SetTargetPos(soundPos);
            agent.isStopped = true;
            isTurningToInvestigate = true;
            isInvestigating = false;
            lookTimer = 0f;
        }
    }

    private void StartInvestigating()
    {
        isInvestigating = true;
        investigateTimer = investigateDelay;
    }

    private void GiveUpInvestigating()
    {
        state = State.Patrol;
        isInvestigating = false;
        GoToNextPatrolPoint();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerCaught();

        }
    }


    protected virtual void OnPlayerCaught()
    {
        SceneManager.LoadScene(1);
    }

    protected virtual void Idle(float time, IdleType idle)
    {
        idleTimer = time;
        this.idle = idle;

        state = State.Idle;

    }

    protected virtual void Idle()
    {
        idleTimer -= Time.deltaTime;

        switch (idle)
        {
            case IdleType.IdleP  :
                IdleP();
                break;
            case IdleType.Tired:
                Tired();
                break;
            case IdleType.BeControlled:
                BeControlled();
                break;
        }



    }

    protected virtual void IdleP()
    {
        if (idleTimer <= 0.0f)
        {
            BackToPatrol();
        }
    }

    protected virtual void Tired()
    {
        canBeInterrupt = false;
        agent.isStopped = true;
        LookAtDir(player.position, 1000.0f);
        if(idleTimer  <= 0.0f)
        {
            GiveUpChasing();
        }
    }

    protected virtual void BeControlled()
    {
        agent.isStopped = true;

        if (idleTimer <= 0.0f)
        {
            BackToPatrol();
        }
    }


    protected virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0.0f)
        {
            Died();
        }
    }


    private void Died()
    {
        agent.isStopped = true;

        if (isDead) return;
        isDead = true;

        canBeInterrupt = false;

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        animator.SetTrigger("Die");
        StartCoroutine(FadeAfterAnimation());
    }

    private IEnumerator FadeAfterAnimation()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Die"));

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length + 0.3f);

        StartCoroutine(FadeAndDestroy(0.5f));

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = false;

    }

    private IEnumerator FadeAndDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = false;

    }
}
