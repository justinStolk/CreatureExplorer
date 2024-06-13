using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestPictureInterface : PageComponentInteractor
{
    [SerializeField] private float questCompletionWaitTime = 3f;

    [SerializeField] private TMP_Text descriptionText;

    [SerializeField] private Image handInBackground, descriptionBackground;
    [SerializeField] private Color defaultColour, incorrectColour, correctColour;

    [SerializeField] private Image handInFrame;
    [SerializeField] private Sprite defaultFrame, incorrectFrame, correctFrame;

    [SerializeField] private GameObject pictureSlot;

    private PagePicture slottedPicture;
    private bool questInterfaceOpened;

    private void Start()
    {
        StaticQuestHandler.OnQuestOpened += () => 
        {
            questInterfaceOpened = true;
            descriptionBackground.color = Color.white;
            handInBackground.color = defaultColour;
            pictureSlot.SetActive(true);

            descriptionText.gameObject.SetActive(true);
            descriptionText.text = StaticQuestHandler.CurrentQuestStatue.TitanQuest.QuestDescription;
        };

        StaticQuestHandler.OnShrineCompleted += () => StartCoroutine(CompleteQuest());

        StaticQuestHandler.OnQuestFailed += () => 
        { 
            descriptionText.text = "Hmm, I don't believe this is what I'm looking for...";
            handInFrame.sprite = incorrectFrame;
            handInBackground.color = incorrectColour;
        };

        StaticQuestHandler.OnPictureClicked += (PagePicture picture) => 
        {
            picture.SetStepBackParent();
            if (OnComponentDroppedOn(picture))
            {
                picture.SetInteractor(this);
            }
        };

        StaticQuestHandler.OnQuestClosed += () =>
        {            
            questInterfaceOpened = false;
            descriptionBackground.color = new Color(1, 1, 1, 0);
            handInBackground.color = new Color(1, 1, 1, 0);
            handInFrame.sprite = defaultFrame;
            descriptionText.gameObject.SetActive(false); 
            pictureSlot.SetActive(false); 
            if(slottedPicture != null)
            {
                slottedPicture.OnRevert();
            }
        };

        pictureSlot.SetActive(false);
        descriptionText.gameObject.SetActive(false);
        descriptionBackground.color = new Color(1, 1, 1, 0);
    }

    private void OnDestroy()
    {
        //StaticQuestHandler.OnQuestOpened -= () =>
        //{
        //    questInterfaceOpened = true;
        //    descriptionBackground.color = Color.white;
        //    handInBackground.color = defaultColour;
        //    pictureSlot.SetActive(true);
        //
        //    descriptionText.gameObject.SetActive(true);
        //    descriptionText.text = StaticQuestHandler.CurrentQuestStatue.TitanQuest.QuestDescription;
        //};
        //
        //StaticQuestHandler.OnShrineCompleted -= () => StartCoroutine(CompleteQuest());
        //
        //StaticQuestHandler.OnQuestFailed -= () =>
        //{
        //    descriptionText.text = "Hmm, I don't believe this is what I'm looking for...";
        //    handInFrame.sprite = incorrectFrame;
        //    handInBackground.color = incorrectColour;
        //};
        //
        //StaticQuestHandler.OnPictureClicked -= (PagePicture picture) =>
        //{
        //    picture.SetStepBackParent();
        //    if (OnComponentDroppedOn(picture))
        //    {
        //        picture.SetInteractor(this);
        //    }
        //};
        //
        //StaticQuestHandler.OnQuestClosed -= () =>
        //{
        //    questInterfaceOpened = false;
        //    descriptionBackground.color = new Color(1, 1, 1, 0);
        //    handInBackground.color = new Color(1, 1, 1, 0);
        //    handInFrame.sprite = defaultFrame;
        //    descriptionText.gameObject.SetActive(false);
        //    pictureSlot.SetActive(false);
        //    if (slottedPicture != null)
        //    {
        //        slottedPicture.OnRevert();
        //    }
        //};
    }

    public override bool OnComponentDroppedOn(PageComponent component)
    {
        if (!questInterfaceOpened || component.GetType() != typeof(PagePicture) || slottedPicture != null) 
            return false;

        PagePicture picture = component as PagePicture;
        SlotPicture(picture);
        return true;
    }

    public override void RemoveFromInteractor(PageComponent component)
    {
        component.transform.localScale = Vector3.one;
        slottedPicture = null;

        handInBackground.color = defaultColour;
        handInFrame.sprite = defaultFrame;
        descriptionText.text = StaticQuestHandler.CurrentQuestStatue.TitanQuest.QuestDescription;
    }

    private void SlotPicture(PagePicture picture)
    {
        picture.transform.SetPositionAndRotation(pictureSlot.transform.position, Quaternion.identity);
        picture.transform.localScale = Vector3.one * 2;

        picture.transform.SetParent(pictureSlot.transform, true);

        slottedPicture = picture;

        StaticQuestHandler.OnPictureDisplayed?.Invoke(slottedPicture);
    }

    private IEnumerator CompleteQuest()
    {
        handInBackground.color = correctColour;
        handInFrame.sprite = correctFrame;
        descriptionText.text = "Ah! This is what I was looking for!";

        yield return new WaitForSeconds(questCompletionWaitTime);

        handInBackground.color = new Color(1, 1, 1, 0);

        Destroy(slottedPicture.gameObject);
        slottedPicture = null;

        handInFrame.sprite = defaultFrame;

        pictureSlot.SetActive(false);

        StaticQuestHandler.OnQuestClosed?.Invoke();
    }

}
