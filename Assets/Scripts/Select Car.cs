using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SelectCar : MonoBehaviour
{
    public GameObject[] cars;
    int currentCar = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(PlayerPrefs.HasKey("PlayerCar"))
        {
            currentCar = PlayerPrefs.GetInt("PlayerCar");
        }
        this.transform.LookAt(cars[currentCar].transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            currentCar++;
            if (currentCar > cars.Length - 1)
                currentCar = 0;

            PlayerPrefs.SetInt("PlayerCar", currentCar);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            currentCar--;
            if (currentCar < 0)
                currentCar = cars.Length - 1; 

            PlayerPrefs.SetInt("PlayerCar", currentCar);
        }

        Quaternion lookDir = Quaternion.LookRotation(cars[currentCar].transform.position - this.transform.position);
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, lookDir, Time.deltaTime * 2.0f);
    }
}
