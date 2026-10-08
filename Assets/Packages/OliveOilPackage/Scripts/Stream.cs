using System.Collections;
using UnityEngine;

public class Stream : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private ParticleSystem splashParticle;

    private Coroutine pourRoutine;
    private Coroutine particleRoutine;

    private Vector3 targetPosition;
    private bool isEnding;

    [Header("Stream")]
    public float streamSpeed = 8f;
    public float endSpeed = 5f;
    public float maxDistance = 2f;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Prevent bottle rotation from affecting stream direction
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2;

        splashParticle = GetComponentInChildren<ParticleSystem>();

        if (splashParticle != null)
            splashParticle.gameObject.SetActive(false);

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position);
    }

    public void Begin()
    {
        if (pourRoutine != null)
            return;

        isEnding = false;

        pourRoutine = StartCoroutine(BeginPour());
        particleRoutine = StartCoroutine(UpdateParticle());
    }

    public void End()
    {
        if (isEnding)
            return;

        isEnding = true;

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
            Vector3 startPosition = transform.position;

            targetPosition = FindEndPoint();

            // Always follow bottle opening
            lineRenderer.SetPosition(0, startPosition);

            // Keep stream directly below opening
            Vector3 bottomPosition = lineRenderer.GetPosition(1);

            Vector3 newPosition = Vector3.MoveTowards(
                bottomPosition,
                targetPosition,
                streamSpeed * Time.deltaTime
            );

            // Lock horizontal position to bottle opening
            newPosition.x = startPosition.x;
            newPosition.z = startPosition.z;

            lineRenderer.SetPosition(1, newPosition);

            yield return null;
        }
    }

    private IEnumerator EndPour()
    {
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

                bool reachedSurface = HasReachedPosition(1, targetPosition);

                if (splashParticle.gameObject.activeSelf != reachedSurface)
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

    private void AnimateToPosition(int index, Vector3 position, float speed)
    {
        Vector3 currentPoint = lineRenderer.GetPosition(index);

        lineRenderer.SetPosition(
            index,
            Vector3.MoveTowards(currentPoint, position, Time.deltaTime * speed)
        );
    }

    private bool HasReachedPosition(int index, Vector3 position)
    {
        return Vector3.Distance(
            lineRenderer.GetPosition(index),
            position
        ) < 0.02f;
    }
}