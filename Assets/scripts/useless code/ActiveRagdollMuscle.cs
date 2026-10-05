using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class RagdollMuscle: MonoBehaviour
{
    [Header("References")]
    public Rigidbody body;
    public Rigidbody parentBody;
    [Header("Muscle Settings")]
    public float strength = 2000f;
    public float damping = 100f;
    public float maxTorque = 4000f;
    [Header("Target Pose")]
    public Vector3 targetEulerOffset = Vector3.zero;
    private Quaternion startingRelativeRotation;
    private void Awake()
    {
        if (body == null)
        {
            body = GetComponent<Rigidbody>();
        }
        if (parentBody == null)
        {
            Debug.LogError(name + ": RagdollMuscle has no Parent Body assigned.", this);
            enabled = false;
            return;
        }
        startingRelativeRotation = Quaternion.Inverse(parentBody.rotation) * body.rotation;
    }
    private void FixedUpdate()
    {
        Quaternion targetOffset = Quaternion.Euler(targetEulerOffset);
        Quaternion desireRelativeRotation = startingRelativeRotation * targetOffset;
        Quaternion currentRelativeRotation = Quaternion.Inverse(parentBody.rotation) * body.rotation;
        Quaternion rotationError = desireRelativeRotation * Quaternion.Inverse(currentRelativeRotation);
        rotationError.ToAngleAxis(
            out float errorAngle,
            out Vector3 errorAxis
        );
        if (errorAngle > 180f)
        {
            errorAngle -= 360f;
        }
        if (Mathf.Abs(errorAngle) < 0.01f || float.IsNaN(errorAxis.x))
        {
            return;
        }
        errorAxis.Normalize();
        float errorRadians = errorAngle * Mathf.Deg2Rad;
        Vector3 springTroque = errorAxis * errorRadians * strength;
        Vector3 relativeAngularVelocity = body.angularVelocity = parentBody .angularVelocity;
        Vector3 dampingTorque = -relativeAngularVelocity * damping;
        Vector3 finalTorque = springTroque + dampingTorque;
        finalTorque = Vector3.ClampMagnitude(finalTorque, maxTorque);
        body.AddTorque(finalTorque, ForceMode.Acceleration);
        // parentBody.AddTorque(-finalTorque, ForceMode.Acceleration);
    }
    public void ResetTarget()
    {
        targetEulerOffset = Vector3.zero;
    }
}