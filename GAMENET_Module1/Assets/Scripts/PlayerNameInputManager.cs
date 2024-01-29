using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerNameInputManager : MonoBehaviour
{
    // Set the player's name in the Photon Network
    public void SetPlayerName(string playerName)
    {
        // Check if the player name is empty or null
        if (string.IsNullOrEmpty(playerName))
        {
            // Log a warning if the player name is empty
            Debug.LogWarning("Player name is empty!");
            return;
        }

        // Set the player's nickname in the Photon Network
        PhotonNetwork.NickName = playerName;
    }
}
