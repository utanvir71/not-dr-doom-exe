using UnityEngine;

class lookAround : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform player;

    public float yRotation = 0f;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yRotation -= mouseY;
        yRotation = Mathf.Clamp(yRotation, -90f, 90f);

        player.Rotate(Vector3.up * mouseX);
        transform.localRotation = Quaternion.Euler(yRotation, 0f, 0f);
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true; 
        }
    }
}