using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;

public class Scrapbook : MonoBehaviour
{
    public static Scrapbook Instance { get; private set; }

    public delegate void TextTypingHandler();

    public static TextTypingHandler OnBeginType;
    public static TextTypingHandler OnEndType;

    public GraphicRaycaster Raycaster;
    public ScrapbookPage CurrentPage { get { return allPages[currentPageIndex]; } }

    [SerializeField] private int scrapbookPageCount = 6;

    [SerializeField] private RectTransform book;
    [SerializeField] private Button bookQuestButton;

    [SerializeField] private Vector2 menuPosition;
    [SerializeField] private Vector2 questDockPosition;
    [SerializeField] private Vector2 questExtendPosition;

    [SerializeField] private Image elementsPanel;
    [SerializeField] private GameObject extrasGroup;
    [SerializeField] private GameObject progressTrackerTab;
    [SerializeField] private GameObject questTrackerTab;
    [SerializeField] private GameObject creditsTab;
    [SerializeField] private GameObject stickerGroup;

    [SerializeField] private RectTransform pagesParent;

    [SerializeField] private GameObject previousPageButton;
    [SerializeField] private GameObject nextPageButton;
    [SerializeField] private GameObject TabButtons;

    [SerializeField] private ScrapbookPage scrapbookPagePrefab;
    [SerializeField] private PageText textEntryPrefab;

    private int currentPageIndex;


    private ScrapbookPage[] allPages;

    private void Awake()
    {
        if(Instance != null)
        {
            throw new System.Exception("Multiple Scrapbooks exist in the world, this shouldn't happen!");
        }
        Instance = this;

        previousPageButton.SetActive(false);
    }

    private void Start()
    {
        StaticQuestHandler.OnQuestOpened += OpenBookForQuest;
        StaticQuestHandler.OnQuestClosed += CloseBookForQuest;

        StaticQuestHandler.OnAltarActivated += CheckAllMainQuestProgress;

        //DialogueTrigger.OnDialogueTriggered += UnlockBook;

        SetupScrapbook();

        CloseTrackers();
        ClosePages(); 
        StaticQuestHandler.OnPictureClicked += DockDelegate;
    }

    private void OnDestroy()
    {
        OnBeginType = null;
        OnEndType = null;
    }

    private void SetupScrapbook()
    {
        allPages = new ScrapbookPage[scrapbookPageCount];

        for (int i = 0; i < scrapbookPageCount; i++)
        {
            ScrapbookPage newPage = Instantiate(scrapbookPagePrefab, pagesParent);
            newPage.SetPageNumber(i + 1);
            newPage.gameObject.SetActive(i == 0);
            allPages[i] = newPage;
        }
    }

    public void UnlockBook()
    {
        book.gameObject.SetActive(true);
        stickerGroup.SetActive(true);
    }

    public void ClosePages()
    {
        Cursor.lockState = CursorLockMode.Locked;
        extrasGroup.SetActive(false);
        elementsPanel.gameObject.SetActive(false);

        if ((Vector2)book.transform.localPosition != menuPosition)
            StaticQuestHandler.OnQuestClosed?.Invoke();
    }

    public void OpenPages()
    {
        Cursor.lockState = CursorLockMode.None;
        elementsPanel.gameObject.SetActive(true);
        extrasGroup.SetActive(true);
    }

    public void ToggleNotePages()
    {
        if (progressTrackerTab.activeSelf || questTrackerTab.activeSelf || creditsTab)
        {
            CloseTrackers();
        }
    }

    public void ToggleProgressTracker()
    {
        if (progressTrackerTab.activeSelf)
        {
            CloseTrackers();
            return;
        }
        OpenProgressTracker();
    }

    public void ToggleQuestTracker()
    {
        if (questTrackerTab.activeSelf)
        {
            CloseTrackers();
            return;
        }
        OpenQuestTracker();
    }

