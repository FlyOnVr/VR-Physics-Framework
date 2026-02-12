using UnityEngine;

public class HandPresencePhysics : MonoBehaviour
{
    public Transform target;
    private Rigidbody rb;
    private Collider[] handColliders;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        handColliders = GetComponentsInChildren<Collider>();
    }

    public void EnableHandCollider()
    {
        foreach (var item in handColliders)
        {
            item.enabled = true;
        }
    }
    public void EnableHandColliderDelay(float delay)
    {
        Invoke("EnableHandCollider", delay);
    }

    public void DisableHandCollider()
    {
        foreach (var item in handColliders)
        {
            item.enabled = false;
        }
    }


    void FixedUpdate()
    {
        // Move towards target position
        rb.linearVelocity = (target.position - transform.position) / Time.fixedDeltaTime;

        // Calculate rotation difference
        Quaternion rotationDifference = target.rotation * Quaternion.Inverse(transform.rotation);
        rotationDifference.ToAngleAxis(out float angleInDegree, out Vector3 rotationAxis);

        // Convert to degrees in vector form
        Vector3 rotationDifferenceInDegree = angleInDegree * rotationAxis;

        // Apply angular velocity
        rb.angularVelocity = (rotationDifferenceInDegree * Mathf.Deg2Rad) / Time.fixedDeltaTime;
    }
}
