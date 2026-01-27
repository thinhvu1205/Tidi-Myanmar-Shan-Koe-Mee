using System;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

public class DetailItem : BaseView
{

    [SerializeField] private TextMeshProUGUI m_Content;
    public void setContentDetail(string content)
    {
        m_Content.text = content;
    }
   
}