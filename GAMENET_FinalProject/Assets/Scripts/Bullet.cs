using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Cannon : MonoBehaviourPunCallbacks
{
    public PhotonView targetPhotonView;
    public int damage = 15;

    // Start is called before the first frame update
    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log(collider.gameObject.name);

        if (collider.gameObject.CompareTag("Player") && !collider.gameObject.GetComponent<PhotonView>().IsMine)
        {
            PhotonView targetPhotonView = collider.gameObject.GetComponent<PhotonView>();
            targetPhotonView.RPC("TakeDamage", RpcTarget.AllBuffered, damage);

            Vector3 hitPoint = collider.ClosestPoint(transform.position);
            targetPhotonView.RPC("CreateExplosionEffects", RpcTarget.All, hitPoint);
        }
        Destroy(gameObject);
    }
}