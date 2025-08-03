using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gravity gun that grabs a Rigidbody and keeps it in front of the camera.
/// Works both in the editor and in standalone builds (Unity 6 API).
/// </summary>
public class GravityGunRework : MonoBehaviour
{
    /* ─── Inspector fields ───────────────────────────────────────────────── */

    [Header("Grab Settings")]
    [SerializeField] private float maxGrabDistance = 100f;
    [SerializeField] private float holdDistance = 4f;

    [Header("Linear Motion Limits (m/s²)")]
    [SerializeField] private float maxAcceleration = 25f;
    [SerializeField] private float maxDeceleration = 50f;

    [Header("Rotation Control")]
    [SerializeField] private float angularDampingDuringHold = 4f;
    [SerializeField] private float maxAngularSpeed = 20f;

    [Header("Input (New Input System)")]
    [SerializeField] private InputActionReference grabAction;

    /* ─── Internals ─────────────────────────────────────────────────────── */

    private Camera _cam;
    private Rigidbody _held;
    private Vector3 _localHoldPoint;
    private float _origAngularDamping;

    /* ─── Lifecycle ─────────────────────────────────────────────────────── */

    private void Awake() => _cam = Camera.main;

    private void OnEnable()
    {
        /* ✦ IMPORTANT ✦ — enable the action explicitly for builds */
        grabAction.action.Enable();

        grabAction.action.performed += TryGrab;
        grabAction.action.canceled += Release;

        LockCursor(true);
    }

    private void OnDisable()
    {
        grabAction.action.performed -= TryGrab;
        grabAction.action.canceled -= Release;

        grabAction.action.Disable();
        Release();
        LockCursor(false);
    }

    /* ─── FixedUpdate: translate + tame spin ────────────────────────────── */

    private void FixedUpdate()
    {
        if (!_held) return;

        // target position in front of camera + original grab offset
        Vector3 targetPos = _cam.transform.position +
                            _cam.transform.forward * holdDistance +
                            _cam.transform.TransformVector(_localHoldPoint);

        Vector3 desiredVel = (targetPos - _held.position) / Time.fixedDeltaTime;
        Vector3 accel = (desiredVel - _held.linearVelocity) / Time.fixedDeltaTime;

        bool speedingUp = desiredVel.sqrMagnitude >
                          _held.linearVelocity.sqrMagnitude + 0.0001f;
        float accelCap = speedingUp ? maxAcceleration : maxDeceleration;

        if (accel.sqrMagnitude > accelCap * accelCap)
            accel = accel.normalized * accelCap;

        _held.AddForce(accel, ForceMode.Acceleration);

        // spin control
        if (_held.angularVelocity.sqrMagnitude > maxAngularSpeed * maxAngularSpeed)
            _held.angularVelocity =
                _held.angularVelocity.normalized * maxAngularSpeed;
    }

    /* ─── Grab / release helpers ────────────────────────────────────────── */

    private void TryGrab(InputAction.CallbackContext _)
    {
        if (_held) return;
        Debug.Log("TryGrab performed at " + Time.time);


        Ray ray = _cam.ScreenPointToRay(new Vector2(Screen.width * 0.5f,
                                                    Screen.height * 0.5f));

        if (!Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance)) return;

        Rigidbody rb = hit.rigidbody;
        if (!rb || rb.isKinematic) return;

        _held = rb;
        _held.useGravity = false;

        _origAngularDamping = rb.angularDamping;
        _held.angularDamping = angularDampingDuringHold;
        _held.linearDamping = 4f;

        _localHoldPoint = _cam.transform.InverseTransformVector(hit.point - rb.position);
    }

    private void Release(InputAction.CallbackContext _ = default)
    {
        if (!_held) return;

        _held.useGravity = true;
        _held.linearDamping = 0f;
        _held.angularDamping = _origAngularDamping;
        _held = null;
    }

    /* ─── Utility ───────────────────────────────────────────────────────── */

    private static void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
