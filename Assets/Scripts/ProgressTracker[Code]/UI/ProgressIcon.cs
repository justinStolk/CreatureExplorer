using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressIcon : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image categoryImage;
    [SerializeField] private TextMeshProUGUI iconText;

    protected ProgressObject progressData;

    public virtual void Initialise(ProgressCategory progressInfo)
    {
        Initialise(progressInfo as ProgressObject);
    }

    public virtual void Initialise(ProgressObject progressInfo)
    {
        progressData = progressInfo;

        iconImage.sprite = progressData.Completed? progressData.FinishedIcon : progressData.UnfinishedIcon;

        if (categoryImage != null) 
            categoryImage.sprite = progressData.FinishedIcon;
        
        iconText.text = progressData.Name;

    }

    protected virtual void OnEnable()
    {
        SetProgress();
        if (progressData.DetailPage == null)
        {
            GetComponentInChildren<Button>().interactable = false;
        }
    }

    public virtual void SetProgress()
    {
        if (progressData.Completed)
        {
            iconImage.sprite = progressData.FinishedIcon;
            return;
        } else if (progressData.GetType() == typeof(ProgressCategory) && categoryImage != null)
        {
            ProgressCategory category = progressData as ProgressCategory;
            categoryImage.fillAmount = category.GetProgress();
        }
    }

    public void GoToPage()
    {
        if (progressData.DetailPage != null)
        {
            //Debug.Log($"Page found {progressData.DetailPage.name}");
            GetComponentInParent<ProgressUIHandler>().OpenPage(progressData.DetailPage);
        }
# if UNITY_EDITOR
        else
        {
            Debug.Log("No page set");
        }
#endif
    }
}
