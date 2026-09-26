using UnityEngine;

//controls max procedural leg movement while he is intentionally moving.
public class ProceduralMovement : MonoBehaviour
{
    [Header("References")]
    public Rigidbody pelivs;
    public Transform movementReference;
    public ConfigurableJoint leftThigh;
    public ConfigurableJoint rightThigh;
    public ConfigurableJoint leftLowerLeg;
    public ConfigurableJoint rightLowerLeg;

    [Header("Starting Pose")]
    [SerializeField]
    private Quaternion leftThighStart;
    [SerializeField]
    private Quaternion rightThighStart;
    [SerializeField]
    private Quaternion leftLowerLegStart;
    [SerializeField]
    private Quaternion rightLowerLegStart;

    [Header("Forward Step Settings")]
    //how far the active thigh rotates forward.
    public float forwardThighAngle = 55f;
    //how much the lower leg counter-rotaes
    //while the thigh lifts forward.
    //this helps keep the skin roughly vertical in 
    public float lowerLegCounterAngle = -120f;
    //How long the lifting phase takes.
    public float lifttime = 0.15f;
    //how long  the reaching phase takes.
    public float reachTime = 0.15f;
    //How long the planting phase takes.
    public float plantTime = 0.12f;

    [Header("Backword Step Settings")]
    //How far the active thigh rotates backward.
    public float backwardThighAngle = 90f;
    //how far the lower leg roates first while the thigh stays in its normal position.
    public float backwardLowerLegAngle = -120f;
    
    [Header("Sidewyas Step Settings")]
    //how far an upper leg moves outward during a sideways step.
    public float sidewaysThighAngle = 25f;
    //let us correct the thigh's locall Z direction from the insepcter
    public float leftSideSign = 1f;
    public float rightSideSign = -1f;
    //How long max spend shifting his weight on the support legt before stepping sideways.
    public float sideShiftTime = 0.12f;
    //How strongly the pelvis is pushed toward the support leg during the weight shift.
    public float sideShiftAcceleration = 3f;
    

    [Header("Bone Direction")]
    //converts our logical thigh and lower leg angle into Max's actual local X rotation.
    public float leftThighSign = -1f;
    public float rightThighSign = -1f;
    public float leftLowerLegSign = -1f;
    public float rightLowerLegSign = -1f;

    
    //Stores which direction the current step is trying to move.
    private enum StepDirection
    {
        None,
        Forward,
        Backward,
        Left, 
        Right
    }
    [SerializeField]
    private StepDirection stepDirection = StepDirection.None;
    // These are the different phases of me forward procedural step
    private enum WalkPhase
    {
        Idle,
        SideShift,
        Lift,
        Reach,
        Plant
    }
    [Header("Waling State")]
    [SerializeField]
    private WalkPhase walkPhase = WalkPhase.Idle;
    //True = left leg is taking the current step.
    //False = right leg is taking th current step.
    [SerializeField]
    private bool currentStepUsesLeft = true;
    //this tells us which leg shouold take the next step.
    private bool useLeftLegNext = true;
    //Tracks how long Max has been inside the current walking phase.
    private float phaseTimer;

    [Header("Movement Input Debug")]
    //Stores W/S input so we can it in the Inspector.
    // W = +1
    //s=-1
    //nothing = 0
    [SerializeField]
    private float forwardInput;
    private float horizontalInput;


    private void Awake()
    {
        if(pelivs == null)
        {
            pelivs = GetComponent<Rigidbody>();
        }
        //Save the left thigh's normal local rotation.
        leftThighStart = leftThigh.transform.localRotation;
        //Save the right thigh's normal local rotation
        rightThighStart = rightThigh.transform.localRotation;
        //Save the left lower leg's normal local rotation.
        leftLowerLegStart = leftLowerLeg.transform.localRotation;
        //save the right lower leg's normal local rotation
        rightLowerLegStart = rightLowerLeg.transform.localRotation;
    }

    private void Update()
    {
        //read forward/bacward moveement input
        // forwardInput = Input.GetAxisRaw("Vertical");
        // horizontalInput = Input.GetAxisRaw("Horizontal");

    }

