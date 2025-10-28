using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.Loading;
using UnityEngine;

public class ItemScript : MonoBehaviourPunCallbacks
{
    public bool isCollected = false;
    public bool isThrowable = false;
    public bool isFreezeItem;
    public bool isSpeedBoostItem;
    public bool isDoubleScoreItem;
    public bool isThrowwed = false;

    public Transform owner;
    public string ownerName;

    private void Update()
    {
        if (PhotonNetwork.LocalPlayer.NickName == ownerName)
            if (isCollected && !isThrowwed)
                gameObject.GetComponent<Transform>().position = owner.position + new Vector3(0.5f, -0.5f);
    }

    public void itemThrowwed()
    {
        isThrowwed = true;
        gameObject.GetComponent<Transform>().localScale = new Vector2(2f, 2f);
        StartCoroutine(DestroyItemAfterTime());
    }

    public IEnumerator DestroyItemAfterTime()
    {
        yield return new WaitForSeconds(2f);
        gameObject.GetComponent<Transform>().localScale = new Vector2(6, 6);
        yield return new WaitForSeconds(0.65f);
        DestroyItem();
    }

    public void DestroyItem()
    {
        if (this != null && gameObject != null)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
    }

    [PunRPC]
    void RPC_DestroyItem(int viewID)
    {
        PhotonView pv = PhotonView.Find(viewID);
        if (pv != null)
        {
            PhotonNetwork.Destroy(pv.gameObject);
        }
    }

    [PunRPC]
    public void RPC_itemCollected(string itemOwner)
    {
        isCollected = true;
        gameObject.GetComponent<Transform>().localScale = new Vector2(0.35f, 0.35f);
        owner = GameObject.Find(itemOwner).GetComponent<Transform>();
        ownerName = itemOwner;
    }
}
