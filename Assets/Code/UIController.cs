using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum Page
{
    Profile,
    Messages,
    Post,
    Rating,
    World,
    Notifications,
    Tutorial
}

public class UIController : MonoBehaviour {
    [SerializeField]
    private GameObject _bottomNavBackground;

    [SerializeField]
    public GameObject _profileButton;
    private Image _profileIcon;
    [SerializeField]
    public GameObject _messagesButton;
    private Image _messagesIcon;

    public GameObject _postButton;
    private Image _postIcon;
    [SerializeField]
    private GameObject _ratingButton;
    private Image _ratingIcon;
    [SerializeField]
    private GameObject _worldButton;
    private Image _worldIcon;

    [SerializeField]
    private Sprite _profileButtonUnselected;
    [SerializeField]
    private Sprite _profileButtonSelected;
    [SerializeField]
    private Sprite _messagesButtonUnselected;
    [SerializeField]
    private Sprite _messagesButtonSelected;
    [SerializeField]
    private Sprite _postButtonUnselected;
    [SerializeField]
    private Sprite _postButtonSelected;
    [SerializeField]
    private Sprite _ratingButtonUnselected;
    [SerializeField]
    private Sprite _ratingButtonSelected;
    [SerializeField]
    private Sprite _worldButtonUnselected;
    [SerializeField]
    private Sprite _worldButtonSelected;

    [SerializeField]
    private Button _notificationButton;
    [SerializeField]
    private TextMeshProUGUI _notificationText;
    [SerializeField]
    private Image _notificationHeartIcon;
    [SerializeField]
    private GameObject _notificationAnimation;

    private UserSerializer _userSerializer;
    private CharacterSerializer _characterSerializer;

    private ProfileScreenController _profileController;
    private MessagesScreenController _messagesController;
    private NewPostController _newPostController;
    private RatingScreenController _ratingController;
    private WorldScreenController _worldController;
    private NotificationScreenController _notificationController;

    private IOController _ioController;
    private TutorialScreenController _tutorialController;
    private AlertsController _alertsController;
    private GoalsController _goalsController;

    private Page _currentPage = Page.World;
    private List<Page> _lastPages;

    private float _postTimeTimer = 0.0f;
    private float _backOutTimer = 0.0f;

    private GameObject _nextPostText;
    private GameObject _postTimeText;

    private GameObject _levelUpPopup = null;
    private GameObject _avatarTransitionPopup = null;

    public Button.ButtonClickedEvent MessageButtonClicked = new Button.ButtonClickedEvent();

    // Use this for initialization
    void Start () {
        this._bottomNavBackground.transform.Find("BottomNavImage").GetComponent<Image>().enabled = true;
        this._profileButton.GetComponent<Button>().onClick.AddListener(this.OnProfileClick);
        this._profileIcon = this._profileButton.transform.Find("ProfileButtonIcon").GetComponent<Image>();
        this._messagesButton.GetComponent<Button>().onClick.AddListener(this.OnMessagesClick);
        this._messagesIcon = this._messagesButton.transform.Find("MessageButtonIcon").GetComponent<Image>();
        this._postButton.GetComponent<Button>().onClick.AddListener(this.OnPostClick);
        this._postIcon = this._postButton.transform.Find("CameraButtonIcon").GetComponent<Image>();
        this._nextPostText = this._postButton.transform.Find("NextPostText").gameObject;
        this._postTimeText = this._postButton.transform.Find("PostTimeText").gameObject;
        this._postTimeText.GetComponent<TextMeshProUGUI>().text = "";
        this._ratingButton.GetComponent<Button>().onClick.AddListener(this.OnRatingClick);
        this._ratingIcon = this._ratingButton.transform.Find("RatingButtonIcon").GetComponent<Image>();
        this._worldButton.GetComponent<Button>().onClick.AddListener(this.OnWorldClick);
        this._worldIcon = this._worldButton.transform.Find("WorldButtonIcon").GetComponent<Image>();

        this._userSerializer = UserSerializer.Instance;
        this._characterSerializer = CharacterSerializer.Instance;

        this._profileController = GetComponent<ProfileScreenController>();
        this._messagesController = GetComponent<MessagesScreenController>();
        this._newPostController = GetComponent<NewPostController>();
        this._ratingController = GetComponent<RatingScreenController>();
        this._worldController = GetComponent<WorldScreenController>();
        this._notificationController = GetComponent<NotificationScreenController>();
        this._notificationController.NewNotificationsPulled.AddListener(this.UpdateNotificationCount);

        this._ioController = GetComponent<IOController>();
        this._tutorialController = GetComponent<TutorialScreenController>();
        this._alertsController = GetComponent<AlertsController>();
        this._goalsController = GetComponent<GoalsController>();

        this._notificationButton.onClick.AddListener(this.GoToNotificationsPage);

        this._lastPages = new List<Page>();

        if (this._userSerializer.NextPostTime > DateTime.Now)
        {
            this._postTimeTimer = 0.3f;

            this._postIcon.enabled = false;
#if !UNITY_EDITOR
            this._postButton.GetComponent<Button>().enabled = false;
#endif
            this._nextPostText.SetActive(true);
        }

        // StartCoroutine(this.StartGameEndOfFrame());
    }

