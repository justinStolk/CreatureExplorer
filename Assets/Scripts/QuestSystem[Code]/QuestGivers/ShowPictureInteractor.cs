using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShowPictureInteractor : PageComponentInteractor
{
    [SerializeField] private UnityEvent<PagePicture> OnPictureDroppedOn;
    [SerializeField] private Vector2 pictureSize;

    private PagePicture shownPicture;

    private RectTransform rectTransform;

    private Vector2 originalPicScale;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public override bool OnComponentDroppedOn(PageComponent component)
    {
        if (component.GetType() == typeof(PagePicture))
        {
            originalPicScale = component.GetComponent<RectTransform>().sizeDelta;

            component.transform.SetParent(transform);

            StartCoroutine(ResizePicture(component as PagePicture));
            return true;
        }
        return false;
    }

    public override void RemoveFromInteractor(PageComponent component)
    {
        if (shownPicture == component)
        {
            component.GetComponent<RectTransform>().rotation = Quaternion.identity;

            SetSize(component.GetComponent<RectTransform>(), originalPicScale);
            shownPicture = null;
        }
    }

    public void LockPicture()
    {
        shownPicture.enabled = false;
        GetComponent<Collider>().enabled = false;
        this.enabled = false;
    }

    private void SetSize(RectTransform rect, Vector2 size)
    {
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
    }

    private IEnumerator ResizePicture(PagePicture picture)
    {
        yield return new WaitForEndOfFrame();

        RectTransform picTransform = picture.GetComponent<RectTransform>();

        picTransform.localPosition = new Vector3(0,0, 2f);

        picTransform.rotation = Quaternion.identity;
        SetSize(picTransform, pictureSize);

        picTransform.rotation = rectTransform.rotation;

        OnPictureDroppedOn?.Invoke(picture);
    }
}
