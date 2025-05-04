using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public int lap = 0;
    public int checkPoint = -1;
    public int checkPointCount;
    public int nextCheckPoint;
    public GameObject lastCheckPoint;
    public float timeEntered = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject[] cps = GameObject.FindGameObjectsWithTag("CheckPoint");
        checkPointCount = cps.Length;
        foreach(GameObject c in cps)
        {
            if (c.name == "0")
            {
                lastCheckPoint = c;
                break;
            }
        }
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "CheckPoint")
        {
            int thisChPtNum = int.Parse(col.gameObject.name);
            if (thisChPtNum == nextCheckPoint)
            {
                lastCheckPoint = col.gameObject;
                checkPoint = thisChPtNum;
                timeEntered = Time.time;
                if (checkPoint == 0) lap++;

                nextCheckPoint++;
                if (nextCheckPoint >= checkPointCount)
                {
                    nextCheckPoint = 0;
                }
            }

        }
    }
}
