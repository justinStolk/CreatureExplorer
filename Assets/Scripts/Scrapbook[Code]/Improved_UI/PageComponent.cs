using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public abstract class PageComponent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IGrabbable
{
    //public System.Action OnComponentRemoved;
    [SerializeField] private LayerMask uiLayer;

    public Vector3 Pos { get => _rectTransform.position; }

    protected RectTransform _rectTransform;
    protected Transform previousParent;

    private Vector3 startPosition;
    private Vector2 startSize;
    private PageComponentInteractor currentInteractor;


    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }
    public void SetInteractor(PageComponentInteractor interactor)
    {
        if(currentInteractor != null)
        {
            currentInteractor.RemoveFromInteractor(this);
        }
        currentInteractor = interactor;
    }

    public virtual void Grab(Transform handTransform)
    {
        OnBeginDrag();

        _rectTransform.SetParent(handTransform, true);
    }

    public virtual void Release()
    {
        RaycastHit[] results = Physics.RaycastAll(_rectTransform.position, _rectTransform.forward, 10, uiLayer);

        foreach (RaycastHit rayResult in results)
        {
            if (rayResult.collider.TryGetComponent(out PageComponentInteractor pageComponentInteractor))
            {
                //_rectTransform.position = rayResult.point;
                _rectTransform.position +=  _rectTransform.forward * rayResult.distance;
                //_rectTransform.SetPositionAndRotation(rayResult.point, rayResult.transform.rotation);

                if (pageComponentInteractor.OnComponentDroppedOn(this))
                {
                    _rectTransform.localEulerAngles = new Vector3(0,0, _rectTransform.localEulerAngles.z);
                    // TODO: implement scaling
                    _rectTransform.sizeDelta = startSize;
                    
                    if (currentInteractor != pageComponentInteractor)
                    {
                        SetInteractor(pageComponentInteractor);
                    }
                    return;
                }
            }
        }

        // This should never be null if the player can drag the component
        if (previousParent != null)
        {
            _rectTransform.SetParent(previousParent);
        }

        _rectTransform.localEulerAngles = new Vector3(0, 0, _rectTransform.localEulerAngles.z);
        _rectTransform.sizeDelta = startSize;
        _rectTransform.localPosition = startPosition;
        //_rectTransform.SetLocalPositionAndRotation(startPosition, Quaternion.identity);
    }

    public virtual void OnBeginDrag(PointerEventData eventData = null)
    {
        startPosition = _rectTransform.localPosition;
        startSize = _rectTransform.sizeDelta;
        previousParent = _rectTransform.parent;
    }

    public virtual void OnDrag(PointerEventData eventData = null)
    {
        if (!VRChecker.IsVR)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                Move(eventData.position);
            }
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                TurnAndScale(eventData.delta);
            }
        }
    }

    public virtual void OnEndDrag(PointerEventData eventData = null)
    {
            List<RaycastResult> results = new();
        if (!VRChecker.IsVR)
        {
            Scrapbook.Instance.Raycaster.Raycast(eventData, results);
        }
        else
        {
            Scrapbook.Instance.VRRaycaster.Raycast(eventData as ExtendedPointerEventData, results);
        }

            foreach (RaycastResult rayResult in results)
            {
                if (rayResult.gameObject.TryGetComponent(out PageComponentInteractor pageComponentInteractor))
                {
                    if (pageComponentInteractor.OnComponentDroppedOn(this))
                    {
                        if (currentInteractor != pageComponentInteractor)
                        {
                            SetInteractor(pageComponentInteractor);
                        }
                        return;
                    }
                }
            }
            // This should never be null if the player can drag the component
            if (previousParent != null)
            {
                transform.SetParent(previousParent);
            }
            _rectTransform.localPosition = startPosition;
    }

    private void Move(Vector2 position)
    {
        _rectTransform.localPosition = position;
    }

    protected virtual void TurnAndScale(Vector2 delta)
    {
        _rectTransform.Rotate(new(0, 0, delta.x));
        float scale = Mathf.Clamp(_rectTransform.localScale.x + delta.y * Time.fixedDeltaTime, 0.5f, 1.5f);
        _rectTransform.localScale = Vector3.one * scale;
    }
}
