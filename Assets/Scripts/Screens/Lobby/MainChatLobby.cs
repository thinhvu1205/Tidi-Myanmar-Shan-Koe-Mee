using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using GIKCore.Pool;
using Globals;
using Newtonsoft.Json.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MainChatLobby : MonoBehaviour
{
    public static MainChatLobby instance;
    [SerializeField] private TMP_InputField m_Message;
    [SerializeField] private VerticalPool m_ChatTableVPG;
    private List<PoolInfo> _ControlPIs = new();
    [SerializeField] private TextMeshProUGUI m_Test;

    void Awake()
    {
        instance = this;
        SocketSend.sendGetChatWorld();
        Globals.CURRENT_VIEW.setCurView(Globals.CURRENT_VIEW.CHAT);

        const float FONT_SIZE = 25f;
        const float MAX_WIDTH = 800f;
        const float MIN_WIDTH = 100f;
        const float PADDING = 12f;

        m_ChatTableVPG.SetApplyDataCb((go, data, index) =>
        {
            ItemChat aIC = go.GetComponent<ItemChat>();
            ChatWorldLobbyData aCWLD = (ChatWorldLobbyData)data.Data;
            aIC.SetInfo(aCWLD, null, index, (cellW, cellH) =>
                {
                    data.SetCellWidth(cellW + 20);
                    data.SetCellHeight(cellH + 40);
                });
        }, true);
    }

    public void OnClickClose()
    {
        Destroy(gameObject);
    }
    public void setInfo(JObject data = null, bool isAdd = false, JObject dataChat = null)
    {
        if (!isAdd)
        {
            Debug.Log("vao day ma");
            _ControlPIs.Clear();

            // Parse data array
            JArray items;
            var raw = data["data"];
            if (raw is JArray arr)
            {
                items = arr;
            }
            else
            {
                items = JArray.Parse((string)raw ?? "[]");
            }
            foreach (JObject item in items)
            {
                string contentText = (string)(item["Data"] ?? "");
                ChatWorldLobbyData chatData = new ChatWorldLobbyData
                {
                    GameID = (int)(item["GameID"] ?? 0),
                    Type = (int)(item["Type"] ?? 1),
                    Name = (string)(item["Name"] ?? ""),
                    Content = contentText,
                    Vip = (int)(item["Vip"] ?? 0),
                    //   Avatar = (int)(item["Avatar"] ?? 0),
                    ID = (int)(item["ID"] ?? 0),
                    FaceID = (int)(item["FaceID"] ?? 0),
                    Time = item["time"] != null
    ? DateTimeOffset.FromUnixTimeMilliseconds(item["time"].Value<long>())
                    .ToLocalTime()
                    .ToString("dd/MM/yyyy HH:mm")
    : DateTime.Now.ToString("dd/MM/yyyy HH:mm")
                };
                _ControlPIs.Add(new PoolInfo { Data = chatData });
            }
        }
        else
        {
            string contentText = (string)(dataChat["D"] ?? "");
            ChatWorldLobbyData chatData = new ChatWorldLobbyData
            {

                Type = (int)(dataChat["T"] ?? 1),
                Name = (string)(dataChat["N"] ?? ""),
                Content = contentText,
                Vip = (int)(dataChat["V"] ?? 0),
                //  Avatar = (int)(dataChat["Avatar"] ?? 0),
                Time = dataChat["Time"] != null
    ? DateTimeOffset.FromUnixTimeMilliseconds(dataChat["Time"].Value<long>())
                    .ToLocalTime()
                    .ToString("dd/MM/yyyy HH:mm")
    : DateTime.Now.ToString("dd/MM/yyyy HH:mm")


            };
            _ControlPIs.Add(new PoolInfo { Data = chatData });
        }
        m_ChatTableVPG.SetControlInfo(_ControlPIs, _ControlPIs.Count - 1);
    }
    private const float LOBBY_MAX_WIDTH = 800f;
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






    public void SendChat()
    {
        if (Globals.User.userMain.VIP <= Config.vip_block_chat)
        {
            m_Message.text = "";
            UIManager.instance.showToast("You need to reach at least VIP " + (Config.vip_block_chat + 1));
            return;
        }

        string mess = m_Message.text.Trim(); // bỏ khoảng trắng đầu/cuối
        if (string.IsNullOrEmpty(mess))
        {
            UIManager.instance.showToast("Please enter a message.");
            return;
        }

        int maxLength = 190;
        if (mess.Length >= maxLength)
        {
            UIManager.instance.showToast($"Message too long! Max {maxLength} characters allowed.");
            return; // không gửi lên server
        }

        SocketSend.sendChatW(Globals.User.userMain.Username, mess);
        m_Message.text = "";
    }

}
public class ChatWorldLobbyData
{
    public int GameID { get; set; }
    public int Type { get; set; }
    public string Name { get; set; }
    public string Content { get; set; }
    public int Vip { get; set; }
    public int Avatar { get; set; } = -1;
    public int ID { get; set; }
    public int FaceID { get; set; }
    public string Time { get; set; }
    public bool IsAudio { get; set; }
}
