using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MaxAgent : Agent
{
    [Header("References")]

    // This code is made by Duct Tape and Prayer please don't judge me.
    // The commented code were there for training simulator before Max started fighting me.

    // old training target
    public Transform dummy;

    // final boss fight target aka me
    public Transform playerTarget;

    public ProceduralMovement proceduralMovement;
    public ProceduralAxeSwing proceduralAxeSwing;
    public PhysicsMoveMentController physicsMovement;
    public Rigidbody pelvis;

    // old fuk yu reset
    public MasterReset masterReset;


    [Header("Turning")]

    public float turnSpeed = 120f;
    public float previousDistance;

    private float turnInput;


    // ragdoll reset stuff
    private Rigidbody[] ragdollBodies;
    private Vector3[] startLocalPositions;
    private Quaternion[] startLocalRotations;

    private Vector3 startPosition;
    private Quaternion startRotation;


    // old hud stuff
    public int EpisodeNumber { get; private set; }
    public int LastAction { get; private set; }
    public float CurrentDistance { get; private set; }

    public int MaxHitsOnMaster { get; private set; }
    public int MasterHitsOnMax { get; private set; }

    public int MaxWinCount { get; private set; }
    public int MaxLostCount { get; private set; }


    // final fight stuff
    public int MaxHitsOnPlayer { get; private set; }
    public int PlayerHitsOnMax { get; private set; }

    public int hitsToWin = 5;

    public bool fightOver = false;


    public override void Initialize()
    {
        ragdollBodies = GetComponentsInChildren<Rigidbody>();

        startLocalPositions = new Vector3[ragdollBodies.Length];
        startLocalRotations = new Quaternion[ragdollBodies.Length];

        for (int i = 0; i < ragdollBodies.Length; i++)
        {
            startLocalPositions[i] = ragdollBodies[i].transform.localPosition;
            startLocalRotations[i] = ragdollBodies[i].transform.localRotation;
        }

        startPosition = transform.position;
        startRotation = transform.rotation;


        // OLD TRAINING

        // MaxStep = 1000;
        // MaxStep = 3000;

        // final fight should not randomly end because ML Agents reached max steps
        MaxStep = 0;
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        Transform target = playerTarget;


        // emergency duct tape in case i forgot to assign the player
        // it will use the old dummy instead of exploding

        if (target == null)
            target = dummy;


        if (target == null)
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);

            return;
        }


        // variable is still called directionToDummy because renaming things
        // at this point sounds dangerous

        Vector3 directionToDummy = target.position - pelvis.position;

        directionToDummy.y = 0f;

        float distance = directionToDummy.magnitude;

        Vector3 localDirection = transform.InverseTransformDirection(directionToDummy.normalized);

        sensor.AddObservation(localDirection);
        sensor.AddObservation(distance);

        CurrentDistance = distance;
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        if (fightOver)
        {
            turnInput = 0f;

            proceduralMovement.SetMovementInput(0f, 0f);
            physicsMovement.SetMovementInput(0f, 0f);

            return;
        }


        int action = actions.DiscreteActions[0];

        LastAction = action;

        turnInput = 0f;


        proceduralMovement.SetMovementInput(0f, 0f);
        physicsMovement.SetMovementInput(0f, 0f);


        // 0 = idle


        // 1 = walk forward

        if (action == 1)
        {
            proceduralMovement.SetMovementInput(1f, 0f);
            physicsMovement.SetMovementInput(1f, 0f);
        }


        // 2 = turn left

        else if (action == 2)
        {
            turnInput = -1f;
        }


        // 3 = turn right

        else if (action == 3)
        {
            turnInput = 1f;
        }


        // 4 = axe swing

        else if (action == 4)
        {
            if (!proceduralAxeSwing.IsSwinging)
                proceduralAxeSwing.StartSwing();
        }


        // ==========================================================
        // OLD TRAINING REWARD STUFF
        // DO NOT DELETE
        // THIS IS HOW MAX LEARNED HIS BLACK MAGIC
        // ==========================================================

        /*
        AddReward(-0.0002f);

        Transform target = dummy;

        if (target != null)
        {
            float currentDistance = Vector3.Distance(pelvis.position, target.position);

            float distanceChange = previousDistance - currentDistance;

            AddReward(distanceChange * 0.05f);

            previousDistance = currentDistance;


            if (action == 4 && currentDistance > 1.0f)
                AddReward(-0.01f);
        }
        */
    }


    private void Update()
    {
        if (fightOver)
            return;


        if (turnInput != 0f)
        {
            transform.RotateAround(
                pelvis.position,
                Vector3.up,
                turnInput * turnSpeed * Time.deltaTime
            );


            // ==================================================
            // OLD ROOT SYNC
            // ==================================================
            //
            // THIS WAS USED DURING TRAINING.
            //
            // Problem:
            // pelvis is a Rigidbody inside Max.
            //
            // Then this code moves Max's ROOT to the pelvis position.
            //
            // Moving the root also moves the pelvis because pelvis is
            // inside the root.
            //
            // Which means Max can basically chase his own ass across
            // the map.
            //
            // Keeping it here because Duct Tape and Prayer history.
            //
            // ==================================================

            /*
            Vector3 rootPosition = transform.position;

            rootPosition.x = pelvis.position.x;
            rootPosition.z = pelvis.position.z;

            transform.position = rootPosition;
            */
        }
    }


    public override void OnEpisodeBegin()
    {
        EpisodeNumber++;

        Time.timeScale = 1f;

        turnInput = 0f;


        proceduralMovement.SetMovementInput(0f, 0f);
        physicsMovement.SetMovementInput(0f, 0f);


        // ==================================================
        // OLD MAX RESET
        // ==================================================
        //
        // During training every episode had to reset Max.
        //
        // Final fight:
        // Max stays wherever I placed him in the scene.
        //
        // ==================================================

        /*
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
        */


        // OLD FUK YU RESET

        // MaxHitsOnMaster = 0;
        // MasterHitsOnMax = 0;

        // masterReset.ResetMaster();


        Transform target = playerTarget;

        if (target == null)
            target = dummy;


        if (target != null)
        {
            previousDistance = Vector3.Distance(pelvis.position, target.position);
            CurrentDistance = previousDistance;
        }
    }


    // ============================================================
    // FINAL FIGHT
    // ============================================================


    public void MaxHitPlayer()
    {
        if (fightOver)
            return;


        MaxHitsOnPlayer++;


        Debug.Log(
            "MAX HIT PLAYER: " +
            MaxHitsOnPlayer +
            " / " +
            hitsToWin
        );


        if (MaxHitsOnPlayer >= hitsToWin)
        {
            fightOver = true;

            turnInput = 0f;

            proceduralMovement.SetMovementInput(0f, 0f);
            physicsMovement.SetMovementInput(0f, 0f);

            Debug.LogError("MAX WON THE FINAL FIGHT");


            // later:
            // player death animation
            // disable player movement
            // show YOU LOST
            // Max laughs at my 500k training steps
        }
    }


    public void PlayerHitMax()
    {
        if (fightOver)
            return;


        PlayerHitsOnMax++;


        Debug.Log(
            "PLAYER HIT MAX: " +
            PlayerHitsOnMax +
            " / " +
            hitsToWin
        );


        if (PlayerHitsOnMax >= hitsToWin)
        {
            fightOver = true;

            turnInput = 0f;

            proceduralMovement.SetMovementInput(0f, 0f);
            physicsMovement.SetMovementInput(0f, 0f);

            Debug.LogError("MAX LOST THE FINAL FIGHT");


            // later:
            // Max commits not alive
            // ragdoll collapse
            // victory screen
            // emotional damage
        }
    }


    // ============================================================
    // OLD MAX VS FUK YU FUNCTIONS
    // KEEPING THESE BECAUSE HISTORY MUST REMEMBER THE WAR
    // ============================================================


    public void MaxHitMaster()
    {
        MaxHitsOnMaster++;


        float reward = 0.1f * MaxHitsOnMaster;


        // OLD TRAINING
        // AddReward(reward);


        Debug.Log(
            "hit master: " +
            MaxHitsOnMaster +
            " reward: " +
            reward
        );


        /*
        if (MaxHitsOnMaster >= 5)
        {
            MaxWinCount++;

            AddReward(1.0f);

            Debug.LogError("max won!");

            EndEpisode();
        }
        */
    }


    public void MasterHitMax()
    {
        MasterHitsOnMax++;


        float penalty = 0.1f * MasterHitsOnMax;


        // OLD TRAINING
        // AddReward(-penalty);


        Debug.Log(
            "master hit " +
            MasterHitsOnMax +
            " penalty: " +
            penalty
        );


        /*
        if (MasterHitsOnMax >= 5)
        {
            MaxLostCount++;

            AddReward(-1f);

            Debug.LogError("Max Lost");

            EndEpisode();
        }
        */
    }


    // ============================================================
    // OLD DUMMY TRAINING
    // ============================================================


    public void AxeHitTarget()
    {
        // AxeHitReward.cs still calls this method.
        //
        // So apparently this empty corpse has to stay here
        // or Unity starts screaming.


        // OLD TRAINING

        // AddReward(1.0f);

        // Debug.Log("AXE HIT DUMMY");

        // EndEpisode();
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var action = actionsOut.DiscreteActions;

        action[0] = 0;


        // ==================================================
        // OLD HUMAN CONTROL TESTING
        // ==================================================

        /*
        if (Input.GetKey(KeyCode.W))
            action[0] = 1;

        else if (Input.GetKey(KeyCode.A))
            action[0] = 2;

        else if (Input.GetKey(KeyCode.D))
            action[0] = 3;

        else if (Input.GetMouseButton(0))
            action[0] = 4;

        else
            action[0] = 0;
        */
    }
}