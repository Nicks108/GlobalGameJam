using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_RotateOverTime : MonoBehaviour
{
    public Vector3 RotationDirection;


    public float Speed=1;

    // Update is called once per frame
    void Update()
    {


        this.transform.Rotate(RotationDirection * Time.deltaTime * Speed);
    }
}
