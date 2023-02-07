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
    public static bool canMove;
    public AudioSource rollingAudioSource;
     

    // Start is called before the first frame update
    void Start()
    {
        canMove = true;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(!canMove)
        {
            rb.velocity = Vector3.zero;
            return;
        }
        Vector3 temp = Camera.main.transform.forward * movmentVector.y;
        temp += Camera.main.transform.right * movmentVector.x;
        float volume = GetComponent<S_VolumeObject>().Volume;
        rb.AddForce(temp * (Speed));
        //rb.mass= volume*1.5f;
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, MaxVelocity);


        rollingAudioSource.volume = Mathf.Clamp01(rb.velocity.magnitude)-0.2f;

    }

    private void OnMove(InputValue MovmentValue)
    {
        movmentVector = MovmentValue.Get<Vector2>();
    }
}
