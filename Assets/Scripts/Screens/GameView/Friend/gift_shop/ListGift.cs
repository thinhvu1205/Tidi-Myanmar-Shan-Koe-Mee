using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ListGift : MonoBehaviour
{
    public static ListGift instance;
    [SerializeField]
    private TextMeshProUGUI m_InfoText;
    [SerializeField]
    private GiftItem m_NoGiftObj;
    [SerializeField]
    private Transform m_ContentGift;
    private long userId;
    private string nameUser;
    public void Awake()
    {
        instance = this;
    }
    public void setInfoText(string name, long id)
    {
        userId = id;
        nameUser = name;
        m_InfoText.text = name + " ID- " + id.ToString();
    }

    public void SetInfoListData(JArray data)

    {
        foreach (JObject giftData in data)
        {
            string nameGift = giftData["name"].ToObject<string>();
            long chip = giftData["value"].ToObject<long>();
            int vip = giftData["vip"].ToObject<int>();
            long point = giftData["point"].ToObject<long>();
            GiftItem item = Instantiate(m_NoGiftObj, m_ContentGift);
            item.SetInfoItemGift(chip, point, ConvertItemToIndex(nameGift));
            if (vip <= Globals.User.userMain.VIP && chip <= Globals.User.userMain.AG)
            {
                item.GetComponent<Button>().onClick.AddListener(() =>
                {
                    UIManager.instance.showConfirm(nameGift, userId, ConvertItemToIndex(nameGift), nameUser, point, chip);
                });
            }
            else
            {
                item.GetComponent<Button>().interactable = false;
            }
        }

    }
    private int ConvertItemToIndex(string item)
    {
        switch (item)
        {
            case "ROSE":
                return 0;
            case "WINE":
                return 1;
            case "REDBOX":
                return 2;
            case "BOUQUET":
                return 3;
            case "GOLD":
                return 4;
            case "DIAMOND":
                return 5;
            case "WATCH":
                return 6;
            case "CAR":
                return 7;
            case "BOAT":
                return 8;
            case "ROCKET":
                return 9;
            default:
                Debug.LogError($"Unknown item name: {item}");
                return -1;
        }
    }
    public void close()
    {
        Destroy(gameObject);
    }

}


