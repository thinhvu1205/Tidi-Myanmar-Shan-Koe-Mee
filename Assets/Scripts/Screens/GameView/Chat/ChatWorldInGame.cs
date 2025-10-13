

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using GIKCore.Pool;
using Globals;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatWorldInGame : BaseView
{
    public static ChatWorldInGame instance;

    [SerializeField] private GameObject ItemChat, m_PanelRecording;
    [SerializeField] private TMP_InputField m_Message;
    // [SerializeField] private ScrollRect scrListWorld;
    [SerializeField] private VerticalPoolGroup m_ChatTableVPG;
    [SerializeField] private MicrophoneRecorder m_ThisMR;
    [SerializeField] private AudioSource m_ThisAS;
    private List<ChatWorldLobbyData> _PoolDataCWLDs = new();
    [SerializeField] private GameObject m_Mic_on, m_Mic_off;




    protected override void Awake()
    {
        base.Awake();
        m_Mic_off.SetActive(true);
        m_Mic_on.SetActive(false);
        Globals.CURRENT_VIEW.isInChatVoice = true;
        instance = this;
        m_ChatTableVPG.SetCellDataCallback<ChatWorldLobbyData>((go, data, index) =>
        {
            ItemChat aIC = go.GetComponent<ItemChat>();
            aIC.SetInfo(data, m_ThisAS, index);
        });
        // for (var i = 1; i <= 10; i++)
        // {
        //     string msg = Config.getTextConfig($"chat_text_{i}");

        //     var item = Instantiate(ItemChat, scrListWorld.content);
        //     var txt = item.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        //     txt.text = msg;

        //     // force rebuild để text tính lại chiều cao
        //     LayoutRebuilder.ForceRebuildLayoutImmediate(item.GetComponent<RectTransform>());

        //     item.transform.localScale = Vector3.one;

        //     item.GetComponent<Button>().onClick.AddListener(() =>
        //     {
        //         onClickChatText(msg);
        //     });
        // }
        if (COMMON_DATA.ListDataChatInGame.Count > 0)
        {
            _PoolDataCWLDs.Clear(); // Clear list hiện tại
            _PoolDataCWLDs.AddRange(COMMON_DATA.ListDataChatInGame); // Add tất cả items từ list gốc
            m_ChatTableVPG.SetAdapter(_PoolDataCWLDs);
            m_ChatTableVPG.ScrollToLast(0);
        }
        m_ThisMR.SetData(30, null, null, () =>
        {
            byte[] returnedBytes;
            using (MemoryStream output = new())
            {
                using (DeflateStream deflate = new(output, System.IO.Compression.CompressionLevel.Optimal))
                    deflate.Write(m_ThisMR.GetBytes(), 0, m_ThisMR.GetBytes().Length);
                returnedBytes = output.ToArray();
            }
            Debug.Log("check byte " + m_ThisMR.GetBytes().Length);
            string base64 = Convert.ToBase64String(returnedBytes);

            long timeNowInSeconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            List<string> splitBytes = new();
            for (int i = 0; i < base64.Length; i += 350000) splitBytes.Add(base64.Substring(i, Mathf.Min(350000, base64.Length - i)));

            if (splitBytes.Count <= 1) SocketSend.sendChatVoice(User.userMain.displayName, splitBytes[0], isAudio: true);
            else
            {
                for (int i = 0; i < splitBytes.Count; i++)
                    SocketSend.sendChatVoice(User.userMain.displayName, splitBytes[i], i + 1, splitBytes.Count, timeNowInSeconds, true);
            }
            m_ThisMR.DoClickClose();
        });
    }
    private void OnEnable()
    {
        m_Mic_off.SetActive(true);
        m_Mic_on.SetActive(false);
    }
    public void SetUIMicON(bool isMicOn)
    {
        m_Mic_off.SetActive(!isMicOn);
        m_Mic_on.SetActive(isMicOn);
    }

    void onClickChatText(string msg)
    {
        SocketSend.sendChat(User.userMain.displayName, msg);
    }
    public void SendChat()
    {
        string mess = m_Message.text.Trim(); // bỏ khoảng trắng đầu/cuối

        if (string.IsNullOrEmpty(mess))
        {
            UIManager.instance.showToast("Please enter a message.");
            return;
        }

        int maxLength = 120;
        if (mess.Length >= maxLength)
        {
            UIManager.instance.showToast($"Message too long! Max {maxLength} characters allowed.");
            return; // không gửi lên server
        }

        SocketSend.sendChat(User.userMain.displayName, mess);
        m_Message.text = "";
    }
    public void ShowRecordingPanel()
    {
        m_PanelRecording.SetActive(true);
        Globals.CURRENT_VIEW.isInChatVoice = true;
    }
    public void setInfo(JObject dataChat, int Vip, int ava)
    {
        string name = (string)dataChat["Name"];
        int totalSentData = (int)dataChat["TotalMultipleSend"];
        string timeStr = DateTime.Now.ToString("HH:mm:ss");
        if (totalSentData <= 1)
        {
            ChatWorldLobbyData chatData = new()
            {
                Name = name,
                Content = (string)dataChat["Data"],
                Vip = Vip,
                Avatar = ava,
                Time = timeStr,
                IsAudio = (bool)dataChat["IsAudio"],
            };
            _PoolDataCWLDs.Add(chatData);
            COMMON_DATA.ListDataChatInGame.Add(chatData);
            Debug.Log("đếm count sau khi add" + _PoolDataCWLDs.Count);
            m_ChatTableVPG.SetAdapter(_PoolDataCWLDs, false);
            m_ChatTableVPG.ScrollToLast(0);
        }
        else
        {   // at least 2 requests sent
            if (!COMMON_DATA.MultiSendChatDataD.ContainsKey(name))
            {
                COMMON_DATA.MultiSendChatDataD.Add(name, new() { dataChat });
                return;
            }
            COMMON_DATA.MultiSendChatDataD.TryGetValue(name, out List<JObject> chunksData);
            chunksData.Add(dataChat);
            List<JObject> sameTimeSentData = new();
            long timeSent = (long)dataChat["TimeSendMultiple"];
            foreach (JObject item in chunksData) if ((long)item["TimeSendMultiple"] == timeSent) sameTimeSentData.Add(item);

            if (sameTimeSentData.Count >= totalSentData)
            {
                ChatWorldLobbyData chatData = new()
                {
                    Name = name,
                    Vip = Vip,
                    Avatar = ava,
                    Time = timeStr,
                    IsAudio = (bool)dataChat["IsAudio"],
                };
                for (int i = 1; i <= totalSentData; i++)
                {
                    foreach (JObject item in sameTimeSentData)
                    {
                        if ((int)item["IdMultiple"] == i)
                        {
                            chatData.Content += item["Data"];
                            break;
                        }
                    }
                }
                _PoolDataCWLDs.Add(chatData);
                COMMON_DATA.ListDataChatInGame.Add(chatData);
                m_ChatTableVPG.SetAdapter(_PoolDataCWLDs, false);
                m_ChatTableVPG.ScrollToLast(0);
                for (int i = 0; i < chunksData.Count; i++) if (sameTimeSentData.Contains(chunksData[i])) chunksData.RemoveAt(i--);
            }
        }
    }
    public override void onClickClose(bool isDestroy = true)
    {
        Globals.CURRENT_VIEW.isInChatVoice = false;
        base.onClickClose(isDestroy);

    }
    private void Update()
    {
        if (m_Mic_off.activeSelf)
        {
            if (m_Message.text.Trim().Length > 0)
            {
                m_Mic_off.transform.GetChild(1).gameObject.SetActive(false);
                m_Mic_off.transform.GetChild(2).gameObject.SetActive(true);
                Debug.Log("Có chữ trong TMP_InputField, thực hiện hành động!");
            }
            else
            {
                m_Mic_off.transform.GetChild(1).gameObject.SetActive(true);
                m_Mic_off.transform.GetChild(2).gameObject.SetActive(false);
            }

        }
        if (!m_Mic_on.activeSelf)
        {
            m_Mic_off.SetActive(true);
        }
    }

}
