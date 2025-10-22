using UnityEngine;
using System.Collections.Generic;

public class XocDiaHistory : MonoBehaviour {
    public GameObject nodeResult;
    public List<GameObject> nodeShowResults = new List<GameObject>();

    void Start()
    {
        // Nếu cần logic khởi tạo
    }

    public void setResult(GameObject result)
    {
        // Xóa tất cả con trong nodeResult
        foreach (Transform child in nodeResult.transform)
        {
            Destroy(child.gameObject);
        }

        // Thêm result vào nodeResult
        result.transform.SetParent(nodeResult.transform, false);
    }

    public void setTableHistory(List<int> data)
    {
        // Tắt toàn bộ node trong danh sách
        foreach (GameObject obj in nodeShowResults)
        {
            obj.SetActive(false);
        }

        // Bật các node theo index từ data (giả sử data chứa số 1-based)
        foreach (int index in data)
        {
            int actualIndex = index - 1;
            if (actualIndex >= 0 && actualIndex < nodeShowResults.Count)
            {
                nodeShowResults[actualIndex].SetActive(true);
            }
        }
    }
}
