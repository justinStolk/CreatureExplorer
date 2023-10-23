using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class MoveablePageComponent : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public bool PlacedOnPage { get; protected set; }

    protected float _scaleFactor = 1.2f;

    protected RectTransform _componentTransform;
    protected RectTransform _parentTransform;

    protected Image _componentGraphic;

    protected float halfWidth;
    protected float halfHeight;

    private void Awake()
    { 
        _componentTransform = GetComponent<RectTransform>();
        if(_componentTransform.parent != null)
        {
            _parentTransform = _componentTransform.parent.GetComponent<RectTransform>();
        }

        _componentGraphic = GetComponent<Image>();
    }
    protected void SetHalfSizes()
    {
        halfWidth = _componentTransform.rect.width * 0.5f;
        halfHeight = _componentTransform.rect.height * 0.5f;
    }
    public virtual void OnBeginDrag(PointerEventData eventData) 
    {
        Color c = _componentGraphic.color;
        _componentGraphic.color = new Color(c.r, c.g, c.b, 0.5f);
        _componentTransform.localScale = _componentTransform.localScale * _scaleFactor;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            float componentX = Mathf.Clamp(eventData.position.x, _parentTransform.rect.xMin + halfWidth * _componentTransform.localScale.x, _parentTransform.rect.xMax - halfWidth * _componentTransform.localScale.x);
            float componentY = Mathf.Clamp(eventData.position.y, _parentTransform.rect.yMin + halfHeight * _componentTransform.localScale.y, _parentTransform.rect.yMax - halfHeight * _componentTransform.localScale.y);

            _componentTransform.localPosition = new Vector2(componentX, componentY);
        }else if(eventData.button == PointerEventData.InputButton.Middle)
        {
            _componentTransform.Rotate(new Vector3(0, 0, eventData.delta.y));
        }
    }

    public virtual void OnEndDrag(PointerEventData eventData) 
    {
        Color c = _componentGraphic.color;
        _componentGraphic.color = new Color(c.r, c.g, c.b, 1);
        _componentTransform.localScale = _componentTransform.localScale / _scaleFactor;
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Right)
        {
            Destroy(gameObject);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Scrapbook.Instance.SwapTargetComponent(null);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Scrapbook.Instance.SwapTargetComponent(this);
    }
}
