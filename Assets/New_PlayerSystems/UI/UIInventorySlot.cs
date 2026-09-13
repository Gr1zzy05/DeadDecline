using UnityEngine;
using UnityEngine.EventSystems;

public class UIInventorySlot : MonoBehaviour, IDropHandler
{
    public enum SlotType { Backpack, WeaponSlot1, WeaponSlot2, DropToWorld }
    public SlotType slotType;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        UIDraggableItem draggableItem = dropped.GetComponent<UIDraggableItem>();

        if (draggableItem == null) return;

        // Logic check: Only allow weapons in Weapon Slots
        if ((slotType == SlotType.WeaponSlot1 || slotType == SlotType.WeaponSlot2) 
            && draggableItem.currentItem.itemType != ItemType.Weapon)
        {
            Debug.LogWarning("Cannot put ammo/generic items in weapon slots!");
            return;
        }

        if (slotType == SlotType.DropToWorld)
        {
           // 1. Calculate drop position in front of player
        Transform cam = Camera.main.transform;
        Vector3 dropPos = cam.position + (cam.forward * 1.5f);
        
        // 2. Spawn the physical 3D model back into the game
        Instantiate(draggableItem.currentItem.dropPrefab, dropPos, Quaternion.identity);
        
        // 3. Remove from backend backend and destroy UI icon
        PlayerInventory.Instance.backpack.Remove(draggableItem.currentItem);
        Destroy(draggableItem.gameObject);
        return;
        }

        // If the slot is empty, accept the item
        if (transform.childCount == 0)
        {
           if ((slotType == SlotType.WeaponSlot1 || slotType == SlotType.WeaponSlot2) 
            && draggableItem.currentItem.itemType != ItemType.Weapon) return;

        draggableItem.parentAfterDrag = transform;

        // Sync with Weapon Manager
        WeaponsManager wm = PlayerInventory.Instance.GetComponentInParent<WeaponsManager>();
        
        if (slotType == SlotType.WeaponSlot1)
        {
            PlayerInventory.Instance.weaponSlot1 = draggableItem.currentItem;
            wm.SpawnAndEquipWeapon(draggableItem.currentItem, 1);
        }
        else if (slotType == SlotType.WeaponSlot2)
        {
            PlayerInventory.Instance.weaponSlot2 = draggableItem.currentItem;
            wm.SpawnAndEquipWeapon(draggableItem.currentItem, 2);
        }
        }
    }
}