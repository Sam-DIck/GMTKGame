using System;
using System.Linq; // For ToList()
using UnityEngine;
using UnityEngine.InputSystem;   // Esc key overlay toggle
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor; // SceneAsset drag-and-drop in Inspector
#endif

public class MainMenuController : MonoBehaviour
{
    // ---------------------------- Nested data
    [Serializable]
    private class LevelEntry
    {
        public string buttonName; // e.g. "level1-button"
#if UNITY_EDITOR
        public SceneAsset sceneAsset;     // Assigned in Editor
#endif
        [HideInInspector] public string sceneName; // Used at runtime
    }

    // ---------------------------- Inspector fields
    [Header("UI Toolkit")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Levels (drag Scene Assets)")]
    [SerializeField]
    private LevelEntry[] levels = new LevelEntry[]
    {
        new LevelEntry { buttonName = "level1-button" },
        new LevelEntry { buttonName = "level2-button" },
        new LevelEntry { buttonName = "level3-button" },
        new LevelEntry { buttonName = "level4-button" },
        new LevelEntry { buttonName = "level5-button" },
    };

    // ---------------------------- UXML element names
    private const string kButtonsPanelName = "buttons-panel";
    private const string kLevelsPanelName = "levels-panel";
    private const string kControlsPanelName = "controls-panel";
    private const string kPlayButtonName = "play-button";
    private const string kLevelsButtonName = "levels-button";
    private const string kControlsButtonName = "controls-button";
    private const string kQuitButtonName = "quit-button";
    private const string kBackButtonName = "back-button"; // Shared by overlays

    // ---------------------------- Cached references
    private VisualElement root;
    private VisualElement buttonsPanel;
    private VisualElement levelsPanel;
    private VisualElement controlsPanel;

    private InputAction cancelAction; // Esc key

    // -------------------------------------------------- Editor-time sync
    private void OnValidate()
    {
#if UNITY_EDITOR
        foreach (var entry in levels)
            if (entry.sceneAsset != null)
                entry.sceneName = entry.sceneAsset.name;
#endif
    }

    // -------------------------------------------------- Runtime init
    private void OnEnable()
    {
        if (uiDocument == null)
        {
            Debug.LogError("MainMenuController: UIDocument reference missing.");
            return;
        }

        // --- Panel cache ---
        root = uiDocument.rootVisualElement;
        buttonsPanel = root.Q<VisualElement>(kButtonsPanelName);
        levelsPanel = root.Q<VisualElement>(kLevelsPanelName);
        controlsPanel = root.Q<VisualElement>(kControlsPanelName);

        if (buttonsPanel == null || levelsPanel == null || controlsPanel == null)
        {
            Debug.LogError("MainMenuController: One or more panels not found. Check element names in UXML.");
            return;
        }

        levelsPanel.style.display = DisplayStyle.None;
        controlsPanel.style.display = DisplayStyle.None;

        // --- Main buttons wiring ---
        Button playBtn = root.Q<Button>(kPlayButtonName);
        Button levelsBtn = root.Q<Button>(kLevelsButtonName);
        Button controlsBtn = root.Q<Button>(kControlsButtonName);
        Button quitBtn = root.Q<Button>(kQuitButtonName);

        if (playBtn != null) playBtn.clicked += () => LoadLevel(0);
        if (levelsBtn != null) levelsBtn.clicked += ShowLevels;
        if (controlsBtn != null) controlsBtn.clicked += ShowControls;
        if (quitBtn != null) quitBtn.clicked += QuitGame;

        // --- Back buttons in overlays ---
        var backButtons = root.Query<Button>(name: kBackButtonName).ToList();
        foreach (var b in backButtons)
            if (b != null) b.clicked += ReturnToMainMenu;

        // --- Level-specific buttons ---
        for (int i = 0; i < levels.Length; ++i)
        {
            int idx = i; // local copy for closure
            Button lvlBtn = root.Q<Button>(levels[i].buttonName);
            if (lvlBtn != null)
                lvlBtn.clicked += () => LoadLevel(idx);
        }

        // --- Esc key overlay toggle ---
        cancelAction = new InputAction("Cancel", binding: "<Keyboard>/escape");
        cancelAction.Enable();
        cancelAction.performed += _ =>
        {
            if (levelsPanel.style.display == DisplayStyle.Flex || controlsPanel.style.display == DisplayStyle.Flex)
                ReturnToMainMenu();
        };
    }

    private void OnDisable()
    {
        if (cancelAction != null)
        {
            cancelAction.performed -= _ => ReturnToMainMenu();
            cancelAction.Disable();
        }
    }

    // -------------------------------------------------- Panel helpers
    private void ShowLevels()
    {
        levelsPanel.style.display = DisplayStyle.Flex;
        controlsPanel.style.display = DisplayStyle.None;
    }

    private void ShowControls()
    {
        controlsPanel.style.display = DisplayStyle.Flex;
        levelsPanel.style.display = DisplayStyle.None;
    }

    private void ReturnToMainMenu()
    {
        levelsPanel.style.display = DisplayStyle.None;
        controlsPanel.style.display = DisplayStyle.None;
    }

    // -------------------------------------------------- Scene loading
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

    // -------------------------------------------------- Quit
    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}