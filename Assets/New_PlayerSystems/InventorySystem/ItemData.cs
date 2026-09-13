using UnityEngine;
public enum ItemType { Weapon, Ammo, Generic }
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
public string itemName;
    public bool isWeapon;
    public ItemType itemType;
    public Sprite uiIcon;
    [Header("Weapon Data")]
    public GameObject dropPrefab;      
    public GameObject viewModelPrefab;
    [Header("Ammo Data")]
    public int ammoAmount;
}