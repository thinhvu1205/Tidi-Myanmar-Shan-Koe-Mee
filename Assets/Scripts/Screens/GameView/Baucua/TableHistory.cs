using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TableHistory : MonoBehaviour
{
    [SerializeField] private GameObject m_ItemHistory;
    [SerializeField] private List<TextMeshProUGUI> m_ListLabelHistory;
    [SerializeField] private GameObject m_BorderGold;
    [SerializeField] private Transform m_ContentScoll;
    public List<List<int>> ListHistoryData = new List<List<int>>();

    void Awake()
    {
        m_BorderGold.SetActive(false);
    }

    public void SetData(List<List<int>> ListHistoryData1)
    {
        m_BorderGold.SetActive(false);

        ListHistoryData = ListHistoryData1;
        if (ListHistoryData.Count != 0)
        {
    
            m_BorderGold.SetActive(true);
        }
        foreach (Transform child in m_ContentScoll)
        {
            Destroy(child.gameObject);
        }

        int count = 0;
        for (int i = ListHistoryData.Count - 1; i >= 0; i--)
        {
            if (count >= 12)
            {
                break;
            }
            GameObject item = Instantiate(m_ItemHistory, m_ContentScoll);
            item.SetActive(true);
            ItemHistory itemHistory = item.GetComponent<ItemHistory>();
            itemHistory.SetSquarabet(ListHistoryData[i]);
            count++;
        }
        List<int> valueCounts = new List<int>(new int[6]);
        int totalCount = 0;
        foreach (var item in ListHistoryData)
        {
            foreach (var value in item)
            {
                valueCounts[value - 1]++;
                totalCount++;
            }
        }

        for (int i = 0; i < m_ListLabelHistory.Count; i++)
        {
            if (valueCounts[i] != 0)
            {
                float percentage = (float)valueCounts[i] / totalCount * 100;
                m_ListLabelHistory[i].text = $"{percentage:F2}%";
            }
            else
            {
                m_ListLabelHistory[i].text = "0%";
            }
        }
    }

    public void Exit()
    {
        gameObject.SetActive(false);
    }
}