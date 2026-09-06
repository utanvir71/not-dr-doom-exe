using UnityEngine;

public class PelvisUprightController : MonoBehaviour
{
    [Header("References")]
    //Rigidbody of max's pelvis
    public Rigidbody pelvis;
    //A transform attached to the pelvis whose Green Y axis point upward when max is sanding nomally.
    public Transform uprightReference;

    [Header("Upright Control")]
    //How strongly the pelvis tries to return upright.
    public float uprightStrength = 20f;
    //fights rotational velocity so Max does not wobble forever.
    public float angularDamping = 5f;
    //saftey limit max AngularAcceleration = 30f;
    public float maxAngularAcceleration = 30f;

    
    [Header("Debug")]
    //how tilted Max currently is in degrees.
    public float tiltAngle;
    private void Awake()
    {
        if(pelvis == null)
            pelvis = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        //Get Max's current "up" direction.
        Vector3 currentUp = uprightReference.up;
        //world up is always straing upward.
        Vector3 targetUp = Vector3.up;
        //measure how far max is tilted away from world up.
        tiltAngle = Vector3.Angle(currentUp, targetUp);
        //Find the axis Max needs to rotate around to move currentUp toward world up.
        Vector3 correctionAxis = Vector3.Cross(currentUp, targetUp);
        //Turn the tilt amout into a correction strength.
        Vector3 uprightAcceleration = correctionAxis * tiltAngle * uprightStrength;
        //Fight the pelvis's current rotation velocity.
        Vector3 dampingAcceleration = -pelvis.angularVelocity * angularDamping;
        Vector3 finalAngularAcceleration = uprightAcceleration + dampingAcceleration;
        //Safetly limit
        finalAngularAcceleration = Vector3.ClampMagnitude(finalAngularAcceleration, maxAngularAcceleration);
        //Apply the rotation acceleration to the pelvis.
        pelvis.AddTorque(finalAngularAcceleration, ForceMode.Acceleration);
    }

    
}