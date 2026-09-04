using Unity.Cinemachine;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class BalanceBrain: MonoBehaviour
{
    [Header("Freferences")]
    public CenterOfMassTracker centerOfMassTracker;
    public FootSupportTracker footSupportTracker;
    public Rigidbody pelvis;
    [Header("Balance Settongs")]
    public float balanaceStrength = 20f;
    public float balanceDamping = 4f;
    public float maxBalanceAcceleration = 15f;
    public float deadZone = 0.03f;
    [Header("Debug")]
    public Vector3 balanceError;
    public float balanceErrorDistance;
    public bool showDebug = true;
    private void FixedUpdate()
    {
        if(
            centerOfMassTracker == null ||
            footSupportTracker == null ||
            pelvis == null
        )
        {
            return;
        }
        if (!footSupportTracker.hasSupport)
        {
            balanceError = Vector3.zero;
            balanceErrorDistance = 0f;
            return;
        }
        Vector3 centerOfMass = centerOfMassTracker.centerOfMass;
        Vector3 supportCenter = footSupportTracker.supportCenter;
        balanceError = supportCenter - centerOfMass;
        balanceError.y = 0f;
        balanceErrorDistance = balanceError.magnitude;
        if (balanceErrorDistance < deadZone)
        {
            balanceError = Vector3.zero;
            return;
        }
        Vector3 correnctionAcceleration = balanceError * balanaceStrength;
        Vector3 pelvisVelocity = pelvis.linearVelocity;
        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(pelvisVelocity, Vector3.up);
        Vector3 dampingAcceleration = -horizontalVelocity * balanceDamping;
        Vector3 finalAcceleration = correnctionAcceleration + dampingAcceleration;
        finalAcceleration = Vector3.ClampMagnitude(finalAcceleration, maxBalanceAcceleration);
        pelvis.AddForce(finalAcceleration, ForceMode.Acceleration);
        if (showDebug)
        {
            Vector3 projectedCenterOfmass = 
            new Vector3(
                centerOfMass.x,
                supportCenter.y,
                centerOfMass.z
            );
            Debug.DrawLine(projectedCenterOfmass,
            supportCenter,
            Color.red);
        }
    }
}