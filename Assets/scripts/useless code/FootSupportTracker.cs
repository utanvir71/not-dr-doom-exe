using UnityEngine;

public class FootSupportTracker : MonoBehaviour
{
    [Header("Feet")]
    public Transform leftFoot;
    public Transform rightFoot;
    [Header("Ground Detection")]

    public LayerMask groundMask;
    public float rayStartheight = 0.15f;
    public float rayDistance = 0.5f;
    [Header("Current Support")]
    public bool leftGrounded;
    public bool rightGrounded;
    public bool hasSupport;
    public Vector3 leftGroundedPoint;
    public Vector3 rightGroundedPoint;
    public Vector3 supportCenter;
    [Header("Debug")]
    public bool showDebug = true;
    public float debugSize = 0.2f;

    public void FixedUpdate()
    {
        leftGrounded = CheckFoot(leftFoot, out leftGroundedPoint);
        rightGrounded = CheckFoot(rightFoot, out rightGroundedPoint);
        if(leftGrounded && rightGrounded)
        {
            supportCenter = 
            (
                leftGroundedPoint + rightGroundedPoint
            ) * 0.5f;
            hasSupport = true;
        }
        else if (leftGrounded)
        {
            supportCenter = leftGroundedPoint;
            hasSupport = true;
        }
        else if (rightGrounded)
        {
            supportCenter = rightGroundedPoint;
            hasSupport = true;
        }
        else
        {
            hasSupport = false;
            supportCenter = Vector3.zero;
        }
        if(showDebug && hasSupport)
        {
            Debug.DrawLine(
                supportCenter - Vector3.up * debugSize,
                supportCenter + Vector3.up * debugSize,
                Color.green
            );
            Debug.DrawLine(
                supportCenter - Vector3.right * debugSize,
                supportCenter + Vector3.right * debugSize,
                Color.green
            );
            Debug.DrawLine(
                supportCenter - Vector3.forward * debugSize,
                supportCenter + Vector3.forward * debugSize,
                Color.green
            );
        }
    }
    private bool CheckFoot(Transform foot, out Vector3 groundPoint)
    {
        if(foot == null)
        {
            groundPoint = Vector3.zero;
            return false;
        }
        Vector3 rayStart = foot.position + Vector3.up * rayStartheight;
        bool hitGround = Physics.Raycast(
            rayStart,
            Vector3.down,
            out RaycastHit hit,
            rayDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
        if (hitGround)
        {
            groundPoint = hit.point;
        }
        else
        {
            groundPoint = foot.position;
        }
        if (showDebug)
        {
            Color rayColor = hitGround ? Color.green : Color.red;
                Debug.DrawRay(
                rayStart,
                Vector3.down * rayDistance,
                rayColor
            );
        }
        return hitGround;
    }
}