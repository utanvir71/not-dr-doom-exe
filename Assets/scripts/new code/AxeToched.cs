using UnityEngine;

public class AxeToched : MonoBehaviour
{
    [Header("References")]
    public Collider axe;
    public GameObject max_armeture;
    public Animator anim;
    public MonoBehaviour animation_scirpt;
    public ConfigurableJoint leftUpperLeg;
    public ConfigurableJoint rightUpperLeg;
    public ConfigurableJoint leftLowerLeg;
    public ConfigurableJoint rightLowerLeg;
    
    public Rigidbody head;
   

    public float health = 5f;
    public bool isDead = false;


    [Header("disable shits")]
    public MonoBehaviour[] crontollerSscript;
     public ConfigurableJoint[] alljoint;

    public void Start()
    {
        
        if(axe == null || max_armeture == null)
        {
            Debug.LogError("Axe collider or dummy is not assigned in AxeToched Script");
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if(collision.collider == axe)
        {
            health--;
            Debug.Log("OMG axe toched me.");
        }
    }
    void Update()
    {
        if(health <= 0 && !isDead)
        {
            isDead = true;
            
        }

        if(isDead == true)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalking", false);
            anim.SetBool("isMaxDead", true);
            anim.SetBool("isAttacking", false);
            anim.SetBool("isMasterDead", false);
            anim.SetBool("isWalkingBack", false);
            animation_scirpt.enabled = false;

            foreach (MonoBehaviour script in crontollerSscript)
            {
                script.enabled = false;
                Debug.LogError("Max is doed n word.");
            }

            foreach (ConfigurableJoint joint in alljoint)
            {
                joint.angularXMotion = ConfigurableJointMotion.Free;
                joint.angularYMotion = ConfigurableJointMotion.Free;
                joint.angularZMotion = ConfigurableJointMotion.Free;
            }

            head.freezeRotation = false;
        }
        
        
    }
}