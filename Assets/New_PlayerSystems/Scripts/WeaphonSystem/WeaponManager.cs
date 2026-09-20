using UnityEngine;
using UnityEngine.InputSystem;
public class WeaponsManager : MonoBehaviour
{
  [Header("Input")]
    public InputActionReference attackAction;
    public InputActionReference switchWeaponAction;

[Header("Input")]
public InputActionReference reloadAction;

    [Header("Setup")]
    public Transform weaponHolder; // Assign an empty GameObject attached to your Camera

    // Tracks the physical 3D models currently spawned in the player's hands
    private WeaponSystem[] equippedWeapons = new WeaponSystem[2];
    private int currentSlotIndex = 0;

    private void OnEnable()
    {
        attackAction.action.Enable();
        switchWeaponAction.action.Enable();
        switchWeaponAction.action.performed += ctx => SwitchWeapon();
        reloadAction.action.Enable();
    reloadAction.action.performed += ctx => ReloadCurrentWeapon();
    }

    private void OnDisable()
    {
        attackAction.action.Disable();
        switchWeaponAction.action.Disable();
        switchWeaponAction.action.performed -= ctx => SwitchWeapon();
        reloadAction.action.Disable();
    reloadAction.action.performed -= ctx => ReloadCurrentWeapon();
    }

    private void Update()
    {
        // Check if the current slot actually has a spawned weapon before trying to fire
        if (attackAction.action.IsPressed() && equippedWeapons[currentSlotIndex] != null)
        {
            equippedWeapons[currentSlotIndex].TryAttack();
        }
    }
    public bool AddAmmoToCurrentWeapon(int amount)
{
    if (equippedWeapons[currentSlotIndex] != null)
    {
        return equippedWeapons[currentSlotIndex].TryAddAmmo(amount);
    }
    return false; // Fails if hands are empty
}
    private void SwitchWeapon()
    {
        int nextSlot = (currentSlotIndex + 1) % 2;
        
        // Only allow switching if the other slot actually contains a weapon
        if (equippedWeapons[nextSlot] != null)
        {
            currentSlotIndex = nextSlot;
            UpdateWeaponVisibility();
        }
        equippedWeapons[currentSlotIndex].UpdateUI();
    }

    // Called dynamically by PlayerInventory
    public void SpawnAndEquipWeapon(ItemData weaponData, int slotNumber)
    {
        int arrayIndex = slotNumber - 1; // Convert Slot 1/2 to array index 0/1

        // If a weapon already exists in this hand slot, destroy its 3D model first to prevent overlapping meshes
        if (equippedWeapons[arrayIndex] != null)
        {
            Destroy(equippedWeapons[arrayIndex].gameObject);
        }

        // Spawn the new View Model prefab as a child of the weaponHolder
        GameObject newWeaponObj = Instantiate(weaponData.viewModelPrefab, weaponHolder);
        
        // Force the weapon to snap directly to the weaponHolder's exact position and rotation
        newWeaponObj.transform.localPosition = Vector3.zero;
        newWeaponObj.transform.localRotation = Quaternion.identity;

        // Store the component and auto-switch to this new weapon
        equippedWeapons[arrayIndex] = newWeaponObj.GetComponent<WeaponSystem>();
        currentSlotIndex = arrayIndex; 

        UpdateWeaponVisibility();
        equippedWeapons[currentSlotIndex].UpdateUI();
    }
    private void ReloadCurrentWeapon()
{
    if (equippedWeapons[currentSlotIndex] != null)
    {
        equippedWeapons[currentSlotIndex].TryReload();
    }
}

    private void UpdateWeaponVisibility()
    {
        for (int i = 0; i < equippedWeapons.Length; i++)
        {
            if (equippedWeapons[i] != null)
            {
                equippedWeapons[i].gameObject.SetActive(i == currentSlotIndex);
            }
        }
    }
}
