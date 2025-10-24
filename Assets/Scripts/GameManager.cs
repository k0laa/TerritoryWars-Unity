using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public int playerTileColorIndex = -1;
    public TilemapManager tm;
    public GameObject selectedImg;
    public Transform[] buttons;

    private void Awake()
    {
        PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0, null);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log("Player Joined: " + newPlayer.NickName);
        tm.photonView.RPC("SyncAllTiles", newPlayer);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        int leftID = -1;
        Player[] players = FindObjectsOfType<Player>();

        foreach (var kvp in tm.TilemapValues)
        {
            bool found = false;
            foreach( var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                if (player.ActorNumber == kvp.Value.y)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                leftID = kvp.Value.y;
                break;
            }
        }

        tm.GetComponent<PhotonView>().RPC("ClearTileForLeftPlayer", RpcTarget.AllBuffered, leftID);

    }


    public void selectColor(int index)
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().tileIndex = index;
        Vector2.MoveTowards(selectedImg.transform.position, buttons[index].position, 1f);
    }
}
