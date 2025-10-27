using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviourPunCallbacks
{
    public GameObject joinPanel;
    public TMP_InputField input;

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    #region Hazýr Metotlar

    ///////////// GÝRÝÞ /////////////

    public override void OnConnectedToMaster()
    {
        Debug.Log("Sunucuya baðlanýldý.");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Lobiye girildi.");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Odaya Girildi");
        PhotonNetwork.NickName = input.text;
        joinPanel.SetActive(false);
        PhotonNetwork.LoadLevel(1);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Odaya girme baþarýsýz.");
    }

    #endregion

    public void JoinRoom()
    {
        PhotonNetwork.JoinOrCreateRoom("kolaa", new RoomOptions { MaxPlayers = 20, IsOpen = true, IsVisible = true }, TypedLobby.Default);
    }

    public void NameInputChanged()
    {
        string name = GameObject.Find("UsernameInputField").GetComponent<TMP_InputField>().text.Trim();

        if(name.Length >= 3)
        {
            GameObject.Find("JoinButton").GetComponent<UnityEngine.UI.Button>().interactable = true;
        }
        else
        {
            GameObject.Find("JoinButton").GetComponent<UnityEngine.UI.Button>().interactable = false;
        }
    }
}
