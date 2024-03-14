using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class Shooting : MonoBehaviourPunCallbacks
{
    public Camera camera;
    public GameObject hitEffectPrefab;

    [Header("HP Related Stuff")]
    public float startHealth = 100;
    private float health;
    public Image healthbar;

    [Header("Kill Related Stuff")]
    public int killCount = 0;
    private static Queue<string> killFeedMessages = new Queue<string>();

    private Animator animator;
    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        health = startHealth;
        healthbar.fillAmount = health / startHealth;
        animator = this.GetComponent<Animator>();

        // Find the GameManager object and get its GameManager script
        GameObject gameManagerObject = GameObject.Find("GameManager");
        gameManager = gameManagerObject.GetComponent<GameManager>();
    }

    public void Fire()
    {
        RaycastHit hit;
        Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        if (Physics.Raycast(ray, out hit, 200))
        {
            Debug.Log(hit.collider.gameObject.name);

            photonView.RPC("CreateHitEffects", RpcTarget.All, hit.point);

            if (hit.collider.gameObject.CompareTag("Player") && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine)
            {
                // Deal damage to the player hit
                PhotonView targetPhotonView = hit.collider.gameObject.GetComponent<PhotonView>();
                targetPhotonView.RPC("TakeDamage", RpcTarget.AllBuffered, 25);

                if (targetPhotonView.GetComponent<Shooting>().health <= 0)
                {
                    // If the player dies, increment the kill count
                    photonView.RPC("IncrementKillCount", RpcTarget.AllBuffered);
                }
            }
        }
    }

    [PunRPC]
    public void TakeDamage(int damage, PhotonMessageInfo info)
    {
        this.health -= damage;
        this.healthbar.fillAmount = health / startHealth;

        // Check if the player's health has dropped to or below zero
        if (health <= 0)
        {
            // If so, trigger the Die() method to handle the player's death
            Die();
            // Construct a kill feed message indicating the player who inflicted the damage and the player who was killed
            string killMessage = info.Sender.NickName + " killed " + info.photonView.Owner.NickName;
            // Add the kill feed message to the kill feed UI
            AddKillFeedMessage(killMessage);
        }
    }

    // Function to add kill feed messages
    private static void AddKillFeedMessage(string message)
    {
        // Enqueue the new kill feed message
        killFeedMessages.Enqueue(message);

        // Check if the queue length exceeds the maximum limit
        if (killFeedMessages.Count > 3)
        {
            // If so, dequeue the oldest message(s) until the limit is satisfied
            killFeedMessages.Dequeue();
        }

        // Find the UI element responsible for displaying kill feed messages
        GameObject killedText = GameObject.Find("Killed Text");
        TMP_Text killedTextComponent = killedText.GetComponent<TMP_Text>();

        // Update the UI text to display the current kill feed messages
        killedTextComponent.text = string.Join("\n", killFeedMessages);
    }

    // Function to reset kill feed messages
    private static void ResetKillFeedMessages()
    {
        // Clear all messages from the kill feed messages queue
        killFeedMessages.Clear();
    
        // Find the UI element responsible for displaying kill feed messages
        GameObject killedText = GameObject.Find("Killed Text");
        TMP_Text killedTextComponent = killedText.GetComponent<TMP_Text>();
    
        // Clear the text of the UI element, effectively clearing any previously displayed kill feed messages
        killedTextComponent.text = "";
    }

    [PunRPC]
    public void CreateHitEffects(Vector3 position)
    {
        GameObject hitEffectGameObject = Instantiate(hitEffectPrefab, position, Quaternion.identity);
        Destroy(hitEffectGameObject, 0.2f);
    }

    public void Die()
    {
        if (photonView.IsMine)
        {
            animator.SetBool("isDead", true);
            StartCoroutine(RespawnCountdown());
        }
    }

    IEnumerator RespawnCountdown()
    {
        // Check if the game has not ended yet
        if (!gameManager.GameHasEnded())
        {
            GameObject respawnText = GameObject.Find("Respawn Text");
            float respawnTime = 5.0f;

            while (respawnTime > 0)
            {
                yield return new WaitForSeconds(1.0f);
                respawnTime--;

                // Check if the game has ended while counting down
                if (gameManager.GameHasEnded())
                {
                     // If the game has ended, log a message and stop the respawn countdown
                    Debug.Log("Game has ended. Respawn disabled.");
                    yield break;  // Exit the coroutine
                }

                transform.GetComponent<PlayerMovementController>().enabled = false;
                respawnText.GetComponent<TMP_Text>().text = "You are killed. Respawning in " + respawnTime.ToString(".00");
            }

            animator.SetBool("isDead", false);
            respawnText.GetComponent<TMP_Text>().text = "";

            Transform spawnPoint = SpawnManager.Instance.spawnPoints[Random.Range(0, SpawnManager.Instance.spawnPoints.Length)];

            this.transform.position = spawnPoint.position;
            transform.GetComponent<PlayerMovementController>().enabled = true;

            photonView.RPC("RegainHealth", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void RegainHealth()
    {
        health = 100;
        healthbar.fillAmount = health / startHealth;
    }

    // Function to increment kill count
    [PunRPC]
    public void IncrementKillCount()
    {
        // Increment the kill count of the player
        killCount++;

        // Check if the player has reached the win condition after increasing the kill count
        CheckForWin();
    }

    // Function to check for win condition
    public void CheckForWin()
    {
        // Check if the kill count is equal to or greater than 10
        if (killCount >= 10) 
        {
            // Set the game to ended in the GameManager
            gameManager.SetGameEnded(true);

            // Log a message in the console indicating the winning player's name
            Debug.Log(photonView.Owner.NickName + " wins!");

            // Find the "Winner Text" UI element and update its text to display the winning player's name
            GameObject winnerText = GameObject.Find("Winner Text");
            winnerText.GetComponent<TMP_Text>().text = photonView.Owner.NickName + " wins!";

            // Start the EndGame coroutine to handle the end of the game
            StartCoroutine(EndGame());
        }
    }

    // Coroutine to end the game
    IEnumerator EndGame()
    {
        // Wait for a specified duration before continuing execution
        yield return new WaitForSeconds(3.0f);

        // After the delay, call the ReturnAllToLobby method over the network for all players
        photonView.RPC("ReturnAllToLobby", RpcTarget.AllBuffered);
    }

    // Function to return all players to lobby
    [PunRPC]
    public void ReturnAllToLobby()
    {
        // Call the ReturnToLobby method of the GameManager to return all players to the lobby
        gameManager.ReturnToLobby();
    
        // Reset the kill feed messages queue to prepare for the next game
        ResetKillFeedMessages();
    }
}