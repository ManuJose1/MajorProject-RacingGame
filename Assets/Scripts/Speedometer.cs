using UnityEngine;
using TMPro;

public class Speedometer : MonoBehaviour
{
    public TextMeshProUGUI speedText; // Reference to TextMeshPro or Text element for numeric speed
    public RectTransform needle;     // Reference to the needle's transform
    public GameObject car;           // Reference to the car GameObject
    public float maxSpeed = 240f;    // Max speed for the car

    private CarController carController;

    void Start()
    {
        carController = car.GetComponent<CarController>();
    }

    void Update()
    {
        // Get the current speed of the car
        float speed = carController.GetSpeed();

        // Update speed text
        if (speedText != null)
        {
            speedText.text = $"{Mathf.Round(speed)} km/h";
        }

        // Rotate the needle
        if (needle != null)
        {
            // Map speed to needle rotation
            float needleRotation = Mathf.Lerp(-120f, 120f, speed / maxSpeed); 
            needle.localRotation = Quaternion.Euler(0, 0, -needleRotation);
        }
    }
}
