using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// This automatically adds the CanvasGroup component to your prefab
[RequireComponent(typeof(CanvasGroup))] 
public class UIDraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Transform parentAfterDrag;
    public ItemData currentItem;
    
    private Image image;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Setup(ItemData data)
    {
        currentItem = data;
        if (data.uiIcon != null)
        {
            image.sprite = data.uiIcon;
        }
        else
        {
            Debug.LogWarning($"Warning: {data.itemName} is missing a UI Icon Sprite in its ItemData!");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        
        // This makes the entire UI object ignore the mouse while dragging
        canvasGroup.blocksRaycasts = false; 
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag);
        
        // Turn the mouse detection back on when dropped
        canvasGroup.blocksRaycasts = true; 
    }
}