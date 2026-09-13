using UnityEngine;
using TMPro; // Requires TextMeshPro

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance;

    [Header("UI Elements")]
    public TextMeshProUGUI ammoText;

    private void Awake()
    {
        // Sets up the Singleton so other scripts can find it easily
        Instance = this; 
    }

    public void UpdateAmmoDisplay(int current, int reserve)
    {
        ammoText.text = $"{current} / {reserve}";
    }

    public void ShowMeleeDisplay()
    {
        ammoText.text = "---";
    }
}