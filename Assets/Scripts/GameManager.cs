using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public TilemapManager tm;
    public ScoreManager sm;
    public TimeManager timeManager;
    public GameObject selectedImg;
    public GameObject[] buttons;
    public GameObject StartButton;
    public GameObject TimeScrollbar;
    public GameObject ScrollbarTimeText;
    public GameObject ReadyButton;

    private GameObject joinPanel;
    private int time = 60;

    private void Awake()
    {
        joinPanel = GameObject.Find("JoinPanel");
        PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0, null);
        if (PhotonNetwork.IsMasterClient)
        {
            StartButton.SetActive(true);
            TimeScrollbar.SetActive(true);
        }
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
        photonView.RPC("RPC_ColorSelected", RpcTarget.AllBuffered, index, GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().tileIndex);
        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().tileIndex = index;
        if (!GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().isReady)
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
        timeManager.photonView.RPC("StartTime", RpcTarget.All, time);
    }

    public void OnTimeScrollbarChanged()
    {
        float value = TimeScrollbar.GetComponent<Scrollbar>().value;

        int step = Mathf.RoundToInt(value * 9);
        int time = 60 + 20 * step;
        ScrollbarTimeText.GetComponent<TMP_Text>().text = time.ToString();
        this.time = time;
    }


    #endregion

    #region RPCs


    [PunRPC]
    void RPC_StartGame()
    {
        joinPanel.SetActive(false);
    }

    [PunRPC]
    void RPC_EndGame()
    {
        //oyuncu 0 notsanýna dön
        GameObject.Find(PhotonNetwork.NickName).transform.position = new Vector3(0, 0, 0);
        GameObject mj = GameObject.Find("FixedMoveJoystick");
        GameObject tj = GameObject.Find("FixedThrowJoystick");
        GameObject.Find(PhotonNetwork.NickName).GetComponent<Player>().freezeItem.gameObject.SetActive(false);
        GameObject.Find(PhotonNetwork.NickName).GetComponent<Player>().slowItem.gameObject.SetActive(false);
        joinPanel.SetActive(true);
        if (PhotonNetwork.IsMasterClient)
        {
            StartButton.SetActive(true);
            TimeScrollbar.SetActive(true);
        }
        else
        {
            StartButton.SetActive(false);
            TimeScrollbar.SetActive(false);
        }
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

    [PunRPC]
    public void RPC_ColorSelected(int index, int prevIndex)
    {
        if (prevIndex != -1)
            buttons[prevIndex].GetComponent<Button>().interactable = true;
        buttons[index].GetComponent<Button>().interactable = false;
    }


    #endregion

    #region Player Shooter Methods


    public void OnPointerDownShoot()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Pointer.SetActive(true);

    }

    public void OnDragShoot()
    {
        GameObject shooter = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Shooter;
        FixedJoystick joystik = GameObject.Find("FixedThrowJoystick").GetComponent<FixedJoystick>();

        Vector2 direction = new Vector2(joystik.Horizontal, joystik.Vertical);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        shooter.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

    }

    public void OnPointerUpShoot()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Pointer.SetActive(false);
    }



    #endregion
}
