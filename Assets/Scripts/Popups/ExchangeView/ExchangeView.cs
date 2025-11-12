using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System;
using Globals;

public class ExchangeView : BaseView
{
    public static ExchangeView instance;
    [SerializeField] List<Sprite> spTab;
    [SerializeField] GameObject tabTop, itemEx, itemAgency, itemHistory;
    [SerializeField] Transform m_PrefabHistoryTf, m_HistoryTf;
    [SerializeField] TextMeshProUGUI lbChips, m_RewardTMP, m_HistoryTMP;
    [SerializeField] BaseView popupInput;
    [SerializeField] ScrollRect scrContentRedeem, scrContentAgency, scrContentHistory, scrTabs, scrTabsHis;
    [SerializeField] private InputField m_PhoneIF, m_ConfirmPhoneIF;

    private List<JObject> listDataHis = new List<JObject>();
    private JObject firstTabHistItem, curDataTabNap;
    private JArray dataCO;
    private string typeTabHistory = "";
    private int indexTabHis = 0, indexTabNap = 1;
    private float _contentHeight = 0;

    #region Button
    public void onConfirmCashOut()
    {
        SoundManager.instance.soundClick();
        //require('SMLSocketIO').getInstance().emitSIOCCC(cc.js.formatStr("onConfirmCashOut_%s", require('GameManager').getInstance().getCurrentSceneName()));
        var value = valueCO;
        var typeName = typeNet;
        var phoneNumber = m_PhoneIF.text;
        var phoneNumberRetype = m_ConfirmPhoneIF.text;

        if (phoneNumber.Equals("") || phoneNumberRetype.Equals(""))
            UIManager.instance.showMessageBox(Globals.Config.formatStr(Globals.Config.getTextConfig("txt_notEmty"), typeNet.Equals("Mobile") ? Globals.Config.getTextConfig("txt_phone_numnber") : (string)rewardData["TypeName"], ""));
        else if (!phoneNumber.Equals(phoneNumberRetype))
            UIManager.instance.showMessageBox(Globals.Config.formatStr(Globals.Config.getTextConfig("txt_notSame"), typeNet.Equals("Mobile") ? Globals.Config.getTextConfig("txt_phone_numnber") : (string)rewardData["TypeName"]));
        else
        {
            m_PhoneIF.text = "";
            m_ConfirmPhoneIF.text = "";
            SocketSend.sendCashOut(value, phoneNumber, typeName);
            UIManager.instance.showWaiting();
        }
    }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        instance = this;
        SocketSend.SendGiftsHistory();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        SocketIOManager.getInstance().emitSIOCCCNew(Globals.Config.formatStr("ClickShowExchange_%s", Globals.CURRENT_VIEW.getCurrentSceneName()));
        Globals.CURRENT_VIEW.setCurView(Globals.CURRENT_VIEW.DT_VIEW);
        Debug.Log("-==infoDT  " + Globals.Config.infoDT);
        LoadConfig.instance.getInfoEX(updateInfo);
        lbChips.text = Globals.Config.FormatNumber(Globals.User.userMain.AG);
    }
    public async void HandleGiftHistory(JObject data)
    {
        JArray content = (JArray)data["content"];
        foreach (Transform tf in m_HistoryTf) Destroy(tf.gameObject);
        for (int i = 0; i < content.Count; i++)
        {
            Transform tf = Instantiate(m_PrefabHistoryTf, m_HistoryTf);
            tf.gameObject.SetActive(true);
            tf.GetChild(0).GetComponent<TextMeshProUGUI>().text = DateTimeOffset.FromUnixTimeMilliseconds((long)content[i]["time"]).DateTime.ToString("dd/MM/yyyy hh:mm:ss tt");
            tf.GetChild(1).GetComponent<TextMeshProUGUI>().text = (string)content[i]["content"];

        }
        await ScrollHistory();
        async Awaitable ScrollHistory()
        {
            try
            {
                await Awaitable.NextFrameAsync();
                await Awaitable.NextFrameAsync();
                _contentHeight = m_HistoryTf.GetComponent<RectTransform>().rect.height;
                float viewportheight = m_HistoryTf.parent.GetComponent<RectTransform>().rect.height;
                while (true)
                {
                    if (m_HistoryTf.localPosition.y > (_contentHeight - viewportheight)) m_HistoryTf.localPosition = Vector3.zero;
                    await Awaitable.FixedUpdateAsync();
                    m_HistoryTf.localPosition += Time.fixedDeltaTime * new Vector3(0, 100, 0);
                }
            }
            catch
            {

            }
        }
    }
    public void HandleUpdateHistory(JObject data)
    {
        Transform tf = Instantiate(m_PrefabHistoryTf, m_HistoryTf);
        tf.gameObject.SetActive(true);
        tf.GetChild(0).GetComponent<TextMeshProUGUI>().text = DateTimeOffset.FromUnixTimeMilliseconds((long)data["time"]).DateTime.ToString();
        tf.GetChild(1).GetComponent<TextMeshProUGUI>().text = (string)data["content"];
        _contentHeight += tf.GetComponent<RectTransform>().rect.height;

    }
    public void UpdateAg()
    {
        lbChips.text = Globals.Config.FormatNumber(Globals.User.userMain.AG);
    }
    void updateInfo(string strData)
    {
        dataCO = JArray.Parse(strData);
        if (dataCO.Count > 0 && dataCO[0]["child"] == null)
        {
            JObject fakeParent = new JObject();
            fakeParent["title"] = "reward";
            fakeParent["type"] = (string)dataCO[0]["type"];
            fakeParent["child"] = dataCO;
            dataCO = new JArray(fakeParent);
        }
        SetDataButtons();
    }


    async void SetDataButtons()
    {
        if (dataCO.Count <= 0) return;

        JObject objData = (JObject)dataCO[0];
        m_RewardTMP.text = ((string)objData["title"]).ToUpper();
        GameObject go = m_RewardTMP.transform.parent.gameObject;
        go.GetComponent<Button>().onClick.AddListener(() => DoClickButton(go, objData));
        if (!((string)objData["type"]).Equals("agency"))
        {
            m_HistoryTMP.text = Globals.Config.getTextConfig("history").ToUpper();
            GameObject historyObj = m_HistoryTMP.transform.parent.gameObject;
            historyObj.GetComponent<Button>().onClick.AddListener(() => DoClickButton(historyObj, null));
        }
        if (((string)objData["title"]).Equals("reward") && objData["child"] != null)
        {
            await genTabTop((JArray)objData["child"]);
        }
        DoClickButton(go, objData);
    }


    async Task genTabTop(JArray arrayData)
    {
        scrTabs.enabled = arrayData.Count > 4;
        JObject item0 = null;
        var indSelect = 0;
        for (var i = 0; i < arrayData.Count; i++)
        {
            JObject obItem = (JObject)arrayData[i];

            if (i == 0) { item0 = obItem; indSelect = i; }
            Globals.Logging.Log(obItem);
            string title = (string)(obItem["TypeName"] ?? obItem["title"]);
            string title_img = (string)obItem["title_img"];

            GameObject btn = Instantiate(tabTop, scrTabs.content);

            var bkg = btn.transform.Find("Bkg").GetComponent<Image>();
            bkg.sprite = spTab[(i == 0 || i >= arrayData.Count - 1) ? 0 : 1];
            if (i >= arrayData.Count - 1)
            {
                bkg.transform.localScale = new Vector3(-1, 1, 1);
                btn.transform.Find("Line").gameObject.SetActive(false);
            }
            var txt = btn.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            txt.text = "";

            var spLogo = btn.transform.Find("Icon").GetComponent<Image>();
            spLogo.gameObject.SetActive(false);
            if (title_img.Equals(""))
            {
                txt.text = title.ToUpper();
            }
            else
            {
                Sprite spr = await Globals.Config.GetRemoteSprite(title_img);
                if (spr != null)
                {
                    spLogo.sprite = spr;
                    if (spLogo != null && spLogo.sprite != null)
                    {
                        spLogo.gameObject.SetActive(true);
                        spLogo.SetNativeSize();
                    }
                    else
                    {
                        txt.text = title.ToUpper();
                    }
                }

            }
            btn.transform.localScale = Vector3.one;
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                onClickTab(btn.gameObject, obItem);
            });

        }

        if (item0 == null && arrayData.Count > 0)
        {
            indSelect = 0;
            item0 = (JObject)arrayData[0];
        }
        if (scrTabs.content.childCount > indSelect)
        {
            Globals.Logging.Log("item   " + item0.ToString());
            onClickTab(scrTabs.content.GetChild(indSelect).gameObject, item0);
            curDataTabNap = item0;
        }
        genTabHis(arrayData);
    }
    private async void genTabHis(JArray arrayData)
    {
        scrTabsHis.enabled = arrayData.Count > 4;
        JObject item0 = null;
        indexTabHis = 0;

        for (var i = 0; i < arrayData.Count; i++)
        {
            JObject obItem = (JObject)arrayData[i];

            if (i == 0) { item0 = obItem; indexTabHis = i; }
            Globals.Logging.Log(obItem);
            string title = (string)(obItem["title"] ?? obItem["TypeName"]);
            string title_img = (string)obItem["title_img"];

            GameObject btn = Instantiate(tabTop, scrTabsHis.content);

            var bkg = btn.transform.Find("Bkg").GetComponent<Image>();
            bkg.sprite = spTab[(i == 0 || i >= arrayData.Count - 1) ? 0 : 1];
            if (i >= arrayData.Count - 1)
            {
                bkg.transform.localScale = new Vector3(-1, 1, 1);
                btn.transform.Find("Line").gameObject.SetActive(false);
            }

            var txt = btn.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            txt.text = "";

            var spLogo = btn.transform.Find("Icon").GetComponent<Image>();
            spLogo.gameObject.SetActive(false);

            if (string.IsNullOrEmpty(title_img))
            {
                txt.text = title.ToUpper();
            }
            else
            {
                Sprite spr = await Globals.Config.GetRemoteSprite(title_img);
                if (spr != null)
                {
                    spLogo.sprite = spr;
                    spLogo.gameObject.SetActive(true);
                    spLogo.SetNativeSize();
                }
                else
                {
                    txt.text = title.ToUpper();
                }
            }

            btn.transform.localScale = Vector3.one;

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                onClickTabHis(btn.gameObject, obItem);
            });
            if (typeTabHistory.Equals(title, StringComparison.OrdinalIgnoreCase))
            {
                firstTabHistItem = obItem;
                indexTabHis = i;
            }
        }
    }

    void onClickTabHis(GameObject evv, JObject dataItem)
    {
        SoundManager.instance.soundClick();
        for (var i = 0; i < scrTabsHis.content.childCount; i++)
        {
            var bkg = scrTabsHis.content.GetChild(i).transform.Find("Bkg");
            bool isActive = (evv == scrTabsHis.content.GetChild(i).gameObject);
            bkg.gameObject.SetActive(isActive);
            if (isActive)
                indexTabNap = i;
        }
        if (dataItem["TypeName"] != null)
        {
            typeTabHistory = (string)dataItem["TypeName"];
        }
        else if (dataItem["title"] != null)
        {
            typeTabHistory = (string)dataItem["title"];
        }
        else if (dataItem["child"] != null)
        {
            JArray tabNamesJA = (JArray)dataItem["child"];
            if (tabNamesJA.Count > indexTabNap)
                typeTabHistory = (string)(tabNamesJA[indexTabNap]["TypeName"] ?? tabNamesJA[indexTabNap]["title"]);
        }

        curDataTabNap = dataItem;
        if (listDataHis.Count > 0)
        {
            reloadListItemHistory(listDataHis);
        }
    }


    JObject rewardData = null;
    void onClickTab(GameObject evv, JObject dataItem)
    {
        SoundManager.instance.soundClick();
        rewardData = dataItem;
        for (var i = 0; i < scrTabs.content.childCount; i++)
        {
            var bkg = scrTabs.content.GetChild(i).transform.Find("Bkg");
            bkg.gameObject.SetActive(evv == scrTabs.content.GetChild(i).gameObject);
            if (evv == scrTabs.content.GetChild(i).gameObject)
            {
                indexTabHis = i;
                indexTabNap = i;
            }
        }
        typeTabHistory = (string)(dataItem["TypeName"] ?? dataItem["title"]);
        firstTabHistItem = dataItem;
        reloadListItem(rewardData);
    }

    void DoClickButton(GameObject obj, JObject objDataItem)
    {
        SoundManager.instance.soundClick();

        GameObject rewardGo = m_RewardTMP.transform.parent.gameObject;
        GameObject historyGo = m_HistoryTMP.transform.parent.gameObject;
        rewardGo.SetActive(obj != rewardGo);
        historyGo.SetActive(obj != historyGo);
        if (objDataItem == null && obj == historyGo)
        {
            scrContentRedeem.transform.parent.gameObject.SetActive(false);
            scrContentAgency.transform.parent.gameObject.SetActive(false);
            scrContentHistory.transform.parent.gameObject.SetActive(true);

            onClickTabHis(scrTabsHis.content.GetChild(indexTabHis).gameObject, firstTabHistItem);
            SocketSend.sendDTHistory();
            return;
        }
        if (objDataItem != null && objDataItem.ContainsKey("type") && ((string)objDataItem["type"]).Equals("agency"))
        {
            typeNet = (string)objDataItem["type"];
            scrContentRedeem.transform.parent.gameObject.SetActive(false);
            scrContentAgency.transform.parent.gameObject.SetActive(true);
            scrContentHistory.transform.parent.gameObject.SetActive(false);
            reloadListItem(objDataItem);
            return;
        }
        if (curDataTabNap != null)
        {
            if (curDataTabNap.ContainsKey("type"))
                typeNet = (string)curDataTabNap["type"];
            else if (curDataTabNap.ContainsKey("TypeName"))
                typeNet = (string)curDataTabNap["TypeName"];
            else
                typeNet = "unknown";

            scrContentRedeem.transform.parent.gameObject.SetActive(true);
            scrContentAgency.transform.parent.gameObject.SetActive(false);
            scrContentHistory.transform.parent.gameObject.SetActive(false);

            if (indexTabNap != -1)
                onClickTab(scrTabs.content.GetChild(indexTabNap).gameObject, objDataItem);
        }
    }


    void reloadListItem(JObject objDataItem)
    {
        if (objDataItem == null) return;

        JArray items = new JArray();
        Transform parent;

        Debug.Log("-=-= reloadListItem " + objDataItem.ToString());
        if (objDataItem["TypeName"] != null)
            typeNet = (string)objDataItem["TypeName"];
        else if (objDataItem["title"] != null)
            typeNet = (string)objDataItem["title"];
        else if (objDataItem["child"] != null)
        {
            JArray tabNamesJA = (JArray)objDataItem["child"];
            if (tabNamesJA.Count > indexTabNap)
                typeNet = (string)(tabNamesJA[indexTabNap]["TypeName"] ?? tabNamesJA[indexTabNap]["title"]);
        }

        bool isAgency = objDataItem.ContainsKey("type") && ((string)objDataItem["type"]).Equals("agency");
        if (objDataItem["items"] is JArray)
            items = (JArray)objDataItem["items"];
        else if (objDataItem["child"] != null)
        {
            JArray childArr = (JArray)objDataItem["child"];
            if (childArr.Count > indexTabNap && childArr[indexTabNap]["items"] != null)
                items = (JArray)childArr[indexTabNap]["items"];
        }

        parent = isAgency ? scrContentAgency.content : scrContentRedeem.content;
        if (items == null || items.Count == 0) return;
        for (var i = 0; i < items.Count; i++)
        {
            JObject dt = (JObject)items[i];
            GameObject item = i < parent.childCount ? parent.GetChild(i).gameObject : Instantiate(isAgency ? itemAgency : itemEx, parent);

            if (isAgency)
                item.GetComponent<ItemAgency>().setInfo(dt);
            else
                item.GetComponent<ItemEx>().setInfo(dt, () => onChooseCashOut((int)dt["ag"], (int)dt["m"]));

            item.SetActive(true);
            item.transform.localScale = Vector3.one;
        }
        for (var i = items.Count; i < parent.childCount; i++)
            parent.GetChild(i).gameObject.SetActive(false);
    }


    public void reloadListItemHistory(List<JObject> listItem)
    {
        listDataHis = listItem;
        for (int i = 0; i < scrContentHistory.content.childCount; i++)
        {
            scrContentHistory.content.GetChild(i).gameObject.SetActive(false);
        }
        for (var i = 0; i < listDataHis.Count; i++)
        {
            JObject data = listDataHis[i];
            string typeNameItem = "";
            if (data["typeName"] != null) typeNameItem = (string)data["typeName"];
            else if (data["TypeName"] != null) typeNameItem = (string)data["TypeName"];
            else if (data["title"] != null) typeNameItem = (string)data["title"];
            else if (data["type"] != null) typeNameItem = (string)data["type"];
            if (!string.IsNullOrEmpty(typeTabHistory) && typeNameItem.Equals(typeTabHistory, StringComparison.OrdinalIgnoreCase))
            {
                GameObject objItem;
                if (i < scrContentHistory.content.childCount)
                {
                    objItem = scrContentHistory.content.GetChild(i).gameObject;
                }
                else
                {
                    objItem = Instantiate(itemHistory, scrContentHistory.content);
                }

                objItem.SetActive(true);
                objItem.transform.SetParent(scrContentHistory.content);
                objItem.transform.localScale = Vector3.one;
                int cashValue = data.ContainsKey("CashValue") ? (int)data["CashValue"] : 0;
                objItem.GetComponent<ItemHistoryEx>().setInfo(data, cashValue);
            }
        }
    }


    int valueCO;
    string typeNet;
    void onChooseCashOut(int ag, int value)
    {
        SoundManager.instance.soundClick();
        Debug.Log("typenet ==" + typeNet);
        Debug.Log("Current Tab=" + indexTabNap);

        if (Globals.User.userMain.AG < ag)
        {
            UIManager.instance.showMessageBox(Globals.Config.getTextConfig("txt_koduchip"));
            return;
        }

        popupInput.show();

        if (rewardData != null)
        {
            JArray textBox = null;
            if (rewardData["textBox"] != null && rewardData["textBox"] is JArray)
            {
                textBox = (JArray)rewardData["textBox"];
            }
            else
            {
                if (rewardData != null && rewardData.ContainsKey("child") && rewardData["child"] is JArray)
                {
                    JArray childArr = (JArray)rewardData["child"];
                    if (childArr.Count > indexTabNap && childArr[indexTabNap] is JObject)
                    {
                        JObject childObj = (JObject)childArr[indexTabNap];
                        if (childObj["textBox"] != null && childObj["textBox"] is JArray)
                        {
                            textBox = (JArray)childObj["textBox"];
                        }
                    }
                }
            }

            if (textBox != null && textBox.Count >= 2)
            {
                string key1 = (string)textBox[0]["key_placeHolder"];
                string key2 = (string)textBox[1]["key_placeHolder"];

                m_PhoneIF.placeholder.GetComponent<Text>().text = Config.getTextConfig(key1);
                m_ConfirmPhoneIF.placeholder.GetComponent<Text>().text = Config.getTextConfig(key2);
            }
            else
            {
                m_PhoneIF.placeholder.GetComponent<Text>().text = "Enter ID";
                m_ConfirmPhoneIF.placeholder.GetComponent<Text>().text = "Confirm ID";
            }
        }

        valueCO = value;
    }

    public void cashOutReturn(JObject data)
    {
        Globals.Logging.Log("-=-=-=-=cashOutReturn  " + data.ToString());
        UIManager.instance.showMessageBox((string)data["data"]);
        if ((bool)data["status"])
        {
            m_PhoneIF.text = "";
            m_ConfirmPhoneIF.text = "";
            SocketSend.sendUAG();
            popupInput.hide(false);
            DoClickButton(m_HistoryTMP.transform.parent.gameObject, null);

        }
    }
}
