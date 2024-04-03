using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PictureStorage : PageComponentInteractor
{
    [SerializeField] private Transform[] photoSpots;
    [SerializeField] private int startingCapacity = 1;

    [SerializeField] private TMP_Text camStorageText;
    [SerializeField] private TMP_Text maxStorageText;

    [SerializeField] private Image storageBackgroundImage;
    [SerializeField] private Sprite storageBackgroundDefault;
    [SerializeField] private Sprite storageBackgroundFull;

    private Image storageRaycastImage;
    //private ushort currentPictureIndex;
    private Inventory<PagePicture> pictureInventory;

    private void Awake()
    {
        pictureInventory = new Inventory<PagePicture>((ushort)startingCapacity);
        //pictureInventory = new Inventory<PagePicture>((ushort)photoSpots.Length);
        //maxStorageText.text = pictureInventory.GetCapacity().ToString();
        UpdateCameraStorageText();

        if (!VRChecker.IsVR)
        {
            storageRaycastImage = GetComponent<Image>();

            PagePicture.OnBeginPictureDrag += () => storageRaycastImage.raycastTarget = true;
            PagePicture.OnEndPictureDrag += () => storageRaycastImage.raycastTarget = false;
        }
    }

    public bool StorageIsFull() => pictureInventory.InventoryIsFull();

    public void AddStorageCapacity(ushort addedCapacity = 1)
    {
        if (pictureInventory.GetCapacity() < photoSpots.Length)
        {
            pictureInventory.AdjustCapacity(addedCapacity);
            UpdateCameraStorageText();
        }
# if UNITY_EDITOR
        else
        {
            Debug.Log("Camera storage is already at max capacity");
        }
#endif
    }

    public void CreatePictureFromCamera(PagePicture picture)
    {
        for (int i = 0; i < photoSpots.Length; i++)
        {
            Transform t = photoSpots[i];
            if (t.childCount == 0)
            {
                pictureInventory.AddItemToInventory(picture);

                picture.transform.SetPositionAndRotation(t.position, t.rotation);
                picture.transform.SetParent(t, true);
                picture.transform.localScale = Vector3.one;

                picture.SetInteractor(this);

                UpdateCameraStorageText();

                return;
            }
        }
# if UNITY_EDITOR
        Debug.LogWarning("An attempt to add a picture while there are no available photo spots has been made! This should be impossible!");
#endif
    }

    public override bool OnComponentDroppedOn(PageComponent component)
    {
        if (component.GetType() != typeof(PagePicture)) return false;

        PagePicture picture = component as PagePicture;
        if (!pictureInventory.GetContents().Contains(picture))
        {
            if (StorageIsFull()) return false;

            foreach(Transform t in photoSpots)
            {
                if(t.childCount == 0)
                {
                    pictureInventory.AddItemToInventory(picture);

                    component.transform.SetPositionAndRotation(t.position, t.rotation);
                    component.transform.SetParent(t, true);

                    UpdateCameraStorageText();
                    return true;

                }
            }
        }
        return false;
    }

    public void DeleteStorage()
    {
        pictureInventory.EmptyInventory();


        foreach (Transform t in photoSpots)
        {
            for (int i = t.childCount; i>0; i--)
            {
                DestroyImmediate(t.GetChild(i-1).gameObject);
            }
        }

        UpdateCameraStorageText();
    }

    public override void RemoveFromInteractor(PageComponent component)
    {
        if (component.GetType() != typeof(PagePicture)) return;

        pictureInventory.RemoveItemFromInventory(component as PagePicture);
        UpdateCameraStorageText();
    }

    private void UpdateCameraStorageText()
    {
        if (pictureInventory.InventoryIsFull())
        {
            camStorageText.color = Color.red;
            storageBackgroundImage.sprite = storageBackgroundFull;
        }
        else
        {
            camStorageText.color = Color.black;
            storageBackgroundImage.sprite = storageBackgroundDefault;
        }
        camStorageText.text = pictureInventory.GetItemCount().ToString();
        maxStorageText.text = pictureInventory.GetCapacity().ToString();
    }
}
