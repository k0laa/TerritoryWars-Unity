using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using System.Linq;

public class ScoreManager : MonoBehaviourPunCallbacks
{
    public TMP_Text[] nameTexts;
    public TMP_Text[] scoreTexts;

    private Dictionary<int, string> players = new Dictionary<int, string>();

    TilemapManager tm;

    void Start()
    {
        tm = GameObject.Find("Tilemap Manager").GetComponent<TilemapManager>();
        StartCoroutine(UpdateScoreboardRoutine());
    }

    IEnumerator UpdateScoreboardRoutine()
    {
        while (true)
        {
            photonView.RPC("RPC_UpdateScoreboard", RpcTarget.AllBuffered);
            yield return new WaitForSeconds(0.5f);
        }
    }

    [PunRPC]
    public void RPC_UpdateScoreboard()
    {
        // pwId -> count
        Dictionary<int, int> counts = new Dictionary<int, int>();
        foreach (var kv in tm.TilemapValues)
        {
            Vector2Int val = kv.Value;
            int pwId = val.y; // UpdateTilemapValue kullanýmý nedeniyle .y büyük ihtimalle player id
            if (counts.ContainsKey(pwId))
                counts[pwId]++;
            else
                counts[pwId] = 1;
        }

        // Sýrala ve en büyük 3'ü al
        var top = counts.OrderByDescending(kv => kv.Value).Take(3).ToList();

        // Doldur
        for (int i = 0; i < 3 ; i++)
        {
            if (i >= top.Count)
            {
                nameTexts[i].text = "";
                scoreTexts[i].text = "";
                continue;
            }
            int pwId = top[i].Key;
            int count = top[i].Value;
            // players sözlüðünden pwId'ye karþýlýk gelen nickname bul
            string name = "Unknown";
            if (players.ContainsKey(pwId))
                name = players[pwId];
            nameTexts[i].text = name;
            scoreTexts[i].text = count.ToString();
        }
    }

    [PunRPC]
    public void RPC_addPlayer(string playerName, int playerTileIndex)
    {
        if (!players.ContainsKey(playerTileIndex))
            players.Add(playerTileIndex, playerName);
    }

    [PunRPC]
    public void RPC_removePlayer()
    {
        // players sözlüðündeki nicknamelerden aktif olmayanlarý sil
        List<int> toRemove = new List<int>();
        foreach (var kvp in players)
        {
            bool found = false;
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                if (player.NickName == kvp.Value)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (int pwID in toRemove)
        {
            players.Remove(pwID);
        }
    }
}
