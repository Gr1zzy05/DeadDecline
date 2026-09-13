using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour
{
    [Header("Interaction")]
    public float pickupRange = 10f;
    public InputActionReference interactAction;
    public LayerMask interactLayer;

    [Header("Inventory Slots")]
    public ItemData weaponSlot1;
    public ItemData weaponSlot2;
    public List<ItemData> backpack = new List<ItemData>();

    [Header("UI References")]
public Transform backpackGridUI; // Drag your UI BackpackGrid panel here
public GameObject uiItemPrefab;  // Drag your UIDraggableItem Prefab here

    private WeaponsManager weaponManager;
    private Camera playerCam;

    public static PlayerInventory Instance;
    private void Awake()
{
    Instance = this;
}
    private void Start()
    {
        playerCam = GetComponent<Camera>();
        weaponManager = GetComponentInParent<WeaponsManager>();
    }

    private void OnEnable() => interactAction.action.Enable();
    private void OnDisable() => interactAction.action.Disable();

    private void Update()
    {
       if (interactAction.action.triggered)
    {
        Debug.Log("1. Interact button triggered.");
        TryPickupItem();
    }
    }

    private void TryPickupItem()
    {
       Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);
    // Draws a visible green line in the Scene View to show where the ray is shooting
    Debug.DrawRay(ray.origin, ray.direction * pickupRange, Color.green, 2f);

    if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, interactLayer))
    {
        Debug.Log($"2. Raycast hit object: {hit.collider.name} on Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
        
       PickupItem pickup = hit.collider.GetComponentInParent<PickupItem>();
        if (pickup != null)
        {
            Debug.Log("3. PickupItem component found. Routing to inventory.");
            RouteItemToInventory(pickup.itemData);
            pickup.OnPickedUp();
        }
        else
        {
            Debug.LogWarning($"3. Object {hit.collider.name} was hit, but it does NOT have a PickupItem script attached.");
        }
    }
    else
    {
        Debug.LogWarning("2. Raycast fired but hit NOTHING. Check LayerMask, Range, and Colliders.");
    }
    }

    private void RouteItemToInventory(ItemData item)
    {   
        if (item.itemType == ItemType.Ammo)
    {
        bool consumed = weaponManager.AddAmmoToCurrentWeapon(item.ammoAmount);
        
        if (consumed)
        {
            Debug.Log($"Consumed {item.itemName}. Added {item.ammoAmount} bullets.");
            return; // Stop here. The item is consumed and will NOT go to the backpack.
        }
        else
        {
            Debug.Log($"Holding a melee weapon or hands empty. Stashing {item.itemName} in backpack.");
            // If we are holding a sword, it fails to consume, so we let it fall through to the backpack
        }
    }

   if (item.isWeapon)
    {
        if (weaponSlot1 == null)
        {
            weaponSlot1 = item;
            weaponManager.SpawnAndEquipWeapon(item, 1); // Pass the data and specify Slot 1
            Debug.Log($"Equipped {item.itemName} to Slot 1");
            return;
        }
        else if (weaponSlot2 == null)
        {
            weaponSlot2 = item;
            weaponManager.SpawnAndEquipWeapon(item, 2); // Pass the data and specify Slot 2
            Debug.Log($"Equipped {item.itemName} to Slot 2");
            return;
        }
    }
    
     backpack.Add(item);
    Debug.Log($"Added {item.itemName} to Backpack");

    UIInventorySlot[] allSlots = backpackGridUI.GetComponentsInChildren<UIInventorySlot>();
    UIInventorySlot targetSlot = null;
    
    foreach (var slot in allSlots)
    {
        // Check if it is a backpack slot AND it has no children (meaning it is empty)
        if (slot.slotType == UIInventorySlot.SlotType.Backpack && slot.transform.childCount == 0)
        {
            targetSlot = slot;
            break; // Stop searching once we find an empty one
        }
    }

    if (targetSlot != null)
    {
        // Spawn the UI item INSIDE the empty slot
        GameObject newUI = Instantiate(uiItemPrefab, targetSlot.transform);
        newUI.GetComponent<UIDraggableItem>().Setup(item);
    }
    else
    {
        Debug.LogWarning("The visual Backpack UI is full! Cannot spawn UI icon.");
    
}
    }
}