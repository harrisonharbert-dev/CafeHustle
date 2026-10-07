using System.Collections;
using UnityEngine;

public class Stream : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private ParticleSystem splashParticle;

    private Coroutine pourRoutine;
    private Coroutine particleRoutine;

    private Vector3 targetPosition;

    [Header("Stream")]
    public float streamSpeed = 8f;
    public float endSpeed = 5f;
    public float maxDistance = 2f;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        splashParticle = GetComponentInChildren<ParticleSystem>();

        if (splashParticle != null)
            splashParticle.gameObject.SetActive(false);
    }

    private void Start()
    {
        MoveToPosition(0, transform.position);
        MoveToPosition(1, transform.position);
    }

    public void Begin()
    {
        pourRoutine = StartCoroutine(BeginPour());
        particleRoutine = StartCoroutine(UpdateParticle());
    }

    public void End()
    {
        if (pourRoutine != null)
            StopCoroutine(pourRoutine);

        if (particleRoutine != null)
            StopCoroutine(particleRoutine);

        if (splashParticle != null)
            splashParticle.gameObject.SetActive(false);

        StartCoroutine(EndPour());
    }

    private IEnumerator BeginPour()
    {
        while (true)
        {
            targetPosition = FindEndPoint();

            // Top follows bottle opening
            MoveToPosition(0, transform.position);

            // Bottom moves down toward stove
            AnimateToPosition(1, targetPosition, streamSpeed);

            yield return null;
        }
    }

    private IEnumerator EndPour()
    {
        // Stop following the bottle.
        // Move the TOP of the oil downward toward the bottom.
        Vector3 bottomPosition = lineRenderer.GetPosition(1);

        while (!HasReachedPosition(0, bottomPosition))
        {
            AnimateToPosition(0, bottomPosition, endSpeed);

            yield return null;
        }

        Destroy(gameObject);
    }

    private IEnumerator UpdateParticle()
    {
        while (true)
        {
            if (splashParticle != null)
            {
                splashParticle.transform.position = targetPosition;

                bool reachedSurface =
                    HasReachedPosition(1, targetPosition);

                splashParticle.gameObject.SetActive(reachedSurface);
            }

            yield return null;
        }
    }

    private Vector3 FindEndPoint()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
            return hit.point;

        return ray.GetPoint(maxDistance);
    }

    private void MoveToPosition(int index, Vector3 position)
    {
        lineRenderer.SetPosition(index, position);
    }

    private void AnimateToPosition(int index, Vector3 position, float speed)
    {
        Vector3 currentPoint = lineRenderer.GetPosition(index);

        Vector3 newPosition = Vector3.MoveTowards(
            currentPoint,
            position,
            Time.deltaTime * speed
        );

        lineRenderer.SetPosition(index, newPosition);
    }

    private bool HasReachedPosition(int index, Vector3 position)
    {
        return Vector3.Distance(
            lineRenderer.GetPosition(index),
            position
        ) < 0.02f;
    }
}