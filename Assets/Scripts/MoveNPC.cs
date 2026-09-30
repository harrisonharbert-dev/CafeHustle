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
            GoToDestination(destination);
        }



        if (navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance)
        {
            navMeshAgent.updateRotation = false;
            if (!isLookingAtPlayer)
            {
                isLookingAtPlayer = true;
                transform.DOLookAt(PlayerInputController.instance.transform.position, 0.3f, AxisConstraint.Y);
            }
        }
        else
        {
            navMeshAgent.updateRotation = true;
            isLookingAtPlayer = false;
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

        if (destination != null)
        {
            Vector3 targetVector = destination.transform.position;

            navMeshAgent.SetDestination(targetVector);
        }
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
