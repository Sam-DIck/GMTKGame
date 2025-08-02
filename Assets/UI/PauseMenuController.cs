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
    private Button backButton;

    private bool isPaused;
    private bool inControlsView;

    private void OnEnable()
    {
        // Enable pause input
        if (pauseAction != null)
        {
            pauseAction.action.Enable();
            pauseAction.action.performed += OnPausePerformed;
        }

        // Cache UI elements
        if (pauseMenuDocument != null)
        {
            root = pauseMenuDocument.rootVisualElement;
            root.style.display = DisplayStyle.None; // hidden until paused

            panelMain = root.Q<VisualElement>("panel-main");
            panelControls = root.Q<VisualElement>("panel-controls");
            if (panelControls != null)
            {
                panelControls.style.display = DisplayStyle.None;
                panelControls.BringToFront(); // ensure overlay order

                backButton = panelControls.Q<Button>("back-button");
                if (backButton != null)
                    backButton.RegisterCallback<ClickEvent>(_ => BackToMain());
                else
                    Debug.LogWarning("PauseMenuController: No 'back-button' found inside 'panel-controls'.");
            }
            else
            {
                Debug.LogError("PauseMenuController: No VisualElement named 'panel-controls' found. Controls overlay will not work.");
            }

            // Main panel button callbacks
            if (panelMain != null)
            {
                panelMain.Q<Button>("resume-button")?.RegisterCallback<ClickEvent>(_ => Resume());
                panelMain.Q<Button>("restart-button")?.RegisterCallback<ClickEvent>(_ => Restart());
                panelMain.Q<Button>("controls-button")?.RegisterCallback<ClickEvent>(_ => ShowControls());
                panelMain.Q<Button>("quit-button")?.RegisterCallback<ClickEvent>(_ => Quit());
            }
            else
            {
                Debug.LogError("PauseMenuController: No VisualElement named 'panel-main' found. Main pause menu will not show.");
            }
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

    // ------------------------- Input
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

    // ------------------------- State management
    private void Pause()
    {
        isPaused = true;
        inControlsView = false;

        Time.timeScale = 0f;
        root.style.display = DisplayStyle.Flex;
        panelMain.style.display = DisplayStyle.Flex;
        panelControls.style.display = DisplayStyle.None;

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }

    public void Resume()
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

    // ------------------------- Panel switching
    private void ShowControls()
    {
        inControlsView = true;
        panelMain.style.display = DisplayStyle.None;
        panelControls.style.display = DisplayStyle.Flex;
    }

    private void BackToMain()
    {
        inControlsView = false;
        panelControls.style.display = DisplayStyle.None;
        panelMain.style.display = DisplayStyle.Flex;
    }
}
