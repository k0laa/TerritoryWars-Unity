using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class Player : MonoBehaviourPunCallbacks
{
    public TMP_Text Name;
    public float speed;
    public GameObject direct;

    float horizontal, vertical;
    bool hor, ver;

    void Start()
    {
        if (photonView.IsMine)
        {
            Name.text = PhotonNetwork.NickName;
            name = PhotonNetwork.NickName;
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
        }
    }

    void Movement()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        transform.Translate(new Vector2(horizontal, vertical) * speed * Time.deltaTime);
    }
    void Direction()
    {
        if (horizontal < 0 && hor)
        {
            direct.transform.localScale = new Vector3(-1, transform.localScale.y, 1);
            hor = !hor;
        }

        if (horizontal > 0 && !hor)
        {
            direct.transform.localScale = new Vector3(1, transform.localScale.y, 1);
            hor = !hor;
        }

        if (vertical < 0 && ver)
        {
            direct.transform.localScale = new Vector3(transform.localScale.x, -1, 1);
            ver = !ver;
        }

        if (vertical > 0 && !ver)
        {
            direct.transform.localScale = new Vector3(transform.localScale.x, 1, 1);
            ver = !ver;
        }
    }
}
