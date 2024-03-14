using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab; // Reference to the player prefab
    private bool gameHasEnded = false; // Flag to track whether the game has ended

    // Start is called before the first frame update
    void Start()
    {
        // Check if Photon is connected and ready
        if (PhotonNetwork.IsConnectedAndReady)
        {
            // Select a random spawn point from the SpawnManager and instantiate the player
            Transform spawnPoint = SpawnManager.Instance.spawnPoints[Random.Range(0, SpawnManager.Instance.spawnPoints.Length)];
            PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, Quaternion.identity);
        }
    }

    // Function to set the game ended flag
    public void SetGameEnded(bool value)
    {
        gameHasEnded = value;
    }

    // Function to check if the game has ended
    public bool GameHasEnded()
    {
        return gameHasEnded;
    }

    // Function to return to the lobby
    public void ReturnToLobby()
    {
        StartCoroutine(LeaveRoomAndLoadLobby());
    }

    // Coroutine to leave the room and load the lobby scene
    public IEnumerator LeaveRoomAndLoadLobby()
    {
        // Leave the Photon room
        PhotonNetwork.LeaveRoom();

        // Wait for a short duration before loading the lobby scene
        yield return new WaitForSeconds(0.2f);

        // Disable automatic scene synchronization and load the lobby scene
        PhotonNetwork.AutomaticallySyncScene = false;
        SceneManager.LoadScene("LobbyScene");
    }
}