using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class TakingDamage : MonoBehaviourPunCallbacks
{
    // Reference to the health bar UI element
    [SerializeField]
    Image healthBar;

    // Initial health value
    private float startHealth = 100;

    // Current health value
    public float health;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize health and set the health bar fill amount
        health = startHealth;
        healthBar.fillAmount = health / startHealth;
    }

    // Remote Procedure Call (RPC) method to handle taking damage
    [PunRPC]
    public void TakeDamage(int damage)
    {
        // Reduce health by the specified damage amount
        health -= damage;

        // Log the current health to the console
        Debug.Log(health);

        // Update the health bar fill amount
        healthBar.fillAmount = health / startHealth;

        // Check if health is less than 0 and call the Die method
        if (health < 0)
        {
            Die();
        }
    }

    // Method to handle the player's death
    private void Die()
    {
        // Check if the current player is the local player
        if (photonView.IsMine)
        {
            // If it is, leave the room using the GameManager instance
            GameManager.instance.LeaveRoom();
        }
    }
}
