using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

using Photon.Realtime;
using Photon.Pun;


public class LaunchManager : MonoBehaviourPunCallbacks
{
    byte maxPlayersPerRoom = 4;
    bool isConnecting;

    public TextMeshProUGUI feedbackText;
    string gameVersion = "1";
    public TMP_InputField playerNameInput;

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        if(PlayerPrefs.HasKey("PlayerName"))
        {
            playerNameInput.text = PlayerPrefs.GetString("PlayerName");
        }
    }

    public void ConnectNetwork() //Connect to photon network and join a room
    {
        feedbackText.text = "."; //Feedback from photon is added to the log field 
        isConnecting = true;       
        PhotonNetwork.NickName = playerNameInput.text;
        if(PhotonNetwork.IsConnected)
        {
            feedbackText.text += "\nJoinging Room...";
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            feedbackText.text += "\nConnecting...";
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }   
    }

    public void SetName(string name)
    {
        PlayerPrefs.SetString("PlayerName", name);
    }

    public void StartSingle()
    {
        SceneManager.LoadScene("NurburgringGP");
    }
    
    public void QuitGame()
    {
        Application.Quit();        
    }

    //Network Callbacks
    public override void OnConnectedToMaster() 
    {
        
        if(isConnecting)
        {
            feedbackText.text += "\nOnConnectedToMaster: Joining Room...";
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        feedbackText.text += "\nOnJoinRoomFailed: No Room Found. Creating New Room...";
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = this.maxPlayersPerRoom });
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        feedbackText.text += "\nOnDisconnected: " + cause.ToString();
        isConnecting = false;
    }

    public override void OnJoinedRoom() //Called when player joins a room
    {
        feedbackText.text += "\nOnJoinedRoom: " + PhotonNetwork.CurrentRoom.Name;
        feedbackText.text += "\nPlayers in Room: " + PhotonNetwork.CurrentRoom.PlayerCount.ToString();
        feedbackText.text += "\nLoading Level...";
        SceneManager.LoadScene("NurburgringGP");
    }
}
