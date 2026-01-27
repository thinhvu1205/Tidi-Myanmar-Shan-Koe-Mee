using System.Data.Common;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
public class TabFriendChat : MonoBehaviour
{
    [SerializeField] GameObject m_FrameCenter;
    [SerializeField] GameObject m_FrameUp;
    [SerializeField] GameObject m_Avatar;
    [SerializeField] TextMeshProUGUI m_NameU, m_Id;
    [SerializeField] private GameObject m_Noti;
    public long idFriend;
    private int indexPosition;
    private bool isChoose = false;
    public void Start()
    {
        m_Noti.SetActive(false);
    }
    public void SetInfo(string name, long id, int avatar, int postion, bool isChoose = false)
    {
        indexPosition = postion;
        Avatar av = m_Avatar.GetComponent<Avatar>();
        if (av != null)
        {
            av.setSpriteWithID(avatar);
            av.idAvt = avatar;
        }
        // m_FrameCenter.SetActive(!isFirst);
        m_FrameUp.SetActive(isChoose);
        m_NameU.text = name;
        m_Id.text = "ID: " + id.ToString();
        idFriend = id;
    }
    public void setOffTab()
    {
        m_NameU.color = Color.white;
        isChoose = false;
        m_FrameUp.SetActive(false);
        m_FrameCenter.SetActive(false);
    }
    public void setOnNoti(bool isOn)
    {
        m_Noti.SetActive(isOn);
    }
    public void OnClickChooseFriendChat()
    {
        setOnNoti(false);
        this.transform.SetAsFirstSibling();
        if (!isChoose && !m_FrameUp.activeSelf)
        {
            if (ChatFriend.Instance != null && ChatFriend.Instance.dictionary.ContainsKey(idFriend))
            {
                ChatFriend.Instance.setInfo(ChatFriend.Instance.dictionary[idFriend]);
            }
            else
            {
                Debug.Log("có gọi ở đây ko");
                SocketSend.sendGetDetailChatFriend(idFriend);
            }

        }
        ChatFriend.Instance.id_friends = idFriend;
        isChoose = true;
        ChatFriend.Instance.setOffAllTab();
        m_FrameUp.SetActive(true);
        m_NameU.color = Color.yellow;
    }
}