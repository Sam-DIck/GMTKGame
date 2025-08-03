using UnityEngine;

public abstract class EventReceiver : MonoBehaviour
{
    [SerializeField] protected EventSender eventSender;
    void OnEnable()
    {
        if (eventSender)
            eventSender.EventChange+= OnEventChange;
    }
    void OnDisable()
    {
        if (eventSender)
            eventSender.EventChange-= OnEventChange;
    }

    public abstract void OnEventChange(bool newState);
}
