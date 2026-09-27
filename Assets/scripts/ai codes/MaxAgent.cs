using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MaxAgent : Agent
{
    [Header("References")]
    public Transform dummy;
    public ProceduralMovement proceduralMovement;
    public ProceduralAxeSwing proceduralAxeSwing;
    public PhysicsMoveMentController physicsMovement;
    public Rigidbody pelvis;

    [Header("Turning")]
    public float turnSpeed = 120f;

    private float turnInput = 0f;

    private Rigidbody[] ragdollBodies;
    private Vector3[] startLocalPositions;
    private Quaternion[] startLocalRotations;

    private Vector3 startPosition;
    private Quaternion startRotation;

    [SerializeField]
    private float previousDistance;


    // =========================
    // HUD / TRAINING DATA
    // =========================

    public int EpisodeNumber { get; private set; }

    // Normal axe touch WITHOUT swinging
    public int HitCount { get; private set; }

    // Proper axe hit WHILE swinging
    public int SwingHitCount { get; private set; }

    public int LastAction { get; private set; }


    // Prevent reward farming
    private bool gotTouchRewardThisEpisode = false;
    private bool gotSwingHitThisEpisode = false;


    public float CurrentDistance
    {
        get
        {
            Vector3 toDummy =
                dummy.position - pelvis.position;

            toDummy.y = 0f;

            return toDummy.magnitude;
        }
    }


    public override void Initialize()
    {
        MaxStep = 1000;

        startPosition = transform.position;
        startRotation = transform.rotation;


        ragdollBodies =
            GetComponentsInChildren<Rigidbody>();


        startLocalPositions =
            new Vector3[ragdollBodies.Length];

        startLocalRotations =
            new Quaternion[ragdollBodies.Length];


        for (int i = 0; i < ragdollBodies.Length; i++)
        {
            startLocalPositions[i] =
                ragdollBodies[i]
                    .transform.localPosition;

            startLocalRotations[i] =
                ragdollBodies[i]
                    .transform.localRotation;
        }
    }


    public override void CollectObservations(
        VectorSensor sensor)
    {
        Vector3 directionToDummy =
            dummy.position - pelvis.position;


        // Ignore vertical movement
        directionToDummy.y = 0f;


        float distance =
            directionToDummy.magnitude;


        Vector3 localDirection =
            physicsMovement
                .movementReference
                .InverseTransformDirection(
                    directionToDummy.normalized
                );


        // 3 observations
        sensor.AddObservation(localDirection);

        // 1 observation
        sensor.AddObservation(distance);
    }


    public override void OnActionReceived(
        ActionBuffers actions)
    {
        int action =
            actions.DiscreteActions[0];


        LastAction = action;


        // =========================
        // DEFAULT STATE
        // =========================

        proceduralMovement.SetMovementInput(
            0f,
            0f
        );


        physicsMovement.SetMovementInput(
            0f,
            0f
        );


        turnInput = 0f;


        // =========================
        // ACTION 1
        // WALK FORWARD
        // =========================

        if (action == 1)
        {
            proceduralMovement.SetMovementInput(
                1f,
                0f
            );


            physicsMovement.SetMovementInput(
                1f,
                0f
            );
        }


        // =========================
        // ACTION 2
        // TURN LEFT
        // =========================

        else if (action == 2)
        {
            turnInput = -1f;
        }


        // =========================
        // ACTION 3
        // TURN RIGHT
        // =========================

        else if (action == 3)
        {
            turnInput = 1f;
        }


        // =========================
        // ACTION 4
        // SWING AXE
        // =========================

        else if (
            action == 4 &&
            !proceduralAxeSwing.IsSwinging
        )
        {
            proceduralAxeSwing.StartSwing();


            Vector3 axeToDummy =
                dummy.position - pelvis.position;


            axeToDummy.y = 0f;


            float attackDistance =
                axeToDummy.magnitude;


            // Punish useless far-away swings
            if (attackDistance > 1.0f)
            {
                AddReward(-0.01f);
            }
        }


        // =========================
        // DISTANCE REWARD
        // =========================

        Vector3 toDummy =
            dummy.position - pelvis.position;


        toDummy.y = 0f;


        float currentDistance =
            toDummy.magnitude;


        float distanceChange =
            previousDistance - currentDistance;


        // Positive if getting closer
        // Negative if moving farther away
        AddReward(
            distanceChange * 0.05f
        );


        previousDistance =
            currentDistance;


        // Tiny penalty for wasting time
        AddReward(-0.0002f);
    }


    private void Update()
    {
        // =========================
        // TURN AROUND PELVIS
        // =========================

        if (turnInput < 0f)
        {
            transform.RotateAround(
                pelvis.position,
                Vector3.up,
                -turnSpeed * Time.deltaTime
            );
        }


        else if (turnInput > 0f)
        {
            transform.RotateAround(
                pelvis.position,
                Vector3.up,
                turnSpeed * Time.deltaTime
            );
        }
    }


    public override void Heuristic(
        in ActionBuffers actionsOut)
    {
        var action =
            actionsOut.DiscreteActions;


        // W = forward
        if (Input.GetKey(KeyCode.W))
        {
            action[0] = 1;
        }


        // A = turn left
        else if (Input.GetKey(KeyCode.A))
        {
            action[0] = 2;
        }


        // D = turn right
        else if (Input.GetKey(KeyCode.D))
        {
            action[0] = 3;
        }


        // Mouse = swing
        else if (Input.GetMouseButtonDown(0))
        {
            action[0] = 4;
        }


        else
        {
            action[0] = 0;
        }
    }


    // =====================================================
    // CALLED WHEN AXE COLLIDER TOUCHES THE DUMMY
    // =====================================================

    public void AxeHitTarget()
    {
        // =============================================
        // PROPER SWING HIT
        // =============================================

        if (proceduralAxeSwing.IsSwinging)
        {
            // Don't count twice
            if (gotSwingHitThisEpisode)
                return;


            gotSwingHitThisEpisode = true;


            // If Max already got +0.5 for touching,
            // only give another +0.5.
            //
            // Total target reward = +1.0
            if (gotTouchRewardThisEpisode)
            {
                AddReward(0.5f);
            }

            // Otherwise give full +1 immediately
            else
            {
                AddReward(1.0f);
            }


            SwingHitCount++;


            Debug.Log(
                "SWING HIT! Reward total = +1"
            );


            // Successful episode
            EndEpisode();

            return;
        }


        // =============================================
        // NORMAL TOUCH
        // =============================================

        if (!gotTouchRewardThisEpisode)
        {
            gotTouchRewardThisEpisode = true;


            HitCount++;


            AddReward(0.5f);


            Debug.Log(
                "NORMAL HIT! Axe touched dummy. Reward +0.5"
            );
        }
    }


    public override void OnEpisodeBegin()
    {
        Time.timeScale = 1f;


        EpisodeNumber++;


        // Reset reward flags
        gotTouchRewardThisEpisode = false;
        gotSwingHitThisEpisode = false;


        // =========================
        // STOP MOVEMENT
        // =========================

        proceduralMovement.SetMovementInput(
            0f,
            0f
        );


        physicsMovement.SetMovementInput(
            0f,
            0f
        );


        turnInput = 0f;


        // =========================
        // RESET ROOT
        // =========================

        transform.position =
            startPosition;


        transform.rotation =
            startRotation;


        // =========================
        // RESET RAGDOLL
        // =========================

        for (
            int i = 0;
            i < ragdollBodies.Length;
            i++
        )
        {
            ragdollBodies[i].linearVelocity =
                Vector3.zero;


            ragdollBodies[i].angularVelocity =
                Vector3.zero;


            ragdollBodies[i]
                .transform.localPosition =
                startLocalPositions[i];


            ragdollBodies[i]
                .transform.localRotation =
                startLocalRotations[i];
        }


        Physics.SyncTransforms();


        // =========================
        // RESET DISTANCE
        // =========================

        Vector3 toDummy =
            dummy.position - pelvis.position;


        toDummy.y = 0f;


        previousDistance =
            toDummy.magnitude;


        Debug.Log(
            "new episode ! timescale= "
            + Time.timeScale
            + " | MaxStep= "
            + MaxStep
        );
    }
}