using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[DefaultExecutionOrder(-10)]
public class PauseMenuController : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference pauseAction;

    [Header("UI Toolkit")]
    public UIDocument pauseMenuDocument;

    private VisualElement root;
    private VisualElement panelMain;
    private VisualElement panelControls;

    private Button resumeButton;
    private Button restartButton;
    private Button controlsButton;
    private Button quitButton;
    private Button backButton;

    private bool isPaused;
    private bool inControlsView;

    // -------------------------------------------------- Unity events
    private void OnEnable()
    {
        // Input setup
        if (pauseAction != null)
        {
            pauseAction.action.Enable();
            pauseAction.action.performed += OnPausePerformed;
        }

        // UI cache
        if (pauseMenuDocument == null)
        {
            Debug.LogError("PauseMenuController: UIDocument reference missing.");
            return;
        }

        root = pauseMenuDocument.rootVisualElement;
        root.style.display = DisplayStyle.None; // invisible until paused

        panelMain = root.Q<VisualElement>("panel-main");
        panelControls = root.Q<VisualElement>("panel-controls");

        if (panelMain == null || panelControls == null)
        {
            Debug.LogError("PauseMenuController: Required panels not found. Check names in UXML.");
            return;
        }

        panelControls.style.display = DisplayStyle.None;

        // Buttons in main panel
        resumeButton = panelMain.Q<Button>("resume-button");
        restartButton = panelMain.Q<Button>("restart-button");
        controlsButton = panelMain.Q<Button>("controls-button");
        quitButton = panelMain.Q<Button>("quit-button");

        // Button in controls panel
        backButton = panelControls.Q<Button>("back-button");

        // Wire callbacks safely
        if (resumeButton != null) resumeButton.clicked += Resume;
        if (restartButton != null) restartButton.clicked += Restart;
        if (controlsButton != null) controlsButton.clicked += ShowControls;
        if (quitButton != null) quitButton.clicked += Quit;

        if (backButton != null) backButton.clicked += BackToMain;
        {
            Debug.LogError("BackButton clicked");
        }
       

    }

    private void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.performed -= OnPausePerformed;
            pauseAction.action.Disable();
        }
    }

    // -------------------------------------------------- Input handler
    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        if (!isPaused)
        {
            Pause();
            return;
        }

        // Already paused
        if (inControlsView)
            BackToMain();
        else
            Resume();
    }

    // -------------------------------------------------- State helpers
    private void Pause()
    {
        isPaused = true;
        inControlsView = false;

        Time.timeScale = 0f;

        root.style.display = DisplayStyle.Flex;
        panelMain.style.display = DisplayStyle.Flex;
        panelControls.style.display = DisplayStyle.None;
        panelMain.BringToFront();

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }

    private void Resume()
    {
        isPaused = false;
        inControlsView = false;

        Time.timeScale = 1f;

        root.style.display = DisplayStyle.None;

        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    private void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // -------------------------------------------------- Panel switching
    private void ShowControls()
    {
        inControlsView = true;
        panelMain.style.display = DisplayStyle.None;
        panelControls.style.display = DisplayStyle.Flex;
        panelControls.BringToFront();
    }

    private void BackToMain()
    {
        Debug.LogError("BacktoMain Ran.");
        inControlsView = false;
        panelControls.style.display = DisplayStyle.None;
        panelMain.style.display = DisplayStyle.Flex;
        panelControls.SendToBack();
        panelMain.BringToFront();
    }
}
