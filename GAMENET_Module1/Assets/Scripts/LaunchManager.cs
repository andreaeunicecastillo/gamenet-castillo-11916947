using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LaunchManager : MonoBehaviourPunCallbacks
{
    // Reference to UI panels
    public GameObject EnterGamePanel;
    public GameObject ConnectionStatusPanel;
    public GameObject LobbyPanel;

    // Start is called before the first frame update
    void Start()
    {
        // Set initial UI states
        EnterGamePanel.SetActive(true);
        ConnectionStatusPanel.SetActive(false);
        LobbyPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Enable automatic scene synchronization with Photon
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Called when connected to the Photon master server
    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.NickName + " connected to Photon Servers");
        // Update UI panels upon successful connection
        ConnectionStatusPanel.SetActive(false);
        LobbyPanel.SetActive(true);
    }

    // Called when connected to the internet
    public override void OnConnected()
    {
        Debug.Log("Connected to Internet");
    }

    // Called when attempting to join a random room fails
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning(message);
        // Create and join a new room upon failure
        CreateAndJoinRoom();
    }

    // Method to connect to the Photon server
    public void ConnectToPhotonServer()
    {
        if (!PhotonNetwork.IsConnected)
        {
            // Connect to Photon using settings
            PhotonNetwork.ConnectUsingSettings();
            // Update UI panels during the connection process
            ConnectionStatusPanel.SetActive(true);
            EnterGamePanel.SetActive(false);
        }
    }

    // Method to join a random room
    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    // Method to create and join a room
    private void CreateAndJoinRoom()
    {
        string randomRoomName = "Room " + Random.Range(0, 10000);

        // Set room options
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 20;

        // Create and join the room
        PhotonNetwork.CreateRoom(randomRoomName, roomOptions);
    }

    // Called when the local player successfully joins a room
    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.NickName + " has entered " + PhotonNetwork.CurrentRoom.Name);
        // Load the game scene after joining a room
        PhotonNetwork.LoadLevel("GameScene");
    }

    // Called when a new player enters the room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " has entered room " + PhotonNetwork.CurrentRoom.Name + ". Room has now " +
        PhotonNetwork.CurrentRoom.PlayerCount + " players.");
    }
}