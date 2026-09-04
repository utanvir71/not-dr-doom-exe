using UnityEngine;

public class PelvisHeightSupport : MonoBehaviour
{
    [Header("References")]
    public Rigidbody pelvis;
    //Only objects on this layer count as ground.
    //This stops the raycast from detecting Max's own body.
    public LayerMask groundLayer;


    [Header("height Support")]
    //The Height we want the pelvis to stay above the ground.
    public float targetHeight = 1.0f;
    //How far downard we search for the ground.
    public float raycastDistance = 2.0f;
    //How strongly Max reacts when he is bellow/ above target height.
    public float springStrength = 40f;
    //Stop the pelvis from bouncing up and down forever
    public float damping = 8f;
    //sefty limit so the support cannot launch Max into space.
    public float maxAcceleration = 50f;


    [Header("Debug")]
    //current distance between pelvis and ground.
    public float currentHeight;
    //The final upward acceleration being applied.
    public float supportAcceleration;

    private void Awake()
    {
        //t if we forgot to assign the pelvis Rigidbody.
        if (pelvis == null)
        {
            pelvis = GetComponent<Rigidbody>();
        }
    }
    private void FixedUpdate()
    {
        //Stoot an invisible ray Straight downward from the pelvis.
        //if it hits the ground, unity stores information about that hit inside the variable called "hit".
        if(Physics.Raycast(
            pelvis.worldCenterOfMass,
            Vector3.down,
            out RaycastHit hit,
            raycastDistance,
            groundLayer
        ))
        {
            currentHeight = hit.distance;
            //calcuate how far max is from the height we want
            //positive = max too high
            // negative = max is not too high
            float heightError = targetHeight - currentHeight;
            //get only the up/down part of the pelvis velocity.
            //positive = upward
            float verticalVelocity = Vector3.Dot(pelvis.linearVelocity, Vector3.up);
            //The spring pushes hard when max is farther away form the target height.
            float springAcceleration = heightError * springStrength;
            //Damping fights the pelvis's vertical movement like brake in a car.
            float dampingAcceleration = verticalVelocity * damping;
            //Gravity constantly accelerates max downard.
            float gravityCompensition = -Physics.gravity.y;//anti gravity
            //Combine everything.
            supportAcceleration = springAcceleration - dampingAcceleration + gravityCompensition;

            //Never allow the support to pull Max Downward.
            //If max is too high, gravity can naturally bring him down.
            supportAcceleration = Mathf.Clamp(supportAcceleration, 0f, maxAcceleration);
            //apply the calculation acceleration straing upward.
            pelvis.AddForce(Vector3.up * supportAcceleration, ForceMode.Acceleration);


        }
        else
        {
            supportAcceleration = 0f;
            return;
        }
    }
}