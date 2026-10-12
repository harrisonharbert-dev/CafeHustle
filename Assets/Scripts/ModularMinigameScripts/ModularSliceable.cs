using EzySlice;
using UnityEngine;

public class ModularSliceable : MonoBehaviour
{

    [SerializeField] private Transform slicePlane;
    [SerializeField] private Material crossSectionMat;
    [SerializeField] private float cutForce = 100f;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SliceObject();
        }
    }

    void SliceObject()
    {
        Debug.Log("Sliced");
        SlicedHull hull = gameObject.Slice(slicePlane.position,slicePlane.up);
 
        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(gameObject,crossSectionMat);
            GameObject lowerHull = hull.CreateLowerHull(gameObject,crossSectionMat);

            SetupPhysics(upperHull, gameObject.transform.position);
            SetupPhysics(lowerHull, gameObject.transform.position);

            Destroy(gameObject);
        }
    }

        private void SetupPhysics(GameObject hullObj, Vector3 originalPosition)
    {
        // Copy the original object's transform coordinates
        hullObj.transform.position = originalPosition;

        // Add a Rigidbody and set up a Convex MeshCollider (required for moving physics meshes)
        Rigidbody rb = hullObj.AddComponent<Rigidbody>();
        MeshCollider collider = hullObj.AddComponent<MeshCollider>();
        collider.convex = true;


        // Apply an outward burst force away from the cut location
        rb.AddExplosionForce(cutForce, originalPosition, 1f);
    }
}
