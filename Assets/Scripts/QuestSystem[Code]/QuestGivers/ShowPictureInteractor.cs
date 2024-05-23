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
        if (shownPicture == null && component.GetType() == typeof(PagePicture))
        {
            shownPicture = component as PagePicture;

            //originalPicScale = new Vector2(component.GetComponent<RectTransform>().rect.width, component.GetComponent<RectTransform>().rect.height);

            component.transform.SetParent(transform);
            originalPicScale = component.GetComponent<RectTransform>().localScale;

            StartCoroutine(ResizePicture(shownPicture));
            return true;
        }
        return false;
    }

    public override void RemoveFromInteractor(PageComponent component)
    {
        if (shownPicture == component)
        {
            component.GetComponent<RectTransform>().localScale = Vector3.one * originalPicScale.x;
            shownPicture = null;
        }
    }

    public void LockPicture()
    {
        shownPicture.GetComponent<Collider>().enabled = false;
        shownPicture.enabled = false;
        this.enabled = false;
    }

    /*
    private void SetSize(RectTransform rect, Vector2 size)
    {
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
    }
    */

    private IEnumerator ResizePicture(PagePicture picture)
    {
        yield return new WaitForEndOfFrame();

        RectTransform picTransform = picture.GetComponent<RectTransform>();

        picTransform.localPosition = new Vector3(0,0, 21f);
        picTransform.localScale = Vector3.one * pictureSize;

        picTransform.localRotation = Quaternion.Euler(0,180,0);

        OnPictureDroppedOn?.Invoke(picture);
    }
}
