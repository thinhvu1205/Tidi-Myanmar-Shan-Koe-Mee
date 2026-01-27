using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmSendGift : MonoBehaviour
{
    public static ConfirmSendGift instance;

    [SerializeField] private List<Sprite> m_ListSpriteGift;
    [SerializeField] private Image m_ImageItem;
    [SerializeField] private TextMeshProUGUI m_TextConfirm, m_Ip, m_Money;
    private string Name;
    private long UserId;
    public void Awake()
    {
        instance = this;
    }
    public void SetInfoConfirm(string nameGift, long id, int index, string name, long ip, long money)
    {
        Name = nameGift;
        UserId = id;
        m_Ip.text = "+" + ip.ToString();
        m_Money.text = Globals.Config.FormatMoney(money, true);
        m_ImageItem.sprite = m_ListSpriteGift[index];
        m_ImageItem.SetNativeSize();
        m_TextConfirm.text = "You are sending gift to " + name + "-" + id + " in your friendlist. Please check and confirm your action.";
    }
    public void SendGift()
    {
        SocketSend.sendGiftItem(Name, UserId);
        Destroy(gameObject);
    }
    public void Close()
    {
        Destroy(gameObject);
    }

}
