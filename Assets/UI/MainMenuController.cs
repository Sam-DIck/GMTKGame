
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor; // for SceneAsset field in Inspector
#endif

public class MainMenuController : MonoBehaviour
{
    [Serializable]
    private class LevelEntry
    {
        public string buttonName;           // e.g. "level1-button"
#if UNITY_EDITOR
        public SceneAsset sceneAsset;       // drag & drop in Inspector
#endif
        [HideInInspector] public string sceneName; // stored for runtime builds
    }

    // ---------------- Inspector fields
    [Header("UI Toolkit")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Levels (drag Scene Assets)")]
    [SerializeField]
    private LevelEntry[] levels = new LevelEntry[]
    {
        new LevelEntry { buttonName = "level1-button" },
        new LevelEntry { buttonName = "level2-button" },
        new LevelEntry { buttonName = "level3-button" },
    };

    // ---------------- UXML element names (edit if your UXML differs)
    private const string kButtonsPanelName = "buttons-panel";
    private const string kLevelsPanelName = "levels-panel";
    private const string kControlsPanelName = "controls-panel";
    private const string kTitleName = "title-text";

    private const string kPlayButtonName = "play-button";
    private const string kLevelsButtonName = "levels-button";
    private const string kControlsButtonName = "controls-button";
    private const string kQuitButtonName = "quit-button";
    private const string kBackButtonName = "back-button"; // shared

    // ---------------- Cached UI references
    private VisualElement root;
    private VisualElement buttonsPanel;
    private VisualElement levelsPanel;
    private VisualElement controlsPanel;
    private VisualElement titleElement;

    // ---------------- Input action for Esc
    private InputAction cancelAction;

    // ---------------- Unity callbacks
    private void OnValidate()
    {
#if UNITY_EDITOR
        // Keep sceneName in sync so it’s available in builds
        foreach (var entry in levels)
        {
            if (entry.sceneAsset != null)
                entry.sceneName = entry.sceneAsset.name;
        }
#endif
    }

    private void Awake()
    {
        cancelAction = new InputAction("Cancel", binding: "<Keyboard>/escape");
    }

    private void OnEnable()
    {
        if (uiDocument == null)
        {
            Debug.LogError("MainMenuController: UIDocument not assigned.");
            return;
        }

        root = uiDocument.rootVisualElement;
        buttonsPanel = root.Q<VisualElement>(kButtonsPanelName);
        levelsPanel = root.Q<VisualElement>(kLevelsPanelName);
        controlsPanel = root.Q<VisualElement>(kControlsPanelName);
        titleElement = root.Q<VisualElement>(kTitleName);

        if (buttonsPanel == null || levelsPanel == null || controlsPanel == null || titleElement == null)
        {
            Debug.LogError("MainMenuController: One or more required elements not found. Check UXML names.");
            return;
        }

        // Hide overlays at start
        levelsPanel.style.display = DisplayStyle.None;
        controlsPanel.style.display = DisplayStyle.None;
        titleElement.style.display = DisplayStyle.Flex;

        // ---- Main menu buttons ----
        Button playBtn = root.Q<Button>(kPlayButtonName);
        Button levelsBtn = root.Q<Button>(kLevelsButtonName);
        Button controlsBtn = root.Q<Button>(kControlsButtonName);
        Button quitBtn = root.Q<Button>(kQuitButtonName);

        if (playBtn != null) playBtn.clicked += () => LoadLevel(0);
        if (levelsBtn != null) levelsBtn.clicked += ShowLevels;
        if (controlsBtn != null) controlsBtn.clicked += ShowControls;
        if (quitBtn != null) quitBtn.clicked += QuitGame;

        // ---- Back buttons (could be multiple) ----
        var backButtons = root.Query<Button>(name: kBackButtonName).ToList();
        foreach (var b in backButtons)
            b.clicked += ReturnToMainMenu;

        // ---- Level buttons ----
        for (int i = 0; i < levels.Length; ++i)
        {
            string btnName = levels[i].buttonName;
            Button b = root.Q<Button>(btnName);
            if (b != null)
            {
                int idx = i; // capture
                b.clicked += () => LoadLevel(idx);
            }
        }

        // ---- Esc key closes overlay ----
        cancelAction.Enable();
        cancelAction.performed += ctx =>
        {
            if (levelsPanel.style.display == DisplayStyle.Flex || controlsPanel.style.display == DisplayStyle.Flex)
                ReturnToMainMenu();
        };
    }

    private void OnDisable()
    {
        cancelAction.performed -= ctx => ReturnToMainMenu(); // clean-up
        cancelAction.Disable();
    }

    // ---------------- Panel helpers
    private void ShowLevels()
    {
        levelsPanel.style.display = DisplayStyle.Flex;
        controlsPanel.style.display = DisplayStyle.None;
        titleElement.style.display = DisplayStyle.None;
    }

    private void ShowControls()
    {
        controlsPanel.style.display = DisplayStyle.Flex;
        levelsPanel.style.display = DisplayStyle.None;
        titleElement.style.display = DisplayStyle.None;
    }

    private void ReturnToMainMenu()
    {
        levelsPanel.style.display = DisplayStyle.None;
        controlsPanel.style.display = DisplayStyle.None;
        titleElement.style.display = DisplayStyle.Flex;
    }

    // ---------------- Scene loading
    private void LoadLevel(int index)
    {
        if (index < 0 || index >= levels.Length)
        {
            Debug.LogError($"MainMenuController: Level index {index} out of range.");
            return;
        }

        string sceneName = levels[index].sceneName;
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"MainMenuController: Scene not assigned for level {index + 1}.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    // ---------------- Quit
    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
