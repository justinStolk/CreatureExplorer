using UnityEngine;
using UnityEngine.EventSystems;

// A sticker class for the scrapbook. May have added functionality later.
public class ScrapbookSticker : PageComponent, IPointerUpHandler, IDragHandler
{
    public bool IsTemplate = false;

    //private RectTransform rectTransform;

    [field: SerializeField] private AudioClip dragSound, stickSound;
    private SoundPlayer soundPlayer;

    private bool dragging;
    private Transform stepBackParent;
    private Canvas parentCanvas;


    private void Awake()
    {
        if (_rectTransform == null)
        {
            _rectTransform = GetComponent<RectTransform>();
        }
        parentCanvas = GetComponentInParent<Canvas>();

        soundPlayer = GetComponent<SoundPlayer>();
        if (soundPlayer == null)
            soundPlayer = GetComponentInParent<SoundPlayer>();
    }

    public override void Grab(Transform handTransform)
    {
        if (soundPlayer != null)
        {
            soundPlayer.PlaySound(dragSound, true);
        }

        dragging = true;

        if (parentCanvas == null)
        {
            parentCanvas = GetComponentInParent<Canvas>();
        }

        base.Grab(handTransform);

        SetStepBackParent();
    }

    public override void Release()
    {
        if (soundPlayer != null)
        {
            soundPlayer.PlaySound(stickSound, true);
        }

        dragging = false;

        base.Release();
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (soundPlayer != null)
        {
            soundPlayer.PlaySound(dragSound, true);
        }

        dragging = true;
        base.OnBeginDrag(eventData);

        SetStepBackParent();

        if (parentCanvas == null)
        {
            parentCanvas = GetComponentInParent<Canvas>();
        }
        transform.SetParent(parentCanvas.transform);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (soundPlayer != null)
        {
            soundPlayer.PlaySound(stickSound, true);
        }

        base.OnEndDrag(eventData);
        dragging = false;
    }

    public void SetStepBackParent()
    {
        stepBackParent = previousParent;
    }

    public void OnRevert()
    {
        PageComponentInteractor interactor = stepBackParent.GetComponentInParent(typeof(PageComponentInteractor), true) as PageComponentInteractor;

        if (interactor == null)
        {
# if UNITY_EDITOR
            Debug.Log(stepBackParent.name);
#endif
            throw new System.NullReferenceException("Something went wrong, in the parent transform or one of their parent's transforms, there should be a Page Component Interactor");
        }
        if (interactor.OnComponentDroppedOn(this))
        {
            SetInteractor(interactor);
        }
# if UNITY_EDITOR
        else
        {
            Debug.Log(interactor.name);
            Debug.LogError("For some reason, the picture cannot be return to it's parent, this shouldn't happen");
        }
#endif
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (IsTemplate)
        {
            // Instantiate a new sticker on this spot, without additional functions.
            Vector3 dropLocation = _rectTransform.anchoredPosition;
            ScrapbookSticker newSticker = Instantiate(this, dropLocation, Quaternion.identity);
            newSticker.IsTemplate = false;
        } 
    }
}
