using UnityEngine;

public class itemData : ScriptableObject
{
    public string itemName;
    public bool isWeapon;
    public int weaponIndex; // Maps to the index in your WeaponManager's array
    public GameObject dropPrefab; // For dropping it back into the world
}
