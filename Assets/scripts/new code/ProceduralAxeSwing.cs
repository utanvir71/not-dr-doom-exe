using UnityEngine;

public class ProceduralAxeSwing : MonoBehaviour
{
    [Header("References")]
    public Rigidbody lowerArm;
    public Rigidbody upperArm;
    public Transform forwardReference;

    [Header("Settings")]
    public float lowerArmTorque = 4000f;
    public float upperArmTorque = 1000000f;

    [SerializeField]
    private bool IsMouseLeft = false;
    private void Awake()
    {
        if(lowerArm == null)
        {
            lowerArm = GetComponent<Rigidbody>();
        }
        if(forwardReference == null || upperArm == null)
        {
            Debug.LogError("Plese add references to swing axe code");
            enabled = false;
        }
        
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            IsMouseLeft = true;
        }
    }
    private void FixedUpdate()
    {
        if (IsMouseLeft)
        {
            lowerArm.AddTorque(-forwardReference.right* lowerArmTorque);
            upperArm.AddTorque(-forwardReference.right * upperArmTorque);
            IsMouseLeft = false;
        }
    
    }
}   