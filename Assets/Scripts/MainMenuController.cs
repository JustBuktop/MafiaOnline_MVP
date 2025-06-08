using UnityEngine;
using UnityEngine.UIElements;
using Steamworks;

public class MainMenuController : MonoBehaviour
{
    public UIDocument uiDocument;
    public UIDocument settingsUIDocument;
    public WebcamRenderer webcamRenderer;
    [SerializeField] private GameObject lobbyUI;
    private VisualElement mainContainer;
    private VisualElement lobbyPanel;
    private VisualElement settingsPanel;
    private ScrollView lobbyList;
    private SettingsManager settingsManager;
    private CallResult<LobbyCreated_t> _lobbyCreatedCallResult;
    void Awake()
    {
        var root = uiDocument.rootVisualElement;
        var settingsRoot = settingsUIDocument.rootVisualElement;
        InitSteamInfo();
        mainContainer = root.Q<VisualElement>("main-container");
        lobbyPanel = root.Q<VisualElement>("lobby-panel");
        settingsPanel = settingsRoot.Q<VisualElement>("settings-container");
        _lobbyCreatedCallResult = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);
        settingsManager = new SettingsManager(settingsUIDocument, webcamRenderer, this);
        lobbyList = uiDocument.rootVisualElement.Q<ScrollView>("lobby-list");
        root.Q<Button>("create-lobby-button").clicked += CreateLobby;
        root.Q<Button>("create-button").clicked += OnCreateGame;
        root.Q<Button>("join-button").clicked += ShowLobbyPanel;
        root.Q<Button>("settings-button").clicked += ShowSettings;
        root.Q<Button>("exit-button").clicked += OnExit;
        root.Q<Button>("ru-button").clicked += () => OnLanguageSwitch("ru");
        root.Q<Button>("en-button").clicked += () => OnLanguageSwitch("en");
        root.Q<Button>("back-button").clicked += HideLobbyPanel;
        settingsRoot.Q<Button>("back-setting-button").clicked += HideSettings;
        if (SteamManager.Initialized)
        {
            Debug.Log("Steam успешно инициализирован.");
            Debug.Log("Твой Steam ID: " + Steamworks.SteamUser.GetSteamID());
            Debug.Log("Имя в Steam: " + Steamworks.SteamFriends.GetPersonaName());
        }
        else
        {
            Debug.LogError("Steam НЕ инициализирован!");
        }
    }

    void Start() {
        settingsManager.InitSettingsUI(); 
    }

    void OnCreateGame()
    {
        Debug.Log("Создание нового лобби...");
        mainContainer.style.display = DisplayStyle.None;
        lobbyPanel.style.display = DisplayStyle.None;
        lobbyUI.SetActive(true);
    }
    void OnExit() => Application.Quit();
    void OnLanguageSwitch(string lang) => Debug.Log($"Язык переключён: {lang}");
    void ShowLobbyPanel() {
        PopulateLobbyList();
        lobbyPanel.style.display = DisplayStyle.Flex; 
    }
    void HideLobbyPanel() => lobbyPanel.style.display = DisplayStyle.None;
    void CreateLobby()
    {
        Debug.Log("Создаём лобби через Steam...");
        var handle = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 12);
        _lobbyCreatedCallResult.Set(handle);
    }
    void ShowSettings()
    {
        settingsPanel.style.display = DisplayStyle.Flex;
        mainContainer.style.display = DisplayStyle.None;
    }

    void HideSettings()
    {
        settingsPanel.style.display = DisplayStyle.None;
        mainContainer.style.display = DisplayStyle.Flex;
        settingsManager.HideCameraPreview();
    }

    private void InitSteamInfo()
    {
        if (!SteamManager.Initialized) return;

        var root = uiDocument.rootVisualElement;
        var nameLabel = root.Q<Label>("steam-name");
        var avatarImage = root.Q<UnityEngine.UIElements.VisualElement>("steam-avatar");

        nameLabel.text = Steamworks.SteamFriends.GetPersonaName();

        int avatarInt = Steamworks.SteamFriends.GetLargeFriendAvatar(Steamworks.SteamUser.GetSteamID());
        if (avatarInt == -1) return;

        uint width, height;
        if (Steamworks.SteamUtils.GetImageSize(avatarInt, out width, out height))
        {
            byte[] image = new byte[4 * width * height];
            if (Steamworks.SteamUtils.GetImageRGBA(avatarInt, image, image.Length))
            {
                Texture2D tex = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                tex.LoadRawTextureData(image);
                tex.Apply();
                Texture2D flippedAvatar = FlipTextureVertically(tex);
                avatarImage.style.backgroundImage = new StyleBackground(flippedAvatar);
            }
        }
    }

    private Texture2D FlipTextureVertically(Texture2D original)
    {
        Texture2D flipped = new Texture2D(original.width, original.height);
        for (int y = 0; y < original.height; y++)
        {
            flipped.SetPixels(0, y, original.width, 1, original.GetPixels(0, original.height - y - 1, original.width, 1));
        }
        flipped.Apply();
        return flipped;
    }

    private void PopulateLobbyList()
    {
        lobbyList.Clear();

        for (int i = 0; i < 5; i++)
        {
            var item = new Label($"Лобби #{i + 1} — Игроков: {Random.Range(1, 12)}");
            item.style.paddingBottom = 4;
            item.style.unityFontStyleAndWeight = FontStyle.Bold;
            item.style.fontSize = 14;
            item.style.marginBottom = 6;
            item.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            item.style.color = Color.white;
            item.style.paddingLeft = 10;
            item.style.paddingTop = 6;
            item.style.paddingBottom = 6;
            item.style.borderBottomWidth = 1;
            lobbyList.Add(item);
        }
    }
    private void OnLobbyCreated(LobbyCreated_t result, bool ioFailure)
    {
        if (ioFailure)
        {
            Debug.LogError("Ошибка ввода/вывода при создании лобби.");
            return;
        }

        if (result.m_eResult == EResult.k_EResultOK)
        {
            Debug.Log($"✅ Лобби создано. ID: {result.m_ulSteamIDLobby}");
            // TODO: Переход в лобби, загрузка сцены, отображение участников и т.п.
        }
        else
        {
            Debug.LogError($"❌ Ошибка создания лобби: {result.m_eResult}");
        }
    }
}
