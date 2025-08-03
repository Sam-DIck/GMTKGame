using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    Animator _animator;
    MeshCollider _meshCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _animator = GetComponent<Animator>();
        _meshCollider = GetComponent<MeshCollider>();
        _animator.StopPlayback();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.E)) 
        {
            _animator.Play(0);
            _meshCollider.enabled=false;
        }
        
    }
}
