using UnityEngine;


// This is Max's main balance controller.
//
// It does four jobs:
//
// 1. Calculates Max's whole-body Center Of Mass.
// 2. Detects whether the feet are touching the ground.
// 3. Tries to keep the pelvis upright.
// 4. Gently moves the pelvis toward the foot support area.
//
// We keep all of this inside ONE script so different
// balance scripts don't fight each other.
public class BalanceController : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("References")]


    // Max's main pelvis Rigidbody.
    public Rigidbody pelvis;


    // Left foot bone.
    public Transform leftFoot;


    // Right foot bone.
    public Transform rightFoot;


    // =========================================================
    // GROUND DETECTION
    // =========================================================

    [Header("Ground Detection")]


    // Only objects on this layer count as ground.
    public LayerMask groundMask;


    // Ray starts slightly above the foot.
    public float rayStartHeight = 0.15f;


    // Maximum distance downward that we check for ground.
    public float rayDistance = 0.5f;


    // =========================================================
    // UPRIGHT CONTROL
    // =========================================================

    [Header("Upright Control")]


    // How strongly Max tries to keep his pelvis vertical.
    public float uprightStrength = 50f;


    // Slows down rotational wobbling.
    public float uprightDamping = 20f;


    // Prevents ridiculously large upright torque.
    public float maxUprightAcceleration = 50f;


    // =========================================================
    // CENTER OF MASS BALANCE
    // =========================================================

    [Header("Center Of Mass Balance")]


    // How strongly Max moves his body back toward his feet.
    //
    // Keep this LOW initially.
    public float balanceStrength = 1f;


    // Slows horizontal movement so Max does not overshoot.
    public float balanceDamping = 1f;


    // Maximum horizontal correction acceleration.
    public float maxBalanceAcceleration = 1f;


    // Ignore extremely tiny balance errors.
    public float balanceDeadZone = 0.03f;


    // =========================================================
    // OPTIONAL UPWARD SUPPORT
    // =========================================================

    [Header("Optional Upward Support")]


    // Leave this OFF initially.
    //
    // We already discovered that too much upward support
    // can add energy and make Max jiggle.
    public bool useUpwardSupport = false;


    // Small upward acceleration while grounded.
    public float upwardSupport = 1f;


    // =========================================================
    // DEBUG INFORMATION
    // =========================================================

    [Header("Debug Information")]


    // Max's calculated whole-body Center Of Mass.
    public Vector3 centerOfMass;


    // Total mass of all Max's Rigidbodies.
    public float totalMass;


    // Number of Rigidbodies found.
    public int bodyCount;


    // Is the left foot touching the ground?
    public bool leftGrounded;


    // Is the right foot touching the ground?
    public bool rightGrounded;


    // Is at least one foot supporting Max?
    public bool hasSupport;


    // Ground point underneath the left foot.
    public Vector3 leftGroundPoint;


    // Ground point underneath the right foot.
    public Vector3 rightGroundPoint;


    // Current center of Max's support area.
    public Vector3 supportCenter;


    // Horizontal difference between COM and support.
    public Vector3 balanceError;


    // Distance between COM and support horizontally.
    public float balanceErrorDistance;


    // Draw useful lines in Scene View.
    public bool showDebug = true;


    // =========================================================
    // PRIVATE DATA
    // =========================================================


    // Stores all Rigidbodies belonging to Max.
    private Rigidbody[] bodies;


    // Stores which local direction represented
    // "up" when Max started standing.
    //
    // This avoids problems caused by weird Blender bone axes.
    private Vector3 pelvisLocalUp;


    // =========================================================
    // STARTUP
    // =========================================================

    private void Awake()
    {
        // Make sure the pelvis was assigned.
        if (pelvis == null)
        {
            Debug.LogError(
                "BalanceController: Pelvis is not assigned.",
                this
            );

            // Disable the script because it cannot work.
            enabled = false;

            return;
        }


        // Make sure both feet were assigned.
        if (leftFoot == null || rightFoot == null)
        {
            Debug.LogError(
                "BalanceController: Feet are not assigned.",
                this
            );

            enabled = false;

            return;
        }


        // Find every Rigidbody underneath this Armature.
        bodies =
            GetComponentsInChildren<Rigidbody>();


        // Save how many we found.
        bodyCount =
            bodies.Length;


        // Remember which direction inside the pelvis
        // corresponds to world-up at the starting pose.
        pelvisLocalUp =
            pelvis.transform.InverseTransformDirection(
                Vector3.up
            );


        // Print useful startup information.
        Debug.Log(
            "BalanceController found " +
            bodyCount +
            " Rigidbodies.",
            this
        );
    }


    // =========================================================
    // PHYSICS LOOP
    // =========================================================

    private void FixedUpdate()
    {
        // First calculate Max's whole-body COM.
        CalculateCenterOfMass();


        // Then check both feet.
        UpdateFootSupport();


        // Try to keep the pelvis upright.
        ApplyUprightControl();


        // Only perform standing balance
        // while at least one foot is touching ground.
        if (hasSupport)
        {
            // Correct horizontal COM drift.
            ApplyCenterOfMassBalance();


            // Only use upward help if we explicitly enable it.
            if (useUpwardSupport)
            {
                ApplyUpwardSupport();
            }
        }
        else
        {
            // If Max is airborne,
            // there is no foot-support balance error.
            balanceError =
                Vector3.zero;


            balanceErrorDistance =
                0f;
        }


        // Draw visual debugging information.
        if (showDebug)
        {
            DrawDebug();
        }
    }


    // =========================================================
    // WHOLE-BODY CENTER OF MASS
    // =========================================================

    private void CalculateCenterOfMass()
    {
        // Stores the sum of:
        //
        // Rigidbody position × Rigidbody mass.
        Vector3 weightedPosition =
            Vector3.zero;


        // Start total mass from zero every physics frame.
        totalMass =
            0f;


        // Go through every Rigidbody belonging to Max.
        foreach (Rigidbody body in bodies)
        {
            // Ignore missing references.
            if (body == null)
            {
                continue;
            }


            // Heavier body parts contribute more
            // to the final Center Of Mass.
            weightedPosition +=
                body.worldCenterOfMass *
                body.mass;


            // Add this body's mass to Max's total.
            totalMass +=
                body.mass;
        }


        // Avoid dividing by zero.
        if (totalMass <= 0f)
        {
            centerOfMass =
                pelvis.worldCenterOfMass;

            return;
        }


        // Calculate the weighted average position.
        centerOfMass =
            weightedPosition /
            totalMass;
    }


    // =========================================================
    // FOOT SUPPORT
    // =========================================================

    private void UpdateFootSupport()
    {
        // Check the left foot.
        leftGrounded =
            CheckFoot(
                leftFoot,
                out leftGroundPoint
            );


        // Check the right foot.
        rightGrounded =
            CheckFoot(
                rightFoot,
                out rightGroundPoint
            );


        // If both feet are grounded...
        if (leftGrounded && rightGrounded)
        {
            // Support center becomes the midpoint
            // between both ground contact points.
            supportCenter =
                (
                    leftGroundPoint +
                    rightGroundPoint
                ) * 0.5f;


            hasSupport =
                true;
        }


        // If only the left foot is grounded...
        else if (leftGrounded)
        {
            // Left foot becomes the support point.
            supportCenter =
                leftGroundPoint;


            hasSupport =
                true;
        }


        // If only the right foot is grounded...
        else if (rightGrounded)
        {
            // Right foot becomes the support point.
            supportCenter =
                rightGroundPoint;


            hasSupport =
                true;
        }


        // If neither foot is grounded...
        else
        {
            hasSupport =
                false;


            supportCenter =
                Vector3.zero;
        }
    }


    // Checks one foot for ground.
    private bool CheckFoot(
        Transform foot,
        out Vector3 groundPoint
    )
    {
        // Start the ray slightly above the foot.
        Vector3 rayStart =
            foot.position +
            Vector3.up *
            rayStartHeight;


        // Shoot a ray downward.
        bool hitGround =
            Physics.Raycast(
                rayStart,
                Vector3.down,
                out RaycastHit hit,
                rayDistance,
                groundMask,
                QueryTriggerInteraction.Ignore
            );


        // If we hit ground...
        if (hitGround)
        {
            // Store the exact point of contact.
            groundPoint =
                hit.point;
        }
        else
        {
            // No ground was found.
            groundPoint =
                foot.position;
        }


        // Draw the foot ray.
        if (showDebug)
        {
            // Green means grounded.
            // Red means not grounded.
            Color color =
                hitGround
                ? Color.green
                : Color.red;


            Debug.DrawRay(
                rayStart,
                Vector3.down * rayDistance,
                color
            );
        }


        // Return whether ground was detected.
        return hitGround;
    }


    // =========================================================
    // UPRIGHT CONTROL
    // =========================================================

    private void ApplyUprightControl()
    {
        // Find Max's current anatomical-up direction.
        Vector3 currentUp =
            pelvis.transform.TransformDirection(
                pelvisLocalUp
            );


        // Find the axis required to rotate
        // currentUp toward world-up.
        Vector3 tiltAxis =
            Vector3.Cross(
                currentUp,
                Vector3.up
            );


        // Bigger tilt means stronger correction.
        Vector3 correction =
            tiltAxis *
            uprightStrength;


        // Get pelvis angular velocity.
        Vector3 angularVelocity =
            pelvis.angularVelocity;


        // Ignore rotation around the vertical axis.
        //
        // We want Max to eventually be able
        // to turn left and right.
        Vector3 tippingVelocity =
            Vector3.ProjectOnPlane(
                angularVelocity,
                Vector3.up
            );


        // Resist rapid tipping.
        Vector3 damping =
            -tippingVelocity *
            uprightDamping;


        // Combine correction and damping.
        Vector3 finalTorque =
            correction +
            damping;


        // Prevent enormous torque.
        finalTorque =
            Vector3.ClampMagnitude(
                finalTorque,
                maxUprightAcceleration
            );


        // Apply physical torque to the pelvis.
        pelvis.AddTorque(
            finalTorque,
            ForceMode.Acceleration
        );
    }


    // =========================================================
    // CENTER OF MASS BALANCE
    // =========================================================

    private void ApplyCenterOfMassBalance()
    {
        // Calculate the direction from
        // Max's COM back toward his support center.
        balanceError =
            supportCenter -
            centerOfMass;


        // Remove vertical error.
        //
        // We only care about horizontal balance.
        balanceError.y =
            0f;


        // Store the current error distance.
        balanceErrorDistance =
            balanceError.magnitude;


        // Ignore tiny differences.
        if (
            balanceErrorDistance <
            balanceDeadZone
        )
        {
            balanceError =
                Vector3.zero;

            return;
        }


        // Calculate proportional correction.
        //
        // Larger error means stronger correction.
        Vector3 correction =
            balanceError *
            balanceStrength;


        // Get pelvis horizontal velocity.
        Vector3 horizontalVelocity =
            Vector3.ProjectOnPlane(
                pelvis.linearVelocity,
                Vector3.up
            );


        // Resist horizontal overshooting.
        Vector3 damping =
            -horizontalVelocity *
            balanceDamping;


        // Combine the two.
        Vector3 finalAcceleration =
            correction +
            damping;


        // Limit the maximum correction.
        finalAcceleration =
            Vector3.ClampMagnitude(
                finalAcceleration,
                maxBalanceAcceleration
            );


        // Apply real physical force.
        pelvis.AddForce(
            finalAcceleration,
            ForceMode.Acceleration
        );
    }


    // =========================================================
    // OPTIONAL UPWARD SUPPORT
    // =========================================================

    private void ApplyUpwardSupport()
    {
        // Apply a small upward acceleration
        // while Max has ground support.
        pelvis.AddForce(
            Vector3.up *
            upwardSupport,
            ForceMode.Acceleration
        );
    }


    // =========================================================
    // DEBUG DRAWING
    // =========================================================

    private void DrawDebug()
    {
        // Draw the whole-body COM in yellow.
        Debug.DrawLine(
            centerOfMass,
            centerOfMass + Vector3.up * 0.5f,
            Color.yellow
        );


        // Only draw support information
        // when Max actually has support.
        if (!hasSupport)
        {
            return;
        }


        // Draw support center in green.
        Debug.DrawLine(
            supportCenter,
            supportCenter + Vector3.up * 0.5f,
            Color.green
        );


        // Project COM down to support height.
        Vector3 projectedCOM =
            new Vector3(
                centerOfMass.x,
                supportCenter.y,
                centerOfMass.z
            );


        // Red line shows how far Max's COM
        // is horizontally from his foot support.
        Debug.DrawLine(
            projectedCOM,
            supportCenter,
            Color.red
        );
    }
}