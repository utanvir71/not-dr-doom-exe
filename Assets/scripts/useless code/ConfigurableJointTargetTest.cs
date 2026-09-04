using UnityEngine;


// This script is ONLY a test.
//
// Its job is:
// "Take a simple Euler angle offset and correctly convert it
// into ConfigurableJoint target-rotation space."
//
// Once this works on Knee.L, we can reuse the same math
// for stepping, walking, and later RL.
public class ConfigurableJointTargetTest : MonoBehaviour
{
    // The ConfigurableJoint on this same bone.
    public ConfigurableJoint joint;


    // Simple angle we can edit in the Inspector.
    //
    // Example:
    // X = 20
    // or
    // X = -20
    //
    // We will test X/Y/Z one at a time.
    public Vector3 targetEulerOffset;


    // Remembers how this bone was rotated
    // when the game started.
    private Quaternion startingLocalRotation;


    private void Awake()
    {
        // If we forgot to assign the joint manually,
        // automatically grab the ConfigurableJoint
        // from this same GameObject.
        if (joint == null)
        {
            joint = GetComponent<ConfigurableJoint>();
        }


        // Stop if there is no ConfigurableJoint.
        if (joint == null)
        {
            Debug.LogError(
                "ConfigurableJointTargetTest: No ConfigurableJoint found.",
                this
            );

            enabled = false;

            return;
        }


        // Save Knee.L's REAL starting rotation.
        //
        // This becomes our neutral standing pose.
        startingLocalRotation = transform.localRotation;
    }


    private void FixedUpdate()
    {
        // Convert our simple Euler offset
        // into a Quaternion.
        Quaternion offsetRotation =
            Quaternion.Euler(targetEulerOffset);


        // Apply that offset on top of
        // the bone's original starting pose.
        Quaternion desiredLocalRotation =
            startingLocalRotation *
            offsetRotation;


        // Convert the desired bone rotation
        // into ConfigurableJoint targetRotation space.
        SetTargetRotationLocal(
            joint,
            desiredLocalRotation,
            startingLocalRotation
        );
    }


    private void SetTargetRotationLocal(
        ConfigurableJoint targetJoint,
        Quaternion desiredLocalRotation,
        Quaternion startLocalRotation
    )
    {
        // ConfigurableJoint has its own coordinate system.
        //
        // "axis" represents one direction of that system.
        Vector3 jointRight =
            targetJoint.axis;


        // Build another perpendicular direction
        // using Axis and Secondary Axis.
        Vector3 jointForward =
            Vector3.Cross(
                targetJoint.axis,
                targetJoint.secondaryAxis
            ).normalized;


        // Build the third perpendicular axis.
        Vector3 jointUp =
            Vector3.Cross(
                jointForward,
                jointRight
            ).normalized;


        // Create a Quaternion representing
        // the ConfigurableJoint's coordinate system.
        Quaternion jointSpace =
            Quaternion.LookRotation(
                jointForward,
                jointUp
            );


        // Start converting from normal local rotation
        // into the strange inverted coordinate system
        // ConfigurableJoint.targetRotation expects.
        Quaternion result =
            Quaternion.Inverse(jointSpace);


        // Calculate how far the desired pose is
        // from the original starting pose.
        //
        // ConfigurableJoint targetRotation works
        // in the opposite direction, which is why
        // desiredLocalRotation is inverted here.
        result *=
            Quaternion.Inverse(desiredLocalRotation) *
            startLocalRotation;


        // Convert the result back into joint space.
        result *=
            jointSpace;


        // Finally give Unity the correctly converted
        // target rotation.
        targetJoint.targetRotation =
            result;
    }
}