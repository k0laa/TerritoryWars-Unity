using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    public TilemapManager tm;

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
    }
}