    private void FixedUpdate()
    {
        //only start a new step when max is currently waiting.
        if (walkPhase == WalkPhase.Idle)
        {
            //When max is not stepping, keep both legs in the normal pose.
            SetBothLegsNeutral();
            //only start forward walking when W is being held
            if (forwardInput > 0f)
            {
                //Remember what kind of step Max is doing.
                stepDirection = StepDirection.Forward;
                //chose which leg will take this step.
                currentStepUsesLeft =  useLeftLegNext;
                //nake the opposite leg take the next step
                useLeftLegNext = !useLeftLegNext;
                //Every forward stepbegins with the Lift Phase.
                walkPhase = WalkPhase.Lift;
                //start the lift timer from zero
                phaseTimer = 0f;

            }
            else if (forwardInput < 0f)
            {
                //Remember that this is a backward step.
                stepDirection = StepDirection.Backward;
                //Chose which leg takes this step.
                currentStepUsesLeft = useLeftLegNext;
                //Alternate leg for the next step.
                useLeftLegNext = !useLeftLegNext;
                //Backward stepping also begins in lift.
                walkPhase = WalkPhase.Lift;
                //reset the timer.
                phaseTimer = 0f;
            }
            else if (horizontalInput < 0f)
            {
                //remember that max is trying to move right
                stepDirection = StepDirection.Left;
                // Sidewyas movement also begins in lift.
                walkPhase = WalkPhase.SideShift;
                //Reset the phase timer.
                phaseTimer = 0f;
            }
            else if(horizontalInput > 0f)
            {
                stepDirection = StepDirection.Right;
                walkPhase = WalkPhase.SideShift;
                phaseTimer=0f;
            }
        }
        if(walkPhase == WalkPhase.SideShift)
        {
            //Cont how long Max has been shifting his weight
            phaseTimer += Time.fixedDeltaTime;
            //convert the timer into a value from 0 to 1
            float t = Mathf.Clamp01(phaseTimer/sideShiftTime);
            //get max's right direction
            Vector3 right = movementReference.right;
            //remove any vertical direction.
            right.y = 0f;
            //make the direction length exactly 1
            right.Normalize();
            // max wants to stop right
            if(stepDirection == StepDirection.Right)
            {
                //first shift the pelvis left, so Max's weight moves onto the left support leg.
                pelivs.AddForce(-right*sideShiftAcceleration, ForceMode.Acceleration);
            }
            else if (stepDirection == StepDirection.Left)
            {
                //First shitf the pelvis right
                pelivs.AddForce(right * sideShiftAcceleration, ForceMode.Acceleration);
            }
            if (t >= 1f)
            {
                walkPhase = WalkPhase.Lift;
                phaseTimer = 0f;
            }
            return;
        }
        // Run this code while Max is in the Lift phase.
        if(walkPhase == WalkPhase.Lift)
        {
            //Increse the phase timer every physics frame.
            phaseTimer += Time.fixedDeltaTime;
            //Convert the timer into a value from 0 to 1
            float t = Mathf.Clamp01(phaseTimer / lifttime);
            
            
            if(stepDirection == StepDirection.Forward){
                //Gradually rotate the thigh forward.
                float thighAngle = Mathf.SmoothStep(0f, forwardThighAngle, t);
                //Gradually counter-rotate the lowerLeg.
                //This tries to keep the shin roughly verithycal while the upperleg moves forward.
                float lowerLegAngle = Mathf.SmoothStep(0f, lowerLegCounterAngle, t);
                //if the left leg is taking this step...
                if (currentStepUsesLeft)
                {
                    //Roteate thel left thigh forward.
                    SetJointXOffset(leftThigh, leftThighStart, thighAngle * leftThighSign);
                    //Counter-rotate the left lower leg.
                    SetJointXOffset(leftLowerLeg, leftLowerLegStart, lowerLegAngle * leftLowerLegSign);
                    //Keep the right lower leg in its normal standing pose.
                    SetJointXOffset(rightThigh, rightThighStart, 0f);
                    //keep the right lower leg in its normal standing pose
                    SetJointXOffset(rightLowerLeg, rightLowerLegStart, 0f);
                }
                // if the right leg is taking the step
                else
                {
                    //keep the left thigh neutral
                    SetJointXOffset(leftThigh, leftThighStart, 0f);
                    SetJointXOffset(leftLowerLeg, leftLowerLegStart, 0f);
                    SetJointXOffset(rightThigh, rightThighStart, thighAngle*rightThighSign);
                    SetJointXOffset(rightLowerLeg, rightLowerLegStart, lowerLegAngle*rightLowerLegSign);
                }
                //When lift reach 100%
                if(t >= 1f)
                {
                    //move the reach phase.
                    walkPhase = WalkPhase.Reach;
                    //Reset the timer because reach needs its own timer.
                    phaseTimer = 0f;
                }
                //Don't run any other walking phase this physics frame.
                return;
            }
            else if(stepDirection == StepDirection.Backward)
            {
                //The thigh stays in its normal positon
                float thighAngle = 0f;
                float lowerLegAgnle = Mathf.SmoothStep(0f, backwardLowerLegAngle, t);
                //left leg is steppiing backward.
                if (currentStepUsesLeft)
                {
                    //keep the thigh still.
                    SetJointXOffset(leftThigh, leftThighStart, thighAngle);
                    //roate the lower leg first.
                    SetJointXOffset(leftLowerLeg, leftLowerLegStart, lowerLegAgnle * leftLowerLegSign);
                    //keeep the other leg nutral
                    SetJointXOffset(rightThigh,rightThighStart, 0f);
                    //keep the other leg nutral
                    SetJointXOffset(rightLowerLeg, rightLowerLegStart, 0f);
                }
                else
                {
                    //keep the other leg neutral
                    SetJointXOffset(leftThigh, leftThighStart, 0f);
                    SetJointXOffset(leftLowerLeg, leftLowerLegStart, 0f);
                    //keep the high still
                    SetJointXOffset(rightThigh, rightThighStart, thighAngle);
                    //roate the lower leg first.
                    SetJointXOffset(rightLowerLeg, rightLowerLegStart, lowerLegAgnle * rightLowerLegSign);
                }
                //When lift reach 100%
                if(t >= 1f)
                {
                    //move the reach phase.
                    walkPhase = WalkPhase.Reach;
                    //Reset the timer because reach needs its own timer.
                    phaseTimer = 0f;
                }
                //Don't run any other walking phase this physics frame.
                return;
            }
            else if(stepDirection == StepDirection.Right)
            {
                //After shifting Max's wight onto the Left leg
                float sideAngle = Mathf.SmoothStep(0f, sidewaysThighAngle, t);
                //move the right thigh outward,
                SetJointZOffset(rightThigh, rightThighStart, -sideAngle * rightSideSign);
                //The left leg is supporting Max, so keep the left thigh neutral.
                SetJointZOffset(leftThigh, leftThighStart, 0f);
                //kkeep both lower legs  neutral for now.
                SetJointXOffset(rightLowerLeg, rightLowerLegStart, 0f);
                SetJointXOffset(leftLowerLeg, leftLowerLegStart, 0f);
                if (t >= 1f)
                {
                    //move to the next sidewaysPhase.
                    walkPhase = WalkPhase.Reach;
                    phaseTimer = 0f;
                
                }
                return;
            }
            else if(stepDirection == StepDirection.Left)
            {
                float sideAngle = Mathf.SmoothStep(0f, sidewaysThighAngle, t);
                SetJointZOffset(leftThigh, leftThighStart, -sideAngle * leftSideSign);
                SetJointZOffset(rightThigh, rightThighStart, 0f);
                SetJointZOffset(leftLowerLeg, leftLowerLegStart, 0f);
                SetJointZOffset(rightLowerLeg, rightLowerLegStart, 0f);
                if (t >= 1)
                {
                    walkPhase = WalkPhase.Reach;
                    phaseTimer = 0f;
                }
                return;
            }
            
        }
        //run this after the foot has been lifted.
        if(walkPhase == WalkPhase.Reach)
        {
            //Increase the timer for the reach phase
            phaseTimer += Time.fixedDeltaTime;
            //convert the timer into a value between 0 and 1
            float t = Mathf.Clamp01(phaseTimer/reachTime);
            if(stepDirection == StepDirection.Forward)
            {
                //keep the thigh fully forward during the phase.
                float thighAngle = forwardThighAngle;
                //Move the lower leg from its counter-rotate position back toward straigh. This extends the foot forward.
                float lowerLegAngle = Mathf.SmoothStep(lowerLegCounterAngle,0f,t);
                //Left leg is currently stepping.
                if (currentStepUsesLeft)
                {
                    //Keep the left Thihg forward.
                    SetJointXOffset(leftThigh, leftThighStart, thighAngle * leftThighSign);
                    //Straighten the left lower leg.
                    SetJointXOffset(leftLowerLeg, leftLowerLegStart, lowerLegAngle * leftLowerLegSign);
                    //Keep the right leg neutral,
                    SetJointXOffset(rightThigh, rightThighStart, 0f);
                    SetJointXOffset(rightLowerLeg, rightLowerLegStart, 0f);
                }
                //Right leg is currently stepping.
                else
                {
                    //keep the left leg nutral
                    SetJointXOffset(leftThigh, leftThighStart, 0f);
                    SetJointXOffset(leftLowerLeg, leftLowerLegStart, 0f);
                    //keep the right thigh forward and lower leg straight.
                    SetJointXOffset(rightThigh, rightThighStart, thighAngle * rightThighSign);
                    SetJointXOffset(rightLowerLeg, rightLowerLegStart, lowerLegAngle * rightLowerLegSign);
                }
                //Reach is complete
                if (t >= 1f)
                {
                    //The foot has reached forward.
                    //next we let it plant on the ground.
                    walkPhase = WalkPhase.Plant;
                    //reset the timer for plant.
                    phaseTimer = 0f;
                }
                return;
            }
            else if(stepDirection == StepDirection.Backward)
            {
                //Gradually roate the upperLeg backward.
                float thighAngle = Mathf.SmoothStep(0f, backwardThighAngle, t);
                //At the same time, straghten the lower leg.It started rotated during bacward lift, anD NOW RETURN TWARD ITS NORMAL ANGLE
                float lowerLegAngle = Mathf.SmoothStep(backwardLowerLegAngle , 0f, t);
                //Left leg is taking the backward step
                if(currentStepUsesLeft)
                {
                    //Move the left upper leg backwad.
                    SetJointXOffset(leftThigh, leftThighStart, thighAngle * -leftThighSign);
                    //straighten the left lower leg at the same time.
                    SetJointXOffset(leftLowerLeg, leftLowerLegStart, lowerLegAngle * -leftLowerLegSign);
                    //keep the right thigh neutral.
                    SetJointXOffset(rightThigh, rightThighStart, 0f);
                    //keep the right lower leg neutral.
                    SetJointXOffset(rightLowerLeg, rightLowerLegStart, 0f);
                }
                // RIGHT leg is taking the backward step.
                else
                {
                    //keep the left thigh neutral
                    SetJointXOffset(leftThigh, leftThighStart, 0f);
                    //keep the left lower leg neutral
                    SetJointXOffset(leftLowerLeg, leftLowerLegStart, 0f);
                    //Move the right uppper leg backward.
                    SetJointXOffset(rightThigh, rightThighStart, thighAngle * -rightThighSign);
                    //Straighten the right lower leg at the same time.
                    SetJointXOffset(rightLowerLeg, rightLowerLegStart, lowerLegAngle * -rightLowerLegSign);
                }
                 //Reach is complete
                if (t >= 1f)
                {
                    //The foot has reached forward.
                    //next we let it plant on the ground.
                    walkPhase = WalkPhase.Plant;
                    //reset the timer for plant.
                    phaseTimer = 0f;
                }
                return;
            }
            else if(stepDirection == StepDirection.Right)
            {
                //get max's right direction
                Vector3 right = movementReference.right;
                //remove the vertical tilt;
                right.y = 0f;
                //make the direction length exactly 1.
                right.Normalize();
                //Right thigh goes from outward back to straight
                SetJointZOffset(rightThigh, rightThighStart, -sidewaysThighAngle * rightSideSign);
                //left thigh moves outward is opposite direction.
                SetJointZOffset(leftThigh, leftThighStart, 0f);
                //lower leg stay neutral during this sideways transer
                SetJointXOffset(rightLowerLeg, rightLowerLegStart, 0f);
                SetJointXOffset(leftLowerLeg, leftLowerLegStart, 0f);
                //Physically move Max's pelvis toward the newly placed Right foot.
                pelivs.AddForce(right * sideShiftAcceleration, ForceMode.Acceleration);
                //When the weight transfer is finished 
                if (t >= 1f)
                {
                    //Move to plant
                    walkPhase = WalkPhase.Plant;
                    phaseTimer = 0f;
                }
                return;
            }
            else if(stepDirection == StepDirection.Left)
            {
                //Get Max's right direction.
                Vector3 right = movementReference.right;
                right.y = 0f;
                right.Normalize();
                //left thigh goes from outward back to straght
                SetJointZOffset(leftThigh, leftThighStart, -sidewaysThighAngle * leftSideSign);
                //right thigh moves outward to right
                SetJointZOffset(rightThigh, rightThighStart, 0f);
                //kepp the both lower legt neutral during the transer.
                SetJointXOffset(leftLowerLeg, leftLowerLegStart, 0f);
                SetJointXOffset(rightLowerLeg, rightLowerLegStart, 0f);
                pelivs.AddForce(-right * sideShiftAcceleration, ForceMode.Acceleration);

                if (t >= 1f)
                {
                    //move to plant.
                    walkPhase = WalkPhase.Plant;
                    phaseTimer  = 0f;
                }
                return;
            }
        }

        //The foot has reached forward. Hold if there while the pelvis continuews moving over the leg.
        //Count how long we have been planing the foot.
        if(walkPhase == WalkPhase.Plant)
        {
            phaseTimer += Time.fixedDeltaTime;
            //Convert plant time into a vlaue from 0 to 1.
            float t = Mathf.Clamp01(phaseTimer / plantTime);
            //if max is moving to the right..
            if(stepDirection == StepDirection.Right)
            {
                //The right leg already planed outward. Keep it there while the left trailing leg follows
                SetJointZOffset(rightThigh, rightThighStart, -sidewaysThighAngle * rightSideSign);
                //Move the left thigh toward the right side.
                float leftFollowAngle = Mathf.SmoothStep(0f, sidewaysThighAngle, t);
                SetJointZOffset(leftThigh, leftThighStart, leftFollowAngle * leftSideSign);
                //keep both lower legs neutral.
                SetJointXOffset(rightLowerLeg, rightLowerLegStart, 0f);
                SetJointXOffset(leftLowerLeg, leftLowerLegStart, 0f);
                if (t >= 1)
                {
                    walkPhase = WalkPhase.Idle;
                    phaseTimer = 0f;
                    stepDirection = StepDirection.None;
                }
                return;
            }
            // if max moving to the left
            if(stepDirection == StepDirection.Left)
            {
                //The left leg is already planted outward.
                //keep it there while the right trailing leg follows.
                SetJointZOffset(leftThigh, leftThighStart, -sidewaysThighAngle * leftSideSign);
                //Mvoe the right thigh toward the left side.
                // this is not right leg outward right the trailing leg is follwoing the planted left leg.
                float rightFollowAngle = Mathf.SmoothStep(0f, sidewaysThighAngle,t);
                SetJointZOffset(rightThigh, rightThighStart, rightFollowAngle * rightSideSign);
                //keep both lower legs neutral,
                SetJointXOffset(leftLowerLeg, leftLowerLegStart, 0f);
                SetJointXOffset(rightLowerLeg, rightLowerLegStart, 0f);
                if (t >= 1f)
                {
                    walkPhase = WalkPhase.Idle;
                    phaseTimer = 0f;
                    stepDirection = StepDirection.None;
                }
                return;
            }
            //Decie which thigh angle should be held during plant
            float plantThighAngle = 0f;
            if(stepDirection == StepDirection.Forward)
                plantThighAngle = forwardThighAngle;
            else if(stepDirection == StepDirection.Backward)
                plantThighAngle = backwardThighAngle;
            
            //left leg took this step.
            if (currentStepUsesLeft)
            {
                //keep the left thigh forward.
                SetJointXOffset(leftThigh, leftThighStart, forwardThighAngle * -leftThighSign);
                //keep the left lower is forward, this places the foot ahead
                SetJointXOffset(leftLowerLeg, leftLowerLegStart, 0f);
                //keep the right legt neutral for now.
                SetJointXOffset(rightThigh, rightThighStart, 0f);
                SetJointXOffset(rightLowerLeg, rightLowerLegStart, 0f);
            }
            else
            {
                //keep the left leg neutral.
                SetJointXOffset(leftThigh,leftThighStart, 0f);
                SetJointXOffset(leftLowerLeg, leftLowerLegStart, 0f);
                SetJointXOffset(rightThigh, rightThighStart, forwardThighAngle * -rightThighSign);
                SetJointXOffset(rightLowerLeg, rightLowerLegStart, 0f);
            }
            //Once the pelvis has had time to move over the planted foot...
            if (t >= 1f)
            {
                //return to idle.
                //Because useLeftLegNext was already flipped,
                //The opposite leg will start the next step
                walkPhase = WalkPhase.Idle;
                //Reset the timer.
                phaseTimer = 0f;
                //Forget whether the finished step was forward or backward
                stepDirection = StepDirection.None;
            }
            return;
        }
    }
        


