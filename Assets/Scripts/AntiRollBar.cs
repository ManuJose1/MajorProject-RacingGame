using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiRollBar : MonoBehaviour
{
    public float antiRoll = 5000.0f; // Anti-roll bar strength

    // Wheel Colliders
    public WheelCollider wheelFL; 
    public WheelCollider wheelFR; 
    public WheelCollider wheelRL;
    public WheelCollider wheelRR; 

    Rigidbody rb;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    // Stiffens the suspension of the wheels to reduce body roll
    // when the car is turning. The anti-roll bar applies a force to the wheels
    void GroundWheels(WheelCollider WL, WheelCollider WR)
    {
        WheelHit hit;
        float travelL = 1.0f;
        float travelR = 1.0f;

        bool groundedL = WL.GetGroundHit(out hit);
        if (groundedL)
        {
            travelL = (-WL.transform.InverseTransformPoint(hit.point).y - WL.radius) / WL.suspensionDistance;
        }

        bool groundedR = WR.GetGroundHit(out hit);
        if (groundedR)
        {
            travelR = (-WR.transform.InverseTransformPoint(hit.point).y - WR.radius) / WR.suspensionDistance;
        }

        float antiRollForce = (travelL - travelR) * antiRoll;
        if (groundedL)
        {
            rb.AddForceAtPosition(WL.transform.up * -antiRollForce, WL.transform.position);
        }

        if (groundedR)
        {
            rb.AddForceAtPosition(WR.transform.up * antiRollForce, WR.transform.position);
        }
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GroundWheels(wheelFL, wheelFR); 
        GroundWheels(wheelRL, wheelRR); 
    }
}
