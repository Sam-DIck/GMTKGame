using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class LockedDoor : EventReceiver
{
    [Header("Door States")]
    [SerializeField] private Transform openTransform;
    [SerializeField] private Transform closedTransform;
    [SerializeField] private bool invert;
    
    [Header("Door Transition")]
    [SerializeField] private float duration = 1f;
    
    [Header("Safety Zone")]
    [SerializeField]  private TriggerBoxEventTrigger safetyZone;
    
    private float _openAmount;

    private bool ShouldOpen => (eventSender&&eventSender.EventActive) ^ invert;
    private bool ShouldClose => !ShouldOpen && _openAmount > 0f;
    private bool Opening => ShouldOpen || (safetyZone && safetyZone.EventActive && ShouldClose);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _openAmount = ShouldOpen ? 1f : 0f;
    }

    // Update is called once per frame
    void Update()
    {
        
        
        if (_openAmount<0f && !Opening) return;               // closed  AND NOT opening
        if (_openAmount>1f && Opening) return;                // open    AND     opening
        _openAmount += Opening ? Time.deltaTime/duration : -Time.deltaTime/duration;
        transform.position = Vector3.Lerp(closedTransform.position, openTransform.position, _openAmount);
        transform.localScale = Vector3.Lerp(closedTransform.localScale, openTransform.localScale, _openAmount);
    }

    public override void OnEventChange(bool newState)
    {
        
    }

    void OnDrawGizmosSelected()
    {
        if (openTransform)
        {
            Gizmos.color = Opening ? Color.green : Color.red;
            Gizmos.DrawWireCube(openTransform.position, openTransform.lossyScale);
        }

        if (closedTransform)
        {
            Gizmos.color = Opening ? Color.red : Color.green;
            Gizmos.DrawWireCube(closedTransform.position, closedTransform.lossyScale);
        }
    }
    void OnDrawGizmos()
    {
        if (openTransform && closedTransform)
        {
            Gizmos.color = Color.orange;
            Gizmos.DrawLine(openTransform.position, closedTransform.position);
        }

    }
}
