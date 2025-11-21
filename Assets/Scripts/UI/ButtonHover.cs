using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Transform target;   // 커질 대상 (보통 버튼 자신)
    public float scale = 1.1f; // 얼마나 커질지

    private Vector3 originalScale;

    void Start()
    {
        if (target == null)
            target = transform;

        originalScale = target.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        target.localScale = originalScale * scale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        target.localScale = originalScale;
    }
}
