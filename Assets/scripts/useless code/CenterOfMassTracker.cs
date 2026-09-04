using UnityEngine;

public class CenterOfMassTracker : MonoBehaviour
{
    [Header("Center of Mass")]
    public Vector3 centerOfMass;
    public float totalMass;
    public int bodyCount;
    [Header("Debug")]
    public bool showDebug = true;
    public float debugSize = 0.25f;
    private Rigidbody[] bodies;
    private void Awake()
    {
        bodies = GetComponentsInChildren<Rigidbody>();
        bodyCount=bodies.Length;
        Debug.Log("centerOfMassTracker found " + bodyCount + " Rigidbodies.", this);
    }
    private void FixedUpdate()
    {
        CalculateCenterOfMass();
        if (showDebug)
        {
            Debug.DrawLine(
                centerOfMass - Vector3.up * debugSize,
                centerOfMass + Vector3.up * debugSize,
                Color.yellow
            );
            Debug.DrawLine(
                centerOfMass - Vector3.right * debugSize,
                centerOfMass + Vector3.right * debugSize,
                Color.yellow
            );
            Debug.DrawLine(
                centerOfMass - Vector3.forward * debugSize,
                centerOfMass + Vector3.forward * debugSize,
                Color.yellow
            );
        }
    }
    private void CalculateCenterOfMass()
    {
        Vector3 weightedPosition = Vector3.zero;
        totalMass = 0f;
        foreach(Rigidbody body in bodies)
        {
            if(body == null)
            {
                continue;
            }
            weightedPosition += body.worldCenterOfMass * body.mass;
            totalMass += body.mass;
        }
        if(totalMass <= 0f)
        {
            centerOfMass = transform.position;
            return;
        }
        centerOfMass = weightedPosition / totalMass;
    }
}