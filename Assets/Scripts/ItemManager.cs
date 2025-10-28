using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    public GameObject FreezeButton;
    public GameObject SlowButton;

    #region Instantiate Methods


    public void RandomItemInstantiate()
    {
        int randomItem = Random.Range(0, 4);
        switch (randomItem)
        {
            case 0:
                InstantiateFreeze();
                break;
            case 1:
                InstantiateSpeedBoost();
                break;
            case 2:
                InstantiateDoubleScore();
                break;
            case 3:
                InstantiateSlow();
                break;

        }
    }

    public void InstantiateFreeze()
    {
        Vector2 position = new Vector2(Random.Range(-17f, 17f), Random.Range(-9f, 9f));
        PhotonNetwork.Instantiate("Freeze", position, Quaternion.identity, 0, null);
    }

    public void InstantiateSpeedBoost()
    {
        Vector2 position = new Vector2(Random.Range(-17f, 17f), Random.Range(-9f, 9f));
        PhotonNetwork.Instantiate("SpeedBoost", position, Quaternion.identity, 0, null);
    }

    public void InstantiateDoubleScore()
    {
        Vector2 position = new Vector2(Random.Range(-17f, 17f), Random.Range(-9f, 9f));
        PhotonNetwork.Instantiate("DoubleScore", position, Quaternion.identity, 0, null);
    }

    public void InstantiateSlow()
    {
        Vector2 position = new Vector2(Random.Range(-17f, 17f), Random.Range(-9f, 9f));
        PhotonNetwork.Instantiate("Slow", position, Quaternion.identity, 0, null);
    }

    #endregion

    #region Item Activation Methods


    public void OnFreezeButton()
    {
        if (GameObject.FindWithTag("Player").GetComponent<Player>().activeItemType == 0)
        {
            DeActivateItemThrowable(0);
        }
        else
        {
            ActivateItemThrowable(0);
        }
    }

    public void OnSlowButton()
    {
        if (GameObject.FindWithTag("Player").GetComponent<Player>().activeItemType == 3)
        {
            DeActivateItemThrowable(3);
        }
        else
        {
            ActivateItemThrowable(3);
        }
    }

    // itemType: 0 - Freeze, 1 - SpeedBoost, 2 - DoubleScore, 3 - Slow
    public void ActivateItemThrowable(int itemType)
    {
        if (GameObject.FindWithTag("Player").GetComponent<Player>().activeItemType != -1)
            return;

        List<GameObject> list = GameObject.FindWithTag("Player").GetComponent<Player>().Items;

        foreach (GameObject item in list)
        {
            ItemScript itemScript = item.GetComponent<ItemScript>();
            if (itemScript.isThrowable == false)
            {
                if (itemType == 0 && itemScript.isFreezeItem)
                {
                    GameObject.FindWithTag("Player").GetComponent<Player>().activeItemType = 0;
                    FreezeButton.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                    itemScript.isThrowable = true;
                    break;
                }
                else if(itemType == 3 && itemScript.isSlowItem)
                {
                    GameObject.FindWithTag("Player").GetComponent<Player>().activeItemType = 3;
                    SlowButton.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                    itemScript.isThrowable = true;
                    break;
                }
            }
        }
    }

    public void DeActivateItemThrowable(int itemType)
    {

        if (GameObject.FindWithTag("Player").GetComponent<Player>().activeItemType != itemType)
            return;

        List<GameObject> list = GameObject.FindWithTag("Player").GetComponent<Player>().Items;
        foreach (GameObject item in list)
        {
            ItemScript itemScript = item.GetComponent<ItemScript>();
            if (itemScript.isThrowable == true)
            {
                if (itemType == 0 && itemScript.isFreezeItem)
                {
                    GameObject.FindWithTag("Player").GetComponent<Player>().activeItemType = -1;
                    FreezeButton.GetComponent<Image>().color = new Color(90f / 255f, 90f / 255f, 90f / 255f, 1f);
                    itemScript.isThrowable = false;
                    break;
                }
                else if (itemType == 3 && itemScript.isSlowItem)
                {
                    GameObject.FindWithTag("Player").GetComponent<Player>().activeItemType = -1;
                    SlowButton.GetComponent<Image>().color = new Color(90f / 255f, 90f / 255f, 90f / 255f, 1f);
                    itemScript.isThrowable = false;
                    break;
                }
            }
        }
    }


    #endregion

    public void DestroyAllItems()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        GameObject[] allItems = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in allItems)
        {
            int viewID = item.GetComponent<PhotonView>().ViewID;
            item.GetComponent<ItemScript>().photonView.RPC("RPC_DestroyItem", RpcTarget.MasterClient, viewID);
        }
    }
}
