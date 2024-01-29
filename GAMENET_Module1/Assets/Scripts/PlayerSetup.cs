using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    // Reference to the player's camera
    [SerializeField]
    private GameObject camera;

    // Reference to the TextMeshProUGUI component for displaying the player's name
    [SerializeField]
    TextMeshProUGUI playerNameText;

    // Start is called before the first frame update
    void Start()
    {
        // Check if this object belongs to the local player
        if (photonView.IsMine)
        {
            // Enable MovementController script for local player
            transform.GetComponent<MovementController>().enabled = true;
            
            // Enable the camera for the local player
            camera.GetComponent<Camera>().enabled = true;
        }
        else 
        {
            // Disable MovementController script for remote players
            transform.GetComponent<MovementController>().enabled = false;

            // Disable the camera for remote players
            camera.GetComponent<Camera>().enabled = false;
        }

        // Set the player's name displayed using TextMeshProUGUI
        playerNameText.text = photonView.Owner.NickName;
    }
}