    public void ToggleCredits()
    {
        if (creditsTab.activeSelf)
        {
            CloseTrackers();
            return;
        }
        OpenCredits();
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

    public void CreateNewTextEntry()
    {
        PageText newText = Instantiate(textEntryPrefab, CurrentPage.transform.position, CurrentPage.transform.rotation);
        CurrentPage.AddComponentToPage(newText);
        newText.transform.localScale = Vector3.one;
        newText.TextField.onSelect.AddListener((string s) => OnBeginType?.Invoke());
        newText.TextField.onDeselect.AddListener((string s) => OnEndType?.Invoke());
    }

    public void CreateNewPageComponent(PageComponent createdComponent)
    {
        PageComponent newComponent = Instantiate(createdComponent, CurrentPage.transform.position, CurrentPage.transform.rotation);
        CurrentPage.AddComponentToPage(newComponent);
        newComponent.transform.localScale = Vector3.one;
    }

    private void CheckAllMainQuestProgress(MainQuest quest)
    {
        foreach(ScrapbookPage page in allPages)
        {
            page.CheckPicsForQuest();
        }
    }

    private void OpenBookForQuest()
    {
        CloseTrackers();
        TabButtons.SetActive(false);    
        
        elementsPanel.gameObject.SetActive(true);
        elementsPanel.color = new Color(1, 1, 1, 0);
        book.transform.localPosition = questDockPosition;
        bookQuestButton.gameObject.SetActive(true);
        bookQuestButton.onClick.AddListener(UndockBook);
        bookQuestButton.transform.rotation = Quaternion.Euler(Vector3.forward * -90);

        PagePicture.OnPictureClicked += DockDelegate;
        PagePicture.OnBeginPictureDrag += DockBook;
    }

    private void CloseBookForQuest()
    {
        TabButtons.SetActive(true);

        bookQuestButton.onClick.RemoveAllListeners();
        bookQuestButton.gameObject.SetActive(false);
        book.transform.localPosition = menuPosition;
        elementsPanel.gameObject.SetActive(false);
        elementsPanel.color = new Color(0, 0, 0, 0.8f);

        PagePicture.OnPictureClicked -= DockDelegate;
        PagePicture.OnBeginPictureDrag -= DockBook;
    }

    private void UndockBook()
    {
        book.transform.localPosition = questExtendPosition;
        bookQuestButton.onClick.RemoveListener(UndockBook);
        bookQuestButton.onClick.AddListener(DockBook);
        bookQuestButton.transform.rotation = Quaternion.Euler(Vector3.forward * 90);
    }

    private void DockDelegate(PagePicture pict)
    {
        DockBook();
    }

    private void DockBook()
    {
        book.transform.localPosition = questDockPosition;
        bookQuestButton.onClick.RemoveListener(DockBook);
        bookQuestButton.onClick.AddListener(UndockBook);
        bookQuestButton.transform.rotation = Quaternion.Euler(Vector3.forward * -90);
    }

    private void OpenProgressTracker()
    {
        progressTrackerTab.SetActive(true);
        creditsTab.SetActive(false);
        questTrackerTab.SetActive(false);
        previousPageButton.SetActive(false);
        nextPageButton.SetActive(false);
        CurrentPage.gameObject.SetActive(false);
    }

    private void OpenQuestTracker()
    {
        questTrackerTab.SetActive(true);
        creditsTab.SetActive(false);
        progressTrackerTab.SetActive(false);
        previousPageButton.SetActive(false);
        nextPageButton.SetActive(false);
        CurrentPage.gameObject.SetActive(false);
    }

    private void OpenCredits()
    {
        creditsTab.SetActive(true);
        questTrackerTab.SetActive(false);
        progressTrackerTab.SetActive(false);
        previousPageButton.SetActive(false);
        nextPageButton.SetActive(false);
        CurrentPage.gameObject.SetActive(false);
    }

    private void CloseTrackers()
    {
        progressTrackerTab.SetActive(false);

        questTrackerTab.SetActive(false);

        creditsTab.SetActive(false);

        if (currentPageIndex != 0)
        {
            previousPageButton.SetActive(true);
        }
        if (currentPageIndex + 1 != allPages.Length)
        {
            nextPageButton.SetActive(true);
        }

        CurrentPage.gameObject.SetActive(true);
    }

}
