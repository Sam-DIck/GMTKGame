 using UnityEngine;

public class GravityGun : MonoBehaviour
{
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private float maxSpeed = 1f;
    [SerializeField] private float maxForce = 50f;
    [SerializeField] private  float forceScale = 1f;
    
    private Camera _mainCamera;
    private Rigidbody _lockedRigidbody;
    private float _lockDistance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            Ray ray = _mainCamera.ScreenPointToRay(screenCenter);
            if (Physics.Raycast(ray, out  RaycastHit hit))
            {
                //Debug.Log("Hit object: " + hit.collider.name);

                if (hit.distance < maxDistance)
                {
                    Debug.Log(hit.collider.name);
                    Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                    if (rb && !rb.isKinematic)
                    {
                        _lockDistance = (_mainCamera.transform.position - hit.transform.position).magnitude;
                        _lockedRigidbody = rb;
                        _lockedRigidbody.useGravity = false;
                    }
                }
            }
        }

        if (_lockedRigidbody)
        {
            Vector3 targetPosition = _mainCamera.transform.position + _lockDistance * _mainCamera.transform.forward;
            Vector3 direction = targetPosition - _lockedRigidbody.transform.position;
            Vector3 targetVelocity = Vector3.ClampMagnitude(direction,maxSpeed);
            
            Vector3 delta = targetVelocity - _lockedRigidbody.linearVelocity;
            Vector3 force = Vector3.ClampMagnitude(delta * forceScale, maxForce);
            _lockedRigidbody.AddForce(force);
            
        }

        if (!Input.GetMouseButton(0) && _lockedRigidbody)
        {
            _lockedRigidbody.useGravity = true;
            _lockedRigidbody = null;
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
