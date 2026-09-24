using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [Header("References")]
    public Transform camera;
    public Transform player;
    [Header("Settings")]
    public Vector3 offset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(camera == null)
        {
            camera = GetComponent<Transform>();
        }
        if (player == null)
        {
            Debug.LogError("player reference is null in the camera follow script.");
            enabled = false;
        }
        camera.transform.position = player.transform.position + offset;

    }

    // Update is called once per frame
    void LateUpdate()
    {
        camera.transform.position = player.transform.position + offset;
    }
}
