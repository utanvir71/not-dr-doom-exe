using UnityEngine;
using UnityEngine.UIElements;

public class masterMovement : MonoBehaviour
{
   
    public Transform max_pelvis;
    public Transform master;
    public float distance;
    public Vector3 direction;
    public float trashhold = 1.5f;
    public Transform forwardReference;
    public float speed=1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(forwardReference == null)
        {
            Debug.LogError("forward reference is null in master movement script");
            enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        distance = Vector3.Distance(max_pelvis.transform.position, master.transform.position);
        direction = max_pelvis.position - master.position;
        direction.y = 0;
        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
        if(distance > trashhold)
        { 
            transform.position += (direction.normalized * speed * Time.deltaTime);
            transform.rotation = rotation;
            
        }

        if (Input.GetKey("e")||Input.GetKey("x")||Input.GetKey("c")) 
        {   
            if(Input.GetKey(KeyCode.LeftShift))
                speed = 2f;
            else speed = 1f;
            transform.position += forwardReference.forward * speed * Time.deltaTime;
        }
        // else if (Input.GetKey("f"))
        // {
        //     transform.position -= forwardReference.forward * speed * Time.deltaTime;
        // }
        // if (Input.GetKeyDown("x"))
        // {
        //     transform.Rotate(Vector3.up, -90f);
        // }
        // else if(Input.GetKeyUp("x"))
        // {
        //     transform.Rotate(Vector3.up, 90f);
        // }
        // else if(Input.GetKeyDown("c"))
        // {
        //     transform.Rotate(Vector3.up, 90f);
        // }
        // else if (Input.GetKeyUp("c"))
        // {
        //     transform.Rotate(Vector3.up, -90f);
        // }
        
    }
}
