using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Photon.Realtime;
using Photon.Pun;

public class RaceMonitor : MonoBehaviourPunCallbacks
{

    public GameObject[] countdownItems;
    CheckpointManager[] carsCPM;
    public static bool racing = false;
    public static int totalLaps = 1;
    public GameObject gameOverPanel;
    public GameObject pauseMenu;
    public GameObject HUD;
    public GameObject[] carPrefabs;
    public Transform[] spawnPoints;

    public GameObject startRaceButton;

    int playerCar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (GameObject g in countdownItems)
        {
            g.SetActive(false);
        }
        gameOverPanel.SetActive(false);
        pauseMenu.SetActive(false);
        startRaceButton.SetActive(true);

        playerCar = PlayerPrefs.GetInt("PlayerCar");
        int randStartPos = Random.Range(0, spawnPoints.Length);
        Vector3 startPos = spawnPoints[randStartPos].position;
        Quaternion startRot = spawnPoints[randStartPos].rotation;
        GameObject pcar = null;

        if (PhotonNetwork.IsConnected)
        {
            startPos = spawnPoints[PhotonNetwork.CurrentRoom.PlayerCount - 1].position;
            startRot = spawnPoints[PhotonNetwork.CurrentRoom.PlayerCount - 1].rotation;

            if (NetworkedPlayer.LocalPlayerInstance == null)
            {
                pcar = PhotonNetwork.Instantiate(carPrefabs[playerCar].name, startPos, startRot, 0);
            }

            if (PhotonNetwork.IsMasterClient)
            {
                startRaceButton.SetActive(true);
            }
        }
        else
        {
            pcar = Instantiate(carPrefabs[playerCar]);
            pcar.transform.position = startPos;
            pcar.transform.rotation = startRot;
            foreach (Transform t in spawnPoints)
            {
                if (t == spawnPoints[randStartPos]) continue; // Skip the spawn point of the player car
                GameObject car = Instantiate(carPrefabs[Random.Range(0, carPrefabs.Length)]); //Spawn AI cars in all the other spawn points
                car.transform.position = t.position;
                car.transform.rotation = t.rotation;
            }
            StartRace();
        }

        SmoothFollow.playerCar = pcar.gameObject.GetComponent<Drive>().rb.transform;
        pcar.GetComponent<AiController>().enabled = false;
        pcar.GetComponent<Drive>().enabled = true;
        pcar.GetComponent<PlayerController>().enabled = true;


    }

    public void StartRace()
    {
        StartCoroutine(PlayCountdown());
        startRaceButton.SetActive(false);

        GameObject[] cars = GameObject.FindGameObjectsWithTag("car");
        carsCPM = new CheckpointManager[cars.Length];
        for (int i = 0; i < cars.Length; i++)
        {
            carsCPM[i] = cars[i].GetComponent<CheckpointManager>();
        }
    }

    IEnumerator PlayCountdown()
    {
        yield return new WaitForSeconds(2);

        foreach (GameObject g in countdownItems)
        {
            g.SetActive(true);
            yield return new WaitForSeconds(1);
            g.SetActive(false);
        }
        racing = true;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        racing = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        racing = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenu.activeSelf)
            {
                Time.timeScale = 1f;
                pauseMenu.SetActive(false);
            }
            else
            {
                Time.timeScale = 0f;
                pauseMenu.SetActive(true);
            }
        }
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if (!racing) return;
        int finishedCount = 0;
        foreach (CheckpointManager cpm in carsCPM)
        {
            if (cpm.lap == totalLaps + 1)
            {
                finishedCount++;
            }

            if (finishedCount == carsCPM.Length)
            {
                gameOverPanel.SetActive(true);
                HUD.SetActive(false);
            }
        }
    }
}
