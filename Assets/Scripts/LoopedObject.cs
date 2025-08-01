using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MotionKey
{
    private static Quaternion NormalizeQuaternion(Quaternion q)
    {
        float mag = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
        if (mag > 0.0001f)
        {
            return new Quaternion(q.x / mag, q.y / mag, q.z / mag, q.w / mag);
        }
        else
        {
            // fallback to identity to prevent invalid rotations
            return Quaternion.identity;
        }
    }
    public MotionKey(Vector3 pos, Quaternion rot, Vector3 vel, Vector3 angVel)
    {
        position = pos;
        rotation = NormalizeQuaternion(rot);
        velocity = vel;
        angularVelocity = angVel;
    }
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public Vector3 angularVelocity;
}

enum LoopState
{
    Forward, Backward
}
[RequireComponent(typeof(Rigidbody))]
public class LoopedObject : MonoBehaviour
{
    [SerializeField] private MotionKey startKey;
    [SerializeField] private List<MotionKey> keys;
    [SerializeField] private float loopDuration;
    [SerializeField] private uint trackRate = 1;
    [SerializeField] private bool useRecorded;
    private float _loopElapsed;
    private LoopState _loopState = LoopState.Forward;
    [SerializeField] private int _currentKey;
    private Rigidbody _rigidbody;

    public void Init(float duration, MotionKey start)
    {
        startKey = start;
        loopDuration = duration;
        _loopElapsed = duration;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (!_rigidbody)
        {
            _rigidbody = GetComponent<Rigidbody>();
        }
    }

    private void RestoreKey(MotionKey key, bool kinematic)
    {
        _rigidbody.MovePosition(key.position);
        _rigidbody.MoveRotation(key.rotation);
        _rigidbody.isKinematic = kinematic;
        if (!kinematic)
        {
            _rigidbody.linearVelocity = key.velocity;
            _rigidbody.angularVelocity = key.angularVelocity;
        }
    }
    // Update is called once per frame
    private void FixedUpdate()
    {
        switch (_loopState)
        {
            case LoopState.Forward:

                if (useRecorded)                                        // if using recorded track
                {
                    RestoreKey(keys[_currentKey],true);             // restore current frame 

                    _currentKey++;                                          // increment frame index
                    if (_currentKey == keys.Count)                          // if finshed track
                    {
                        _loopState = LoopState.Backward;                        // switch to reverse loop
                    }
                    
                }
                else {                                                  // else (not using recorded track)
                    _loopElapsed += Time.fixedDeltaTime;                    // update loop timer
                    
                    keys.Add(new MotionKey(transform.position,          // record motion to track
                        transform.rotation,
                        _rigidbody.linearVelocity,
                        _rigidbody.angularVelocity)
                    );
                    
                    if (_loopElapsed >= loopDuration)                       // if loop time elapsed
                    {
                        _loopElapsed = 0;                                      // reset loop timer
                        _loopState = LoopState.Backward;                       // switch to reverse loop
                        _currentKey = keys.Count - 1;                          // set frame index to the end
                    
                    }
                }
                break;
            case LoopState.Backward:
                if (_currentKey <= trackRate)                           // if trackback finished
                {
                    RestoreKey(startKey,false);                     // restore initial position
                    _loopState = LoopState.Forward;                         // switch to forward loop

                    if (!useRecorded)                                       // if not using recorded track
                    {
                        keys.Clear();                                           // clear recording
                    }
                    break;
                }
                
                                                                        // else (trackback not finished)
                _currentKey-=(int)trackRate;                                // skip to match trackback speed
                while (_currentKey > 0 &&                                   // while has frames remaining    AND
                       keys[_currentKey].velocity.magnitude == 0 &&         // current frame is not moving   AND
                       keys[_currentKey].angularVelocity.magnitude == 0)    // current frame is not rotating
                {
                    _currentKey--;                                          // skip frame
                }

                var key = keys[_currentKey];                    
                RestoreKey(key,true);                               // restore frame
                break;
        }
    }
}
