using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChipXocDia : MonoBehaviour
{
    [SerializeField] private List<GameObject> m_ListSprChip;
    [SerializeField] private TextMeshProUGUI m_ValueChip;
    public List<int> ListValueChip = new List<int>();
    public int playerID = 0;
    private int _valueChip;
    public void SetListValueChip(List<int> listValueChip)
    {
        ListValueChip = listValueChip;
    }
    public int GetValue()
    {
        return _valueChip;
    }
    public void SetValueChip(int valueChip)
    {
        _valueChip = valueChip;
        int positionChip = -1;
        for (int i = 0; i < ListValueChip.Count; i++)
        {
            if (valueChip >= ListValueChip[i])
            {
                positionChip = i;
            }
            if (positionChip == -1)
            {
                positionChip = 0;
            }
        }

        if (m_ListSprChip == null || m_ListSprChip.Count == 0)
        {
            Debug.LogError("Danh sách m_ListSprChip không hợp lệ.");
            return;
        }
        foreach (var sprite in m_ListSprChip)
        {
            if (sprite != null)
            {
                sprite.SetActive(false);
            }
        }

        if (positionChip >= 0 && positionChip < m_ListSprChip.Count)
        {
            GameObject chipToActivate = m_ListSprChip[positionChip];
            if (chipToActivate != null)
            {
                chipToActivate.SetActive(true);
                chipToActivate.transform.localScale = new Vector2(0.5f, 0.5f);
            }
            else
            {
                Debug.LogError($"Chip tại vị trí {positionChip} là null.");
            }
        }
        else
        {
            Debug.LogError($"Chỉ mục {positionChip} vượt quá phạm vi của danh sách.");
        }

        // Cập nhật giá trị chip
        if (m_ValueChip != null)
        {
            m_ValueChip.text = Globals.Config.FormatMoney(valueChip, true);
        }
        else
        {
            Debug.LogError("m_ValueChip chưa được gán.");
        }
    }
}

