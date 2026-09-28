using UnityEngine;

public class ObjectHandler : MonoBehaviour
{
    public void onDestroy()
    {
        Destroy(gameObject);
    }
}
