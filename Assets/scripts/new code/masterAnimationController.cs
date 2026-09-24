using NUnit.Framework;
using UnityEngine;

public class masterAnimationController : MonoBehaviour
{
    [Header("References")]
    public Transform max_pelvis;
    public Transform master;

    [Header("Settings")]
    public float distance;
    public Vector3 direction;
    public float trashhold = 1.5f;
    

    [SerializeField] private Animator anim;
    // public bool ePressed; 
    public void Start()
    {
        if(anim == null)
            anim = GetComponent<Animator>(); 

        anim.SetBool("isIdle", true);
        anim.SetBool("isWalking", true);
        anim.SetBool("isMaxDead", false);
        anim.SetBool("isAttacking", false);
        anim.SetBool("isMasterDead", false);
        anim.SetBool("isWalkingBack", false);
    }
    public void Update()
    {

        distance = Vector3.Distance(max_pelvis.transform.position, master.transform.position);
        if(distance > trashhold)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalking", true);
            anim.SetBool("isMaxDead", false);
            anim.SetBool("isAttacking", false);
            anim.SetBool("isMasterDead", false);
            anim.SetBool("isWalkingBack", false);
        }
        else
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalking", false);
            anim.SetBool("isMaxDead", false);
            anim.SetBool("isAttacking", true); 
            anim.SetBool("isMasterDead", false);
            anim.SetBool("isWalkingBack", false);
        }
        
        // if ((Input.GetKey("e") || Input.GetKey("x") || Input.GetKey("c")))
        // {
        //     anim.SetBool("isIdle", false);
        //     anim.SetBool("isWalking", true);
        //     anim.SetBool("isMaxDead", false);
        //     anim.SetBool("isAttacking", false);
        //     anim.SetBool("isMasterDead", false);
        //     anim.SetBool("isWalkingBack", false);
        //     ePressed = true;
        //     anim.speed = Input.GetKey(KeyCode.LeftShift) ? 2f : 1f; 
        // }
        // else if(Input.GetKey("f"))
        // {
        //     anim.SetBool("isIdle", false);
        //     anim.SetBool("isWalking", false);
        //     anim.SetBool("isMaxDead", false);
        //     anim.SetBool("isAttacking", false);
        //     anim.SetBool("isMasterDead", false);
        //     anim.SetBool("isWalkingBack", true);
        //     ePressed = false;
        // }
        // else
        // {
        //     anim.SetBool("isIdle", true);
        //     anim.SetBool("isWalking", false);
        //     anim.SetBool("isMaxDead", false);
        //     anim.SetBool("isAttacking", false);
        //     anim.SetBool("isMasterDead", false);
        //     anim.SetBool("isWalkingBack", false);
        //     ePressed = false;
        // }
        // if (Input.GetMouseButton(1))
        // {
        //     anim.SetBool("isIdle", false);
        //     anim.SetBool("isWalking", false);
        //     anim.SetBool("isMaxDead", false);
        //     anim.SetBool("isAttacking", true);
        //     anim.SetBool("isMasterDead", false);
        //     anim.SetBool("isWalkingBack", false);
        // }
        
    }
}