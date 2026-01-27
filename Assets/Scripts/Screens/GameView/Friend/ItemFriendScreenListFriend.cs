using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemFriendScreenListFriend : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_NameFriends, m_Id, m_Vip, m_Point;
    [SerializeField] private GameObject m_GroupBtn_Friends;
    [SerializeField] private GameObject m_GroupBtn_Invi_requets;
    [SerializeField] private Button Delete;
    [SerializeField] private GameObject m_Avatar;
    [SerializeField] private GameObject m_HeartYellow, m_HeartBlue;
    [SerializeField] private GameObject m_Tick;
    [SerializeField] private GameObject m_Off;
    [SerializeField] private GameObject m_InfoRequest;
    [SerializeField] private GameObject m_Noti;
    private DataFriend dataFriend;
    public void setInfo(DataFriend data)
    {
        Delete.onClick.RemoveAllListeners();

        dataFriend = data;
        m_NameFriends.text = data.userName;
        m_Id.text = "ID: " + data.userid.ToString();
        m_Vip.text = "VIP " + data.vip.ToString();
        m_Point.text = data.point.ToString();
        Avatar av = m_Avatar.GetComponent<Avatar>();
        if (av != null)
        {
            av.setSpriteWithID(data.avatar);
            av.idAvt = data.avatar;
            av.setVip(data.vip);
        }
        if (data.isTab == 4)
        {
            m_InfoRequest.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = data.requestLevel;
            m_InfoRequest.SetActive(true);
            m_Off.SetActive(false);
            m_HeartYellow.SetActive(true);
            m_Tick.transform.parent.gameObject.SetActive(false);
            m_GroupBtn_Invi_requets.SetActive(true);
            m_GroupBtn_Friends.SetActive(false);
            m_GroupBtn_Invi_requets.transform.GetChild(0).gameObject.SetActive(false);
            Delete.onClick.AddListener(() =>
            {
                CancelRequest();
            });
        }
        else if (data.isTab == 5)
        {
            m_InfoRequest.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = data.requestLevel;
            m_InfoRequest.SetActive(true);
            m_Off.SetActive(false);
            m_HeartYellow.SetActive(true);
            m_Tick.transform.parent.gameObject.SetActive(false);
            m_GroupBtn_Invi_requets.SetActive(true);
            m_GroupBtn_Friends.SetActive(false);
            m_GroupBtn_Invi_requets.transform.GetChild(0).gameObject.SetActive(true);
            Delete.onClick.AddListener(() =>
          {
              CancelInvite();
          });
        }
        else
        {
            m_Off.transform.GetChild(0).gameObject.SetActive(data.isOnline);
            m_Off.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = data.isOnline ? "Online" : "Offline";
            m_InfoRequest.SetActive(false);
            m_Off.SetActive(true);
            if (data.status.Equals("Yellow"))
            {
                m_HeartYellow.SetActive(true);
            }
            else if (data.status.Equals("Green"))
            {
                m_HeartBlue.SetActive(true);
                m_HeartYellow.SetActive(false);
            }
            else
            {
                m_HeartBlue.SetActive(false);
                m_HeartYellow.SetActive(false);
            }
            m_GroupBtn_Invi_requets.SetActive(false);
            m_Tick.transform.parent.gameObject.SetActive(true);
            m_GroupBtn_Friends.SetActive(true);
            if (data.isTab == 0)
            {
                m_Tick.gameObject.SetActive(ScreenFriendView.instance.listFrienDelete.Contains(dataFriend.id));
            }
            else
            {
                m_Tick.gameObject.SetActive(ScreenFriendView.instance.listFrienDelete.Contains(dataFriend.userid));
            }

        }
    }
    public void OnClickFriendChat()
    {

        UIManager.instance.showListChatFriend(dataFriend);
        SocketSend.sendGetDetailChatFriend(dataFriend.userid);
    }
    public void OnClickSendChip()
    {
        SocketSend.sendCheckLastCount(dataFriend.userid, dataFriend.friendLevel);
        UIManager.instance.showSendChips(dataFriend.userName, dataFriend.userid, dataFriend.friendLevel);
    }
    public void onClickTick()
    {
        if (ScreenFriendView.instance.isTab == 0)
        {
            m_Tick.gameObject.SetActive(!m_Tick.activeSelf);
            if (m_Tick.activeSelf)
            {
                if (!ScreenFriendView.instance.listFrienDelete.Contains(dataFriend.id))
                {
                    ScreenFriendView.instance.listFrienDelete.Add(dataFriend.id);
                }

            }
            else
            {
                ScreenFriendView.instance.listFrienDelete.Remove(dataFriend.id);
            }

        }
        else
        {
            bool isOn = m_Tick.activeSelf;
            Debug.Log("xem có bao nhiêu item " + ScreenFriendView.instance.m_ChatTableVPG.transform.childCount);
            foreach (Transform item in ScreenFriendView.instance.m_ChatTableVPG.transform)
            {
                var itemFriend = item.GetComponentInChildren<ItemFriendScreenListFriend>(true);
                if (itemFriend != null && itemFriend.m_Tick != null)
                {
                    itemFriend.m_Tick.SetActive(false);
                }
            }
            ScreenFriendView.instance.listFrienDelete.Clear();
            if (!isOn)
            {
                m_Tick.gameObject.SetActive(true);
                ScreenFriendView.instance.listFrienDelete.Add(dataFriend.userid);
            }

        }
        ScreenFriendView.instance.setButtonDelete(ScreenFriendView.instance.listFrienDelete.Count > 0);
    }
    public void CancelInvite()
    {
        SocketSend.sendApproveReject(new List<long> { dataFriend.userid }, false);
    }
    public void ApproveInvite()
    {
        SocketSend.sendApproveReject(new List<long> { dataFriend.userid }, true);
    }
    public void showGiftShop()
    {
        UIManager.instance.showGiftShop(dataFriend.userName, dataFriend.userid);
    }
    public void onClickInviteGame()
    {
        UIManager.instance.showListChatFriend(dataFriend);
        SocketSend.sendChatFriends(dataFriend.userid, "Halika na, sabay tayo maglaro sa game!");
    }
    public void CancelRequest()
    {
        Debug.Log("xem là nó gọi ở đau sao mà lắm thế");
        SocketSend.deleteRequestFriend(new List<long> { dataFriend.userid });
    }
}