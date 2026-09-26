using UnityEngine;

public class AxeHitReward : MonoBehaviour
{
    public MaxAgent maxAgent;
    public ProceduralAxeSwing axeSwing;
    
    private bool rewardThisSwing = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Axe hit scipt is active on: " + gameObject.name);
    }

    void Update()
    {
        if (!axeSwing.IsSwinging)
        {
            rewardThisSwing = false;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        
        if(
            other.CompareTag("Dummy") && axeSwing.IsSwinging && !rewardThisSwing
        )
        {
            maxAgent.AxeHitTarget();
            rewardThisSwing = true;
            Debug.Log("Axe hit dummy");
        }
    }
    
}
