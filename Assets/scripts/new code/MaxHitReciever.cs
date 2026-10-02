using UnityEngine;

public class MaxHitReciever : MonoBehaviour
{
    public Collider masterSword;
    public MaxAgent maxAgent;
    public Transform pelvis;

    public float hitCooldown = 0.8f;
    private float lastHitTime = -999f;
    
    private void OnCollisionEnter(Collision collision)
    {
        bool isMaxBodyPart = transform == pelvis || transform.IsChildOf(pelvis);
        bool isMasterSword = collision.collider == masterSword;

        if(isMaxBodyPart && isMasterSword)
        {
            if((Time.time - lastHitTime < hitCooldown))
                return;
            lastHitTime = Time.time;
            Debug.LogError("FUK YU SWORD HIT MAX");
            maxAgent.MasterHitMax();
        }
    }
}