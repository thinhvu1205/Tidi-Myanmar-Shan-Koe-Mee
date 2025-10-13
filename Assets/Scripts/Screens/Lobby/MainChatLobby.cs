using System;
using System.Collections.Generic;
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
    [SerializeField] private VerticalPoolGroup m_ChatTableVPG;

    private List<ChatWorldLobbyData> _PoolDataCWLDs = new();

    void Awake()
    {
        instance = this;
        SocketSend.sendGetChatWorld();
        Globals.CURRENT_VIEW.setCurView(Globals.CURRENT_VIEW.CHAT);
        m_ChatTableVPG.SetCellDataCallback<ChatWorldLobbyData>((go, data, index) =>
        {
            ItemChat aIC = go.GetComponent<ItemChat>();
            aIC.SetInfo(data, null, index);
        });
    }

    public void setInfo(JObject data = null, bool isAdd = false, JObject dataChat = null)
    {
        if (!isAdd)
        {
            Debug.Log("vao day ma");
            _PoolDataCWLDs.Clear();

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

            // Process each chat message
            Debug.Log("đếm count" + items.Count);
            foreach (JObject item in items)
            {
                ChatWorldLobbyData chatData = new ChatWorldLobbyData
                {
                    GameID = (int)(item["GameID"] ?? 0),
                    Type = (int)(item["Type"] ?? 1),
                    Name = (string)(item["Name"] ?? ""),
                    Content = Globals.Config.Utf16ToUtf8((string)(item["Data"] ?? "")),
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
                _PoolDataCWLDs.Add(chatData);
            }
        }
        else
        {
            ChatWorldLobbyData chatData = new ChatWorldLobbyData
            {
                Type = (int)(dataChat["T"] ?? 1),
                Name = (string)(dataChat["N"] ?? ""),
                Content = Globals.Config.Utf16ToUtf8((string)(dataChat["D"] ?? "")),
                Vip = (int)(dataChat["V"] ?? 0),
                //  Avatar = (int)(dataChat["Avatar"] ?? 0),
                Time = dataChat["Time"] != null
    ? DateTimeOffset.FromUnixTimeMilliseconds(dataChat["Time"].Value<long>())
                    .ToLocalTime()
                    .ToString("dd/MM/yyyy HH:mm")
    : DateTime.Now.ToString("dd/MM/yyyy HH:mm")


            };
            _PoolDataCWLDs.Add(chatData);
        }
        Debug.Log("đếm count sau khi add" + _PoolDataCWLDs.Count);
        // Update the vertical pool group with new data
        m_ChatTableVPG.SetAdapter(_PoolDataCWLDs, false);
        m_ChatTableVPG.ScrollToLast(0);
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
