using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Fires a ray from the screen-centre: hold the Record action to start recording
/// a LoopedObject, release to stop.  Press the Remove action to delete the
/// looped object closest to the cross-hair.
/// </summary>
public class LooperGun : MonoBehaviour
{
    /* ─── Inspector fields ─────────────────────────────────────────────── */

    [Header("Gun Settings")]
    [SerializeField] private float maxDistance = 100f;

    [Header("Input Actions (New Input System)")]
    [Tooltip("Button held to start recording; released to stop (e.g. RightMouse).")]
    [SerializeField] private InputActionReference recordAction;

    [Tooltip("Button pressed once to delete the nearest looped object (e.g. R key).")]
    [SerializeField] private InputActionReference removeAction;

    /* ─── Internals ────────────────────────────────────────────────────── */

    private Camera _cam;
    private LoopedObject _targeted;          // currently recording

    /* ─── Unity lifecycle ──────────────────────────────────────────────── */

    private void Awake() => _cam = Camera.main;

    private void OnEnable()
    {
        // Make sure the whole asset is enabled so callbacks fire in a build
        recordAction.asset.Enable();
        removeAction.asset.Enable();

        recordAction.action.started += OnRecordStarted;   // button down
        recordAction.action.canceled += OnRecordCanceled;  // button up
        removeAction.action.performed += OnRemovePerformed;
    }

    private void OnDisable()
    {
        recordAction.action.started -= OnRecordStarted;
        recordAction.action.canceled -= OnRecordCanceled;
        removeAction.action.performed -= OnRemovePerformed;

        recordAction.asset.Disable();
        removeAction.asset.Disable();
    }

    /* ─── Callbacks ────────────────────────────────────────────────────── */

    // Start recording when the button is pressed
    private void OnRecordStarted(InputAction.CallbackContext _)
    {
        Ray ray = _cam.ScreenPointToRay(
            new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));

        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance)) return;

        Rigidbody rb = hit.rigidbody;
        if (rb == null || rb.isKinematic) return;

        // Get or add the LoopedObject component
        LoopedObject looped =
            rb.GetComponent<LoopedObject>() ?? rb.gameObject.AddComponent<LoopedObject>();

        looped.StartRecording();
        _targeted = looped;
    }

    // Stop recording when the button is released
    private void OnRecordCanceled(InputAction.CallbackContext _)
    {
        if (_targeted == null) return;

        _targeted.StopRecording();
        _targeted = null;
    }

    // Delete the looped object closest to the cross-hair
    private void OnRemovePerformed(InputAction.CallbackContext _)
    {
        LoopedObject[] all = FindObjectsByType<LoopedObject>(
                                FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        if (all.Length == 0) return;

        Vector2 centre = new(Screen.width * 0.5f, Screen.height * 0.5f);
        LoopedObject best = null;
        float bestDist = float.MaxValue;

        foreach (LoopedObject looped in all)
        {
            Vector3 screenPos = _cam.WorldToScreenPoint(looped.transform.position);
            if (screenPos.z < 0f) continue;            // behind camera

            float dist = Vector2.Distance(centre, new Vector2(screenPos.x, screenPos.y));
            if (dist < bestDist)
            {
                bestDist = dist;
                best = looped;
            }
        }

        if (best) Destroy(best);
    }
}
