using UnityEngine;

public class MaxAxeHitDetector : MonoBehaviour
{
    public ProceduralAxeSwing axeSwing;
    private bool hitThisSwing = false;
    private void Update()
    {
        if (!axeSwing.IsSwinging)
        {
            hitThisSwing = false;
        }
    }
    
     private void OnTriggerEnter(Collider other)
    {
        if(!axeSwing.IsSwinging)
            return;
        if(hitThisSwing)
            return;
        MasterHitRecevier master = other.GetComponentInParent<MasterHitRecevier>();
        if(master == null)
        {
            return;
        }
        hitThisSwing = true;
        master.ReciverHit();
        Debug.Log("yes! axe touched fuk uu hitbox");
        
    }

}
