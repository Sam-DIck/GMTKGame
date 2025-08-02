using UnityEngine;

public abstract class EventSender : MonoBehaviour
{
    public delegate void EventChangeHandler(bool newState);
    
    public event EventChangeHandler EventChange;
    private bool _eventActive;
    public bool EventActive
    {
        get  { return _eventActive; }
        protected set
        {
            if (_eventActive == value) return;
            _eventActive = value;
            EventChange?.Invoke(_eventActive);
        }
    }
}
