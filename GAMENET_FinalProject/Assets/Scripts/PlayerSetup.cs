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
        
        GetComponent<VehicleMovement>().enabled = photonView.IsMine;
        GetComponent<Shooting>().enabled = photonView.IsMine;
        camera.enabled = photonView.IsMine;

        // Display the player names above the vehicles
        playerNameText.text = photonView.Owner.NickName;
    }
}
