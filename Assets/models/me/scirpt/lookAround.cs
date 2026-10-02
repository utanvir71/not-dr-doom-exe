using UnityEngine;

public class lookAround : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float verticalLimit = 90f;
    [SerializeField] private Transform forwardReference;
    [SerializeField] private Transform headPos;
    [SerializeField] private Transform body;

    private float yaw;
    private float pitch;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;

        pitch = Mathf.Clamp(pitch, -verticalLimit, verticalLimit);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        forwardReference.rotation = Quaternion.Euler(0f, yaw, 0f);
        body.rotation = Quaternion.Euler(0f, yaw, 0f);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    void LateUpdate()
    {
        transform.position = headPos.position;
    }
}