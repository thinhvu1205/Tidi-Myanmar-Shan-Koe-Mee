using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
public class ItemFriendUpgrade : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_NameFriends, m_Id, m_Vip, m_Point;
    [SerializeField] private GameObject m_HeartYellow, m_HeartBlue;
    [SerializeField] private GameObject m_Avatar;
    private long userId = 0;
    public void setInfo(JObject data)
    {
        Debug.Log("xem dadataa có chỗ nào null" + data.ToString());
        m_NameFriends.text = (string)data["userName"];
        m_Id.text = "ID: " + ((long)data["userId"]).ToString();
        m_Vip.text = "VIP " + ((int)data["vip"]).ToString();
        m_Point.text = ((long)data["point"]).ToString();
        Avatar av = m_Avatar.GetComponent<Avatar>();
        if (av != null)
        {
            av.setSpriteWithID((int)data["avatar"]);
            av.idAvt = (int)data["avatar"];
            av.setVip((int)data["vip"]);
        }
        if (((string)data["status"]).Equals("Yellow"))
        {
            m_HeartYellow.SetActive(true);
        }
        else if (((string)data["status"]).Equals("Green"))
        {
            m_HeartBlue.SetActive(true);
            m_HeartYellow.SetActive(false);
        }
        else
        {
            m_HeartBlue.SetActive(false);
            m_HeartYellow.SetActive(false);
        }
        userId = (long)data["userId"];
    }
    public void upgrade_Friend()
    {
        SocketSend.sendRequestAddFriend(userId);
    }
}