using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Letter : MonoBehaviour
{
    public bool canAnimate;

    private void OnEnable()
    {
        canAnimate = true;
    }
}
