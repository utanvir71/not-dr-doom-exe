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

    [SerializeField] private float previousDistance;

    public int EpisodeNumber { get; private set; }
    public int HitCount { get; private set; }
    public int SwingHitCount { get; private set; }
    public int LastAction { get; private set; }
    public int MaxHitsOnMaster { get; private set; }
    public int MasterHitsOnMax { get; private set; }
    public int MaxWinCount { get; private set; }
    public int MaxLostCount { get; private set; }

    private bool gotTouchRewardThisEpisode = false;
    private bool gotSwingHitThisEpisode = false;

    public MasterReset masterReset;

    public float CurrentDistance
    {
        get
        {
            Vector3 toDummy = dummy.position - pelvis.position;
            toDummy.y = 0f;
            return toDummy.magnitude;
        }
    }

    public override void Initialize()
    {
        MaxStep = 3000;
        startPosition = transform.position;
        startRotation = transform.rotation;
        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        startLocalPositions = new Vector3[ragdollBodies.Length];
        startLocalRotations = new Quaternion[ragdollBodies.Length];

        for (int i = 0; i < ragdollBodies.Length; i++)
        {
            startLocalPositions[i] = ragdollBodies[i].transform.localPosition;
            startLocalRotations[i] = ragdollBodies[i].transform.localRotation;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 directionToDummy = dummy.position - pelvis.position;
        directionToDummy.y = 0f;
        float distance = directionToDummy.magnitude;
        Vector3 localDirection = physicsMovement.movementReference.InverseTransformDirection(directionToDummy.normalized);

        sensor.AddObservation(localDirection);
        sensor.AddObservation(distance);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];
        LastAction = action;

        proceduralMovement.SetMovementInput(0f, 0f);
        physicsMovement.SetMovementInput(0f, 0f);
        turnInput = 0f;

        if (action == 1)
        {
            proceduralMovement.SetMovementInput(1f, 0f);
            physicsMovement.SetMovementInput(1f, 0f);
        }
        else if (action == 2)
        {
            turnInput = -1f;
        }
        else if (action == 3)
        {
            turnInput = 1f;
        }
        else if (action == 4 && !proceduralAxeSwing.IsSwinging)
        {
            proceduralAxeSwing.StartSwing();

            Vector3 axeToDummy = dummy.position - pelvis.position;
            axeToDummy.y = 0f;

            float attackDistance = axeToDummy.magnitude;

            if (attackDistance > 1.0f)
                AddReward(-0.01f);
        }

        Vector3 toDummy = dummy.position - pelvis.position;
        toDummy.y = 0f;

        float currentDistance = toDummy.magnitude;
        float distanceChange = previousDistance - currentDistance;

        AddReward(distanceChange * 0.05f);

        previousDistance = currentDistance;

        AddReward(-0.0002f);
    }

    private void Update()
    {
        if (turnInput < 0f)
            transform.RotateAround(pelvis.position, Vector3.up, -turnSpeed * Time.deltaTime);
        else if (turnInput > 0f)
            transform.RotateAround(pelvis.position, Vector3.up, turnSpeed * Time.deltaTime);
    }

    // public override void Heuristic(in ActionBuffers actionsOut)
    // {
    //     var action = actionsOut.DiscreteActions;

    //     if (Input.GetKey(KeyCode.W))
    //         action[0] = 1;
    //     else if (Input.GetKey(KeyCode.A))
    //         action[0] = 2;
    //     else if (Input.GetKey(KeyCode.D))
    //         action[0] = 3;
    //     else if (Input.GetMouseButton(0))
    //         action[0] = 4;
    //     else
    //         action[0] = 0;
    // }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var action = actionsOut.DiscreteActions;
        action[0] = 0;
    }

    public void AxeHitTarget()
    {
        if (proceduralAxeSwing.IsSwinging)
        {
            if (gotSwingHitThisEpisode)
                return;

            gotSwingHitThisEpisode = true;

            if (gotTouchRewardThisEpisode)
                AddReward(0.5f);
            else
                AddReward(1.0f);

            SwingHitCount++;

            Debug.Log("SWING HIT! Reward total = +1");

            EndEpisode();

            return;
        }

        if (!gotTouchRewardThisEpisode)
        {
            gotTouchRewardThisEpisode = true;
            HitCount++;
            AddReward(0.5f);

            Debug.Log("NORMAL HIT! Axe touched dummy. Reward +0.5");
        }
    }

    public override void OnEpisodeBegin()
    {
        masterReset.ResetMaster();

        Time.timeScale = 1f;

        EpisodeNumber++;

        gotTouchRewardThisEpisode = false;
        gotSwingHitThisEpisode = false;

        MaxHitsOnMaster = 0;
        MasterHitsOnMax = 0;

        proceduralMovement.SetMovementInput(0f, 0f);
        physicsMovement.SetMovementInput(0f, 0f);
        turnInput = 0f;

        transform.position = startPosition;
        transform.rotation = startRotation;

        for (int i = 0; i < ragdollBodies.Length; i++)
        {
            if (!ragdollBodies[i].isKinematic)
            {
                ragdollBodies[i].linearVelocity = Vector3.zero;
                ragdollBodies[i].angularVelocity = Vector3.zero;
            }

            ragdollBodies[i].transform.localPosition = startLocalPositions[i];
            ragdollBodies[i].transform.localRotation = startLocalRotations[i];
        }

        Physics.SyncTransforms();

        Vector3 toDummy = dummy.position - pelvis.position;
        toDummy.y = 0f;
        previousDistance = toDummy.magnitude;

        Debug.Log("new episode ! timescale= " + Time.timeScale + " | MaxStep= " + MaxStep);
    }

    public void MaxHitMaster()
    {
        MaxHitsOnMaster++;

        float reward = 0.1f * MaxHitsOnMaster;

        AddReward(reward);

        Debug.Log("hit master: " + MaxHitsOnMaster + " reward: " + reward);

        if (MaxHitsOnMaster >= 5)
        {
            MaxWinCount++;
            AddReward(1.0f);
            Debug.LogError("max won!");
            EndEpisode();
        }
    }

    public void MasterHitMax()
    {
        MasterHitsOnMax++;

        float penalty = 0.1f * MasterHitsOnMax;

        AddReward(-penalty);

        Debug.Log("master hit" + MasterHitsOnMax + " penalty: " + penalty);

        if (MasterHitsOnMax >= 5)
        {
            MaxLostCount++;
            AddReward(-1f);
            Debug.LogError("Max Lost");
            EndEpisode();
        }
    }
}