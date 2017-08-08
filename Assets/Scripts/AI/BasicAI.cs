using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BasicAI : MonoBehaviour
{
    public Transform player;
    public Transform head;
    public Transform[] WayPoints;

    private Animator agentAnimator;
    private NavMeshAgent agent;

    public float lineOfSightRange;
    public float attackingRange;
    public float lineOfSightAngle;

    public float minIdleTime;
    public float maxIdleTime;

    private string state;
    private int currentWayPoint;

    // Use this for initialization
    void Start()
    {
        agentAnimator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = false;
        state = "Patrolling";
        currentWayPoint = -1;
        agentAnimator.SetBool("isIdle", false);
        agentAnimator.SetBool("isWalking", true);
        agentAnimator.SetBool("isAttacking", false);
        goToWayPoint();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0;
        float angle = Vector3.Angle(direction, head.up);

        checkIfPlayerNearby(direction, angle);

        if (WayPoints.Length > 0 && state == "Patrolling" && hasReachedDestination())
        {
            StartCoroutine(idleAndGoToWayPoint());
            //goToWayPoint();
        }
    }

    private void goToWayPoint()
    {
        int nextWayPoint = Random.Range(0, WayPoints.Length);

        if (nextWayPoint == currentWayPoint)
        {
            currentWayPoint = (nextWayPoint + 1) % WayPoints.Length;
        }
        else
            currentWayPoint = nextWayPoint;

        agent.SetDestination(WayPoints[currentWayPoint].position);
    }

    private void checkIfPlayerNearby(Vector3 direction, float angle)
    {
        float currentDistance = Vector3.Distance(player.position, transform.position);

        if (currentDistance < lineOfSightRange && (angle < lineOfSightAngle || state == "Pursuing"))
        {
            chase(direction);
        }
        else
        {
            if(state == "Pursuing")
            {
                StartCoroutine(idleAndGoToWayPoint());
            }
        }
    }

    private void chase(Vector3 direction)
    {
        agentAnimator.SetBool("isIdle", false);
        state = "Pursuing";

        if (direction.magnitude > attackingRange)
        {
            agent.SetDestination(player.position);
            agentAnimator.SetBool("isWalking", true);
            agentAnimator.SetBool("isAttacking", false);
        }
        else
        {
            agentAnimator.SetBool("isWalking", false);
            agentAnimator.SetBool("isAttacking", true);
        }
    }

    private IEnumerator idleAndGoToWayPoint()
    {
        state = "Idle";
        agentAnimator.SetBool("isIdle", true);
        agentAnimator.SetBool("isWalking", false);
        agentAnimator.SetBool("isAttacking", false);

        float idleTime = Random.Range(minIdleTime, maxIdleTime);

        yield return new WaitForSeconds(idleTime);

        state = "Patrolling";
        agentAnimator.SetBool("isIdle", false);
        agentAnimator.SetBool("isWalking", true);
        agentAnimator.SetBool("isAttacking", false);

        goToWayPoint();
    }

    private bool hasReachedDestination()
    {
        if (!agent.pathPending && Vector3.Distance(agent.destination, agent.transform.position) <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0.0f)
            {
                return true;
            }
        }

        return false;
    }
}
