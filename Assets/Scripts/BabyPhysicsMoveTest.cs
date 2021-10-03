using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BabyPhysicsMoveTest : MonoBehaviour
{
    private Rigidbody body;
    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        body.AddTorque(transform.up * 30);
    }
}
