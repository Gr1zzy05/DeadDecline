using UnityEngine;
using TMPro; // Requires TextMeshPro

public class PlayerUI : MonoBehaviour
{public static PlayerUI Instance;

    [Header("UI Elements")]
    public TextMeshProUGUI ammoText;
    public GameObject jammedWarningUI; // Add this line

    private void Awake()
    {
        Instance = this; 
        
        // Ensure the warning is hidden when the game starts
        if (jammedWarningUI != null) jammedWarningUI.SetActive(false);
    }

    public void UpdateAmmoDisplay(int current, int reserve)
    {
        ammoText.text = $"{current} / {reserve}";
    }

    public void ShowMeleeDisplay()
    {
        ammoText.text = "---";
        SetJamWarning(false); // Melee weapons cannot jam, so always hide it
    }

    // New method to toggle the visual warning
    public void SetJamWarning(bool isJammed)
    {
        if (jammedWarningUI != null)
        {
            jammedWarningUI.SetActive(isJammed);
        }
    }
}