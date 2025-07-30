using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct MotionKey
{
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
                keys.Add(new MotionKey {
                        position = transform.position,
                        rotation = transform.rotation,
                        velocity = rb.linearVelocity,
                        angularVelocity = rb.angularVelocity
                    }
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
                    transform.position = startKey.position;
                    transform.rotation = startKey.rotation;
                    rb.linearVelocity = startKey.velocity;
                    rb.angularVelocity = startKey.angularVelocity;
                    break;
                }
                var key =  keys[keys.Count - (int)trackRate];
                transform.position = key.position;
                transform.rotation = key.rotation;
                keys = keys.Take(keys.Count - (int)trackRate).ToList();
                break;
        }
        
    }
}
