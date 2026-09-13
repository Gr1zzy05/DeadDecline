using System;
using UnityEngine;

public class PickupItem : MonoBehaviour
{
   public ItemData itemData;

    public void OnPickedUp()
    {
        // Disable instead of Destroy to avoid Garbage Collection spikes
        gameObject.SetActive(false); 
        Debug.Log("Weaponpickup");
    }
}
