using UnityEngine;
public class BalanceRagdoll:MonoBehaviour
{
    [Header("Main Body")]
    public Rigidbody pelvis;
    [Header("Balance Setting")]
    public float uprightStrength = 100f;
    public float uprightDamping = 10f;
    public Vector3 pelvisLocalUp;
    private void Awake()
    {
        if (pelvis == null)
        {
            pelvis = GetComponent<Rigidbody>();
        }
        pelvisLocalUp = pelvis.transform.InverseTransformDirection(Vector3.up);
    }
    private void FixedUpdate()
    {
        Vector3 currentUp = pelvis.transform.TransformDirection(pelvisLocalUp);
        Vector3 tiltAxis = Vector3.Cross(currentUp, Vector3.up);
        Vector3 correctionTorque = tiltAxis * uprightStrength;
        Vector3 angularVelocity = pelvis.angularVelocity;
        Vector3 tippingAngularVelocity = Vector3.ProjectOnPlane(angularVelocity, Vector3.up);
        Vector3 dampingTourqe =- tippingAngularVelocity * uprightDamping;
        Vector3 finalTorque = correctionTorque + dampingTourqe;
        pelvis.AddTorque(finalTorque, ForceMode.Acceleration);
    } 
}