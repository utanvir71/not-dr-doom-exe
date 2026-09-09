using UnityEngine;
using UnityEngine.EventSystems;

//This script moves physical body using force
//it does not controll his legs or procedural stepping.
public class PhysicsMoveMentController : MonoBehaviour
{
    public Rigidbody pelvis;
    //This Transform tells us which directin max consier forward.
    public Transform movementReference;
    public float moveAcceleration = 5f;
    private Vector3 moveInput;
    public void Awake()
    {
        if(pelvis == null)
        {
            pelvis = GetComponent<Rigidbody>();
        }
    }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(horizontal, 0f, vertical);
    }
    private void FixedUpdate()
    {
        Vector3 forward = movementReference.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 right = movementReference.right;
        right.y = 0f;
        right.Normalize();
        Vector3 moveDirection = forward * moveInput.z + right * moveInput.x;
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);
        pelvis.AddForce(moveDirection * moveAcceleration, ForceMode.Acceleration);
    }
}