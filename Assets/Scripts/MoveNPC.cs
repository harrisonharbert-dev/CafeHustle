using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using Yarn.Unity;

public class MoveNPC : MonoBehaviour
{
    [SerializeField] private Transform destination;

    private NavMeshAgent navMeshAgent;
    [SerializeField] private bool UseAnimator;
    private bool isFollowing;
    private bool isLookingAtPlayer;
    private bool hasPendingDestination;
    [SerializeField] private Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        navMeshAgent = this.GetComponent<NavMeshAgent>();

        if (navMeshAgent == null)
        {
            Debug.LogWarning($"Nav Mesh Agent component not attached on {this}");
        }

        if (animator == null && UseAnimator)
        {
            Debug.LogWarning($"Animator component not attached on {this}");
        }
    }

    void FixedUpdate()
    {
        if (isFollowing == true)
        {
            if (destination != null)
            {
                navMeshAgent.SetDestination(destination.position);
            }
        }

        if (hasPendingDestination && navMeshAgent != null && !navMeshAgent.pathPending &&
            navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance &&
            navMeshAgent.velocity.sqrMagnitude <= 0.01f)
        {
            hasPendingDestination = false;
            OnDestinationReached();
        }

        if (!UseAnimator) return;
        float velocity = navMeshAgent.velocity.magnitude;

        if (velocity > 0.1f)
        {
            animator.SetBool("isMoving", true);
        }

        else if (velocity <= 0f)
        {
            animator.SetBool("isMoving", false);
        }

    }

    [YarnCommand("GoToDestination")]
    public void GoToDestination(Transform newDestination)
    {
        destination = newDestination;
        StopFollowTarget();

        if (destination != null && navMeshAgent != null && navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
        {
            hasPendingDestination = true;
            navMeshAgent.SetDestination(destination.position);
        }
        else
        {
            hasPendingDestination = false;
        }
    }

    public virtual void OnDestinationReached()
    {
        transform.DOLookAt(PlayerInputController.instance.transform.position, 1f, AxisConstraint.Y);
    }

    public void FollowTarget(Transform newDestination)
    {
        destination = newDestination;
        isFollowing = true;
    }

    public void StopFollowTarget()
    {
        isFollowing = false;
    }
}
