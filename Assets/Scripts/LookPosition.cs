using FIMSpace.FLook;
using UnityEngine;

public class LookPosition : MonoBehaviour
{

    [SerializeField] private FLookAnimator lookAnimator;
    [SerializeField] private string looktag;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lookAnimator = GetComponent<FLookAnimator>();
    }

    void Update()
    {
        GameObject closest = FindClosestWithTag(looktag);
        if (closest != null)
            lookAnimator.FollowOffset = closest.transform.position;
    }

    private GameObject FindClosestWithTag(string tag)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
        GameObject closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject target in targets)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = target;
            }
        }

        return closest;
    }
}
