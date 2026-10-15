using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public InfimaGames.LowPolyShooterPack.Character characterScript; 

    [Header("Zoom (ADS) Settings")]
    public float normalFOV = 60f;
    public float zoomFOV = 45f;
    public float zoomSpeed = 10f;

    [Header("Recoil Settings")]
    public float recoilX = -2f; 
    public float recoilY = 0.5f; 
    public float recoilZ = 0.2f; 
    public float snappiness = 15f; 
    public float returnSpeed = 5f; 

    private Vector3 currentRotation;
    private Vector3 targetRotation;

    void Start()
    {
        if (cam == null) cam = GetComponent<Camera>();
    }

    void Update()
    {
        HandleZoom();
        HandleRecoil();
    }

    private void HandleZoom()
    {
        if (characterScript == null || cam == null) return;

        float targetFOV = characterScript.IsAiming() ? zoomFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
    }

    private void HandleRecoil()
    {
        if (cam == null) return;

        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.deltaTime);
        
        // THE FIX: We explicitly target 'cam.transform' instead of 'transform'
        // This ensures your player's body rotation (left/right look) is never overwritten.
        cam.transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void ApplyRecoil()
    {
        targetRotation += new Vector3(
            recoilX, 
            Random.Range(-recoilY, recoilY), 
            Random.Range(-recoilZ, recoilZ)
        );
    }
}
