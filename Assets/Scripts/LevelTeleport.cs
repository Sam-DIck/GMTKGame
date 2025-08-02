using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTeleport : MonoBehaviour
{
    [SerializeField] private Eflatun.SceneReference.SceneReference scene;
    [SerializeField] private float waitTime = 1f;
    [SerializeField] private bool requireStay = true;
    [SerializeField] private float _waitElapsed;
    void OnTriggerEnter(Collider other)
    {
        _waitElapsed = 0f;
    }

    void Update()
    {
        if (!requireStay)
        {
            _waitElapsed += Time.deltaTime;
            if (_waitElapsed > waitTime)
            {
                ChangeScene();
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (requireStay)
        {
            _waitElapsed += Time.deltaTime;
            if (_waitElapsed > waitTime)
            {
                ChangeScene();
            }
        }
    }

    void ChangeScene()
    {
        if (scene.State == SceneReferenceState.Regular)
        {
            SceneManager.LoadScene(scene.BuildIndex, LoadSceneMode.Single);
        }
    }
}
