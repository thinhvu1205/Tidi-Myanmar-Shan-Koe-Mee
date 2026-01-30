using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFriendView : MonoBehaviour
{
    public static ScreenFriendView instance;
    [SerializeField] private GameObject m_ItemTabScreenFrien;
    [SerializeField] private Transform m_ParentListTab;
    [SerializeField] private ScrollRect m_ScrollContentFriend;
    [SerializeField] private Button buttonDelete;
    [SerializeField] private GameObject confirmation;
    [SerializeField] private TextMeshProUGUI textContentDeleteFriend;
    [SerializeField] private Button buttonConfirm, buttonCancel, buttonCloseConfirmation;

    private JObject FriendData = new JObject();
    private JArray ListTabFriend = new JArray();
    private JArray ListFriend = new JArray();
    private JArray CloseFriend = new JArray();
    private JArray BestFriend = new JArray();
    private JArray SoulMate = new JArray();
    private List<JObject> listFriendOk = new List<JObject>();
    private List<JObject> listCloseFriendOk = new List<JObject>();
    private List<JObject> listBestFriendOk = new List<JObject>();
    private JArray ListInvited = new JArray();
    private JArray ListRequest = new JArray();
    private List<GameObject> listTabFriend = new();
    public VerticalPool m_ChatTableVPG;
    private List<PoolInfo> _ControlPIs = new();
    public List<long> listFrienDelete = new();
    [SerializeField] private GameObject m_ButtonAddMore;
    [SerializeField] private TMP_Dropdown m_Sort;
    public GameObject m_PopupInviteFriend;

    [SerializeField] private Transform m_ContenListFriendUpgrade;
    [SerializeField] private GameObject m_PrefabItemFriendUpgrade;
    [SerializeField] private GameObject m_IconNotiToNoti;
    [SerializeField] private TextMeshProUGUI m_countNoti;
    [SerializeField] private List<Sprite> m_ListTitle;
    [SerializeField] private Image m_Title;
    [SerializeField] private Notification m_Notification;
    public int isTab = 0;
    public void Awake()
    {
        instance = this;
        SocketSend.sendListFriendChat();
        SocketSend.getListFriend();
        SocketSend.SendFriendNotification();
        //SocketSend.downgrade_Friend(8509775);
        ListTabFriend = new JArray
    {
    new JObject
    {
        ["name"] = "Friend",
        ["quantity"] = 0
    },
    new JObject
    {
        ["name"] = "Close Friend",
        ["quantity"] = 0
    },
     new JObject
    {
        ["name"] = "Best Friend",
        ["quantity"] = 0
    },
     new JObject
    {
        ["name"] = "Soulmate",
        ["quantity"] = 0
    },
     new JObject
    {
        ["name"] = "Request",
        ["quantity"] = 0
    },
     new JObject
    {
        ["name"] = "Invitation",
        ["quantity"] = 0
    },


};
        m_ChatTableVPG.SetApplyDataCb((go, data, index) =>
               {
                   ItemFriendScreenListFriend aIC = go.GetComponent<ItemFriendScreenListFriend>();
                   DataFriend aCWLD = (DataFriend)data.Data;
                   aIC.setInfo(aCWLD);
               }, true);
    }
    public void Start()
    {
        ReloadListTabFriend();
        m_Sort.onValueChanged.AddListener(OnSelectOption);
        Sort(0);
        buttonDelete.onClick.AddListener(ClickButtonDeleteFriend);
        buttonCancel.onClick.AddListener(ClickButtonCancelDeleteFriend);
        buttonCloseConfirmation.onClick.AddListener(ClickButtonCancelDeleteFriend);
        //0:point
        //1:vip
        //2:status
    }
    private void ClickButtonCancelDeleteFriend()
    {
        confirmation.SetActive(false);
    }
    private void ClickButtonDeleteFriend()
    {
        FriendData = Globals.COMMON_DATA.JsonDataFriend;
        JArray rawList = (JArray)FriendData["listFriend"];
        long friendId = listFrienDelete[0];
        string friendName = friendId.ToString();
        long userId = 0;

        if (rawList != null)
        {
            foreach (JObject friend in rawList)
            {
                long id = (long)friend["id"];

                if (id == friendId)
                {
                    friendName = (string)friend["userName"];
                    userId = (long)friend["userId"];
                    break;
                }
            }
        }
        confirmation.SetActive(true);
        if (listFrienDelete.Count <= 1)
        {
            textContentDeleteFriend.text = $"You are deleting {friendName} ID: {userId} from your Friend List. Please check and confirm your action.";
        }
        else
        {
            textContentDeleteFriend.text = $"You are deleting {listFrienDelete.Count} Friends from your Friend List.";
        }
        if (isTab > 0)
        {
            textContentDeleteFriend.text = $"You are Downgrading. Please check and confirm your action.";
        }
    }
    public void OnClickOpenNoti()
    {
        m_Notification.gameObject.SetActive(true);
    }
    public void OnClose()
    {
        m_Notification.gameObject.SetActive(false);
    }
    public void setOnNoti()
    {

        if (Globals.COMMON_DATA.ListDataNotiFriendUnread.Count > 0)
        {
            m_IconNotiToNoti.gameObject.SetActive(true);
            int countNoti = Globals.COMMON_DATA.ListDataNotiFriendUnread.Count;
            m_countNoti.text = countNoti.ToString();
        }
        else
        {
            m_IconNotiToNoti.gameObject.SetActive(false);
        }

    }

    void OnSelectOption(int index)
    {
        string selectedText = m_Sort.options[index].text;

        Debug.Log("Option chọn = " + selectedText + " | Index = " + index);
        Sort(index);
    }
    void Sort(int option)
    {
        if (option == 0)
        {
            _ControlPIs.Sort((a, b) =>
       {
           DataFriend da = (DataFriend)a.Data;
           DataFriend db = (DataFriend)b.Data;

           return db.point.CompareTo(da.point);
       });
        }
        else if (option == 1)
        {
            _ControlPIs.Sort((a, b) =>
                  {
                      DataFriend da = (DataFriend)a.Data;
                      DataFriend db = (DataFriend)b.Data;

                      return db.vip.CompareTo(da.vip);
                  });
        }
        else
        {
            _ControlPIs.Sort((a, b) =>
                  {
                      DataFriend da = (DataFriend)a.Data;
                      DataFriend db = (DataFriend)b.Data;

                      return db.isOnline.CompareTo(da.isOnline);
                  });
        }
        m_ChatTableVPG.SetControlInfo(_ControlPIs, 0);
        DOVirtual.DelayedCall(0.5f, () =>
        {
            m_ScrollContentFriend.content.anchoredPosition += new Vector2(0, -0.2f);

        });

    }
    void ReloadListTabFriend()
    {
        foreach (Transform child in m_ParentListTab)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < ListTabFriend.Count; i++)
        {
            int index = i;
            GameObject itemTab = Instantiate(m_ItemTabScreenFrien, m_ParentListTab);
            itemTab.gameObject.SetActive(true);
            itemTab.transform.localScale = Vector3.one;
            itemTab.transform.GetChild(1).gameObject.SetActive(i == 0 ? true : false);
            itemTab.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = (string)((JObject)ListTabFriend[i])["name"];
            itemTab.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = ((int)((JObject)ListTabFriend[i])["quantity"]).ToString();
            itemTab.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnClickTabFriend(index);
            });
            listTabFriend.Add(itemTab);
        }
    }
    public void close()
    {
        m_PopupInviteFriend.SetActive(false);
    }

    void OnClickTabFriend(int index)
    {
        listFrienDelete.Clear();
        buttonDelete.gameObject.SetActive(false);
        foreach (GameObject itemTab in listTabFriend)
        {
            itemTab.transform.GetChild(1).gameObject.SetActive(false);
        }
        listTabFriend[index].transform.GetChild(1).gameObject.SetActive(true);
        switch (index)
        {
            case 0:

                listFrienDelete.Clear();
                isTab = 0;
                m_ButtonAddMore.SetActive(false);
                reloadFriend(ListFriend, 0);
                break;
            case 1:
                listFrienDelete.Clear();
                isTab = 1;
                m_ButtonAddMore.SetActive(true);
                reloadFriend(CloseFriend, 1);
                break;
            case 2:
                listFrienDelete.Clear();
                isTab = 2;
                m_ButtonAddMore.SetActive(true);
                reloadFriend(BestFriend, 2);
                break;
            case 3:
                listFrienDelete.Clear();
                isTab = 3;
                m_ButtonAddMore.SetActive(true);
                reloadFriend(SoulMate, 3);
                break;
            case 4:
                listFrienDelete.Clear();
                isTab = 4;
                m_ButtonAddMore.SetActive(false);
                ReloadListInviteRequest(ListRequest, 4);
                break;
            case 5:
                listFrienDelete.Clear();
                isTab = 5;
                m_ButtonAddMore.SetActive(false);
                ReloadListInviteRequest(ListInvited, 5);
                break;
            default: reloadFriend(ListFriend, 0); break;
        }

        DOVirtual.DelayedCall(0.5f, () =>
               {
                   m_ScrollContentFriend.content.anchoredPosition += new Vector2(0, -0.2f);

               });
    }

    public void AddMore()
    {
        m_PopupInviteFriend.SetActive(!m_PopupInviteFriend.activeSelf);
        setDataAddMore();
    }
    public void setDataAddMore()
    {
        if (m_PopupInviteFriend.activeSelf)
        {
            switch (isTab)
            {
                case 1:
                    m_Title.sprite = m_ListTitle[0];
                    setContentListFriendUpgrade(listFriendOk);
                    break;
                case 2:
                    m_Title.sprite = m_ListTitle[1];
                    setContentListFriendUpgrade(listCloseFriendOk);
                    break;
                case 3:
                    m_Title.sprite = m_ListTitle[2];
                    setContentListFriendUpgrade(listBestFriendOk);
                    break;
            }
        }
    }
    void setContentListFriendUpgrade(List<JObject> listData)
    {
        foreach (Transform child in m_ContenListFriendUpgrade)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < listData.Count; i++)
        {
            int index = i;
            ItemFriendUpgrade item = Instantiate(m_PrefabItemFriendUpgrade, m_ContenListFriendUpgrade).GetComponent<ItemFriendUpgrade>();
            item.setInfo(listData[index]);
        }

    }
    public void setButtonDelete(bool isTrue)
    {
        buttonDelete.gameObject.SetActive(isTrue);
        if (isTrue)
        {
            setListenerButtonDelete();
        }

    }
    public void setListenerButtonDelete()
    {

        buttonConfirm.GetComponent<Button>().onClick.RemoveAllListeners();
        buttonConfirm.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (isTab == 0)
            {
                onClickDelete();
            }
            else
            {
                DowgradeFriend();
            }
        });

    }
    public void onClickDelete()
    {
        SocketSend.deleteFriend(listFrienDelete);
        confirmation.SetActive(false);
    }
    public void DowgradeFriend()
    {
        SocketSend.downgrade_Friend(listFrienDelete[0]);
        confirmation.SetActive(false);
    }
    public void showFortuneGift()
    {
        UIManager.instance.showFortuneGift();
    }
    void ReloadListInviteRequest(JArray data, int isTab)
    {
        _ControlPIs.Clear();
        for (int i = 0; i < m_ScrollContentFriend.content.childCount; i++)
        {
            m_ScrollContentFriend.content.GetChild(i).gameObject.SetActive(false);
        }
        m_ChatTableVPG.SetControlInfo(_ControlPIs, 0);
        Debug.Log("xem list data count" + data.Count + " " + _ControlPIs.Count);
        for (int i = 0; i < data.Count; i++)
        {
            JObject jObj = (JObject)data[i];
            DataFriend dataFriend = new DataFriend();
            dataFriend.userName = (string)((JObject)data[i])["userName"];
            dataFriend.userid = jObj.ContainsKey("userid") ? (int)jObj["userid"] : (int)jObj["id"];
            dataFriend.avatar = (int)((JObject)data[i])["avatar"];
            dataFriend.fbid = (long)((JObject)data[i])["fbid"];
            dataFriend.vip = (int)((JObject)data[i])["vip"];
            dataFriend.point = (int)((JObject)data[i])["point"];
            dataFriend.requestLevel = (string)((JObject)data[i])["requestLevel"];
            dataFriend.isTab = isTab;
            _ControlPIs.Add(new PoolInfo { Data = dataFriend });
        }
        Debug.Log("xem sau lúc list kia có bao nhiêu" + _ControlPIs.Count);
        m_ChatTableVPG.SetControlInfo(_ControlPIs, 0);




    }
    public void reloadFriend(JArray data, int isTab)
    {
        setListenerButtonDelete();
        _ControlPIs.Clear();
        for (int i = 0; i < m_ScrollContentFriend.content.childCount; i++)
        {
            m_ScrollContentFriend.content.GetChild(i).gameObject.SetActive(false);
        }
        m_ChatTableVPG.SetControlInfo(_ControlPIs, 0);
        Debug.Log("xem list data count" + data.Count + " " + _ControlPIs.Count);
        for (int i = 0; i < data.Count; i++)
        {
            JObject jObj = (JObject)data[i];
            DataFriend dataFriend = new DataFriend();
            dataFriend.id = (int)((JObject)data[i])["id"];
            dataFriend.userName = (string)((JObject)data[i])["userName"];
            dataFriend.userid = (int)jObj["userId"];
            dataFriend.avatar = (int)((JObject)data[i])["avatar"];
            dataFriend.vip = (int)((JObject)data[i])["vip"];
            dataFriend.point = (int)((JObject)data[i])["point"];
            dataFriend.friendLevel = (string)((JObject)data[i])["friendLevel"];
            dataFriend.reactionTime = (long)((JObject)data[i])["reactionTime"];
            dataFriend.isOnline = (bool)((JObject)data[i])["isOnline"];
            dataFriend.status = (string)((JObject)data[i])["status"];
            dataFriend.isTab = isTab;

            _ControlPIs.Add(new PoolInfo { Data = dataFriend });
        }
        Debug.Log("xem sau lúc list kia có bao nhiêu" + _ControlPIs.Count);
        m_ChatTableVPG.SetControlInfo(_ControlPIs, 0);



    }
    public void Rule()
    {
        UIManager.instance.showWebView(Globals.Config.linkRuleFriend);
    }
    public void reloadListFriend()
    {
        FriendData = Globals.COMMON_DATA.JsonDataFriend;
        JArray rawList = (JArray)FriendData["listFriend"];

        ListFriend = new JArray(
               rawList.Where(x => (string)x["friendLevel"] == "Friend")
           );
        CloseFriend = new JArray(
         rawList.Where(x => (string)x["friendLevel"] == "CloseFriend")
     );
        BestFriend = new JArray(
          rawList.Where(x => (string)x["friendLevel"] == "BestFriend")
      );
        SoulMate = new JArray(
          rawList.Where(x => (string)x["friendLevel"] == "SoulMate")
      );
        ListInvited = (JArray)FriendData["listInvite"];
        ListRequest = (JArray)FriendData["listRequest"];
        listBestFriendOk = BestFriend
      .Where(item => (item["canUpgrade"]?.Value<bool>() == true) && validateUserId((long)item["userId"]))
     .Select(item => (JObject)item)
     .ToList();

        listCloseFriendOk = CloseFriend
      .Where(item => (item["canUpgrade"]?.Value<bool>() == true) && validateUserId((long)item["userId"]))
    .Select(item => (JObject)item)
    .ToList();

        listFriendOk = ListFriend
     .Where(item => (item["canUpgrade"]?.Value<bool>() == true) && validateUserId((long)item["userId"]))
    .Select(item => (JObject)item)
    .ToList();
        List<int> friendCountTab = new List<int> { ListFriend.Count, CloseFriend.Count, BestFriend.Count, SoulMate.Count, ListRequest.Count, ListInvited.Count };
        for (int i = 0; i < listTabFriend.Count; i++)
        {
            listTabFriend[i].transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = friendCountTab[i].ToString();
        }
        OnClickTabFriend(isTab);

    }
    private bool validateUserId(long userId)
    {
        if (Globals.COMMON_DATA.IdRequestFriend.Contains(userId) || Globals.COMMON_DATA.IdInviteFriend.Contains(userId))
        {
            return false;
        }
        return true;
    }
    public void OnDestroy()
    {
        Destroy(gameObject);
    }
    public void commingsoon()
    {
        UIManager.instance.showComingsoon();
    }
}

public class DataFriend
{
    public long id { get; set; }
    public string userName { get; set; }
    public long userid { get; set; }
    public int avatar { get; set; }
    public long fbid { get; set; }
    public int vip { get; set; }
    public int point { get; set; }
    public int isTab { get; set; }
    public bool isOnline { get; set; }
    public string friendLevel { get; set; }
    public long reactionTime { get; set; }
    public string status { get; set; }
    public string requestLevel { get; set; }

    //0: friend;
    // 1:closeFriend;
    // 2:BestFriend;
    // 3:soulmate;
    // 4:requets;
    // 5:invitation;

}