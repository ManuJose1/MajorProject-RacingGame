using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script controls the car's movement, including driving, steering, and skid effects
public class Drive : MonoBehaviour
{
    // The wheel colliders that handle the physics of the wheels
    public WheelCollider[] WCs;
    // The visual wheel models that the player sees
    public GameObject[] Wheels;
    // How much power the car has when accelerating
    public float torque = 200;
    // How far the front wheels can turn left or right
    public float maxSteerAngle = 30;
    // How strong the brakes are
    public float maxBrakeTorque = 500;
    // Sound that plays when the car is skidding
    public AudioSource skidSound;
    public AudioSource engineSound;
    public Rigidbody rb;
    public float gearLength = 3;
    public float currentSpeed { get { return rb.linearVelocity.magnitude * gearLength; } }
    public float lowPitch = 1f;
    public float highPitch = 6f;
    public int numGears = 5;
    float rpm;
    int currentGear = 1;
    float currentGearPerc;
    public float maxSpeed = 200;

    public ParticleSystem smokePrefab;
    ParticleSystem[] tireSmoke = new ParticleSystem[4];

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            tireSmoke[i] = Instantiate(smokePrefab);
            tireSmoke[i].Stop();
        }
    }

    public void CalculateEngineSound()
    {
        float gearPercentage = (1 / (float)numGears);
        float targetGearFactor = Mathf.InverseLerp(gearPercentage * currentGear, gearPercentage * (currentGear + 1),
                                                    Mathf.Abs(currentSpeed / maxSpeed));
        currentGearPerc = Mathf.Lerp(currentGearPerc, targetGearFactor, Time.deltaTime * 5f);

        var gearNumFactor = currentGear / (float)numGears;
        rpm = Mathf.Lerp(gearNumFactor, 1, currentGearPerc);

        float speedPercentage = Mathf.Abs(currentSpeed / maxSpeed);
        float upperGearMax = (1 / (float)numGears) * (currentGear + 1);
        float downGearMax = (1 / (float)numGears) * currentGear;

        if (currentGear > 0 && speedPercentage < downGearMax)
            currentGear--;

        if (speedPercentage > upperGearMax && (currentGear < (numGears - 1)))
            currentGear++;

        float pitch = Mathf.Lerp(lowPitch, highPitch, rpm);
        engineSound.pitch = Mathf.Min(highPitch, pitch) * 0.25f;
    }

    // Handles the car's movement based on player input
    public void Go(float accel, float steer, float brake)
    {
        // Make sure input values are within valid ranges
        accel = Mathf.Clamp(accel, -1, 1);
        steer = Mathf.Clamp(steer, -1, 1) * maxSteerAngle;
        brake = Mathf.Clamp(brake, 0, 1) * maxBrakeTorque;

        // Calculate how much power to give the wheels
        float thrustTorque = 0;
        if (currentSpeed < maxSpeed)
            thrustTorque = accel * torque;
        else
            thrustTorque = accel * torque * (maxSpeed / currentSpeed); //Cars acceleration decreases as it approaches max speed

        // Update each wheel
        for (int i = 0; i < 4; i++)
        {
            // Apply power to the wheel
            WCs[i].motorTorque = thrustTorque;

            // Front wheels can steer
            if (i < 2)
                WCs[i].steerAngle = steer;
            // Back wheels can brake
            else
                WCs[i].brakeTorque = brake;

            // Update the visual position and rotation of the wheel model
            Quaternion quat;
            Vector3 position;
            WCs[i].GetWorldPose(out position, out quat);
            Wheels[i].transform.position = position;
            Wheels[i].transform.localRotation = quat;
        }
    }

    // Checks if any wheels are skidding and handles skid effects
    public void CheckSkid()
    {
        // Count how many wheels are currently skidding
        int numSkidding = 0;


        for (int i = 0; i < 4; i++)
        {
            // Get information about the wheel's contact with the ground
            WheelHit wheelHit;
            WCs[i].GetGroundHit(out wheelHit);

            // If the wheel is skidding (either spinning or sliding)
            if (Mathf.Abs(wheelHit.forwardSlip) >= 0.4f || Mathf.Abs(wheelHit.sidewaysSlip) >= 0.2f)
            {
                numSkidding++;
                // Play skid sound if it's not already playing
                if (!skidSound.isPlaying)
                {
                    skidSound.Play();
                }
                //Position tire smoke under each wheel
                tireSmoke[i].transform.position = WCs[i].transform.position - WCs[i].transform.up * WCs[i].radius;
                tireSmoke[i].Emit(1);
            }
        }
        // Stop skid sound if no wheels are skidding
        if (numSkidding == 0 && skidSound.isPlaying)
        {
            skidSound.Stop();
        }
    }
}