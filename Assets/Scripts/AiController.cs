using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiController : MonoBehaviour
{
    public Circuit circuit;
    public float brakeSensitivity = 1f;
    public float accelSensitivity = 1f;
    Drive ds;
    public float steeringSensitivity = 0.01f;

    Vector3 target;
    Vector3 nextTarget;
    int currentWP = 0;
    float totalDistanceToTarget;

    GameObject tracker;
    int currentTrackerWP = 0;
    public float lookAhead = 10;

    float lastTimeMoving = 0;

    CheckpointManager cpm;
    float finishSteer;

    // Start is called before the first frame update
    void Start()
    {
        if (circuit == null)
        {
            circuit = GameObject.FindGameObjectWithTag("Circuit").GetComponent<Circuit>();
        }
        ds = this.GetComponent<Drive>();
        target = circuit.waypoints[currentWP].transform.position;
        nextTarget = circuit.waypoints[currentWP + 1].transform.position;
        totalDistanceToTarget = Vector3.Distance(target, ds.rb.gameObject.transform.position);

        tracker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        DestroyImmediate(tracker.GetComponent<SphereCollider>());
        tracker.GetComponent<MeshRenderer>().enabled = false;
        tracker.transform.position = ds.rb.gameObject.transform.position;
        tracker.transform.rotation = ds.rb.gameObject.transform.rotation;
        this.GetComponent<Ghost>().enabled = true;
        Invoke("ResetLayer", 3);

        finishSteer = Random.Range(-1.0f, 1.0f);
    }

    void ProgressTracker() //The cars steering follows this tracker
    {
        Debug.DrawLine(ds.rb.gameObject.transform.position, tracker.transform.position, Color.red);
        
        if (Vector3.Distance(ds.rb.gameObject.transform.position, tracker.transform.position) > lookAhead) return;

        //The tracker moves towards the current waypoint
        tracker.transform.LookAt(circuit.waypoints[currentTrackerWP].transform.position);
        tracker.transform.Translate(0, 0, 1.0f);

        //When the tracker gets close to the current waypoint, it moves to the next one
        if (Vector3.Distance(tracker.transform.position, circuit.waypoints[currentTrackerWP].transform.position) < 3.0f)
        {
            currentTrackerWP++;
            if (currentTrackerWP >= circuit.waypoints.Length)
                currentTrackerWP = 0;
        }

        //If the tracker is too far from the car, move it back to the car's position
        if (Vector3.Distance(ds.rb.gameObject.transform.position, tracker.transform.position) > lookAhead * 2)
        {
            tracker.transform.position = circuit.waypoints[currentTrackerWP].transform.position;
        }
        
    }

    void ResetLayer()
    {
        ds.rb.gameObject.layer = 0;
        this.GetComponent<Ghost>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!RaceMonitor.racing)
        {
            lastTimeMoving = Time.time;
            return;
        }
        
        if(cpm == null)
            cpm = ds.rb.GetComponent<CheckpointManager>();

        if(cpm.lap == RaceMonitor.totalLaps + 1)
        {
            ds.engineSound.Stop();
            ds.Go(0, finishSteer, 0);
            return;
        }
        
        ProgressTracker();
        Vector3 localTarget;
        float targetAngle;

        if (ds.rb.linearVelocity.magnitude > 1)
            lastTimeMoving = Time.time;

        //If the cars not moving, reset the position of the tracker to the latest & rotation to the next waypoint
        if (Time.time > lastTimeMoving + 4 || ds.rb.gameObject.transform.position.y < -5f)
        {

            ds.rb.gameObject.transform.position = cpm.lastCheckPoint.transform.position + Vector3.up;
            ds.rb.gameObject.transform.rotation = cpm.lastCheckPoint.transform.rotation;
            tracker.transform.position = cpm.lastCheckPoint.transform.position; 
            ds.rb.gameObject.layer = 8;
            this.GetComponent<Ghost>().enabled = true;
            Invoke("ResetLayer", 5);
        }

        //Use the avoid path if avoid time is active
        if (Time.time < ds.rb.GetComponent<AvoidDetector>().avoidTime)
        {
            localTarget = tracker.transform.right * ds.rb.GetComponent<AvoidDetector>().avoidPath;
        }
        else
        {
            localTarget = ds.rb.gameObject.transform.InverseTransformPoint(tracker.transform.position);
        }

        targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        float steer = Mathf.Clamp(targetAngle * steeringSensitivity, -1, 1) * Mathf.Sign(ds.currentSpeed);

        float speedFactor = ds.currentSpeed / ds.maxSpeed;

        float corner = Mathf.Clamp(Mathf.Abs(targetAngle), 0, 90);
        float cornerFactor = corner / 90.0f;

        //Anticipate upcoming corners
        float upcomingCornerAngle = Vector3.Angle(
            circuit.waypoints[(currentTrackerWP + 1) % circuit.waypoints.Length].transform.position - circuit.waypoints[currentTrackerWP].transform.position,
            circuit.waypoints[(currentTrackerWP + 2) % circuit.waypoints.Length].transform.position - circuit.waypoints[(currentTrackerWP + 1) % circuit.waypoints.Length].transform.position
        );

        float upcomingCornerFactor = Mathf.Clamp(upcomingCornerAngle / 90.0f, 0, 1);

        //Adjust braking and acceleration based on the sharpness of the current and upcoming corners
        float combinedCornerFactor = Mathf.Max(cornerFactor, upcomingCornerFactor);

        //Gradual braking for sharp corners
        float brake = Mathf.Lerp(0, brakeSensitivity, combinedCornerFactor) * speedFactor;
        float accel = Mathf.Lerp(1, 0, combinedCornerFactor) * accelSensitivity;

        //Limit speed on long straights if a sharp corner is ahead
        if (upcomingCornerFactor > 0.7f && ds.currentSpeed > 15f)
        {
            brake = Mathf.Lerp(brake, brakeSensitivity, 0.5f);
            accel = Mathf.Lerp(accel, 0, 0.5f); 
        }

        //Ensure the car accelerates at the start of the race
        if (ds.currentSpeed < 45f) 
        {
            accel = 1f; 
            brake = 0f; 
        }

        //Increase cars torque if car is going up and incline and speed is low
        float prevTourque = ds.torque;
        if (speedFactor < 0.3f && ds.rb.gameObject.transform.position.y > 0.1f)
        {
            ds.torque *= 3.0f;
            accel = 1f;
            brake = 0f;
        }

        ds.Go(accel, steer, brake);

        ds.CheckSkid();
        ds.CalculateEngineSound();

        ds.torque = prevTourque;

        Debug.Log("Current Speed: " + ds.currentSpeed);
        if (ds.rb == null)
        {
            Debug.LogError("Rigidbody not found on " + ds.transform.name);
        }
    }
}
