using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Notification : MonoBehaviour
{
    public static Notification instance;
    [SerializeField] private GameObject m_IconNoti;
    [SerializeField] private Transform m_TransformNoti;
    private void Awake()
    {
        instance = this;
    }
    public void OnEnable()
    {
        setListNoti();
    }

    public void setListNoti()
    {
        for (int i = 0; i < m_TransformNoti.childCount; i++)
            m_TransformNoti.GetChild(i).gameObject.SetActive(false);

        var list = Globals.COMMON_DATA.ListDataNotiFriend;

        for (int i = 0; i < list.Count; i++)
        {
            var item = i < m_TransformNoti.childCount
                ? m_TransformNoti.GetChild(i).gameObject
                : Instantiate(m_IconNoti, m_TransformNoti);

            item.SetActive(true);
            item.GetComponent<ItemNoti>()?.setInfo(list[i]);
        }
        
    }
    public void setAllItemAsReaded()
    {
        List<long> ids = Globals.COMMON_DATA.ListDataNotiFriend
             .Where(item => !(bool)item["S"])
             .Select(item => item["Id"].Value<long>())
             .ToList();
        SocketSend.SendFriendReadNotification(ids);
        DOVirtual.DelayedCall(1.5f, () =>
                {

                    SocketSend.SendFriendNotification();
                });
    }

}