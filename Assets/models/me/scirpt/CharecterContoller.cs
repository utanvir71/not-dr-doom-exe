using System.Runtime.Serialization;
using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CharecterContoller : MonoBehaviour
{
    public Rigidbody playerrb;
    public float horizontalMovement;
    public float verticalMovement;
    public bool isJump = false;
    public float speed = 10;
    public float jumpForce = 12;
    public bool isGrounded = true;
    [SerializeField] private Transform forwardReference;
    


    void Start()
    {
        if(playerrb == null)
        {
            playerrb = GetComponent<Rigidbody>();
        }
    }
    void Update()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");
        if(Input.GetButtonDown("Jump"))
        {
           
            isJump = true;
        } 
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        Vector3 move = forwardReference.forward * verticalMovement + transform.right * horizontalMovement;
        Vector3 moveDirection = move.normalized;
        playerrb.linearVelocity = new Vector3(moveDirection.x * speed, playerrb.linearVelocity.y, moveDirection.z * speed);
        if (isJump)
        {
            if (isGrounded)
            {
                playerrb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false;
            }
            isJump = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 3)
        {
            isGrounded = true;
        }
    }

}
