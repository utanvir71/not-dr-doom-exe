using UnityEngine;

public class PhysicsMoveMentController : MonoBehaviour
{
    public Rigidbody pelvis;

    // Defines which direction Max considers forward
    public Transform movementReference;

    public float moveAcceleration = 5f;

    // Start low and increase later if needed
    public float turnAcceleration = 10f;

    private Vector3 moveInput;
    private float turnInput;

    private bool useExternalInput = false;


    private void Awake()
    {
        if (pelvis == null)
        {
            pelvis = GetComponent<Rigidbody>();
        }
    }


    private void Update()
    {
        // Manual keyboard control only when AI
        // is not controlling Max
        if (!useExternalInput)
        {
            float horizontal =
                Input.GetAxisRaw("Horizontal");

            float vertical =
                Input.GetAxisRaw("Vertical");

            moveInput =
                new Vector3(
                    horizontal,
                    0f,
                    vertical
                );


            // Manual turning
            if (Input.GetKey(KeyCode.A))
            {
                turnInput = -1f;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                turnInput = 1f;
            }
            else
            {
                turnInput = 0f;
            }
        }
    }


    public void SetMovementInput(
        float forward,
        float horizontal)
    {
        useExternalInput = true;

        moveInput =
            new Vector3(
                horizontal,
                0f,
                forward
            );
    }


    public void SetTurnInput(float turn)
    {
        useExternalInput = true;

        turnInput = turn;
    }


    public void ReturnToKeyboardControl()
    {
        useExternalInput = false;

        moveInput = Vector3.zero;
        turnInput = 0f;
    }


    private void FixedUpdate()
    {
        // -----------------------------
        // FORWARD / SIDE MOVEMENT
        // -----------------------------

        Vector3 forward =
            movementReference.forward;

        forward.y = 0f;
        forward.Normalize();


        Vector3 right =
            movementReference.right;

        right.y = 0f;
        right.Normalize();


        Vector3 moveDirection =
            forward * moveInput.z +
            right * moveInput.x;


        moveDirection =
            Vector3.ClampMagnitude(
                moveDirection,
                1f
            );


        pelvis.AddForce(
            moveDirection * moveAcceleration,
            ForceMode.Acceleration
        );


        // -----------------------------
        // TURNING
        // -----------------------------

        pelvis.AddTorque(
            Vector3.up *
            turnInput *
            turnAcceleration,
            ForceMode.Acceleration
        );
    }
}