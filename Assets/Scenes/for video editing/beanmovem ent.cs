using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    Rigidbody rigidbody;
    public Transform player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("Vertical") > 0)
        {
            rigidbody.AddForce(0, 0, 10f);
        }
        else if (Input.GetAxis("Vertical") < 0)
        {
            rigidbody.AddForce(0,0,-10f);
        }
        if (Input.GetAxis("Horizontal")>0)
        {
            rigidbody.AddForce(10, 0, 0);
        }
        else if (Input.GetAxis("Horizontal") < 0)
        {
            rigidbody.AddForce(-10, 0, 0);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            rigidbody.AddForce(0, 10, 0);
        }
    }
}
