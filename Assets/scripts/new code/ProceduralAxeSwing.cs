using System.Collections;
using UnityEngine;

public class ProceduralAxeSwing : MonoBehaviour
{
    public Rigidbody lowerArm;
    public Rigidbody upperArm;

    [Header("Swing")]
    public float windUpAngle = -50f; 
    public float attackAngle = -70f;

    public float windUpTime = 0.2f;
    public float attackTime = 0.30f;
    public float recoverTime = 0.18f; 

    private bool swinging;
    [Header("check swing")]
    public bool IsSwinging => swinging;

    private Quaternion upperStart;
    private Quaternion lowerStart;

    private void Awake()
    {
        upperStart = upperArm.transform.localRotation;
        lowerStart = lowerArm.transform.localRotation;
    }

    private void Update()
    {
        // if (Input.GetMouseButtonDown(0))
        // {
        //     StartSwing();
        // }
    }

    public void StartSwing()
    {
        if (!swinging)
            StartCoroutine(Swing());
    }


    



    private IEnumerator Swing()
    {
        swinging = true;

        // Stop physics from fighting us
        upperArm.isKinematic = true;
        lowerArm.isKinematic = true;

        Quaternion upperBack =
            upperStart * Quaternion.Euler(0f, 0f, windUpAngle);

        Quaternion lowerBack =
            lowerStart * Quaternion.Euler(0f, 0f, windUpAngle*0.5f);
        
        
        Quaternion upperAttack =
            upperStart * Quaternion.Euler( 0f,0f, attackAngle);

        Quaternion lowerAttack =
            lowerStart * Quaternion.Euler(0f, 0f, attackAngle * 0.7f);


        // WIND UP
        float timer = 0f;

        while (timer < windUpTime)
        {
            timer += Time.deltaTime;

            float t = timer / windUpTime;

            upperArm.transform.localRotation =
                Quaternion.Slerp(upperStart, upperBack, t);

            lowerArm.transform.localRotation =
                Quaternion.Slerp(lowerStart, lowerBack, t);

            yield return null;
        }


        // ATTACK
        timer = 0f;

        while (timer < attackTime)
        {
            timer += Time.deltaTime;

            float t = timer / attackTime;

            upperArm.transform.localRotation =
                Quaternion.Slerp(upperBack, upperAttack, t);

            lowerArm.transform.localRotation =
                Quaternion.Slerp(lowerBack, lowerAttack, t);

            yield return null;
        }


        // RECOVER
        timer = 0f;

        while (timer < recoverTime)
        {
            timer += Time.deltaTime;

            float t = timer / recoverTime;

            upperArm.transform.localRotation =
                Quaternion.Slerp(upperAttack, upperStart, t);

            lowerArm.transform.localRotation =
                Quaternion.Slerp(lowerAttack, lowerStart, t);

            yield return null;
        }


        upperArm.transform.localRotation = upperStart;
        lowerArm.transform.localRotation = lowerStart;

        // Give physics control back
        upperArm.isKinematic = false;
        lowerArm.isKinematic = false;

        swinging = false;
    }
}


// using UnityEngine;

// public class ProceduralAxeSwing : MonoBehaviour
// {
//     [Header("References")]
//     public Rigidbody lowerArm;
//     public Rigidbody upperArm;
//     public Transform forwardReference;

//     [Header("Swing")]
//     public float windUpAngle = -50f;
//     public float attackAngle = 80f;
    

//     [Header("Settings")]
//     public float lowerArmTorque = 4000f;
//     public float upperArmTorque = 1000000f;

//     [SerializeField]
//     private bool IsMouseLeft = false;
//     private void Awake()
//     {
//         if(lowerArm == null)
//         {
//             lowerArm = GetComponent<Rigidbody>();
//         }
//         if(forwardReference == null || upperArm == null)
//         {
//             Debug.LogError("Plese add references to swing axe code");
//             enabled = false;
//         }
        
//     }
//     private void Update()
//     {
//         if (Input.GetMouseButtonDown(0))
//         {
//             IsMouseLeft = true;
//         }
//     }
//     private void FixedUpdate()
//     {
//         if (IsMouseLeft)
//         {
//             lowerArm.AddTorque(-forwardReference.right* lowerArmTorque, ForceMode.Impulse);
//             upperArm.AddTorque(-forwardReference.right * upperArmTorque, ForceMode.Impulse); 
//             IsMouseLeft = false;
//         }
    
//     }
// }   