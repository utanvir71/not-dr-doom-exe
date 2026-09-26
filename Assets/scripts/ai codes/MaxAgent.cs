using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


public class MaxAgent : Agent
{
    public Transform dummy;
    public ProceduralMovement proceduralMovement;
    public ProceduralAxeSwing proceduralAxeSwing;
    public Rigidbody pelvis;
    public float turnSpeed = 120f;
    private float trunInput;

    private bool endEpisodeAfterSwing = false;

    private Rigidbody[] ragdollBodies;
    private Vector3[] startLocalPositions;
    private Quaternion[] startLocalRotations;

    private Vector3 startPosition;
    private Quaternion startRotation;
    
    [SerializeField]
    private float previousDistance;



    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 directionToDummy = dummy.position - pelvis.position;
        float distance = directionToDummy.magnitude;

        Vector3 localDirection = transform.InverseTransformDirection(directionToDummy.normalized);
        sensor.AddObservation(localDirection);   
        sensor.AddObservation(distance);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];
        proceduralMovement.SetMovementInput(0f, 0f);

        trunInput = 0f;
        if(action == 1)
            proceduralMovement.SetMovementInput(1f, 0f);
        else if (action == 2)
            trunInput = -1f;
        else if (action == 3)
            trunInput = 1f;
        else if (action == 4)
            proceduralAxeSwing.StartSwing();
        
        float currentDistance = Vector3.Distance(pelvis.position, dummy.position);
        float distanceChange = previousDistance - currentDistance;
        AddReward(distanceChange * 0.01f);
        previousDistance = currentDistance;

        AddReward(-0.0001f);
        
        // Debug.Log("Action: " + action + "Turn: " + trunInput);
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var action = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W))
        {
            action[0] = 1;
            // Debug.Log("w");
        }
        else if (Input.GetKey(KeyCode.A))
        {
            action[0] = 2;
            // Debug.Log("A");
        } 
        else if (Input.GetKey(KeyCode.D))
        {
            action[0] = 3;
            // Debug.Log("D");
        }
        else if(Input.GetMouseButtonDown(0))
        {
            action[0] = 4;
        }  
        else 
            action[0] = 0;
        
        
        // Debug.Log("heiuristioc sent: " + action[0]);
    }
    private void Update()
    {
        if(endEpisodeAfterSwing && !proceduralAxeSwing.IsSwinging)
        {
            endEpisodeAfterSwing = false;
            EndEpisode();
            return;
        }
        if(trunInput < 0f)
        {

            transform.RotateAround(pelvis.position, Vector3.up, -turnSpeed * Time.deltaTime);
        }
        else if(trunInput > 0f)
        {
            transform.RotateAround(pelvis.position, Vector3.up, turnSpeed * Time.deltaTime);
        }
            
    }

    public void AxeHitTarget()
    {
        AddReward(1.0f);
        Debug.Log("Max hit target! Reward +1");
        endEpisodeAfterSwing = true;
    }

    public override void Initialize()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;

        ragdollBodies = GetComponentsInChildren<Rigidbody>();

        startLocalPositions = new Vector3[ragdollBodies.Length];
        startLocalRotations = new Quaternion[ragdollBodies.Length];

        for(int i = 0; i<ragdollBodies.Length; i++)
        {
            startLocalPositions[i] = ragdollBodies[i].transform.localPosition;

            startLocalRotations[i] = ragdollBodies[i].transform.localRotation;
        }
    }

    public override void OnEpisodeBegin()
    {
        proceduralMovement.SetMovementInput(0f,0f);
        trunInput = 0f;
        endEpisodeAfterSwing = false;

        //reset max root
        transform.position = startPosition;
        transform.rotation = startRotation;

        //reset every ragdoll body
        for (int i = 0; i<ragdollBodies.Length; i++)
        {
            ragdollBodies[i].linearVelocity = Vector3.zero;
            ragdollBodies[i].angularVelocity = Vector3.zero;

            ragdollBodies[i].transform.localPosition = startLocalPositions[i];
            ragdollBodies[i].transform.localRotation = startLocalRotations[i];
        }
        Physics.SyncTransforms();

        previousDistance = Vector3.Distance(pelvis.position, dummy.position);
        Debug.Log("new episode");
    }
}
