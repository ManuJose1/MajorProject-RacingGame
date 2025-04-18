using UnityEngine;

public class AvoidDetector : MonoBehaviour
{
    public float avoidPath = 1; // Path to avoid the object
    public float avoidTime = 1; // Time to avoid the object
    public float wanderDistance = 2; // Distance to the object being avoided
    public float avoidLength = 2; // Length of the avoidance raycast

    void OnCollisionExit(Collision col)
    {
        if (col.gameObject.tag != "car") return;
        avoidTime = 0;
    }

    void OnColisionStay(Collision col)
    {
        if (col.gameObject.tag != "car") return;

        Rigidbody otherCar = col.rigidbody;
        avoidTime = Time.time + avoidLength;

        Vector3 otherCarLocalTarget = transform.InverseTransformPoint(otherCar.gameObject.transform.position);
        float otherCarAngle = Mathf.Atan2(otherCarLocalTarget.x, otherCarLocalTarget.z);
        avoidPath = wanderDistance * -Mathf.Sign(otherCarAngle);
    }

}
