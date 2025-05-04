using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NameUIController : MonoBehaviour
{
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI lapDisplay;
    public Transform target;
    CanvasGroup canvasGroup;
    public Renderer carRend;
    CheckpointManager checkpointManager;

    int carReg;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.transform.SetParent(GameObject.Find("Canvas").GetComponent<RectTransform>(), false);
        playerName = this.GetComponent<TextMeshProUGUI>();
        canvasGroup = this.GetComponent<CanvasGroup>();
        carReg = Leaderboard.RegisterCar(playerName.text);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!RaceMonitor.racing) { canvasGroup.alpha = 0; return; }
        if (carRend == null) return;
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main); // Create a plane that represents the main camera's view
        bool carInView = GeometryUtility.TestPlanesAABB(planes, carRend.bounds); // Check if the car is in the camera's view
        canvasGroup.alpha = carInView ? 1 : 0; // Set the alpha of the canvas group based on whether the car is in view
        this.transform.position = Camera.main.WorldToScreenPoint(target.position + Vector3.up);
        if (checkpointManager == null)
            checkpointManager = target.GetComponent<CheckpointManager>();

        Leaderboard.SetPosition(carReg, checkpointManager.lap, checkpointManager.checkPoint, checkpointManager.timeEntered);
        string position = Leaderboard.GetPosition(carReg);
            
        lapDisplay.text = position + "Place  | Lap " + checkpointManager.lap.ToString() + "/" + RaceMonitor.totalLaps.ToString();
    }
}
