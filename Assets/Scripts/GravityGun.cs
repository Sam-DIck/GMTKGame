using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple “gravity-gun” pick-up: hold the Grab action, ray-cast from screen
/// centre, lock the first rigidbody hit, and keep it glued to a point in front
/// of the camera.  Release the action to drop it.
/// </summary>
public class GravityGun : MonoBehaviour
{
    [Header("Grab Settings")]
    [SerializeField] private float maxGrabDistance = 100f;   // how far you can grab
    [SerializeField] private float holdDistance = 4f;      // where the object sits
    [SerializeField] private float maxVelChange = 15f;     // snappiness of motion

    [Header("Input (New Input System)")]
    [SerializeField] private InputActionReference grabAction; // performed / canceled

    private Camera _cam;
    private Rigidbody _held;
    private Vector3 _localHoldPoint; // keep original grab offset

    // ─────────────────────────────────────────────────────────────────────────────
    #region Unity lifecycle
    void Awake()
    {
        _cam = Camera.main;
    }

    void OnEnable()
    {
        grabAction.action.performed += TryGrab;
        grabAction.action.canceled += Release;
    }

    void OnDisable()
    {
        grabAction.action.performed -= TryGrab;
        grabAction.action.canceled -= Release;
        Release(); // just in case
    }

    // All physics work in FixedUpdate
    void FixedUpdate()
    {
        if (!_held) return;

        // 1. Desired position = holdDistance units straight ahead
        Vector3 targetPos = _cam.transform.position + _cam.transform.forward * holdDistance;

        // 2. Preserve the grab offset so we don't pull from the collider centre
        targetPos += _cam.transform.TransformVector(_localHoldPoint);

        // 3. Velocity we need to arrive this frame
        Vector3 desiredVel = (targetPos - _held.position) / Time.fixedDeltaTime;
        Vector3 velChange = desiredVel - _held.linearVelocity;

        // 4. Clamp and apply as an instantaneous velocity change
        velChange = Vector3.ClampMagnitude(velChange, maxVelChange);
        _held.AddForce(velChange, ForceMode.VelocityChange);
    }
    #endregion
    // ─────────────────────────────────────────────────────────────────────────────
    #region Grabbing / Releasing
    private void TryGrab(InputAction.CallbackContext ctx)
    {
        // Already holding something? Ignore.
        if (_held) return;

        Ray ray = _cam.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));

        if (Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance))
        {
            Rigidbody rb = hit.rigidbody;
            if (rb && !rb.isKinematic)
            {
                _held = rb;
                _held.useGravity = false;
                _held.linearDamping = 4f;   // optional: dampen spin

                // Save where on the rigidbody we grabbed it (in local space)
                _localHoldPoint = _cam.transform.InverseTransformVector(hit.point - rb.position);
            }
        }
    }

    private void Release(InputAction.CallbackContext ctx = default)
    {
        if (!_held) return;

        _held.useGravity = true;
        _held.linearDamping = 0f;
        _held = null;
    }
    #endregion
}
