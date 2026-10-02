using UnityEngine;

public class MasterHitRecevier : MonoBehaviour
{    
    public MaxAgent maxAgent;

    public void ReciverHit()
    {
        if(maxAgent == null)
        {
            Debug.LogError("MaxAgent is not assigned in MasterHitReciver!");
            return;
        }
        maxAgent.MaxHitMaster();
    }
}
 