using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Drive ds;
    float lastTimeMoving = 0;
    Vector3 lastPosition;
    Quaternion lastRotation;

    CheckpointManager cpm;

    float finishSteer;

    // Start is called before the first frame update
    void Start()
    {
        ds = this.GetComponent<Drive>();
        Invoke("ResetLayer", 3);
        lastPosition = ds.rb.gameObject.transform.position;
        lastRotation = ds.rb.gameObject.transform.rotation;

        finishSteer = Random.Range(-1.0f, 1.0f);
    }

    void ResetLayer()
    {
        ds.rb.gameObject.layer = 0;
        this.GetComponent<Ghost>().enabled = false;
    }


    // Update is called once per frame
    void Update()
    {
        
        if (cpm == null)
            cpm = ds.rb.GetComponent<CheckpointManager>();

        //If the race is over, stop the car and engine sound and return
        if (cpm.lap == RaceMonitor.totalLaps + 1)
        {
            ds.engineSound.Stop();
            ds.Go(0, finishSteer, 0);
            return;
        }

        //Get player input
        float a = Input.GetAxis("Vertical");    //Acceleration
        float s = Input.GetAxis("Horizontal");  //Steering 
        float b = Input.GetAxis("Jump");        //Brake

        if (ds.rb.linearVelocity.magnitude > 0.1f || !RaceMonitor.racing)
        {
            lastTimeMoving = Time.time;
        }

        RaycastHit hit;
        if (Physics.Raycast(ds.rb.gameObject.transform.position, -Vector3.up, out hit, 5))
        {
            if (hit.collider.gameObject.tag == "road")//Check if the cars on the road
            {
                lastPosition = ds.rb.gameObject.transform.position;
                lastRotation = ds.rb.gameObject.transform.rotation;

            }
        }

        if (Time.time > lastTimeMoving + 4 || ds.rb.gameObject.transform.position.y < -5f)//Check if the is moving
        {
            if (cpm == null)
                cpm = ds.rb.GetComponent<CheckpointManager>();

            ds.rb.gameObject.transform.position = cpm.lastCheckPoint.transform.position + Vector3.up;
            ds.rb.gameObject.transform.rotation = cpm.lastCheckPoint.transform.rotation;
            ds.rb.gameObject.layer = 8;
            this.GetComponent<Ghost>().enabled = true;
            Invoke("ResetLayer", 3);
        }

        if (!RaceMonitor.racing) a = 0;

        //Apply the input to make the car move
        ds.Go(a, s, b);
        ds.CheckSkid();
        ds.CalculateEngineSound();
    }
}
