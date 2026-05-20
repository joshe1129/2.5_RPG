using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A utility script to detect UI hover events and pass them via C# Actions.
/// Designed to decouple deep UI interaction details from the core game logic.
/// </summary>
public class UIHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Action<int> OnHoverEntered;
    public Action<int> OnHoverExited;
    
    public int index;

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHoverEntered?.Invoke(index);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnHoverExited?.Invoke(index);
    }
}
