using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// “Gravity-gun” that grabs a Rigidbody and keeps it in front of the camera.
/// Translation obeys separate accel / decel caps, and rotation is tamed with
/// angular damping and an angular-speed clamp (Unity 6 API).
/// </summary>
public class GravityGunRework : MonoBehaviour
{
    // ───────── Grab / hold parameters ──────────────────────────────────────────
    [Header("Grab Settings")]
    [SerializeField] private float maxGrabDistance = 100f;   // ray-cast range
    [SerializeField] private float holdDistance = 4f;     // distance in front of camera

    [Header("Linear Motion Limits (m/s²)")]
    [SerializeField] private float maxAcceleration = 25f;    // speeding up
    [SerializeField] private float maxDeceleration = 50f;    // braking

    // ───────── Rotation control ────────────────────────────────────────────────
    [Header("Rotation Control")]
    [Tooltip("Extra angular damping applied while an object is held " +
             "(0 = none, higher = stronger).")]
    [SerializeField] private float angularDampingDuringHold = 4f;

    [Tooltip("Maximum allowed angular speed (rad/s) while held.")]
    [SerializeField] private float maxAngularSpeed = 20f;

    // ───────── Input (New Input System) ────────────────────────────────────────
    [Header("Input")]
    [SerializeField] private InputActionReference grabAction;

    // ───────── Internals ───────────────────────────────────────────────────────
    private Camera _cam;
    private Rigidbody _held;
    private Vector3 _localHoldPoint;
    private float _origAngularDamping;           // restored on release

    // ───────────────────────────────────────────────────────────────────────────
    #region Unity lifecycle
    private void Awake() => _cam = Camera.main;

    private void OnEnable()
    {
        grabAction.action.performed += TryGrab;
        grabAction.action.canceled += Release;
    }

    private void OnDisable()
    {
        grabAction.action.performed -= TryGrab;
        grabAction.action.canceled -= Release;
        Release();                                   // safety clean-up
    }

    private void FixedUpdate()
    {
        if (!_held) return;

        // ── 1. Target position ───────────────────────────────────────────────
        Vector3 targetPos = _cam.transform.position +
                            _cam.transform.forward * holdDistance +
                            _cam.transform.TransformVector(_localHoldPoint);

        // ── 2. Linear velocity required to reach it next step ────────────────
        Vector3 desiredVel = (targetPos - _held.position) / Time.fixedDeltaTime;

        // ── 3. Acceleration clamp (separate accel / decel) ───────────────────
        Vector3 accelNeeded =
            (desiredVel - _held.linearVelocity) / Time.fixedDeltaTime;

        bool speedingUp = desiredVel.sqrMagnitude >
                          _held.linearVelocity.sqrMagnitude + 0.0001f;
        float accelCap = speedingUp ? maxAcceleration : maxDeceleration;

        if (accelNeeded.sqrMagnitude > accelCap * accelCap)
            accelNeeded = accelNeeded.normalized * accelCap;

        _held.AddForce(accelNeeded, ForceMode.Acceleration);

        // ── 4. Rotation control ──────────────────────────────────────────────
        // a) Angular-speed hard cap
        if (_held.angularVelocity.sqrMagnitude > maxAngularSpeed * maxAngularSpeed)
            _held.angularVelocity =
                _held.angularVelocity.normalized * maxAngularSpeed;
        // b) Extra angular damping is already applied via angularDampingDuringHold
    }
    #endregion
    // ───────────────────────────────────────────────────────────────────────────
    #region Grab / release helpers
    private void TryGrab(InputAction.CallbackContext ctx)
    {
        if (_held) return;                            // already holding something

        Ray ray = _cam.ScreenPointToRay(
            new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));

        if (!Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance)) return;

        Rigidbody rb = hit.rigidbody;
        if (!rb || rb.isKinematic) return;

        // ── Lock it ──────────────────────────────────────────────────────────
        _held = rb;
        _held.useGravity = false;

        // remember & override angular damping
        _origAngularDamping = rb.angularDamping;
        _held.angularDamping = angularDampingDuringHold;

        // mild linear damping for stability (optional)
        _held.linearDamping = 4f;

        // store grab offset in camera space
        _localHoldPoint = _cam.transform.InverseTransformVector(
                              hit.point - rb.position);
    }

    private void Release(InputAction.CallbackContext ctx = default)
    {
        if (!_held) return;

        _held.useGravity = true;
        _held.linearDamping = 0f;
        _held.angularDamping = _origAngularDamping;   // restore
        _held = null;
    }
    #endregion
}
