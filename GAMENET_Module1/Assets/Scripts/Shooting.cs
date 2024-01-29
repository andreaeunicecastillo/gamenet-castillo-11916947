using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Shooting : MonoBehaviour
{
    // Reference to the first-person camera
    [SerializeField]
    Camera fpsCamera;

    // Rate at which the player can fire
    [SerializeField]
    public float fireRate = 0.1f;

    // Timer to control the fire rate
    private float fireTimer = 0;

    // Update is called once per frame
    void Update()
    {
        // Increment the fire timer
        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }

        // Check if the fire button is pressed and the fire timer allows firing
        if (Input.GetButton("Fire1") && fireTimer > fireRate)
        {
            // Reset the fire timer
            fireTimer = 0.0f; 

            // Create a ray from the center of the camera
            Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            RaycastHit hit;

            // Check if the ray hits an object within 100 units
            if (Physics.Raycast(ray, out hit, 100))
            {
                // Log the name of the hit object to the console
                Debug.Log(hit.collider.gameObject.name);

                // Check if the hit object has the "Player" tag and is not the local player
                if (hit.collider.gameObject.CompareTag("Player") && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine)
                {
                    // Call the "TakeDamage" method on the hit player using Photon RPC
                    hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 10);
                }
            }
        }
    }
}
