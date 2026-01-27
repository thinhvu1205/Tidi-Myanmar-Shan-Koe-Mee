using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GiftItem : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_ValueChip;

    [SerializeField]
    private TextMeshProUGUI m_ValueIp;

    [SerializeField]
    private List<Sprite> m_ListItem;

    [SerializeField]
    private Image m_ImageItem;
   

    public void SetInfoItemGift(long chip, long ip, int index)
    {
        m_ImageItem.sprite = m_ListItem[index];
        m_ImageItem.SetNativeSize();
        m_ValueChip.text = Globals.Config.FormatMoney(chip, true);
        m_ValueIp.text = "+" + ip.ToString("N0");
    }
}
