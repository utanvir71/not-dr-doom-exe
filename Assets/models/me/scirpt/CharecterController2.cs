using UnityEngine;

public class CharecterController2 : MonoBehaviour
{
    public CharacterController controller;
    public Rigidbody playerRB;
    public float speed = 12f;
    public float jumpForce = 10f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(controller == null)
        {
            controller = GetComponent<CharacterController>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.forward * z + transform.right * x;
        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButton("Jump"))
        {
            playerRB.AddForce(transform.up * jumpForce);
        }
        
    }
}
