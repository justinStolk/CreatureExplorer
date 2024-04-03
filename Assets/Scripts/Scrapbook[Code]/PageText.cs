using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PageText : PageComponent
{
    public TMP_InputField TextField { get; private set; }

    [SerializeField] private float minimumTextSize, maximumTextSize;

    private void Awake()
    {
        if(_rectTransform == null)
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        TextField = GetComponent<TMP_InputField>();
        if (!TextField)
        {
            throw new System.NullReferenceException("No TextMeshPro InputField found on object named: " + name);
        }
    }

    public override void Grab(Transform handTransform)
    {
        base.Grab(handTransform);

        TextField.interactable = false;
    }

    public override void Release()
    {
        base.Release();

        TextField.interactable = true;
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        TextField.interactable = false;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        TextField.interactable = true;
    }

    protected override void TurnAndScale(Vector2 delta)
    {
        _rectTransform.Rotate(new(0, 0, delta.x));
        TextField.pointSize = Mathf.Clamp(TextField.pointSize + delta.y, minimumTextSize, maximumTextSize);
    }

}
