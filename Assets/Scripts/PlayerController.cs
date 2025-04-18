using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Drive ds;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ds = this.GetComponent<Drive>();
    }

    // Update is called once per frame
    void Update()
    {
        //Get player input
        float a = Input.GetAxis("Vertical");    // Gas/brake 
        float s = Input.GetAxis("Horizontal");  // Steering 
        float b = Input.GetAxis("Jump");        // Hand brake

        // Apply the input to make the car move
        ds.Go(a,s,b);
        // Check for skidding
        ds.CheckSkid();
        ds.CalculateEngineSound();
    }
}
