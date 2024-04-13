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
    public bool isControlEnabled;
    public GameObject shipWreckPrefab;

    // List of currently active players in the room
    List<Player> activePlayers = PhotonNetwork.PlayerList.ToList();

    [Header("Cannon")]
    public GameObject cannonPrefab; 
    public GameObject cannonExplosionEffect; 
    public Transform firePoint;
    public float fireSpeed = 15f; 
    public float fireRate = 1f; 
    private float nextFireTime = 0f;

    [Header("HP Related Stuff")]
    public float health = 100f;
    public float currentHealth;
    public Image healthbar;
    
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
            GameObject eliminatedUiText = GameManager.instance.eliminatedTextUi;

            eliminatedUiText.SetActive(true);
            eliminatedUiText.GetComponent<TMP_Text>().text = nickNameOfEliminatedPlayer + " has been eliminated!";
            eliminatedUiText.GetComponent<TMP_Text>().color = Color.red;
        }
        else if (photonEvent.Code == (byte)RaiseEventsCode.WinnerDeclaredEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;

            string winnerNickName = (string)data[0];

            // Display the last man standing as a winner
            GameObject winnerUiText = GameManager.instance.winnerTextUi;

            winnerUiText.SetActive(true);
            winnerUiText.GetComponent<TMP_Text>().text = winnerNickName + " wins!";
            winnerUiText.GetComponent<TMP_Text>().color = Color.green;
        }
    }

    void Start()
    {
        health = currentHealth;
        healthbar.fillAmount = currentHealth / health;
        isControlEnabled = false;
    }

    void Update()
    {
        if (isControlEnabled)
        {
            if (Input.GetMouseButtonDown(0)) 
            {
                photonView.RPC("Fire", RpcTarget.All, firePoint.position);
            }
        }
    }

    [PunRPC]
    public void Fire(Vector3 firePointPosition)
    {
        if (Time.time > nextFireTime)
        {
            GameObject cannon = Instantiate(cannonPrefab, firePointPosition, transform.rotation);
            Rigidbody cannonRigidbody = cannon.GetComponent<Rigidbody>();

            cannonRigidbody.velocity = transform.forward * fireSpeed;

            cannon.GetComponent<Cannon>().targetPhotonView = photonView;

            StartCoroutine(CreateExplosionAndDestroy(cannon, 5.0f)); 
        }
        nextFireTime = Time.time + fireRate;
    }

    private IEnumerator CreateExplosionAndDestroy(GameObject cannon, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (cannon != null)
        {
            Vector3 explosionPosition = cannon.transform.position;
            photonView.RPC("CreateExplosionEffects", RpcTarget.All, explosionPosition);

            Destroy(cannon);
        }
    }

    [PunRPC]
    public void CreateExplosionEffects(Vector3 position)
    {
        GameObject explosionEffectGameObject = Instantiate(cannonExplosionEffect, position, Quaternion.identity);
        Destroy(explosionEffectGameObject, 0.5f);
    }

    [PunRPC]
    public void TakeDamage(int hitDamage, PhotonMessageInfo info)
    {
        this.currentHealth -= hitDamage;
        this.healthbar.fillAmount = currentHealth / health;

        if (currentHealth <= 0)
        {
            Die();

            // who-killed-who functionality
            GameObject killedUiText = GameManager.instance.killedTextUi;
            killedUiText.SetActive(true);

            killedUiText.GetComponent<TMP_Text>().text = info.Sender.NickName + " killed " + info.photonView.Owner.NickName;
        }
        Debug.Log(currentHealth);
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

        Instantiate(shipWreckPrefab, transform.position, transform.rotation * Quaternion.Euler(0, 180, 0));
        StartCoroutine(DestroyAfterDelay(0.1f));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
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
