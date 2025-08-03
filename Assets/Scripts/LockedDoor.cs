using Unity.VisualScripting;
using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    [Header("Door States")]
    [SerializeField] private Transform openTransform;
    [SerializeField] private Transform closedTransform;
    [SerializeField] private bool invert;

    [Header("Door Transition")]
    [SerializeField] private float duration = 1f;

    [Header("Safety Zone (Optional)")]
    [SerializeField] private TriggerBoxEventTrigger safetyZone;

    [Header("Required Event Senders")]
    [SerializeField] private EventSender[] requiredSenders; // Support multiple senders

    private float _openAmount;

    private bool AllSendersActive
    {
        get
        {
            if (requiredSenders == null || requiredSenders.Length == 0) return false;
            foreach (var sender in requiredSenders)
            {
                if (sender == null) continue;
                if (invert ? sender.EventActive : !sender.EventActive)
                    return false;
            }
            return true;
        }
    }

    private bool ShouldClose => !AllSendersActive && _openAmount > 0f;
    private bool Opening => AllSendersActive || (safetyZone && safetyZone.EventActive && ShouldClose);

    void Start()
    {
        _openAmount = AllSendersActive ? 1f : 0f;
    }

    void Update()
    {
        if (_openAmount < 0f && !Opening) return;
        if (_openAmount > 1f && Opening) return;

        _openAmount += Opening ? Time.deltaTime / duration : -Time.deltaTime / duration;
        _openAmount = Mathf.Clamp01(_openAmount);

        transform.position = Vector3.Lerp(closedTransform.position, openTransform.position, _openAmount);
        transform.localScale = Vector3.Lerp(closedTransform.localScale, openTransform.localScale, _openAmount);
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
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(openTransform.position, closedTransform.position);
        }
    }
}
