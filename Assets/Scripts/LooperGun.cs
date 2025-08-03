using UnityEngine;

public class LooperGun : MonoBehaviour
{
    
    [SerializeField] private float maxDistance = 100f;
    
    private Camera _mainCamera;

    private LoopedObject _targetedObject; // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    void Update()
    {
        // On right mouse button down
        if (Input.GetMouseButtonDown(1))
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

        // On right mouse button up
        if (Input.GetMouseButtonUp(1))
        {
            if (_targetedObject)
            {
                _targetedObject.StopRecording();
                _targetedObject = null;
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
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
}
