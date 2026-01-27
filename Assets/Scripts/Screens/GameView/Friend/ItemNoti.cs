using System;
using System.Collections.Generic;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

public class ItemNoti : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI txtContent, txtTime;
    [SerializeField] private GameObject m_noti;
    JObject content = new JObject();
    public void setInfo(JObject data)
    {
        content = data;
        txtContent.text = (string)data["Msg"];
        txtTime.text = DateTimeOffset.FromUnixTimeMilliseconds((long)data["Time"])
                    .ToLocalTime()
                    .ToString("dd/MM/yyyy HH:mm");
        m_noti.SetActive(!(bool)data["S"]);
    }
    public void showDetail()
    {
        SocketSend.SendFriendReadNotification(
        new List<long> { content["Id"].Value<long>() }
    );

        UIManager.instance.showDetailNoti((string)content["Msg"]);
        DOVirtual.DelayedCall(1.5f, () =>
        {

            SocketSend.SendFriendNotification();
        });
    }
}