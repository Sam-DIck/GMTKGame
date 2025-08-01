using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MotionKey
{
    private static Quaternion NormalizeQuaternion(Quaternion quat)
    {
        bool IsFinite(Quaternion q)
        {
            return float.IsFinite(q.x) && float.IsFinite(q.y) && float.IsFinite(q.z) && float.IsFinite(q.w);
        }
        if (!IsFinite(quat)) return Quaternion.identity;

        float mag = Mathf.Sqrt(quat.x * quat.x + quat.y * quat.y + quat.z * quat.z + quat.w * quat.w);
        if (mag < 0.0001f) return Quaternion.identity;

        return new Quaternion(quat.x / mag, quat.y / mag, quat.z / mag, quat.w / mag);
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
    [SerializeField] private List<MotionKey> keys = new();
    [SerializeField] private float loopDuration;
    [SerializeField] private uint trackRate = 1;
    [SerializeField] private bool useRecorded;
    private float _loopElapsed;
    private LoopState _loopState = LoopState.Forward;
    private int _currentKey;
    private Rigidbody _rigidbody;
    private float _recordedDuration;
    private bool _initialKinematic;

    public void StartRecording()
    {
        useRecorded = false;
        _loopState = LoopState.Forward;
        _loopElapsed = 0;
        loopDuration = int.MaxValue;
        _recordedDuration = 0f;
        _currentKey = 0;
        keys.Clear();
        startKey = new MotionKey(transform.position, transform.rotation, _rigidbody.linearVelocity, _rigidbody.angularVelocity);
    }
    public void StopRecording()
    {
        loopDuration = _recordedDuration;
        useRecorded = true;
        _loopState = LoopState.Backward;
    }

    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (!_rigidbody)
        {
            _rigidbody = GetComponent<Rigidbody>();
            _initialKinematic = _rigidbody.isKinematic;
        }
    }

    private void OnDestroy()
    {
        RestoreKey(keys[_currentKey],_initialKinematic);
    }

    private void RestoreKey(MotionKey key, bool kinematic)
    {
        _rigidbody.MovePosition(key.position);
        _rigidbody.MoveRotation(key.rotation);
        _rigidbody.isKinematic = kinematic;
        if (kinematic) return;
        _rigidbody.linearVelocity = key.velocity;
        _rigidbody.angularVelocity = key.angularVelocity;
        
    }
    // Update is called once per frame
    private void FixedUpdate()
    {
        _recordedDuration += Time.fixedDeltaTime;       
        switch (_loopState)
        {
            case LoopState.Forward:

                if (useRecorded)                                        // if using recorded track
                {
                    if (_currentKey >= keys.Count)                          // if finished track
                    {
                        _loopState = LoopState.Backward;                        // switch to reverse loop
                        break;
                    }
                    RestoreKey(keys[_currentKey],true);             // restore current frame 
                    _currentKey++;                                          // increment frame index
                    
                    
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
