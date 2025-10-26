using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviourPunCallbacks
{
    public TMP_Text timeText;
    public GameManager gm;
    public ItemManager im;

    public float time = 60f;
    float lastItemSpawnTime = 60f;

    [PunRPC]
    public void StartTime(int time)
    {
        this.time = time;
        lastItemSpawnTime = time;
        StartCoroutine(StartTimeCoroutine());
    }

    IEnumerator StartTimeCoroutine()
    {
        while (time > 0)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("RPC_updateTimeText", RpcTarget.All, time);
                if (lastItemSpawnTime - time > 5)
                {
                    im.RandomItemInstantiate();
                    lastItemSpawnTime = time;
                }
            }
            yield return new WaitForSeconds(0.1f);
            time -= 0.1f;
        }
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_updateTimeText", RpcTarget.All, 0f);
            gm.photonView.RPC("RPC_EndGame", RpcTarget.All);
            gm.photonView.RPC("RPC_setReady", RpcTarget.All, false);
        }
    }

    [PunRPC]
    void RPC_updateTimeText(float time)
    {
        timeText.text = time.ToString("F1");
    }

}
