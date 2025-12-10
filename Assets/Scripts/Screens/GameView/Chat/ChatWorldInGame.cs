

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
    [SerializeField] private MicrophoneRecorder m_ThisMR;
    [SerializeField] private AudioSource m_ThisAS;
    [SerializeField] private VerticalPool m_ChatTableVPG;
    private List<PoolInfo> _ControlPIs = new();
    [SerializeField] private GameObject m_Mic_on, m_Mic_off;
    [SerializeField] private TextMeshProUGUI m_Test;


    private void Start()
    {
        if (COMMON_DATA.ListDataChatInGame.Count > 0)
        {
            _ControlPIs.Clear();
            _ControlPIs.AddRange(
         COMMON_DATA.ListDataChatInGame.Select(data => new PoolInfo { Data = data }).ToList()
         );
            m_ChatTableVPG.SetControlInfo(_ControlPIs, _ControlPIs.Count - 1);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        m_Mic_off.SetActive(true);
        m_Mic_on.SetActive(false);
        Globals.CURRENT_VIEW.isInChatVoice = true;
        instance = this;
        m_ChatTableVPG.SetApplyDataCb((go, data, index) =>
        {
            ItemChat aIC = go.GetComponent<ItemChat>();
            ChatWorldLobbyData aCWLD = (ChatWorldLobbyData)data.Data;
            aIC.SetInfo(aCWLD, m_ThisAS, index, (cellW, cellH) =>
                {
                    data.SetCellWidth(cellW + 20);
                    data.SetCellHeight(cellH + 40);
                });
        }, true);

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
    private const float LOBBY_MAX_WIDTH = 450f;
    private const float PADDING = 12f;
    private const float MIN_WIDTH = 100f;
    private Vector2 CalculateTextSizeTMP(string text, TextMeshProUGUI font)
    {
        if (string.IsNullOrEmpty(text))
            return Vector2.zero;
        font.text = text;
        font.enableWordWrapping = true;
        font.ForceMeshUpdate();
        float textWidth = font.preferredWidth;
        float finalWidth = Mathf.Clamp(textWidth + PADDING, MIN_WIDTH, LOBBY_MAX_WIDTH);
        font.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, finalWidth);
        font.ForceMeshUpdate();

        float finalHeight = font.preferredHeight + PADDING;

        return new Vector2(finalWidth + PADDING * 6, finalHeight + PADDING);
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
            UIManager.instance.showToast("စာတို ပို့ရန် စာရိုက်ပါ။");
            return;
        }

        int maxLength = 190;
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

            _ControlPIs.Add(new PoolInfo { Data = chatData });
            COMMON_DATA.ListDataChatInGame.Add(chatData);

            m_ChatTableVPG.SetControlInfo(_ControlPIs, _ControlPIs.Count - 1);

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

                _ControlPIs.Add(new PoolInfo { Data = chatData });
                COMMON_DATA.ListDataChatInGame.Add(chatData);
                m_ChatTableVPG.SetControlInfo(_ControlPIs, _ControlPIs.Count - 1);
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
