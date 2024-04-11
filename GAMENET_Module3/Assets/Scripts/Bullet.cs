using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Bullet : MonoBehaviourPunCallbacks
{
    public PhotonView targetPhotonView;

    // Projectile - rigidbody, spawned object
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log(collider.gameObject.name);
        if (collider.gameObject.CompareTag("Player") && !collider.gameObject.GetComponent<PhotonView>().IsMine)
        {
            PhotonView targetPhotonView = collider.gameObject.GetComponent<PhotonView>();
            targetPhotonView.RPC("TakeDamage", RpcTarget.AllBuffered, 25);
        }
        Destroy(gameObject);
    }
}
