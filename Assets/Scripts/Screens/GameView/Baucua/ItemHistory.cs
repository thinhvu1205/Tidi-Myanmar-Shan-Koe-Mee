using System.Collections.Generic;
using UnityEngine;

public class ItemHistory : MonoBehaviour
{
    [SerializeField] private List<GameObject> m_ListSquareBet;
    private List<int> data;

    public void SetSquarabet(List<int> listValue)
    {
        data = listValue; // Lưu trữ dữ liệu để so sánh sau này
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                m_ListSquareBet[i].transform.GetChild(j).gameObject.SetActive(false);
            }
            m_ListSquareBet[i].transform.GetChild(listValue[i] - 1).gameObject.SetActive(true);
        }
    }

    public bool HasSameData(List<int> otherData)
    {
        if (data == null || otherData == null)
        {
            return false;
        }

        if (data.Count != otherData.Count)
        {
            return false;
        }

        for (int i = 0; i < data.Count; i++)
        {
            if (data[i] != otherData[i])
            {
                return false;
            }
        }

        return true;
    }
}