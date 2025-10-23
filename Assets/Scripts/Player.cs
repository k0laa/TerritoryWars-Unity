using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviourPunCallbacks
{
    public TMP_Text Name;
    public float speed;
    public GameObject direct;


    public TileBase playerTile;
    public TileBase[] tiles;
    private Vector3Int lastCellPos;   

    float horizontal, vertical;
    Tilemap tilemap;
    FixedJoystick joystick;

    void Start()
    {
        // Tüm clientlar için tilemap referansý al
        tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();

        if (photonView.IsMine)
        {
            Name.text = PhotonNetwork.NickName;
            name = PhotonNetwork.NickName;
            joystick = GameObject.Find("Fixed_Joystick").GetComponent<FixedJoystick>();
        }
        else
        {
            Name.text = photonView.Owner.NickName;
            gameObject.name = photonView.Owner.NickName;
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
        Vector3Int cellPos = tilemap.WorldToCell(transform.position);
        if (cellPos != lastCellPos)
        {
            lastCellPos = cellPos;
            int index = System.Array.IndexOf(tiles, playerTile);
            photonView.RPC("RPC_PaintTile", RpcTarget.AllBuffered, cellPos.x, cellPos.y, index);
        }
    }

    [PunRPC]
    void RPC_PaintTile(int x, int y,int color)
    {
        Vector3Int cellPos = new Vector3Int(x, y, 0);
        tilemap.SetTile(cellPos, tiles[color]);
    }
}
