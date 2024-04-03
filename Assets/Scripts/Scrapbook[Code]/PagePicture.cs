using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PagePicture : PageComponent, IPointerClickHandler
{
    public PictureInfo PictureInfo { get; private set; }
    [field: ShowOnly]public bool evaluated = false;

    public static System.Action<PagePicture> OnPictureClicked;
    public static System.Action OnBeginPictureDrag;
    public static System.Action OnEndPictureDrag;

    [SerializeField] private Image pictureGraphic;
    [SerializeField] private float pageScaleFactor = 2.5f;

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

    public void SetPicture(Sprite pictureSprite)
    {
        pictureGraphic.sprite = pictureSprite;
    }

    public override void Grab(Transform handTransform)
    {
        if (soundPlayer != null)
        {
            soundPlayer.PlaySound(dragSound, true);
        }

        dragging = true;
        OnBeginPictureDrag?.Invoke();

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

        OnEndPictureDrag?.Invoke();
        dragging = false;

        base.Release();
    }

    public void LinkPictureInformation(PictureInfo information)
    {
        PictureInfo = information;
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (soundPlayer != null)
        {
            soundPlayer.PlaySound(dragSound, true);
        }

        dragging = true;
        OnBeginPictureDrag?.Invoke();
        base.OnBeginDrag(eventData);

        SetStepBackParent();

        if(parentCanvas == null)
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
        OnEndPictureDrag?.Invoke();
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

    public void OnPointerClick(PointerEventData eventData)
    {
        //return;

        if (eventData.button == PointerEventData.InputButton.Left && !dragging)
        {
            OnPictureClicked?.Invoke(this);
            return;
        }
    }
}
