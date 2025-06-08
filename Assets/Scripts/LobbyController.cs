using UnityEngine;
using UnityEngine.UIElements;

public class LobbyController : MonoBehaviour
{
    public UIDocument lobbyUIDocument;

    private VisualElement root;
    private Button backButton;
    private Button readyButton;
    private Label rolesList;
    private VisualElement playerGrid;
    public GameObject mainMenuUI;

    void Awake()
    {
        root = lobbyUIDocument.rootVisualElement;

        backButton = root.Q<Button>("back-button");
        readyButton = root.Q<Button>("ready-button");
        rolesList = root.Q<Label>("roles-list");
        playerGrid = root.Q<VisualElement>("player-grid");

        backButton.clicked += OnBack;
        readyButton.clicked += OnReady;
    }

    private void OnBack()
    {
        Debug.Log("Нажата кнопка Назад");
        gameObject.SetActive(false);
        if (mainMenuUI != null)
            mainMenuUI.SetActive(true);
        else
            Debug.LogError("Main Menu UI не назначен!");
    }

    private void OnReady()
    {
        Debug.Log("Игрок готов");
        // Твоя логика готовности
    }

    public void AddPlayer(string name, int number, Texture2D avatar, bool isReady)
    {
        VisualElement slot = new VisualElement();
        slot.AddToClassList("player-slot");

        var image = new Image { image = avatar };
        image.AddToClassList("player-avatar");

        var label = new Label($"{number}. {name}") { name = "name-label" };
        label.AddToClassList("player-name");

        var status = new Label(isReady ? "Готов" : "Ожидает");
        status.AddToClassList(isReady ? "ready" : "waiting");

        slot.Add(image);
        slot.Add(label);
        slot.Add(status);

        playerGrid.Add(slot);
    }
}
