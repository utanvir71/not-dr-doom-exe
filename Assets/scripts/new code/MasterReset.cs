using UnityEngine;

public class MasterReset : MonoBehaviour
{
    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;
    private Animator anim;

    private void Awake()
    {
        startLocalPosition = transform.localPosition;
        startLocalRotation = transform.localRotation;
        anim = GetComponent<Animator>();
    }

    public void ResetMaster()
    {
        Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));

        transform.localPosition = startLocalPosition + randomOffset;
        transform.localRotation = startLocalRotation;

        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }

        Debug.Log("Master reset local position: " + transform.localPosition);
    }
}