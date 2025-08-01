using System.Collections.Generic;
using System.Linq;
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
    private float loopElapsed;
    private LoopState loopState = LoopState.Forward;
    private Rigidbody rb;

    void Init(float duration, MotionKey start)
    {
        startKey = start;
        loopDuration = duration;
        loopElapsed = duration;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!rb)
        {
            rb = GetComponent<Rigidbody>();
        }
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        switch (loopState)
        {
            case LoopState.Forward:
                rb.isKinematic = false;
                loopElapsed -= Time.fixedDeltaTime;
                if (rb.linearVelocity.magnitude !=0 || rb.angularVelocity.magnitude != 0)
                keys.Add(new MotionKey(transform.position,
                                       transform.rotation,
                                       rb.linearVelocity,
                                       rb.angularVelocity)
                );
                if (loopElapsed <= 0)
                {
                    loopElapsed =  loopDuration;
                    loopState = LoopState.Backward;
                    
                }
                break;
            case LoopState.Backward:
                
                rb.isKinematic = true;
                if (keys.Count <= trackRate)
                {
                    keys.Clear();
                    rb.isKinematic = false;
                    loopState = LoopState.Forward;
                    rb.MovePosition(startKey.position);
                    rb.MoveRotation(startKey.rotation);
                    rb.linearVelocity = startKey.velocity;
                    rb.angularVelocity = startKey.angularVelocity;
                    break;
                }
                var key =  keys[keys.Count - (int)trackRate];
                rb.MovePosition(key.position);
                rb.MoveRotation(key.rotation);
                keys = keys.Take(keys.Count - (int)trackRate).ToList();
                break;
        }
        
    }
}
