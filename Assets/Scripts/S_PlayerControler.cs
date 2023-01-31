using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class S_PlayerControler : MonoBehaviour
{
    private Rigidbody rb;
    private Vector2 movmentVector;
    public float MaxVelocity =10;
    public float Speed = 10;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 temp = Camera.main.transform.forward * movmentVector.y;
        temp += Camera.main.transform.right * movmentVector.x;
        float volume = GetComponent<S_VolumeObject>().Volume;
        rb.AddForce(temp * (Speed));
        //rb.mass= volume*1.5f;
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, MaxVelocity);
    }

    private void OnMove(InputValue MovmentValue)
    {
        movmentVector = MovmentValue.Get<Vector2>();
    }
}
