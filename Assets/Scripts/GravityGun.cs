using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class GravityGun : MonoBehaviour
{
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private float maxSpeed = 1f;
    [SerializeField] private float maxForce = 50f;
    [SerializeField] private  float forceScale = 1f;


    
    [Header("Input")]
    [SerializeField] private InputActionReference actionReference;

    private Camera _mainCamera;
    private Rigidbody _lockedRigidbody;
    private float _lockDistance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        actionReference.action.started += OnStarted;
        actionReference.action.canceled += OnEnd;
    }



    void OnStarted(InputAction.CallbackContext ctx)
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Ray ray = _mainCamera.ScreenPointToRay(screenCenter);
        if (Physics.Raycast(ray, out  RaycastHit hit))
        {
            //Debug.Log("Hit object: " + hit.collider.name);

            if (hit.distance < maxDistance)
            {
                Debug.Log(hit.collider.name);
                Rigidbody rb = hit.rigidbody;
                if (rb && !rb.isKinematic)
                {
                    _lockDistance = (_mainCamera.transform.position - hit.transform.position).magnitude;
                    _lockedRigidbody = rb;
                    _lockedRigidbody.useGravity = false;
                }
            }
        }
    }

    void OnEnd(InputAction.CallbackContext ctx)
    {
        if (!_lockedRigidbody) return;
        
        _lockedRigidbody.useGravity = true;
        _lockedRigidbody = null;
        
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        if (_lockedRigidbody)
        {
            Vector3 targetPosition = _mainCamera.transform.position + _lockDistance * _mainCamera.transform.forward;
            Vector3 direction = targetPosition - _lockedRigidbody.transform.position;
            float distance = direction.magnitude;
            direction.Normalize();

            // Slow down when close
            float slowingRadius = 2f; // tweak this
            float desiredSpeed = (distance < slowingRadius) ? maxSpeed * (distance / slowingRadius) : maxSpeed;

            Vector3 desiredVelocity = direction * desiredSpeed;
            Vector3 steering = desiredVelocity - _lockedRigidbody.linearVelocity;
            Vector3 force = Vector3.ClampMagnitude(steering * forceScale, maxForce);

            _lockedRigidbody.AddForce(force);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Ray ray = _mainCamera.ScreenPointToRay(screenCenter);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Gizmos.DrawSphere(hit.point, 0.5f);
        }
    }
}