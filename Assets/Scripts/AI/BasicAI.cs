using UnityEngine;
using UnityEngine.AI;

public class BasicAI : MonoBehaviour
{
    public Transform player;
    public Transform head;

    private Animator agentAnimator;
    private NavMeshAgent agent;

    public int agentHealth;

    public float lineOfSightRange;
    public float attackingRange;
    public float lineOfSightAngle;

    public float wanderTimer;
    public float wanderRadius;

    private bool isPursingPlayer;

    public float idleTimer;

    private float timer;

    // Use this for initialization
    void Start ()
    {
        agentAnimator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        timer = wanderTimer;
        isPursingPlayer = false;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(!isAgentDead())
        {
            Vector3 direction = player.position - transform.position;
            direction.y = 0;
            float angle = Vector3.Angle(direction, head.up);

            patrol(direction, angle);

            if (hasReachedDestination())
            {
                timer = wanderTimer;
            }
        }
    }

    private void patrol(Vector3 direction, float angle)
    {
        if (timer >= wanderTimer && !isPursingPlayer && !agentAnimator.GetBool("isIdle"))
        {
            agentAnimator.SetBool("isWalking", true);
            Vector3 newPosition = GenerateRandomPosition(transform.position, wanderRadius * 2 * agent.height);
            agent.SetDestination(newPosition);
            timer = 0.0f;
        }
        else if(agentAnimator.GetBool("isIdle") && timer >= idleTimer)
        {
            agentAnimator.SetBool("isIdle", false);
            agent.isStopped = false;
            timer = wanderTimer;
        }
        else
        {
            float currentDistance = Vector3.Distance(player.position, transform.position);

            if (currentDistance < lineOfSightRange && angle < lineOfSightAngle)
            {
                isPursingPlayer = true;
                chase(direction);
            }
            else
            {
                if(isPursingPlayer)
                {
                    agentAnimator.SetBool("isIdle", true);
                    agentAnimator.SetBool("isWalking", false);
                    agentAnimator.SetBool("isAttacking", false);
                    agent.isStopped = true;
                    timer = 0.0f;
                }

                isPursingPlayer = false;
            }

            timer += Time.deltaTime;
        }
    }

    public static Vector3 GenerateRandomPosition(Vector3 origin, float dist)
    {
        Vector3 randDirection = origin + Random.insideUnitSphere * dist;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, NavMesh.AllAreas);
        return navHit.position;
    }

    private void chase(Vector3 direction)
    {
        agentAnimator.SetBool("isIdle", false);

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

    private bool hasReachedDestination()
    {
        return agent.remainingDistance < agent.stoppingDistance && !agent.pathPending;
    }

    public bool isAgentDead()
    {
        if(agentHealth <= 0)
        {
            return true;
        }

        return false;
    }

    public void takeDamage(int damage)
    {
        agentHealth -= damage;
    }
}
