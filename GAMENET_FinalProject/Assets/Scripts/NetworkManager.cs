using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{

    [Header("Login UI")]
    public GameObject LoginUIPanel;
    public InputField PlayerNameInput;

    [Header("Connecting Info Panel")]
    public GameObject ConnectingInfoUIPanel;

    [Header("Creating Room Info Panel")]
    public GameObject CreatingRoomInfoUIPanel;

    [Header("GameOptions  Panel")]
    public GameObject GameOptionsUIPanel;

    [Header("Create Room Panel")]
    public GameObject CreateRoomUIPanel;
    public InputField RoomNameInputField;

    [Header("Inside Room Panel")]
    public GameObject InsideRoomUIPanel;
    public Text RoomInfoText;
    public GameObject PlayerListPrefab;
    public GameObject PlayerListParent;
    public GameObject StartGameButton;
   
    [Header("Join Random Room Panel")]
    public GameObject JoinRandomRoomUIPanel;

    private Dictionary<int, GameObject> playerListGameObjects;
    #region Unity Methods
    // Start is called before the first frame update
    void Start()
    {
        ActivatePanel(LoginUIPanel.name);
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion

    #region UI Callback Methods
    public void OnLoginButtonClicked()
    {
        string playerName = PlayerNameInput.text;

        if (!string.IsNullOrEmpty(playerName))
        {
            ActivatePanel(ConnectingInfoUIPanel.name);

            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.LocalPlayer.NickName = playerName;
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        else
        {
            Debug.Log("PlayerName is invalid!");
            
        }
    }

    public void OnCancelButtonClicked()
    {
        ActivatePanel(GameOptionsUIPanel.name);
    }
    
    public void OnCreateRoomButtonClicked()
    {
        ActivatePanel(CreatingRoomInfoUIPanel.name);
        string roomName = RoomNameInputField.text;

        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "" + Random.Range(1000, 10000);
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 3;

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void OnJoinRandomRoomClicked()
    {
        ActivatePanel(JoinRandomRoomUIPanel.name);
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnStartGameButtonClicked()
    {
        PhotonNetwork.LoadLevel("GameScene");  
    }
    #endregion

    #region Photon Callbacks
    public override void OnConnected()
    {
        Debug.Log("Connected to Internet");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName+ " is connected to Photon");
        ActivatePanel(GameOptionsUIPanel.name);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom + " has been created!");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " has joined the " + PhotonNetwork.CurrentRoom.Name);
        Debug.Log("Player count: " + PhotonNetwork.CurrentRoom.PlayerCount);

        ActivatePanel(InsideRoomUIPanel.name);

        RoomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + "\nPlayer Count: " + PhotonNetwork.CurrentRoom.PlayerCount + " / " + 
            PhotonNetwork.CurrentRoom.MaxPlayers;

        if (playerListGameObjects == null)
        {
            playerListGameObjects = new Dictionary<int, GameObject>();
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject playerListItem = Instantiate(PlayerListPrefab);
            playerListItem.transform.SetParent(PlayerListParent.transform);
            playerListItem.transform.localScale = Vector3.one;

            playerListItem.GetComponent<PlayerListItemInitializer>().Initialize(player.ActorNumber, player.NickName);

            object isPlayerReady;
            if (player.CustomProperties.TryGetValue(Constants.PLAYER_READY, out isPlayerReady))
            {
                playerListItem.GetComponent<PlayerListItemInitializer>().SetPlayerReady((bool) isPlayerReady);
            }
            playerListGameObjects.Add(player.ActorNumber, playerListItem);
        }

        StartGameButton.SetActive(false);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        GameObject playerListItem = Instantiate(PlayerListPrefab);
        playerListItem.transform.SetParent(PlayerListParent.transform);
        playerListItem.transform.localScale = Vector3.one;

        playerListItem.GetComponent<PlayerListItemInitializer>().Initialize(newPlayer.ActorNumber, newPlayer.NickName);

        playerListGameObjects.Add(newPlayer.ActorNumber, playerListItem);

        RoomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + "\nPlayer Count: " + PhotonNetwork.CurrentRoom.PlayerCount + " / " + 
                PhotonNetwork.CurrentRoom.MaxPlayers;

        StartGameButton.SetActive(CheckAllPlayerReady());
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Destroy(playerListGameObjects[otherPlayer.ActorNumber].gameObject);
        playerListGameObjects.Remove(otherPlayer.ActorNumber);

        RoomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + "\nPlayer Count: " + PhotonNetwork.CurrentRoom.PlayerCount + " / " + 
                PhotonNetwork.CurrentRoom.MaxPlayers;
    }

    public override void OnLeftRoom()
    {
        ActivatePanel(GameOptionsUIPanel.name);

        foreach (GameObject playerListGameObjects in playerListGameObjects.Values)
        {
            Destroy(playerListGameObjects);
        }

        playerListGameObjects.Clear();
        playerListGameObjects = null;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message);

        string roomName = RoomNameInputField.text;

        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "" + Random.Range(1000, 10000);
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 3;

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        GameObject playerListGameObject;
        if (playerListGameObjects.TryGetValue(targetPlayer.ActorNumber, out playerListGameObject))
        {   
            object isPlayerReady;
            if (changedProps.TryGetValue(Constants.PLAYER_READY, out isPlayerReady))
            {
                playerListGameObject.GetComponent<PlayerListItemInitializer>().SetPlayerReady((bool) isPlayerReady);
            }
        }

        StartGameButton.SetActive(CheckAllPlayerReady());
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            StartGameButton.SetActive(CheckAllPlayerReady());
        }
    }
    #endregion

    #region Public Methods
    public void ActivatePanel(string panelNameToBeActivated)
    {
        LoginUIPanel.SetActive(LoginUIPanel.name.Equals(panelNameToBeActivated));
        ConnectingInfoUIPanel.SetActive(ConnectingInfoUIPanel.name.Equals(panelNameToBeActivated));
        CreatingRoomInfoUIPanel.SetActive(CreatingRoomInfoUIPanel.name.Equals(panelNameToBeActivated));
        CreateRoomUIPanel.SetActive(CreateRoomUIPanel.name.Equals(panelNameToBeActivated));
        GameOptionsUIPanel.SetActive(GameOptionsUIPanel.name.Equals(panelNameToBeActivated));
        JoinRandomRoomUIPanel.SetActive(JoinRandomRoomUIPanel.name.Equals(panelNameToBeActivated));
        InsideRoomUIPanel.SetActive(InsideRoomUIPanel.name.Equals(panelNameToBeActivated));
    }
    #endregion

    #region Private Methods
    private bool CheckAllPlayerReady()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return false;
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            object isPlayerReady;

            if (p.CustomProperties.TryGetValue(Constants.PLAYER_READY, out isPlayerReady))
            {
                if (!(bool) isPlayerReady)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    #endregion
}