    //Move one leg joint around its logac x axis
    //relative to max's original standing pose.
    private void SetJointXOffset(
        ConfigurableJoint joint,
        Quaternion startingRotation,
        float xAngle
    )
    {
        //create a rotation only around the x axis
        Quaternion offset = Quaternion.Euler(xAngle, 0f, 0f);
        //Add that procedural rotation on top of the bone's original standing rotation.
        Quaternion desiredRotation = startingRotation * offset;
        //Send our desired rotation to the ConfigurableJoint
        SetTargetRotationLocal(joint, desiredRotation, startingRotation);
    }
    //Moves one joint its local z axes relative to Max's original standing pose.
    private void SetJointZOffset(
        ConfigurableJoint joint,
        Quaternion startingRotation, 
        float zAngle
    )
    {
        Quaternion offset = Quaternion.Euler(0f, 0f, zAngle);
        //Add that sieways rotation on tof of the bone's original standing rotation.
        Quaternion desiredRotation = startingRotation * offset;
        //Send the desired roation to the ConfigurableJoint.
        SetTargetRotationLocal(joint, desiredRotation, startingRotation);
    }
    private void SetBothLegsNeutral()
    {
        //left thigh back to its starting rotation.
        SetJointXOffset(leftThigh, leftThighStart, 0f);
        SetJointXOffset(leftLowerLeg, leftLowerLegStart, 0f);
        SetJointXOffset(rightThigh, rightThighStart, 0f);
        SetJointXOffset(rightLowerLeg, rightLowerLegStart, 0f);
    }
    private void SetTargetRotationLocal(
        ConfigurableJoint joint, 
        Quaternion targetLocalRotation, Quaternion startLocalRoation)
    {
        //The joint's main local axis
        Vector3 right = joint.axis;
        //Build a forward direction from the Joint's main axis and secondary axis
        Vector3 forward = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
        //Build the remainaing preprendicualr axis
        Vector3 up = Vector3.Cross(forward, right).normalized;
        //create a rotatin that represents the joint's own cordinate system
        Quaternion worldToJointSpace = Quaternion.LookRotation(forward, up);
        //Start converting our desired rotation into the joint's coordinate space.
        Quaternion resultRotation = Quaternion.Inverse(worldToJointSpace);
        //Calculate how fow the desired pose is from the original standing pose.
        resultRotation *= Quaternion.Inverse(targetLocalRotation) * startLocalRoation;
        //convert the result back through the joint coordinate systestem
        resultRotation *= worldToJointSpace;
        //This is the actual physical target the configurableJoint will try to reach.
        joint.targetRotation = resultRotation;
    }

    public void SetMovementInput(float forward, float horizontal)
    {
        forwardInput = forward;
        horizontalInput = horizontal;
    }
}