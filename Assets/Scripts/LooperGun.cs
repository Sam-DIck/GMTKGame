using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


public class LooperGun : MonoBehaviour
{
    
    [SerializeField] private float maxDistance = 100f;
    [Header("Input")]
    [SerializeField] private InputActionReference recordActionReference;
    [SerializeField] private InputActionReference removeActionReference;
    private Camera _mainCamera;

    private LoopedObject _targetedObject; // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        recordActionReference.action.started += StartRecord;
        recordActionReference.action.canceled += StopRecord;

        removeActionReference.action.started += RemoveLoop;
        
    }

    void StartRecord(InputAction.CallbackContext ctx)
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Ray ray = _mainCamera.ScreenPointToRay(screenCenter);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            Rigidbody rb = hit.collider.attachedRigidbody;
            if (rb && !rb.isKinematic)
            {
                Debug.Log(hit.collider.name);
                LoopedObject looped = rb.GetComponent<LoopedObject>();
                if (!looped)
                {
                    looped = rb.gameObject.AddComponent<LoopedObject>();
                }

                looped.StartRecording();
                _targetedObject = looped;
            }
        }
    }

    void StopRecord(InputAction.CallbackContext ctx)
    {
        if (_targetedObject)
        {
            _targetedObject.StopRecording();
            _targetedObject = null;
        }
    }
    
    void RemoveLoop(InputAction.CallbackContext ctx)
    {
        Debug.Log("Remove Loop");
        LoopedObject[] allLooped = FindObjectsByType<LoopedObject>(FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);
        if (allLooped.Length == 0) return;

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Camera cam = _mainCamera;
        LoopedObject closest = null;
        float closestDist = float.MaxValue;

        foreach (var looped in allLooped)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(looped.transform.position);

            // Ignore if behind the camera
            if (screenPos.z < 0) continue;

            float dist = Vector2.Distance(screenCenter, new Vector2(screenPos.x, screenPos.y));
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = looped;
            }
        }

        if (closest)
        {
            Destroy(closest);
        }
    }
}
