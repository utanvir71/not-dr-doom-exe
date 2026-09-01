using UnityEngine;

public class MaxIntroCamera : MonoBehaviour
{
    public Vector3 moveDirection = new Vector3(1f,0f,0f);
    public float moveSpeed = 1f;
    public Vector3 rotationSpeed = new Vector3(0f, 10f, 0f);

    private void LateUpdate()
    {
        transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
        Quaternion rotationThisFrame = Quaternion.Euler(rotationSpeed * Time.deltaTime);
        transform.rotation = transform.rotation * rotationThisFrame;
    }
}