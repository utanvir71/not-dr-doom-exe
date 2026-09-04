using UnityEngine;


// This script gives Max a SMALL upward helping force
// while at least one foot is touching the ground.
//
// IMPORTANT:
// This is not supposed to make Max float.
// It only helps reduce how hard gravity crushes the ragdoll.
public class GroundSupport : MonoBehaviour
{
    // Creates a heading in the Inspector.
    [Header("References")]


    // Drag Max's pelvis Rigidbody here.
    public Rigidbody pelvis;


    // Drag the FootSupportTracker component here.
    //
    // We use it only to ask:
    // "Is at least one foot touching the ground?"
    public FootSupportTracker footSupportTracker;


    // Creates another heading in the Inspector.
    [Header("Support Settings")]


    // How much upward acceleration we add.
    //
    // Start LOW.
    //
    // Too high:
    // Max starts floating.
    //
    // Too low:
    // it won't help enough.
    public float upwardSupport = 2f;


    // Maximum upward speed at which
    // we still allow support force.
    //
    // This prevents the support system
    // from continuing to push Max upward
    // if he's already moving upward fast.
    public float maxUpwardVelocity = 1f;


    // FixedUpdate runs with Unity's physics.
    private void FixedUpdate()
    {
        // If we forgot to assign the pelvis,
        // stop this physics frame.
        if (pelvis == null)
        {
            return;
        }


        // If we forgot to assign the foot tracker,
        // stop this physics frame.
        if (footSupportTracker == null)
        {
            return;
        }


        // If neither foot is touching the ground,
        // Max should not receive any support force.
        if (!footSupportTracker.hasSupport)
        {
            return;
        }


        // If Max is already moving upward
        // faster than our allowed limit,
        // don't keep pushing him upward.
        if (pelvis.linearVelocity.y > maxUpwardVelocity)
        {
            return;
        }


        // Apply a small upward acceleration
        // to the pelvis.
        //
        // ForceMode.Acceleration means
        // tuning is easier because mass matters less.
        pelvis.AddForce(
            Vector3.up * upwardSupport,
            ForceMode.Acceleration
        );
    }
}