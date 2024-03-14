using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    // Singleton instance of the SpawnManager accessible from other scripts
    public static SpawnManager Instance { get; private set; }

    // Array of spawn points where players can spawn
    public Transform[] spawnPoints;

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        // Check if an instance of SpawnManager already exists
        if (Instance == null)
        {
            // If not, set this instance as the Singleton instance
            Instance = this;
        }
        else
        {
            // If an instance already exists, destroy this GameObject to ensure there's only one SpawnManager instance
            Destroy(gameObject);
        }
    }
}