    public IEnumerator StartGameEndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        this.EnterGame();
    }
	
	// Update is called once per frame
	void Update () {
        if (this._postTimeTimer > 0.0f)
        {
            this._postTimeTimer -= Time.deltaTime;
            if (this._postTimeTimer <= 0.0f)
            {
                this.UpdateTimeRemaining();
            }
        }
        if (this._backOutTimer > 0.0f)
        {
            this._backOutTimer -= Time.deltaTime;
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                if (this._backOutTimer <= 0.0f)
                {
                    this.BackOut();
                    this._backOutTimer = 0.5f;
                }
            }
        }

        // HACK - for allowing entering through the main scene for development use
        if (Input.GetKeyDown(KeyCode.Semicolon))
        {
            this.EnterGame();
        }
    }

    public void EnterGame()
    {
        if (!this._userSerializer.CreatedCharacter)
        {
            this._goalsController.SetFirstGoal();
            this.GoToTutorialPage();
        }
        else
        {
            this.GoToProfilePage();
        }
    }

    public Page GetCurrentPage()
    {
        return this._currentPage;
    }

    public void CreateLevelUpPopup()
    {
        this._levelUpPopup = GameObject.Instantiate(Resources.Load("UI/LevelUpPopup") as GameObject);
        this._levelUpPopup.transform.position = new Vector3(0.0f, -0.06f, -5.0f);

        var happinessLevel = this._characterSerializer.HappinessLevel;
        var happinessBlock = this._levelUpPopup.transform.Find("HappinessBlock");
        var happinessLevelText = happinessBlock.Find("LevelText");
        if (happinessLevel < 5)
        {
            happinessLevelText.GetComponent<TextMeshPro>().text = happinessLevel.ToString();
        }
        else
        {
            happinessLevelText.GetComponent<TextMeshPro>().text = 
                String.Format("{0} (Max)", happinessLevel);
            happinessBlock.GetComponent<Collider>().enabled = false;
        }

        var fitnessLevel = this._characterSerializer.FitnessLevel;
        var fitnessBlock = this._levelUpPopup.transform.Find("FitnessBlock");
        var fitnessLevelText = fitnessBlock.Find("LevelText");
        if (fitnessLevel < 5)
        {
            fitnessLevelText.GetComponent<TextMeshPro>().text = fitnessLevel.ToString();
        }
        else
        {
            fitnessLevelText.GetComponent<TextMeshPro>().text =
                String.Format("{0} (Max)", fitnessLevel);
            fitnessBlock.GetComponent<Collider>().enabled = false;
        }

        var hygieneLevel = this._characterSerializer.HygieneLevel;
        var hygieneBlock = this._levelUpPopup.transform.Find("HygieneBlock");
        var hygieneLevelText = hygieneBlock.Find("LevelText");
        if (hygieneLevel < 5)
        {
            hygieneLevelText.GetComponent<TextMeshPro>().text = hygieneLevel.ToString();
        }
        else
        {
            hygieneLevelText.GetComponent<TextMeshPro>().text =
                String.Format("{0} (Max)", hygieneLevel);
            hygieneBlock.GetComponent<Collider>().enabled = false;
        }

        if (this._avatarTransitionPopup)
        {
            GameObject.Destroy(this._avatarTransitionPopup);
        }
    }

    public void DestroyLevelPopup(
        CharacterProperties previousCharacterProperties,
        LevelUpAttribute levelUpAttribute)
    {
        if (this._levelUpPopup)
        {
            GameObject.Destroy(this._levelUpPopup);
            this._levelUpPopup = null;
            if (previousCharacterProperties != null)
            {
                this.CreateAvatarTransitionPopup(
                    previousCharacterProperties, levelUpAttribute);
            }
        }
    }

    public void CompleteAvatarTransition()
    {
        this._profileController.LevelUp();
        GameObject.Destroy(this._avatarTransitionPopup);
        this._avatarTransitionPopup = null;
    }

    public bool LevelPopupVisible()
    {
        return (this._levelUpPopup != null || this._avatarTransitionPopup != null);
    }

    /* Private methods */

    private void BackOut()
    {
        switch(this._currentPage)
        {
            case Page.World:
                if (!this._worldController.BackOut())
                {
                    return;
                }
                break;
            case Page.Profile:
                if (!this._profileController.BackOut())
                {
                    return;
                }
                break;
            case Page.Post:
                if (!this._newPostController.BackOut())
                {
                    return;
                }
                break;
            case Page.Rating:
                if (!this._ratingController.BackOut())
                {
                    return;
                }
                break;
            case Page.Messages:
                if (!this._messagesController.BackOut())
                {
                    return;
                }
                break;
            case Page.Notifications:
                if (!this._notificationController.BackOut())
                {
                    return;
                }
                break;
        }

        if (this._lastPages.Count > 0)
        {
            var lastPage = this._lastPages[this._lastPages.Count - 1];
            this._lastPages.RemoveAt(this._lastPages.Count - 1);

            switch (lastPage)
            {
                case Page.World:
                    this.GoToWorldPage();
                    break;
                case Page.Profile:
                    this.GoToProfilePage();
                    break;
                case Page.Post:
                    this.GoToPostPage();
                    break;
                case Page.Rating:
                    this.GoToRatingPage();
                    break;
                case Page.Messages:
                    this.GoToMessagesPage();
                    break;
            }
        }
    }

    private void UpdateNotificationCount(int newCount)
    {
        if (newCount > 0)
        {
            var oldCount = Int32.Parse(this._notificationText.text);
            if (oldCount != newCount)
            {
                this._notificationAnimation.GetComponent<Animator>().Play("Moving");
                StartCoroutine(this.UpdateNotificationColorAfterAnimation(newCount));
            }
        }
        else
        {
            this._notificationButton.GetComponent<Animator>().Play("Idle");
            this._notificationText.text = newCount.ToString();
        }
    }

    IEnumerator UpdateNotificationColorAfterAnimation(int newCount)
    {
        yield return new WaitForSeconds(1.4f); // animation["throw"].length);
        this._notificationButton.GetComponent<Animator>().Play("Wiggling");
        this._notificationText.text = newCount.ToString();
    }

    private void CreateAvatarTransitionPopup(
        CharacterProperties previousCharacterProperties,
        LevelUpAttribute levelUpAttribute)
    {
        this._avatarTransitionPopup = GameObject.Instantiate(Resources.Load("UI/AvatarTransitionPopup") as GameObject);
        this._avatarTransitionPopup.transform.position = new Vector3(0.0f, -0.06f, -5.0f);

        var avatarSection = this._avatarTransitionPopup.transform.Find("AvatarTransition");
        var spriteMask = avatarSection.transform.Find("SpriteMask");
        var oldMaleAvatar = spriteMask.transform.Find("OldMaleAvatar");
        var oldFemaleAvatar = spriteMask.transform.Find("OldFemaleAvatar");
        var newMaleAvatar = spriteMask.transform.Find("NewMaleAvatar");
        var newFemaleAvatar = spriteMask.transform.Find("NewFemaleAvatar");

        var newCharacterProperties = new CharacterProperties(previousCharacterProperties);
        var previousAttributeLevel = 0;
        switch (levelUpAttribute)
        {
            case LevelUpAttribute.Happiness:
                previousAttributeLevel = previousCharacterProperties.happinessLevel;
                newCharacterProperties.happinessLevel = previousAttributeLevel + 1;
                break;
            case LevelUpAttribute.Fitness:
                previousAttributeLevel = previousCharacterProperties.fitnessLevel;
                newCharacterProperties.fitnessLevel = previousAttributeLevel + 1;
                break;
            case LevelUpAttribute.Hygiene:
                previousAttributeLevel = previousCharacterProperties.hygieneLevel;
                newCharacterProperties.hygieneLevel = previousAttributeLevel + 1;
                break;
        }

        var gender = previousCharacterProperties.gender;
        oldMaleAvatar.gameObject.SetActive(gender == Gender.Male);
        oldFemaleAvatar.gameObject.SetActive(gender == Gender.Female);
        var newGender = this._characterSerializer.Gender;
        newMaleAvatar.gameObject.SetActive(newGender == Gender.Male);
        newFemaleAvatar.gameObject.SetActive(newGender == Gender.Female);
        switch (gender)
        {
            case Gender.Female:
                oldFemaleAvatar.GetComponent<AvatarController>().SetCharacterLook(previousCharacterProperties);
                newFemaleAvatar.GetComponent<AvatarController>().SetCharacterLook(newCharacterProperties);
                break;
            case Gender.Male:
                oldMaleAvatar.GetComponent<AvatarController>().SetCharacterLook(previousCharacterProperties);
                newMaleAvatar.GetComponent<AvatarController>().SetCharacterLook(newCharacterProperties);
                break;
        }

        var leftTopText = this._avatarTransitionPopup.transform.Find("TransitionTextLeftTop");
        leftTopText.GetComponent<TextMeshPro>().text = levelUpAttribute.ToString();
        var leftBottomText = this._avatarTransitionPopup.transform.Find("TransitionTextLeftBottom").GetComponent<TextMeshPro>();
        var leftBottomPrevious = leftBottomText.text;
        leftBottomText.text = String.Format(leftBottomPrevious, previousAttributeLevel);
        var rightTopText = this._avatarTransitionPopup.transform.Find("TransitionTextRightTop");
        rightTopText.GetComponent<TextMeshPro>().text = levelUpAttribute.ToString();
        var rightBottomText = this._avatarTransitionPopup.transform.Find("TransitionTextRightBottom").GetComponent<TextMeshPro>();
        var rightBottomPrevious = rightBottomText.text;
        rightBottomText.text = String.Format(rightBottomPrevious, previousAttributeLevel + 1);

        var upgradeText = this.GetUpgradeText(previousCharacterProperties, levelUpAttribute);
        var upgradesPanel = this._avatarTransitionPopup.transform.Find("UpgradesPanel");
        var background1 = upgradesPanel.Find("Background1");
        background1.gameObject.SetActive(upgradeText.Count == 1);
        var background2 = upgradesPanel.Find("Background2");
        background2.gameObject.SetActive(upgradeText.Count == 2);
        var background3 = upgradesPanel.Find("Background2");
        background3.gameObject.SetActive(upgradeText.Count == 3);
        var upgradeText1 = upgradesPanel.Find("UpgradeText1");
        upgradeText1.GetComponent<TextMeshPro>().text = upgradeText[0];
        var upgradeText2 = upgradesPanel.Find("UpgradeText2");
        upgradeText2.gameObject.SetActive(upgradeText.Count >= 2);
        if (upgradeText.Count >= 2)
        {
            upgradeText2.GetComponent<TextMeshPro>().text = upgradeText[1];
        }
        var upgradeText3 = upgradesPanel.Find("UpgradeText3");
        upgradeText3.gameObject.SetActive(upgradeText.Count >= 3);
        if (upgradeText.Count >= 3)
        {
            upgradeText3.GetComponent<TextMeshPro>().text = upgradeText[2];
        }
    }

    private List<string> GetUpgradeText(
        CharacterProperties previousCharacterProperties,
        LevelUpAttribute levelUpAttribute)
    {
        var upgradeText = new List<string>();

        switch (levelUpAttribute)
        {
            case LevelUpAttribute.Happiness:
                switch(previousCharacterProperties.happinessLevel)
                {
                    case 1:
                        upgradeText.Add("+ Happier face");
                        break;
                    case 2:
                        upgradeText.Add("+ Happier face");
                        break;
                    case 3:
                        upgradeText.Add("+ Better posture");
                        upgradeText.Add("+ Happier face");
                        break;
                    case 4:
                        upgradeText.Add("+ Happier face");
                        break;
                    default:
                        break;
                }
                break;
            case LevelUpAttribute.Fitness:
                switch (previousCharacterProperties.fitnessLevel)
                {
                    case 1:
                        upgradeText.Add("+ Lose weight");
                        break;
                    case 2:
                        upgradeText.Add("+ Lose weight");
                        break;
                    case 3:
                        upgradeText.Add("+ Lose weight");
                        break;
                    case 4:
                        upgradeText.Add("+ Get shredded");
                        break;
                    default:
                        break;
                }
                break;
            case LevelUpAttribute.Hygiene:
                switch (previousCharacterProperties.hygieneLevel)
                {
                    case 1:
                        upgradeText.Add("+ Chase away flies");
                        break;
                    case 2:
                        upgradeText.Add("+ Shower, get rid of smell");
                        break;
                    case 3:
                        upgradeText.Add("+ Wash face, no more acne");
                        break;
                    case 4:
                        upgradeText.Add("+ White teeth");
                        upgradeText.Add("+ Clean clothes");
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }

        return upgradeText;
    }

    private void OnWorldClick()
    {
        if (this._userSerializer.PostedPhoto && this._ioController.CanClick())
        {
            this.UpdateLastVisited();
            this.GoToWorldPage();
        }
    }
    public void GoToWorldPage()
    {
        GenerateworldPage();
        UpdateButtonState();
        this._alertsController.ClearNotifications(this._currentPage);
    }
    private void GenerateworldPage()
    {
        if (this._currentPage != Page.World)
        {
            DestroyPage(this._currentPage);
            this._worldController.EnterScreen();
            this._currentPage = Page.World;
        }
    }

    private void OnProfileClick()
    {
        if (this._userSerializer.PostedPhoto && this._ioController.CanClick())
        {
            this.UpdateLastVisited();
            this.GoToProfilePage();
        }
    }
    public void GoToProfilePage()
    {
        GenerateProfilePage();
        UpdateButtonState();
        this._alertsController.ClearNotifications(this._currentPage);
    }
    private void GenerateProfilePage()
    {
        if (this._currentPage != Page.Profile)
        {
            DestroyPage(this._currentPage);
            this._profileController.EnterScreen();
            this._currentPage = Page.Profile;
        }
    }

    private void OnPostClick()
    {
        if (this._userSerializer.CreatedCharacter && this._ioController.CanClick())
        {
            this.UpdateLastVisited();
            this.GoToPostPage();
        }
    }
    public void GoToPostPage()
    {
        GeneratePostPage();
        UpdateButtonState();
        this._alertsController.ClearNotifications(this._currentPage);
    }
    private void GeneratePostPage()
    {
        if (this._currentPage != Page.Post)
        {
            DestroyPage(this._currentPage);
            this._newPostController.ShowPopup(this.FinishedCreatingPicture);
            this._currentPage = Page.Post;
        }
    }

    private void OnRatingClick()
    {
        if (this._userSerializer.PostedPhoto && this._ioController.CanClick())
        {
            this.UpdateLastVisited();
            this.GoToRatingPage();
        }
    }
    public void GoToRatingPage()
    {
        GenerateRatingPage();
        UpdateButtonState();
        this._alertsController.ClearNotifications(this._currentPage);
    }
    private void GenerateRatingPage()
    {
        if (this._currentPage != Page.Rating)
        {
            DestroyPage(this._currentPage);
            this._ratingController.EnterScreen();
            this._currentPage = Page.Rating;
        }
    }

    private void OnMessagesClick()
    {
        this.MessageButtonClicked.Invoke();
        if (this._userSerializer.PostedPhoto && this._ioController.CanClick())
        {
            this.UpdateLastVisited();
            this.GoToMessagesPage();
        }
    }
    public void GoToMessagesPage()
    {
        GenerateMessagesPage();
        UpdateButtonState();
        this._alertsController.ClearNotifications(this._currentPage);
    }
    private void GenerateMessagesPage()
    {
        // Even if we are currently in messages, destroy and refresh inbox
        DestroyPage(this._currentPage);
        this._messagesController.EnterScreen();
        this._currentPage = Page.Messages;
    }

    public void GoToNotificationsPage()
    {
        if (this._userSerializer.PostedPhoto && this._ioController.CanClick())
        {
            GenerateNotificationsPage();
            UpdateButtonState();
            this._alertsController.ClearNotifications(this._currentPage);
        }
    }
    private void GenerateNotificationsPage()
    {
        DestroyPage(this._currentPage);
        this.UpdateNotificationCount(0);
        this._notificationController.EnterScreen();
        this._currentPage = Page.Notifications;
    }

    public void GoToTutorialPage()
    {
        DestroyPage(this._currentPage);
        this._tutorialController.EnterScreen();
        this._currentPage = Page.Tutorial;
    }

    private void UpdateLastVisited()
    {
        if (this._lastPages.Contains(this._currentPage))
        {
            this._lastPages.Remove(this._currentPage);
        }
        this._lastPages.Add(this._currentPage);
    }

    private void DestroyPage(Page page)
    {
        switch (page)
        {
            case Page.World:
                this._worldController.DestroyPage();
                break;
            case Page.Profile:
                this._profileController.DestroyPage();
                break;
            case Page.Post:
                this._newPostController.DestroyPage();
                break;
            case Page.Rating:
                this._ratingController.DestroyPage();
                break;
            case Page.Messages:
                this._messagesController.DestroyPage();
                break;
            case Page.Notifications:
                this._notificationController.DestroyPage();
                break;
        }
    }

    private void FinishedCreatingPicture(DelayGramPost post)
    {
        this._postTimeTimer = 0.1f;
        this._postIcon.enabled = false;
#if !UNITY_EDITOR
        this._postButton.GetComponent<Button>().enabled = false;
#endif
        this._nextPostText.SetActive(true);

        this._userSerializer.SerializePost(post);
        this._profileController.FinishedCreatingPicture(post);
        this.GoToProfilePage();
    }

    private void UpdateTimeRemaining()
    {
        if (this._userSerializer.NextPostTime > DateTime.Now)
        {
            var timeTillCanPost = this._userSerializer.NextPostTime - DateTime.Now;
            var formattedTime = timeTillCanPost.ToString(@"mm\:ss");
            this._postTimeText.GetComponent<TextMeshProUGUI>().text = formattedTime;

            this._postTimeTimer = 1.0f;
        } else {
            this._postIcon.enabled = true;
            this._postButton.GetComponent<Button>().enabled = true;
            this._postTimeText.GetComponent<TextMeshProUGUI>().text = "";
            this._nextPostText.SetActive(false);
        }
    }

    private void UpdateButtonState()
    {
        this._worldIcon.sprite = (this._currentPage == Page.World) ? this._worldButtonSelected : this._worldButtonUnselected;
        this._profileIcon.sprite = (this._currentPage == Page.Profile) ? this._profileButtonSelected : this._profileButtonUnselected;
        this._postIcon.sprite = (this._currentPage == Page.Post) ? this._postButtonSelected : this._postButtonUnselected;
        this._ratingIcon.sprite = (this._currentPage == Page.Rating) ? this._ratingButtonSelected : this._ratingButtonUnselected;
        this._messagesIcon.sprite = (this._currentPage == Page.Messages) ? this._messagesButtonSelected : this._messagesButtonUnselected;
    }
}
