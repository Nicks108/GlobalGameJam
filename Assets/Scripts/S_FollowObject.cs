using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_FollowObject : MonoBehaviour
{

    public GameObject Target;
    public float Distance;
    public float DistanceDelta;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = Vector3.MoveTowards(Target.transform.position + this.transform.forward * Distance, transform.position, DistanceDelta);

        this.transform.LookAt(Target.transform.position);
    }
}
