using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class TriggerBoxEventTrigger : EventSender
{
    [SerializeField] private float decayDuration;

    private float _decayTime;
    
    void OnTriggerStay(Collider other)
    {
        _decayTime = decayDuration;
    }

    void Update()
    {
        _decayTime -= Time.deltaTime;
        EventActive = _decayTime >= 0f;
    }
}
