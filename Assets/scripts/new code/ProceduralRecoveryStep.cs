using System;
using NUnit.Framework.Constraints;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class ProceduralRecoveryStep : MonoBehaviour
{
    [Header("Reference")]
    public Rigidbody pelvis;
    public Transform forwardReference;
    public ConfigurableJoint leftThigh;
    public ConfigurableJoint rightThigh;
    public ConfigurableJoint leftLowerLeg;
    public ConfigurableJoint rightLowerLeg;

    [Header("Step Detection")]
    //!Max must drift faster than this before he takes a step/
    public float triggerSpeed = 0.35f;

    
    [Header("Step Pose")]
    public float forwardStepAngle = 50f;
    public float backwardStepAngle = -500f;
    public float kneeBendAngle = 110f;

    [Header("Bone Directions")]
    public float leftThighSign = -1f;
    public float rightThighSign = -1f;
    public float leftKneeSign = -1f;
    public float rightKneeSign = -1f;

    [Header("Timing")]
    public float liftTime = 0.12f;
    public float swingTime = 0.18f;
    public float plantTime = 0.12f;
    public float recoverTime = 0.30f;
    public float stepCooldown = 0.12f;

    [Header("Debug")]
    public float forwardSpeed;
    public bool isStepping;

    //?Remember the normal starting rotation of the thigh and leg.
    private Quaternion leftThighStart;
    private Quaternion rightThighStart;
    private Quaternion leftLowerLegStart;
    private Quaternion rightLowerLegStart;

    //which leg to use next and then remember
    private bool useLeftLegNext = true;
    private bool currentStepUsesLeft;

    //Stores the target thigh angle for the current step.
    private float currentSteAngle; // +40 to forward and -40 to backward
    private float phaseTimer; //time of current phase
    private float cooldownTimer; //countdown before antother steps



    // State of machine. these are the dirrefrent stages of one procedural steps.
    private enum StepPhase
    {
        Idle,
        Lift,
        Swing,
        Plant,
        Recover
    }
    private StepPhase phase = StepPhase.Idle;
    private bool fallingRight;

    

    private void Awake()
    {
        if(
            pelvis == null ||
            forwardReference == null ||
            leftThigh == null ||
            rightThigh == null ||
            leftLowerLeg == null ||
            rightLowerLeg == null
        )
        {
            Debug.LogError("ForwrdBackStepRecovery: Missing reference.", this);
             enabled = false;
            return;
        }
        //Save Max's normal standing rotations,
        //Every procedural step will be calcuated relative to these poses.
        leftThighStart = leftThigh.transform.localRotation;//localRotation gives object's rotation relative to its parent, not relative to the whole world.
        rightThighStart = rightThigh.transform.localRotation;
        leftLowerLegStart = leftLowerLeg.transform.localRotation;
        rightLowerLegStart = rightLowerLeg.transform.localRotation;
        SetBothLegsNeutral(); // Make sure both legs begin at the normal pose.
    }
    private void FixedUpdate()
    {
        
        //Count the cooldown down toward zero. While this avobe 0 Max cannot start another step.
        if(cooldownTimer > 0f)
        {
            cooldownTimer -= Time.fixedDeltaTime;
        }
        //Get the direction Max is face.
        Vector3 forward = Vector3.ProjectOnPlane(//ProjectOnPlane removes any vertical Y moment,
            forwardReference.forward, // gives us its blue Z direction.
            Vector3.up
        ).normalized;
        //Measure how fast the pelvis is moving
        forwardSpeed = Vector3.Dot(//dot takes two vectors in a vector
            pelvis.linearVelocity,
            forward
        );

        //! --- Idle ----
        //f Max is currently waiting and not already stepping..
        if(phase == StepPhase.Idle)
        {
            //keep both legs in their normal standing pose.
            SetBothLegsNeutral();
            //if a previous step just finished.
            //wait for the cooldown before allowing another one.
            if (cooldownTimer > 0f)
            {
                return;
            }
            //Positive forwardSpeed means the pelvis is drifting forward.
            if(forwardSpeed > triggerSpeed)
            {

                StartStep(forwardStepAngle);
                return;
            }
            //Negative forwardSpeed means the pelvis is drifting backward.
            if(forwardSpeed < -triggerSpeed)
            {
                //Start a recovery step in the backward direction.
                StartStep(backwardStepAngle);
                return;
            }
            //Max is not drfing enoght to requre a step.
            return;
        }

        //! --- Lift ---
        //If a step has started, the first job is to lift the foot.
        if(phase == StepPhase.Lift)
        {
            //Add the amount of the physics time that phassed.
            phaseTimer += Time.fixedDeltaTime;

            // covert ourt timer into a number between 0 and 1
            float t = Mathf.Clamp01(phaseTimer/liftTime);

            //smoothly increase the knee bend:
            float kneeAngle = Mathf.SmoothStep(0f,kneeBendAngle, t);
            //Keep the thing at its normal position
            //but bend the lower leg to lift the foot.
            ApplySteppingPose(0f, kneeAngle);

            //Once t reches 1, lifting is finished.
            if (t >= 1)
            {
                //Move to the next stage of the step.
                phase = StepPhase.Swing;
                //Reset the timer because Swing needs its own timer.
                phaseTimer = 0f;
            }
            //Nothing else should happen during this physics tick.
            return;
        }
        
        //! --- Swing ---
        //AFter the floot is lifted, move the thigh forward or bacward.
        if(phase == StepPhase.Swing)
        {
            phaseTimer += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(phaseTimer)/swingTime;
            float thighAngle = Mathf.SmoothStep(0f, currentSteAngle, t);
            ApplySteppingPose(thighAngle, kneeBendAngle);
            if(t >= 1f)
            {
                phase = StepPhase.Plant;
                phaseTimer = 0f;
            }
            return;
        }

        //! --- Plant ---
        //The thigh has already swung to the new step position.
        //Now stargihten the lower leg so the foot can reach the ground.
        if (phase == StepPhase.Plant)
        {
            phaseTimer += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(phaseTimer / plantTime);
            float kneeAngle = Mathf.SmoothStep(kneeBendAngle, 0f, t);
            ApplySteppingPose(currentSteAngle, kneeAngle);
            //when the knee is fully straightened...
            if (t >= 1f)
            {
                //Move the recovery phase.
                phase = StepPhase.Recover;
                phaseTimer= 0f;
            }
        }
        

        //! --- Recover ---
        if(phase == StepPhase.Recover)
        {
            phaseTimer += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(phaseTimer/recoverTime);
            float thighAngle = Mathf.SmoothStep(currentSteAngle, 0f, t);
            ApplySteppingPose(thighAngle,0f);
            if (t >= 1f)
            {
                //Go back to waiting for the next drift.
                phase = StepPhase.Idle;
                isStepping = false;
                SetBothLegsNeutral();
                cooldownTimer = stepCooldown;
            }
        }
    }

    //this function sets up a new step and switch the state machine from IDLE to LIFT
    private void StartStep(float stepAngle)
    {
        currentStepUsesLeft = useLeftLegNext; //Decide which leg will take the step
        useLeftLegNext = !useLeftLegNext; //Alternate legs for the next step;
        currentSteAngle = stepAngle; //Store wheter this is a foward or bacward step.
        phase = StepPhase.Lift; // Every step starts by lifting the foot.
        phaseTimer = 0f;//reset the timer for the lift phase
        isStepping = true;//usefull for watching the state in Instpector;
    }

        private void ApplySteppingPose(float thighAngle, float kneeAngle)
    {
        // If the current tep uses the left leg..
        if (currentStepUsesLeft)
        {
            SetjointXOffset(leftThigh, leftThighStart, thighAngle * leftThighSign);//Move the left thigh.
            SetjointXOffset(leftLowerLeg, leftLowerLegStart, kneeAngle * leftKneeSign);//Bend of starigthen the left lower leg.
            SetjointXOffset(rightThigh, rightThighStart, 0f); //keep the right thigh in its normal pose.
            SetjointXOffset(rightLowerLeg, rightLowerLegStart, 0f); // keep the right lower leg in its normal pose.
        }
        else
        {
            SetjointXOffset(leftThigh, leftThighStart, 0f);//keep the left thigh in its neutral
            SetjointXOffset(leftLowerLeg, leftLowerLegStart, 0f);//keep the lower leg neutral
            SetjointXOffset(rightThigh, rightThighStart, thighAngle * rightThighSign);
            SetjointXOffset(rightLowerLeg, rightLowerLegStart, kneeAngle * rightKneeSign);
        }
    }


    private void SetBothLegsNeutral()
    {
        SetjointXOffset(leftThigh, leftThighStart, 0f);//put the left thigh back at its original standing pose.
        SetjointXOffset(rightThigh, rightThighStart, 0f);
        SetjointXOffset(leftLowerLeg, leftLowerLegStart, 0f);
        SetjointXOffset(rightLowerLeg, rightLowerLegStart, 0f);
    }


    // Moves leg forward or backward.
    private void SetjointXOffset(ConfigurableJoint joint, Quaternion startingRotation, float xAngle)
    {
        //Create a rotation around the bone's X axis.
        Quaternion offset = Quaternion.Euler(xAngle, 0f, 0f);

        //Add that procedural rotation on top of the bone's original standing rotation
        Quaternion desiredRotation = startingRotation * offset;

        //Convert our normal local rotation into the special rotation format ConfigurableJoint.targetRotation expects.
        SetTargetRotationLocal(joint, desiredRotation, startingRotation);
    }


    private void SetTargetRotationLocal(ConfigurableJoint joint, Quaternion TargetLocalRotation, Quaternion startLocalRotation)
    {
        Vector3 right = joint.axis; //get the configurableJoint's man axis.
        //calculate a forward axis using the joint's man axis and secondary axis.
        Vector3 forward = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
        //calcuate the remaining up axis.
        Vector3 up = Vector3.Cross(forward, right).normalized;
        //Build a rotation that represents the joint's own coordinate system.
        Quaternion worldToJointSpace = Quaternion.LookRotation(forward, up);
        //Start converting our desired rotation into joint space.
        Quaternion resultRotation = Quaternion.Inverse(worldToJointSpace);
        //calcuate the desire rotation relative to the starting pose.
        resultRotation *= Quaternion.Inverse(TargetLocalRotation) * startLocalRotation;
        //Convert the result back through the joint coordinate system.
        resultRotation *= worldToJointSpace;
        //this is the acutal target the physical joint will try to reach.
        joint.targetRotation = resultRotation;
    }   

}