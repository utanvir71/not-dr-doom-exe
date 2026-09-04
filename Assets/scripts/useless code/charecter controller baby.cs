using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;

public class charectercontrollerbaby : MonoBehaviour
{
    public Rigidbody player;
    public float speed = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        UnityEngine.Vector3 move = new  UnityEngine.Vector3(-moveX, 0, -moveZ);
        player.linearVelocity = new  UnityEngine.Vector3(move.x * speed, player.linearVelocity.y, move.z * speed);

        if (Input.GetButtonDown("Jump"))
        {
            player.linearVelocity =  new UnityEngine.Vector3(0, speed, 0);
        }
    }
}
