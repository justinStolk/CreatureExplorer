public static class StaticQuestHandler
{
    public delegate void QuestStatusHandler();
    public delegate void QuestPictureHandler(PagePicture picture);
    public delegate void AltarUpdateHandler(MainQuest altarQuest);

    public static QuestStatusHandler OnQuestOpened;
    public static QuestStatusHandler OnQuestClosed;

    public static QuestStatusHandler OnShrineCompleted;

    public static QuestStatusHandler OnQuestCompleted;
    public static QuestStatusHandler OnQuestFailed;

    public static QuestPictureHandler OnPictureClicked;
    public static QuestPictureHandler OnPictureDisplayed;

    public static TitanStatue CurrentQuestStatue;

    // Altar
    public static AltarUpdateHandler OnAltarActivated;

    public static QuestPictureHandler OnPictureInScrapbook;
    public static AltarUpdateHandler OnAltarProgress;
}
