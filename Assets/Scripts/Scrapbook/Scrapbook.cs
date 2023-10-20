using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Scrapbook : MonoBehaviour
{
    public static Scrapbook Instance { get; private set; }
    public ScrapbookPage CurrentPage { get { return allPages[currentPageIndex]; } }
    public bool CollectionIsFull { get { return collectedPictures.InventoryIsFull(); } }

    [SerializeField] private int scrapbookPageCount = 6;
    [SerializeField] private ushort maximumUnplacedPictureCount = 10;

    [SerializeField] private GameObject elementsPanel;
    [SerializeField] private RectTransform pagesParent;
    [SerializeField] private LayoutGroup picturePanel;
    [SerializeField] private TMPro.TMP_Text camStorageText;

    [SerializeField] private GameObject previousPageButton;
    [SerializeField] private GameObject nextPageButton;

    [SerializeField] private ScrapbookPage scrapbookPagePrefab;
    [SerializeField] private PageText textEntryPrefab;
    [SerializeField] private PlayerInput input;

    private int currentPageIndex;

    private Inventory<PagePicture> collectedPictures;

    private ScrapbookPage[] allPages;

    private void Awake()
    {
        if(Instance != null)
        {
            throw new System.Exception("Multiple Scrapbooks exist in the world, this shouldn't happen!");
        }
        Instance = this;

        allPages = new ScrapbookPage[scrapbookPageCount];
        collectedPictures = new Inventory<PagePicture>(maximumUnplacedPictureCount);

        UpdateCameraStorageText();

        for (int i = 0; i < scrapbookPageCount; i++)
        {
            ScrapbookPage newPage = Instantiate(scrapbookPagePrefab, pagesParent);
            newPage.SetPageNumber(i + 1);
            newPage.gameObject.SetActive(i == 0);
            allPages[i] = newPage;
        }
        previousPageButton.SetActive(false);
    }

    public void OpenPages()
    {
        elementsPanel.SetActive(true);
    }

    public void GoToNextPage()
    {
        allPages[currentPageIndex].gameObject.SetActive(false);
        currentPageIndex++;
        allPages[currentPageIndex].gameObject.SetActive(true);
        if (!previousPageButton.activeSelf)
        {
            previousPageButton.SetActive(true);
        }
        if (currentPageIndex + 1 == allPages.Length)
        {
            nextPageButton.SetActive(false);
        }

    }

    public void GoToPreviousPage()
    {
        allPages[currentPageIndex].gameObject.SetActive(false);
        currentPageIndex--;
        allPages[currentPageIndex].gameObject.SetActive(true);
        if (!nextPageButton.activeSelf)
        {
            nextPageButton.SetActive(true);
        }
        if (currentPageIndex == 0)
        {
            previousPageButton.SetActive(false);
        }
    }

    public bool AddPictureToCollection(PagePicture snappedPicture)
    {
        if (collectedPictures.AddItemToInventory(snappedPicture))
        {
            snappedPicture.transform.SetParent(picturePanel.transform, false);
            UpdateCameraStorageText();
            return true;
        }
        return false;
        // To do: send out a message that the scrapbook's picture storage is full.
    }

    public bool RemovePictureFromCollection(PagePicture removedPicture)
    {
        if (collectedPictures.RemoveItemFromInventory(removedPicture))
        {
            UpdateCameraStorageText();
            return true;
        }
        return false;
    }
    
    public List<PagePicture> GetCollectedPictures()
    {
        return collectedPictures.GetContents();
    }

    public void CreateNewTextEntry() => Instantiate(textEntryPrefab, CurrentPage.transform);
    
    private void UpdateCameraStorageText()
    {
        ushort storageLeft = (ushort)(collectedPictures.GetCapacity() - collectedPictures.GetItemCount());
        if(storageLeft < 3)
        {
            camStorageText.color = Color.red;
        }
        else
        {
            camStorageText.color = Color.white;
        }
        camStorageText.text = "Storage left: " + storageLeft.ToString();

    }
}
