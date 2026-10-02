using UnityEngine;
using TMPro;

public class TraningPairSpawner : MonoBehaviour
{
    public GameObject traningPairPrefab;
    public TMP_InputField amountInput;
    public Transform SpwanOrigin;
    public float spacing = 10f;
    public int columns = 50;
    
    private int spawnedCount = 0;

    public void AddParis()
    {
        if(traningPairPrefab == null || amountInput == null || SpwanOrigin == null)
            return;
        if(!int.TryParse(amountInput.text, out int amount))
            return;
        if(amount <= 0)
            return;
        for(int i = 0; i < amount; i++)
        {
            spawnedCount++;
            int x = spawnedCount % columns;
            int z = spawnedCount / columns;

            Vector3 spwanPosition = SpwanOrigin.position + new Vector3(x * spacing, 0f, z * spacing);
            Instantiate(traningPairPrefab, spwanPosition, SpwanOrigin.rotation); 
        }
    }
} 