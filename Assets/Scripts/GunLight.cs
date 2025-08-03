using System.Runtime.CompilerServices;
using UnityEngine;

public class GunLight : MonoBehaviour
{

    private Light _light;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _light = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _light.color = Color.green;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            _light.color = Color.blue;
        }


    }
}
