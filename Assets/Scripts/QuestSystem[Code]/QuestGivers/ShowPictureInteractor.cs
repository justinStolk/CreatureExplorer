using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShowPictureInteractor : PageComponentInteractor
{
    [SerializeField] private UnityEvent<PagePicture> OnPictureDroppedOn;
    [SerializeField] private Vector2 pictureSize;

    private PagePicture shownPicture;

    private Vector2 originalScale;

    public override bool OnComponentDroppedOn(PageComponent component)
    {
        if (component.GetType() == typeof(PagePicture))
        {
            originalScale = component.GetComponent<RectTransform>().sizeDelta;

            component.transform.SetParent(transform);

            OnPictureDroppedOn?.Invoke(component as PagePicture);

            StartCoroutine(ResizePicture(component as PagePicture));
            return true;
        }
        return false;
    }

    public override void RemoveFromInteractor(PageComponent component)
    {
        if (shownPicture == component)
        {
            SetSize(component.GetComponent<RectTransform>(), originalScale);
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

        picTransform.localPosition = new Vector3(0,0.2f, 0);
        picTransform.localEulerAngles = new Vector3(0, 0, 0);

        SetSize(picTransform, pictureSize);
    }
}
