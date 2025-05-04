using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Car Settings")]
    public float motorForce = 1500f; // Force applied to the wheels when accelerating
    public float brakeForce = 3000f; // Force applied when braking
    public float maxSteerAngle = 30f; // Maximum steering angle for the wheels
    public float driftFactor = 0.95f; // Factor to control drifting

    [Header("Wheel Colliders")]
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    [Header("Wheel Transforms")]
    public Transform frontLeftTransform;
    public Transform frontRightTransform;
    public Transform rearLeftTransform;
    public Transform rearRightTransform;

    private float horizontalInput;
    private float verticalInput;
    private float currentSteerAngle;
    private float currentBrakeForce;
    private bool isHandbraking;

    void Update()
    {
        GetInput();
        Steer();
        UpdateWheels();

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetVehicle();
        }
    }

    void FixedUpdate()
    {
        HandleMotor();
        HandleBraking();
        HandleDrift();
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isHandbraking = Input.GetKey(KeyCode.Space);
    }

    private void HandleMotor()
    {
        // Apply motor torque to rear wheels
        rearLeftWheel.motorTorque = verticalInput * motorForce;
        rearRightWheel.motorTorque = verticalInput * motorForce;

        // Reduce torque if handbrake is active
        if (isHandbraking)
        {
            rearLeftWheel.motorTorque = 0;
            rearRightWheel.motorTorque = 0;
        }

        // Apply a small brake force when there is no vertical input
        if (verticalInput == 0)
        {
            rearLeftWheel.brakeTorque = brakeForce * 0.3f; 
            rearRightWheel.brakeTorque = brakeForce * 0.3f; 
        }
        else
        {
            rearLeftWheel.brakeTorque = 0;
            rearRightWheel.brakeTorque = 0;
        }
    }

    private void Steer()
{
    float steeringAngle = maxSteerAngle * horizontalInput;

    // Apply a greater angle to the right wheel when turning right
    if (horizontalInput > 0)
    {
        frontLeftWheel.steerAngle = steeringAngle * 0.7f;
        frontRightWheel.steerAngle = steeringAngle;
    }
    // Apply a greater angle to the left wheel when turning left
    else if (horizontalInput < 0)
    {
        frontLeftWheel.steerAngle = steeringAngle;
        frontRightWheel.steerAngle = steeringAngle * 0.7f;
    }
    else
    {
        frontLeftWheel.steerAngle = 0;
        frontRightWheel.steerAngle = 0;
    }
}

    private void HandleBraking()
    {
        currentBrakeForce = isHandbraking ? brakeForce : 0f;

        // Apply brake force to rear wheels for handbraking
        rearLeftWheel.brakeTorque = currentBrakeForce;
        rearRightWheel.brakeTorque = currentBrakeForce;
    }

    private void HandleDrift()
    {
        if (isHandbraking)
        {
            // Simulate drift by reducing rear wheel sideways friction
            WheelFrictionCurve sidewaysFriction = rearLeftWheel.sidewaysFriction;
            sidewaysFriction.stiffness = driftFactor;
            rearLeftWheel.sidewaysFriction = sidewaysFriction;
            rearRightWheel.sidewaysFriction = sidewaysFriction;
        }
        else
        {
            // Restore normal friction when not handbraking
            WheelFrictionCurve normalFriction = rearLeftWheel.sidewaysFriction;
            normalFriction.stiffness = 1f;
            rearLeftWheel.sidewaysFriction = normalFriction;
            rearRightWheel.sidewaysFriction = normalFriction;
        }
    }

    private void UpdateWheels()
    {
        UpdateWheelTransform(frontLeftWheel, frontLeftTransform);
        UpdateWheelTransform(frontRightWheel, frontRightTransform);
        UpdateWheelTransform(rearLeftWheel, rearLeftTransform);
        UpdateWheelTransform(rearRightWheel, rearRightTransform);
    }

    private void UpdateWheelTransform(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 position;
        Quaternion rotation;
        wheelCollider.GetWorldPose(out position, out rotation);
        wheelTransform.position = position;
        // wheelTransform.rotation = rotation;
        wheelTransform.rotation = rotation * Quaternion.Euler(0, 90, 0);
    }

    private void ResetVehicle()
    {
        // Reset position and rotation
        transform.rotation = Quaternion.identity;

        // Reset the velocity
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public float GetSpeed()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        return rb.linearVelocity.magnitude * 3.6f; // Convert meters per second to km/h
    }
}