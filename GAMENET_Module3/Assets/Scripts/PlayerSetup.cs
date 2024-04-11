using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public Camera camera;

    public TextMeshProUGUI playerNameText;

    // Start is called before the first frame update
    void Start()
    {
        this.camera = transform.Find("Camera").GetComponent<Camera>();
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("rc"))
        {
            // If the room property value is "rc", enable components for the local player only
            GetComponent<VehicleMovement>().enabled = photonView.IsMine;
            GetComponent<LapController>().enabled = photonView.IsMine;
            camera.enabled = photonView.IsMine;

            // Display the player names above the vehicles
            playerNameText.text = photonView.Owner.NickName;
        }
        else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("dr"))
        {
            // If the room property value is "dr", enable components for the local player only
            GetComponent<VehicleMovement>().enabled = photonView.IsMine;
            GetComponent<Shooting>().enabled = photonView.IsMine;
            camera.enabled = photonView.IsMine;

            // Display the player names above the vehicles
            playerNameText.text = photonView.Owner.NickName;
        }
    }
}
