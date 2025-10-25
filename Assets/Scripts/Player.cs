using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviourPunCallbacks
{
    public TMP_Text Name;
    public GameObject direct;
    public float speed;
    public int score = 0;
    public bool isReady = false;

    TilemapManager tilemapManager;
    ScoreManager scoreManager;
    GameManager gameManager;
    Tilemap tilemap;
    FixedJoystick joystick;

    float horizontal, vertical;
    public int tileIndex;
    private Vector3Int lastCellPos;

    void Start()
    {
        // Tüm clientlar için tilemap referansý al
        tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
        tilemapManager = GameObject.Find("Tilemap Manager").GetComponent<TilemapManager>();
        scoreManager = GameObject.Find("Score Manager").GetComponent<ScoreManager>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();

        //// Mevcut tilemap deðerlerini uygula
        //foreach (var kvp in tilemapManager.TilemapValues)
        //{
        //    Vector3Int cellPos = new Vector3Int(kvp.Key[0], kvp.Key[1], 0);
        //    tilemap.SetTile(cellPos, tiles[kvp.Value.x]);
        //}


        if (photonView.IsMine)
        {
            Name.text = PhotonNetwork.NickName;
            name = PhotonNetwork.NickName;
            joystick = GameObject.Find("Fixed_Joystick").GetComponent<FixedJoystick>();
            gameObject.GetComponentInChildren<Camera>().enabled = true;
            gameObject.GetComponentInChildren<AudioListener>().enabled = true;
            tileIndex = gameManager.playerTileColorIndex;
            gameObject.tag = "Player";
            scoreManager.photonView.RPC("RPC_addPlayer", RpcTarget.AllBuffered, PhotonNetwork.NickName, PhotonNetwork.LocalPlayer.ActorNumber);

            ExitGames.Client.Photon.Hashtable table = new ExitGames.Client.Photon.Hashtable();
            table["Ready"] = isReady;
            PhotonNetwork.LocalPlayer.SetCustomProperties(table);

        }
        else
        {
            Name.text = photonView.Owner.NickName;
            gameObject.name = photonView.Owner.NickName;
            gameObject.tag = "OtherPlayer";
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            Movement();
            Direction();
            PaintTile();
        }
    }

    void Movement()
    {

        if (joystick.Horizontal != 0 || joystick.Vertical != 0)
        {
            horizontal = joystick.Horizontal;
            vertical = joystick.Vertical;
        }
        else
        {

            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");
        }


        transform.Translate(new Vector2(horizontal, vertical) * speed * Time.deltaTime);

        if (transform.position.x < -18f)
            transform.position = new Vector3(-18f, transform.position.y, 0);
        if (transform.position.x > 18f)
            transform.position = new Vector3(18f, transform.position.y, 0);
        if (transform.position.y < -10f)
            transform.position = new Vector3(transform.position.x, -10f, 0);
        if (transform.position.y > 10f)
            transform.position = new Vector3(transform.position.x, 10f, 0);

    }

    void Direction()
    {
        Vector3 scale = direct.transform.localScale;

        // Yatay yön
        if (horizontal < 0)
            scale.x = -Mathf.Abs(scale.x);
        else if (horizontal > 0)
            scale.x = Mathf.Abs(scale.x);

        // Dikey yön
        if (vertical < 0)
            scale.y = -Mathf.Abs(scale.y);
        else if (vertical > 0)
            scale.y = Mathf.Abs(scale.y);

        direct.transform.localScale = scale;
    }

    void PaintTile()
    {
        if (tileIndex == -1)
            return;
        

        Vector3Int cellPos = tilemap.WorldToCell(transform.position);
        if (tilemap.GetTile(cellPos) != tilemapManager.tiles[tileIndex])
        {
                tilemapManager.GetComponent<PhotonView>().RPC("RPC_PaintTile", RpcTarget.AllBuffered, cellPos.x, cellPos.y, tileIndex);
                tilemapManager.GetComponent<PhotonView>().RPC("UpdateTilemapValue", RpcTarget.AllBuffered, cellPos.x, cellPos.y, tileIndex, PhotonNetwork.LocalPlayer.ActorNumber);
                lastCellPos = cellPos;
        }
    }

    public void SetReady(bool r)
    {
        isReady = r;

        ExitGames.Client.Photon.Hashtable table = new ExitGames.Client.Photon.Hashtable();
        table["Ready"] = isReady;
        PhotonNetwork.LocalPlayer.SetCustomProperties(table);
    }
}
