using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public int playerTileColorIndex = -1;
    public TilemapManager tm;
    public ScoreManager sm;
    public GameObject selectedImg;
    public Transform[] buttons;
    public GameObject StartButton;
    public GameObject ReadyButton;
    public TMP_Text timeText;


    private GameObject joinPanel;

    private void Awake()
    {
        joinPanel = GameObject.Find("JoinPanel");
        PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0, null);
        if (PhotonNetwork.IsMasterClient)
            StartButton.SetActive(true);
    }

    #region Photon Callbacks


    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player target, ExitGames.Client.Photon.Hashtable changedProps)
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (!player.CustomProperties.ContainsKey("Ready") ||
                !(bool)player.CustomProperties["Ready"])
            {
                if (PhotonNetwork.IsMasterClient)
                    StartButton.GetComponent<Button>().interactable = false;

                return; // çýk
            }
        }

        //  Tüm oyuncular hazýr
        if (PhotonNetwork.IsMasterClient)
            StartButton.GetComponent<Button>().interactable = true;
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
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
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
        sm.GetComponent<PhotonView>().RPC("RPC_removePlayer", RpcTarget.AllBuffered);

    }


    #endregion

    #region Renk seçme


    public void selectColor(int index)
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().tileIndex = index;
        ReadyButton.GetComponent<Button>().interactable = true;
        StartCoroutine(MoveToButton(index));
    }

    private IEnumerator MoveToButton(int index)
    {
        RectTransform selectedRT = selectedImg.GetComponent<RectTransform>();
        RectTransform buttonRT = buttons[index].GetComponent<RectTransform>();

        Vector3 startPos = selectedRT.position;
        Vector3 endPos = buttonRT.position;

        float elapsed = 0f;

        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.3f;

            // Ease-out efekti
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            selectedRT.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        selectedRT.position = endPos; // tam konumda bitir
    }


    #endregion

    #region butonlar


    public void SetReady(bool r)
    {
        if (r)
            ReadyButton.GetComponent<Button>().interactable = false;
        else
            ReadyButton.GetComponent<Button>().interactable = true;

        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().SetReady(r);
    }

    public void OnStartButton()
    {
        tm.photonView.RPC("RPC_ClearAllTilemap", RpcTarget.AllBuffered);
        photonView.RPC("RPC_StartGame", RpcTarget.All);
        StartCoroutine(StartTime(60f));
    }

    IEnumerator StartTime(float time)
    {
        while (time > 0)
        {
            photonView.RPC("RPC_updateTimeText", RpcTarget.All, time);
            yield return new WaitForSeconds(0.1f);
            time -= 0.1f;
        }
        photonView.RPC("RPC_updateTimeText", RpcTarget.All, 0f);
        photonView.RPC("RPC_EndGame", RpcTarget.All);
        photonView.RPC("RPC_setReady", RpcTarget.All, false);

    }


    #endregion

    #region RPCs


    [PunRPC]
    void RPC_updateTimeText(float time)
    {
        timeText.text = time.ToString("F1");
    }

    [PunRPC]
    void RPC_StartGame()
    {
        joinPanel.SetActive(false);
        //mine.GetComponent<Player>().StartGame();
    }

    [PunRPC]
    void RPC_EndGame()
    {
        //oyuncu 0 notsanýna dön
        GameObject.Find(PhotonNetwork.NickName).transform.position = new Vector3(0, 0, 0);
        joinPanel.SetActive(true);
    }

    [PunRPC]
    public void RPC_setReady(bool r)
    {
        if (r)
            ReadyButton.GetComponent<Button>().interactable = false;
        else
            ReadyButton.GetComponent<Button>().interactable = true;
        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().SetReady(r);

    }


    #endregion
}
