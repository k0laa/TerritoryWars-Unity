using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Player : MonoBehaviourPunCallbacks
{
    public TMP_Text Name;
    public GameObject direct;
    public float speed;
    public int tileIndex;
    public bool isReady = false;
    public bool isFreeze = false;
    public int activeItemType = -1; // -1: none, 0: freeze

    public List<GameObject> Items;

    TilemapManager tilemapManager;
    ScoreManager scoreManager;
    GameManager gameManager;
    ItemManager itemManager;
    Tilemap tilemap;
    FixedJoystick moveJoystick;
    FixedJoystick throwJoystick;

    float horizontal, vertical;
    Button freezeItem;
    Vector2 lastThrowPos = new Vector2(0, 0);

    void Start()
    {
        // Tüm clientlar için referanslarý al
        tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
        tilemapManager = GameObject.Find("Tilemap Manager").GetComponent<TilemapManager>();
        scoreManager = GameObject.Find("Score Manager").GetComponent<ScoreManager>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        itemManager = GameObject.Find("Item Manager").GetComponent<ItemManager>();

        if (photonView.IsMine)
        {
            // yerleþik joystick referansýný al
            moveJoystick = GameObject.Find("FixedMoveJoystick").GetComponent<FixedJoystick>();
            throwJoystick = GameObject.Find("FixedThrowJoystick").GetComponent<FixedJoystick>();
            freezeItem = GameObject.Find("FreezeButton").GetComponent<Button>();

            // sadece yerel oyuncu için kamera ve audio listener etkinleþtir
            gameObject.GetComponentInChildren<Camera>().enabled = true;
            gameObject.GetComponentInChildren<AudioListener>().enabled = true;

            // oyuncu deðerlerini ayarla
            Name.text = PhotonNetwork.NickName;
            name = PhotonNetwork.NickName;
            gameObject.tag = "Player";
            foreach (GameObject button in gameManager.buttons)
                if (button.GetComponent<Button>().interactable)
                {
                    tileIndex = gameManager.buttons.ToList().IndexOf(button);
                    break;
                }
            gameManager.selectColor(tileIndex);

            // oyuncuyu skor yöneticisine ekle
            scoreManager.photonView.RPC("RPC_addPlayer", RpcTarget.AllBuffered, PhotonNetwork.NickName, PhotonNetwork.LocalPlayer.ActorNumber);

            // hazýr durumunu custom property olarak ayarla
            ExitGames.Client.Photon.Hashtable table = new ExitGames.Client.Photon.Hashtable();
            table["Ready"] = isReady;
            PhotonNetwork.LocalPlayer.SetCustomProperties(table);

        }
        else
        {
            // diðer oyuncular için isim ve tag ayarla
            Name.text = photonView.Owner.NickName;
            gameObject.name = photonView.Owner.NickName;
            gameObject.tag = "OtherPlayer";
        }
    }

    void Update()
    {
        if (photonView.IsMine && !isFreeze)
        {
            Movement();
            Direction();
            PaintTile();
            ThrowControl();
        }
    }

    #region Karakter Kontrolleri


    void Movement()
    {

        if (moveJoystick.Horizontal != 0 || moveJoystick.Vertical != 0)
        {
            horizontal = moveJoystick.Horizontal;
            vertical = moveJoystick.Vertical;
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
        }
    }

    void ThrowControl()
    {
        if (activeItemType == -1)
            return;

        if (Input.touchCount > 0)
        {

            foreach (Touch touch in Input.touches)
            {
                if (touch.position.x > 2000 && touch.position.y < 800)
                {
                    if (touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
                    {
                        lastThrowPos = new Vector2(throwJoystick.Horizontal, throwJoystick.Vertical);
                        return;
                    }
                }
                else
                    continue;

                if (touch.phase == TouchPhase.Ended)
                {
                    if (lastThrowPos.x > 0.2f || lastThrowPos.x < -0.2f ||
                        lastThrowPos.y > 0.2f || lastThrowPos.y < -0.2f)
                        ThrowItem(activeItemType);
                    lastThrowPos = new Vector2(throwJoystick.Horizontal, throwJoystick.Vertical);
                }
            }
        }
    }

    void ThrowItem(int itemType)
    {
        if (Items.Count > 0)
        {
            // Atýlacak eþyayý bul
            GameObject itemToThrow = null;
            foreach (GameObject item in Items)
            {
                ItemScript itemScript = item.GetComponent<ItemScript>();
                if (itemType == 0 && itemScript.isFreezeItem)
                {
                    itemToThrow = item;
                    break;
                }
            }
            itemManager.DeACtivateItemThrowable(itemType);
            Items.Remove(itemToThrow);

            // Eþya bittiðinde butonu devre dýþý býrak
            freezeItem.interactable = false;
            if (Items.Count != 0)
                foreach (GameObject item in Items)
                {
                    ItemScript itemScript = item.GetComponent<ItemScript>();
                    if (itemToThrow.GetComponent<ItemScript>().isFreezeItem)
                    {
                        if (itemScript.isFreezeItem)
                        {
                            freezeItem.interactable = true;
                            break;
                        }
                    }
                }


            itemToThrow.GetComponent<ItemScript>().itemThrowwed();
            itemToThrow.GetComponent<Transform>().position = transform.position;

            Vector2 direction = new Vector2(lastThrowPos.x, lastThrowPos.y);

            int viewID = itemToThrow.GetComponent<PhotonView>().ViewID;
            direction.Normalize(); // birim vektör
            photonView.RPC("itemRigid", RpcTarget.AllBuffered, direction, viewID);

        }
    }

    [PunRPC]
    public void itemRigid(Vector2 direction, int viewID)
    {
        GameObject itemToThrow = PhotonView.Find(viewID).gameObject;
        itemToThrow.GetComponent<Rigidbody2D>().velocity = direction * 6.5f;
    }



    #endregion

    #region Property Ayarlarý


    public void SetReady(bool r)
    {
        isReady = r;

        ExitGames.Client.Photon.Hashtable table = new ExitGames.Client.Photon.Hashtable();
        table["Ready"] = isReady;
        PhotonNetwork.LocalPlayer.SetCustomProperties(table);
    }


    #endregion

    #region Item Devre Dýþý Býrakma


    void Unfreeze()
    {
        isFreeze = false;
    }

    void RemoveSpeedBoost()
    {
        speed -= 4.5f;
    }


    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (photonView.IsMine)
            if (collision.gameObject.CompareTag("Item") && !collision.GetComponent<ItemScript>().isCollected)
            {
                Items.Add(collision.gameObject);
                collision.GetComponent<ItemScript>().photonView.RPC("RPC_itemCollected", RpcTarget.AllBuffered, gameObject.name);

                if (collision.GetComponent<ItemScript>().isFreezeItem)
                {
                    freezeItem.interactable = true;
                }
                else if (collision.GetComponent<ItemScript>().isSpeedBoostItem)
                {
                    Items.Remove(collision.gameObject);
                    speed += 4.5f;
                    Invoke("RemoveSpeedBoost", 5f);

                    int viewID = collision.gameObject.GetComponent<PhotonView>().ViewID;
                    collision.GetComponent<ItemScript>().photonView.RPC("RPC_DestroyItem", RpcTarget.MasterClient, viewID);
                }

            }
            else if (collision.gameObject.CompareTag("Item") && collision.GetComponent<ItemScript>().isCollected)
            {
                if (collision.GetComponent<ItemScript>().ownerName != gameObject.name)
                {
                    if (collision.GetComponent<ItemScript>().isFreezeItem)
                    {
                        isFreeze = true;
                        Invoke("Unfreeze", 3f);
                    }
                    int viewID = collision.gameObject.GetComponent<PhotonView>().ViewID;
                    collision.GetComponent<ItemScript>().photonView.RPC("RPC_DestroyItem", RpcTarget.MasterClient, viewID);
                }
            }
    }
}
