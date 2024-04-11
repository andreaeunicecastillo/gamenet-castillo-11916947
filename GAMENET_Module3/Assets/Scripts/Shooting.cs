using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class Shooting : MonoBehaviourPunCallbacks
{
    public string weaponType;
    public bool isControlEnabled;

    // List of currently active players in the room
    List<Player> activePlayers = PhotonNetwork.PlayerList.ToList();

    [Header("Laser")]
    public Camera camera;
    public GameObject hitEffectPrefab;

    [Header("Missile")]
    public GameObject missilePrefab; 
    public float missileSpeed = 10.0f;
    public Transform firePoint;

    // HP
    [Header("HP Related Stuff")]
    public float health = 100;
    
    // Enum to define custom RaiseEvent codes for player elimination and winner declaration
    public enum RaiseEventsCode
    {
        PlayerEliminatedEventCode = 1,
        WinnerDeclaredEventCode = 2
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    // Handles received events from the network
    void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == (byte)RaiseEventsCode.PlayerEliminatedEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;

            string nickNameOfEliminatedPlayer = (string)data[0];
            
            // When a player dies, an event should be raised saying that he/she was eliminated
            GameObject eliminatedUiText = DeathRaceGameManager.instance.eliminatedTextUi;

            eliminatedUiText.SetActive(true);
            eliminatedUiText.GetComponent<TMP_Text>().text = nickNameOfEliminatedPlayer + " has been eliminated!";
            eliminatedUiText.GetComponent<TMP_Text>().color = Color.red;
        }
        else if (photonEvent.Code == (byte)RaiseEventsCode.WinnerDeclaredEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;

            string winnerNickName = (string)data[0];

            // Display the last man standing as a winner
            GameObject winnerUiText = DeathRaceGameManager.instance.winnerTextUi;

            winnerUiText.SetActive(true);
            winnerUiText.GetComponent<TMP_Text>().text = winnerNickName + " is the last man standing and is the winner!";
            winnerUiText.GetComponent<TMP_Text>().color = Color.green;
        }
    }

    void Start()
    {
        isControlEnabled = false;
    }

    void Update()
    {
        if (isControlEnabled)
        {
            if (Input.GetMouseButtonDown(0)) 
            {
                if (weaponType == "Laser")
                {
                    FireLaser();
                }
                else if (weaponType == "Missile")
                {
                    photonView.RPC("FireMissile", RpcTarget.All, firePoint.position);
                }
            }
        }
    }

    // Laser - raycast
    public void FireLaser()
    {
        RaycastHit hit;
        Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        if (Physics.Raycast(ray, out hit, 200))
        {
            Debug.Log(hit.collider.gameObject.name);

            photonView.RPC("CreateHitEffects", RpcTarget.All, hit.point);

            if (hit.collider.gameObject.CompareTag("Player") && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine)
            {
                PhotonView targetPhotonView = hit.collider.gameObject.GetComponent<PhotonView>();
                targetPhotonView.RPC("TakeDamage", RpcTarget.AllBuffered, 25);
            }
        }
    }

    [PunRPC]
    public void CreateHitEffects(Vector3 position)
    {
        GameObject hitEffectGameObject = Instantiate(hitEffectPrefab, position, Quaternion.identity);
        Destroy(hitEffectGameObject, 0.5f);
    }

    // Projectile - rigidbody, spawned object
    [PunRPC]
    public void FireMissile(Vector3 firePointPosition)
    {
        GameObject missile = Instantiate(missilePrefab, firePointPosition, transform.rotation);
        Rigidbody missileRigidbody = missile.GetComponent<Rigidbody>();

        missileRigidbody.velocity = transform.forward * missileSpeed;

        missile.GetComponent<Bullet>().targetPhotonView = photonView;
        Destroy(missile, 1.0f);
    }

    [PunRPC]
    public void TakeDamage(int damage, PhotonMessageInfo info)
    {
        this.health -= damage;

        if (health <= 0)
        {
            Die();

            // who-killed-who functionality
            GameObject killedUiText = DeathRaceGameManager.instance.killedTextUi;
            killedUiText.SetActive(true);

            killedUiText.GetComponent<TMP_Text>().text = info.Sender.NickName + " killed " + info.photonView.Owner.NickName;
        }
    }

    public void Die()
    {
        GetComponent<PlayerSetup>().camera.transform.parent = null;
        isControlEnabled = false;
        activePlayers.Remove(PhotonNetwork.LocalPlayer);
        CheckForWin();

        string nickName = photonView.Owner.NickName;
        // event data
        object[] data = new object[] {nickName};

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All,
            CachingOption = EventCaching.AddToRoomCache
        };

        SendOptions sendOption = new SendOptions{
            Reliability = false
        };

        PhotonNetwork.RaiseEvent((byte) RaiseEventsCode.PlayerEliminatedEventCode, data, raiseEventOptions, sendOption);
    }

    public void CheckForWin()
    {
        // Check if there is only one player left
        if (activePlayers.Count == 1)
        {
            GetComponent<PlayerSetup>().camera.transform.parent = null;
            isControlEnabled = false;
            string winnerNickName = activePlayers[0].NickName;

            // event data
            object[] data = new object[] {winnerNickName};

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All,
                CachingOption = EventCaching.AddToRoomCache
            };

            SendOptions sendOption = new SendOptions{
                Reliability = false
            };

            PhotonNetwork.RaiseEvent((byte) RaiseEventsCode.WinnerDeclaredEventCode, data, raiseEventOptions, sendOption);
        }
    }
}
