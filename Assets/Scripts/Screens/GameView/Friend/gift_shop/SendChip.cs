using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SendChip : MonoBehaviour
{
    public static SendChip instance;
    [SerializeField]
    private TextMeshProUGUI m_InfoText;
    [SerializeField] private TMP_InputField m_InputTranferChip;
    [SerializeField] private GameObject m_Frame;
    [SerializeField] private GameObject m_Confirm;
    [SerializeField] private GameObject m_GroupBtn;
    [SerializeField] private TextMeshProUGUI m_ContLast;

    private long userId;
    private string nameUser;
    private string levelFriend;
    private long moneyChan;
    public void Awake()
    {
        instance = this;

    }

    public void Start()
    {
        m_Confirm.SetActive(true);
        m_Frame.SetActive(false);
        m_GroupBtn.SetActive(false);
    }
    public void setLastCount(int countLast, long money)
    {
        m_ContLast.text = countLast.ToString();
        if (countLast <= 0)
        {
            m_Confirm.GetComponent<Button>().interactable = false;
        }
        moneyChan = money;
        Debug.Log("xem giá trị" + moneyChan);
    }
    public void setInfoText(string name, long id, string level)
    {
        userId = id;
        nameUser = name;
        levelFriend = level;
        m_InfoText.text = name + " ID- " + id.ToString();

    }
    public void Max() { long limit = moneyChan; if (limit > 0) { long ag = Globals.User.userMain.AG; m_InputTranferChip.text = (ag > limit ? limit : ag).ToString(); } }
    public void Confirm()
    {
        int chipSend = Globals.Config.splitToInt(m_InputTranferChip.text);

        if (chipSend > Globals.User.userMain.AG)
        {
            UIManager.instance.showToast("You don’t have enough money!");
            return;
        }
        if (chipSend <= 0)
        {
            UIManager.instance.showToast("Amount must be greater than 0!");
            return;
        }
        if (chipSend > moneyChan)
        {
            UIManager.instance.showToast("You can send max " + moneyChan.ToString() + "chips!");
            return;
        }

        m_Confirm.SetActive(false);
        m_Frame.SetActive(true);
        m_GroupBtn.SetActive(true);

    }
    public void Send()
    {
        int chipSend = Globals.Config.splitToInt(m_InputTranferChip.text);
        SocketSend.sendChipFriend(userId, chipSend, levelFriend);
        Destroy(gameObject);
    }

    public void close()
    {
        Destroy(gameObject);
    }

}


