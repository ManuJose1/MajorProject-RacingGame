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

    // Start is called before the first frame update
    void Start()
    {
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
    }

    void ProgressTracker()
    {
        Debug.DrawLine(ds.rb.gameObject.transform.position, tracker.transform.position, Color.red);

        if (Vector3.Distance(ds.rb.gameObject.transform.position, tracker.transform.position) > lookAhead) return;

        tracker.transform.LookAt(circuit.waypoints[currentTrackerWP].transform.position);
        tracker.transform.Translate(0, 0, 1.0f);

        if (Vector3.Distance(tracker.transform.position, circuit.waypoints[currentTrackerWP].transform.position) < 3.0f)
        {
            currentTrackerWP++;
            if (currentTrackerWP >= circuit.waypoints.Length)
                currentTrackerWP = 0;
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
        ProgressTracker();
        Vector3 localTarget;
        float targetAngle;
        if (ds.rb.linearVelocity.magnitude > 1)
            lastTimeMoving = Time.time;

        //If the car is not moving, reset the position of the tracker to the current waypoint & rotation to the next waypoint
        if (Time.time > lastTimeMoving + 4)
        {
            ds.rb.gameObject.transform.position = circuit.waypoints[currentTrackerWP].transform.position + Vector3.up + new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1));
            Vector3 directionToNextWaypoint = (circuit.waypoints[(currentTrackerWP + 1) % circuit.waypoints.Length].transform.position
                                               - circuit.waypoints[currentTrackerWP].transform.position).normalized;
            ds.rb.gameObject.transform.rotation = Quaternion.LookRotation(directionToNextWaypoint);
            tracker.transform.position = ds.rb.gameObject.transform.position;
            ds.rb.gameObject.layer = 8;
            this.GetComponent<Ghost>().enabled = true;
            Invoke("ResetLayer", 5);
        }

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

        float brake = 0;
        if (corner > 10 && speedFactor > 0.1f)
        {
            brake = Mathf.Lerp(0, 1 + speedFactor * brakeSensitivity, cornerFactor);
        }
        float accel = 1f;
        if (corner > 20 && speedFactor > 0.5f)
        {
            accel = Mathf.Lerp(0, 1 * accelSensitivity, 1 - cornerFactor);
        }

        ds.Go(accel, steer, brake);

        ds.CheckSkid();
        ds.CalculateEngineSound();

        Debug.Log("Steer: " + steer + " Accel: " + accel + " Brake: " + brake);
        Debug.Log("Current Speed: " + ds.currentSpeed);
        if (ds.rb == null)
        {
            Debug.LogError("Rigidbody not found on " + ds.transform.name);
        }
    }
}
