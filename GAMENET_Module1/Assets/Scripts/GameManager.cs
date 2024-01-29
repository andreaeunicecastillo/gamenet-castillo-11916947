using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    // Reference to the player prefab to be instantiated in the network
    [SerializeField]
    private GameObject playerPrefab;

    // Singleton pattern instance
    public static GameManager instance;

    private void Awake()
    {
        // Ensure only one instance of GameManager exists
        if (instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Check if connected to Photon network
        if (PhotonNetwork.IsConnected)
        {
            // Check if the playerPrefab is assigned
            if (playerPrefab != null)
            {
                // Delayed spawning of player after 3 seconds
                StartCoroutine(DelayedPlayerSpawn());
            }
        }
    }

    // Coroutine for delayed player spawning
    IEnumerator DelayedPlayerSpawn()
    {
        yield return new WaitForSeconds(3);

        // Randomly choose spawn points within specified range
        int xRandomPoint = Random.Range(-20, 20);
        int zRandomPoint = Random.Range(-20, 20);

        // Instantiate playerPrefab at a random position
        PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(xRandomPoint, 0, zRandomPoint), Quaternion.identity);
    }

    // Called when the local player successfully joins a room
    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.NickName + " has joined the room!");
    }

    // Called when a new player enters the room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " has joined the Room " + PhotonNetwork.CurrentRoom.Name);
        Debug.Log("Room has now " + PhotonNetwork.CurrentRoom.PlayerCount + "/20 players.");
    }

    // Called when the local player leaves the room
    public override void OnLeftRoom()
    {
        // Transition to the GameLauncherScene
        SceneManager.LoadScene("GameLauncherScene");
    }

    // Method to leave the current Photon room
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
}
