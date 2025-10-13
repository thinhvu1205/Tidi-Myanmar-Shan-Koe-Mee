using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Globals;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Socket.WebSocket4Net.System.Linq;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class HongKongPokerView : GameView
{
    [SerializeField] private ChipReturnHkPoker chipReturnHkPoker;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private Transform boxBetContainer;
    [SerializeField] private PlayerViewHongKongPoker dealerHkPoker;
    [SerializeField] private PotHkPoker potHkPoker;
    [SerializeField] private GameObject bgArrowSwap;
    [SerializeField] private ToggleCheckHkPoker btnCheckToggle;
    [SerializeField] private GameObject lbChangeCard;
    [SerializeField] private Transform layerChip;
    [SerializeField] private BoxBetShowHkPoker boxBetPrefab;
    [SerializeField] private BtnBetContainer btnBetContainer;
    [SerializeField] private GameObject startTime;
    [SerializeField] private SkeletonGraphic animStart;
    [SerializeField] private Sprite spriteFrameMask;
    [SerializeField] private List<Sprite> listImgWinlose;
    [SerializeField] private TextMeshProUGUI m_TipChipsTMP, m_TipThanksTMP;
    private List<GameObject> boxBetPool = new List<GameObject>();
    private List<GameObject> chipBetPool = new List<GameObject>();

    List<List<Card>> playerCards = new List<List<Card>>
    {
        new List<Card>(),
        new List<Card>(),
        new List<Card>(),
        new List<Card>(),
        new List<Card>()
    };

    List<BoxBetShowHkPoker> listBoxBet = new List<BoxBetShowHkPoker>() { null, null, null, null, null };

    List<Vector2> listPosCard = new List<Vector2>
    {
        new Vector2(-12, -207),
        new Vector2(-404, -49),
        new Vector2(-357, 168),
        new Vector2(350, 168),
        new Vector2(403, -60)
    };

    List<Vector2> listPosBoxBet = new List<Vector2>
    {
        new Vector2(10, -105),
        new Vector2(-352, -126),
        new Vector2(-309, 93),
        new Vector2(306, 72),
        new Vector2(395, -144)
    };

    private int isTurn = 2;
    private bool isAllIn = false;
    private int countTurn = 0;
    private bool isAm = false;
    private string pnameBm = "";
    private int myChipCur = 0, myChipStack = 0;
    private int preNextStack = 0;
    private int preChipForCall = 0;
    private int potValue = 0;
    private bool isCheckAddWinListCard = false;


    public override void handleSTable(string strData)
    {
        var data = JObject.Parse(strData);
        setGameInfo((int)data["M"], (int)data["Id"], data.ContainsKey("maxBet") ? (int)data["maxBet"] : 0);
        for (int i = 0; i < players.Count; i++)
            if (players[i].playerView != null)
                Destroy(players[i].playerView.gameObject);

        stateGame = STATE_GAME.WAITING;
        JArray listPlayer = (JArray)data["ArrP"];
        players.Clear();
        for (var i = 0; i < listPlayer.Count; i++)
        {
            var player = new Player();
            players.Add(player);
            player.playerView = createPlayerView();
            readDataPlayer(player, (JObject)listPlayer[i]);

            if (i == 0) player.setHost(true);

            if (player.id == User.userMain.Userid || player.id == 8240)
            { //che do test
                thisPlayer = player;
                player.playerView.transform.localScale = Vector2.one;
            }
            player.ag = (int)listPlayer[i]["chipStack"];
            player.updatePlayerView();
            player.is_ready = true;
            player.playerView.setDark(false);
        }
        //if (thisPlayer != null)
        //	addChatJoin(thisPlayer.displayName);

        updatePositionPlayerView();
        thisPlayer.is_ready = true;

        for (var i = 0; i < listPlayer?.Count(); i++)
        {
            if ((string)data["N"] == User.userMain.Username)
            {
                myChipCur = (int)data["AG"];
                myChipStack = (int)data["chipStack"];
            }
        }
    }

    public override void handleVTable(string strData)
    {
        base.handleVTable(strData);
        thisPlayer.is_ready = false;
        JObject data = JObject.Parse(strData);
        JArray listPlayer = (JArray)data["ArrP"];
        ViewIng(listPlayer, data);
    }

    public override void handleRJTable(string strData)
    {
        stateGame = STATE_GAME.PLAYING;

        var data = JObject.Parse(strData);
        if (data.ContainsKey("maxBet"))
            setGameInfo((int)data["M"], (int)data["Id"], (int)data["maxBet"]);
        else
        {
            setGameInfo((int)data["M"], (int)data["Id"]);
        }
        var listPlayer = (JArray)data["ArrP"];
        players.Clear();

        for (var i = 0; i < listPlayer.Count; i++)
        {
            var player = new Player();
            players.Add(player);
            player.playerView = createPlayerView();
            readDataPlayer(player, (JObject)listPlayer[i]);
            if (i == 0)
            {
                player.setHost(true);
            }
            if (player.id == User.userMain.Userid)
            {
                thisPlayer = player;
                JToken dataPlayer = listPlayer[i];
                player.ag = (int)dataPlayer["chipStack"];
            }
            player.updatePlayerView();
            player.is_ready = true;
        }
        updatePositionPlayerView();
        ViewIng(listPlayer, data);

        for (var i = 0; i < listPlayer?.Count(); i++)
        {
            JToken dataPlayer = listPlayer[i];
            if (dataPlayer["N"].ToString().Equals(User.userMain.Username))
            {
                myChipCur = (int)dataPlayer["AG"];
                myChipStack = preNextStack = (int)dataPlayer["chipStack"];
                if (dataPlayer["playerStatus"].ToString().Equals("Play"))
                {
                    var player = getPlayer((string)data["CN"]);
                    player.playerView.setTurn(true, (float)data["CT"]);
                    if (player == thisPlayer)
                    {
                        btnBetContainer.gameObject.SetActive(true);
                        continue;
                    }
                    else
                    {
                        if (btnCheckToggle != null)
                            btnCheckToggle.gameObject.SetActive(true);
                    }
                }
            }
        }
    }

    public void HandleTimeToStart(JObject data)
    {
        // this.hideAllDialog();
        stateGame = STATE_GAME.WAITING;
        isTurn = 2;
        isAllIn = false;
        countTurn = 0;
        if (!data.TryGetValue("timeAction", out var value)) return;
        int timeStart = (int)value / 1000 - 2;
        startTime.SetActive(true);
        TextMeshProUGUI timeTMP = startTime.transform.GetComponentInChildren<TextMeshProUGUI>();
        timeTMP.text = timeStart.ToString();
        DOTween.Sequence().AppendInterval(1.0f).AppendCallback(() =>
        {
            timeStart--;
            checkAutoExit();
            timeTMP.text = timeStart + "";
        }).SetLoops(timeStart).OnComplete(() =>
        {
            startTime.SetActive(false);
            CallAsyncFunction(
                Awaitable.WaitForSecondsAsync(animStart.SkeletonData.FindAnimation(ShowAnimOnBegin(true)).Duration));
        });
        CheckCardAndPut();
        playSound(SOUND_GAME.START_GAME);
    }

    public void HandleShowCard(JObject data)
    {
        int indexP = getIndexOfPlayer(getPlayer(data["userName"].ToString()));
        DealCardPlayer(indexP, 0, true, 0);
        DealCardPlayer(indexP, 0.31f, true, (int)data["idCard"]);
    }

    public void HandleLc(JObject data)
    {
        isAm = false;
        stateGame = STATE_GAME.PLAYING;
        int indexP = getIndexOfPlayer(thisPlayer);
        var arrCard = (JArray)data["arr"];
        DealCardPlayer(indexP, 0, true, (int)arrCard[0]);
        DealCardPlayer(indexP, 0.5f, true, (int)arrCard[1]);
        thisPlayer.vectorCardP1[0].setDark(true, spriteFrameMask);
        // this.scheduleOnce(() => { this.thisPlayer.vectorCardP1[0].setDark(true, this.spriteFrameMask); }, 0.3);
        if (startTime.activeSelf)
        {
            startTime.SetActive(false);
        }
    }

    public void HandleBm(JObject data)
    {
        pnameBm = (string)data["userName"];
    }

    public void HandleCab(JObject data)
    {
        if (data["userName"].ToString() == User.userMain.Username)
        {
            User.userMain.AG = myChipCur - myChipStack + (int)data["chipStack"];
        }

        if (startTime.activeSelf)
        {
            startTime.SetActive(false);
        }

        SetCurrentTurn(data);

        if (!thisPlayer.is_ready || thisPlayer.isFold || isAllIn)
        {
            Debug.Log("Chạy vào hàm tắt toggle Check");
            btnCheckToggle.gameObject.SetActive(false);
        }
        else
        {
            int valueToggleCheck = listBoxBet[0]?.chip ?? 0;

            btnCheckToggle.setInfo(
                (int)data["chipForCall"],
                valueToggleCheck,
                thisPlayer.ag
            );

            btnCheckToggle.gameObject.SetActive(true);
            lbChangeCard.SetActive(false);
            bgArrowSwap.gameObject.SetActive(false);
        }

        SetNextTurn(data);
        preNextStack = (int)data["nextStack"];
        countTurn++;
        preChipForCall = (int)data["chipForCall"];
    }

    public void HandleBuyIn(JObject data)
    {
        Player player = getPlayer((string)data["userName"]);
        if (player == null) return;
        if (player == thisPlayer)
        {
            if (isAm) return;
            User.userMain.AG = (long)data["ag"];
            myChipCur = (int)data["ag"];
            myChipStack = (int)data["chip"];
        }
        player.ag = (int)data["chip"] + player.ag;
        player.setAg();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void HandleBc(JObject data)
    {
        Player player = getPlayer((string)data["N"]);
        var index = getIndexOfPlayer(player);
        var localPlayer = players[index];
        var indexDYnamic = localPlayer._indexDynamic;
        // if (this.isTurn == 4) {         // xet off;
        //     this.isCheckOffChangeTime++;
        //     if (this.isCheckOffChangeTime >= this.checkNextTurnFail()) {
        //         if (this.thisPlayer.is_ready && !this.thisPlayer.isFold) this.thisPlayer.vectorCardP1[3].setDark(false, this.spriteFrameMask);
        //     }
        // }
        if (player.vectorCardP1[player.vectorCardP1.Count() - 1].code == 0 ||
            player.vectorCardP1[player.vectorCardP1.Count() - 1].code == (int)data["C"])
        {
            // player.vectorCardP1[player.vectorCardP1.length - 1].setTextureWithCode(data.C);
            // 4card
            var card4Th = player.vectorCardP1[^1];
            FoldUp(indexDYnamic, card4Th, (int)data["C"]);
        }
        else
        {
            DealCardPlayer(index, 0, true, (int)data["C"]);
        }

        if (player == thisPlayer && thisPlayer.vectorCardP1.Count == 4)
        {
            if (thisPlayer.is_ready)
            {
                btnCheckToggle.gameObject.SetActive(false);
                btnBetContainer.gameObject.SetActive(false);

                DOTween.Sequence().AppendInterval(0.5f).AppendCallback(() =>
                {
                    if (this == null) return;
                    lbChangeCard.SetActive(true);
                    bgArrowSwap.gameObject.SetActive(true);
                    StartCoroutine(AnimateArrowSwapLoop(10));
                }).AppendInterval(7f).AppendCallback(OnClickCancel);

                thisPlayer.vectorCardP1[3].setDark(true, spriteFrameMask);
                int timeStart = 8;
                startTime.SetActive(true);
                TextMeshProUGUI timeTMP = startTime.transform.GetComponentInChildren<TextMeshProUGUI>();
                timeTMP.text = timeStart.ToString();
                DOTween.Sequence().AppendInterval(1.0f).AppendCallback(() =>
                {
                    timeStart--;
                    checkAutoExit();
                    timeTMP.text = timeStart + "";
                }).SetLoops(timeStart).OnComplete(() =>
                {
                    startTime.SetActive(false);
                });
            }
        }
    }

    public async UniTask HandleFinish(JObject data)
    {

        var countReturnChip = 0;
        btnBetContainer.gameObject.SetActive(false);
        var dataListPlayer = data["declarePacketsTrans"];
        for (var d = 0; d < dataListPlayer.Count(); d++)
        {
            var arrC = dataListPlayer[d]?["arr"];
            var totalChipPlayer = dataListPlayer[d]["chipStack"];
            var chipReturn = dataListPlayer[d]["listChipReturn"];
            if (chipReturn != null)
            {
                countReturnChip += chipReturn.Count();
            }

            var textCard = (string)dataListPlayer[d]?["cardType"];
            var typeWin = 9;

            Player player = getPlayer((string)dataListPlayer[d]["userName"]);
            //  player.ag = totalChipPlayer;

            if (player == thisPlayer)
            {
                User.userMain.AG = myChipCur - myChipStack + (int)dataListPlayer[d]["chipStack"];
            }

            if (arrC != null)
            {
                var delay = 0;
                int count = arrC.Count();
                int playerCardCount = player.vectorCardP1.Count;

                for (int v = 0; v < count; v++)
                {
                    if (v >= playerCardCount || player.vectorCardP1[v] == null)
                    {
                        StartCoroutine(DealCardWithDelay(getIndexOfPlayer(player), (int)arrC[v], delay / 1000f));
                        delay += 200;
                    }
                    else
                    {
                        var card = player.vectorCardP1[v];
                        card.setTextureWithCode((int)arrC[v]);
                        card.transform.DOScale(0.5f, 0.3f)
                            .OnComplete(() => card.transform.DOScale(0.45f, 0.3f));
                    }
                }

                if (textCard == "Pair") typeWin = 0;
                if (textCard == "TwoPair") typeWin = 1;
                if (textCard == "HighCard") typeWin = 2;
                if (textCard == "FullHouse") typeWin = 3;
                if (textCard == "ThreeOfKind") typeWin = 4;
                if (textCard == "Straight") typeWin = 5;
                if (textCard == "FourOfKind") typeWin = 6;
                if (textCard == "Flush") typeWin = 7;
                if (textCard == "TPS") typeWin = 8;
                DOTween.Sequence().AppendInterval(0.7f).AppendCallback(() =>
                {
                    InstantiateResultText(getIndexOfPlayer(player), typeWin);
                });
            }

        }

        SocketIOManager.getInstance().emitUpdateInfo();

        if (!thisPlayer.isFold && thisPlayer.is_ready)
        {
            thisPlayer.vectorCardP1[0].setDark(false, spriteFrameMask);
        }

        Vector2 vtemp;
        Vector2 vCount;
        if (countReturnChip < 2)
        {
            vtemp = new Vector2(0, 115);
            vCount = new Vector2(0, 0);
        }
        else if (countReturnChip < 3)
        {
            vtemp = new Vector2(-60, 115);
            vCount = new Vector2(123, 0);
        }
        else if (countReturnChip < 4)
        {
            vtemp = new Vector2(-82, 115);
            vCount = new Vector2(82, 0);
        }
        else if (countReturnChip < 5)
        {
            vtemp = new Vector2(-183, 115);
            vCount = new Vector2(123.5f, 0);
        }
        else
        {
            vtemp = new Vector2(-183, 115);
            vCount = new Vector2(92.5f, 0);
        }

        var timeDelay = 4;
        for (var t = 0; t < dataListPlayer.Count(); t++)
        {
            var arrC = dataListPlayer[t]["arr"];
            var player = getPlayer((string)dataListPlayer[t]["userName"]);
            var indexDynamic = player._indexDynamic;
            var chipReturn = dataListPlayer[t]["listChipReturn"];
            if (chipReturn != null)
            {
                // InstantiateEffWinLose(indexDynamic);
                player.playerView.setEffectWin("", false);

                for (var z = 0; z < chipReturn.Count(); z++)
                {
                    Debug.Log("chay vao ham khoi tao chip return");
                    ChipReturnHkPoker item;
                    long chipRt = (long)chipReturn[z];
                    if (chipBetPool.Count < 1)
                    {
                        item = Instantiate(chipReturnHkPoker, layerChip);
                    }
                    else
                    {
                        item = chipBetPool[0].GetComponent<ChipReturnHkPoker>();
                        item.gameObject.SetActive(true);
                        chipBetPool.RemoveAt(0);
                    }
                    item.transform.position = potHkPoker.transform.position;
                    var durationTime = item.SetInfo(vtemp, listPosView[indexDynamic], timeDelay, (int)chipReturn[z]);
                    AsyncDelayedAction(durationTime, () =>
                    {
                        if (this == null) return;
                        player.playerView.effectFlyMoney(chipRt);
                        player.ag += (int)chipRt;
                        player.setAg();
                        // if (dataListPlayer[t]["diamond"] != null)
                        // {
                        // await UniTask.Delay(2000);
                        // player.playerView.effectFlDiamond(dataListPlayer[t]?["diamond"] - player.dia);
                        // this.updatePlayerDiamond(player, dataListPlayer[t].diamond);
                        // }

                        if (player == thisPlayer)
                        {
                            isAllIn = false;
                            User.userMain.AG += chipRt;
                            // if ( (int)dataListPlayer[t]["chipWin"] > 0 && !isCheckAddWinListCard) {
                            // var str = "";
                            // str =
                            // 	"Monica: " + 
                            // 	Config.getTextConfig("shan2_you_win").Replace("%lld", dataListPlayer[t]["chipWin"] + "");

                            // if (arrC != null)
                            // {
                            // quickChat.addChatWithCard(str, arrC);
                            // this.itemChatNgoaiGame.setDataChatCard(str, arrC);
                            // }
                            // else
                            // {
                            //this.quickChat.addChatWithText(str);
                            // var arrCardWin = thisPlayer.vectorCardP1.Select(t1 => t1.code).ToList();
                            // quickChat.addChatWithCard(str, arrCardWin);
                            // this.itemChatNgoaiGame.setDataChatCard(str, arrCardWin);
                            // }

                            // isCheckAddWinListCard = true;
                            // }
                        }
                    });

                    vtemp += vCount;
                    timeDelay++;
                }
            }
        }

        potHkPoker.setValue(potValue, 0);
        // await UniTask.Delay(2000);
        // HandleWinnerCard(data);
        await UniTask.Delay(2500);
        if (this != null)
            potHkPoker.setValue(0, 0.2f);

        await UniTask.Delay((5 + timeDelay) * 1000 - 2500);

        if (this != null)
        {
            isCheckAddWinListCard = false;
            stateGame = STATE_GAME.WAITING;
            isTurn = 2;
            countTurn = 0;
            ResetViewGame(data);
        }
    }

    public override void HandlerTip(JObject data)
    {
        if (data["data"] != null)
        {
            // GameManager.getInstance().onShowConfirmDialog(data.data);
            // return;
        }

        for (var i = 0; i < players.Count; i++)
        {
            if (players[i].displayName == (string)data["N"])
            {
                var nameTip = data["N"];
                playSound(SOUND_GAME.TIP);
                // this.EffectMoneyChange(
                //     -data.AGTip,
                //     this.players[i].ag,
                //     this.players[i]._playerView.lbAg
                // );
                long chips = (long)data["AGTip"];
                players[i].ag -= chips;
                players[i].setAg();
                if (players[i].displayName == thisPlayer.displayName)
                {
                    User.userMain.AG -= chips;
                }

                for (var j = 0; j < 2; j++)
                {
                    GameObject temp;
                    if (chipBetPool.Count < 1)
                    {
                        ChipReturnHkPoker go = Instantiate(chipReturnHkPoker, layerChip);
                        go.init(1, 0.4f);
                        temp = go.gameObject;
                    }
                    else
                    {
                        temp = chipBetPool[0];
                        temp.transform.localScale = Vector3.one * 0.4f;
                        temp.SetActive(true);
                        chipBetPool.RemoveAt(0);
                    }

                    temp.transform.position = players[i].playerView.transform.position;

                    Vector3 targetPos1 = temp.transform.position + new Vector3(0, 80, 0);
                    Vector3 targetPos2 = dealerHkPoker.transform.position;

                    temp.transform.DOMove(targetPos1, 0.2f)
                        .SetEase(Ease.OutElastic)
                        .SetDelay(j * 0.2f)
                        .OnComplete(() =>
                        {
                            temp.transform.DOMove(targetPos2, 1f)
                                .SetEase(Ease.InOutCubic)
                                .SetDelay(0.3f)
                                .OnComplete(() =>
                                {
                                    chipBetPool.Add(temp);
                                    temp.SetActive(false);
                                });
                        });

                }
                StartCoroutine(showThanksDialog(chips));
            }
        }
        IEnumerator showThanksDialog(long chips)
        {
            GameObject parentObject = m_TipChipsTMP.transform.parent.gameObject;
            m_TipChipsTMP.text = Config.FormatNumber(chips);
            string playerName = (string)data["N"];
            m_TipThanksTMP.text = (playerName.Length >= 7 ? ((string)data["N"]).Substring(0, 7) + "..., " : playerName + ", ") + Globals.Config.getTextConfig("tip_thanks_1");
            parentObject.SetActive(true);
            yield return new WaitForSeconds(3f);
            parentObject.SetActive(false);
        }
    }

    // private void HandleWinnerCard(JObject data) {
    //  var array = data["declarePacketsTrans"];
    //  for (var i = 0; i < array.Count(); i++) {
    //   var player = getPlayer((string) array[i]?["userName"]);
    //   var indexDynamic = player._indexDynamic;
    //   var startCard = 1;
    //   if (array[i]?["arr"] != null) {
    //    startCard = 0;
    //   }
    //   if ((long) array[i]?["chipWin"] > 0) {
    //    ShowCardResultAnimattion(indexDynamic, startCard, 0);
    //   }
    //  }
    // }

    private IEnumerator DealCardWithDelay(int indexP, int cardValue, float delay)
    {
        yield return new WaitForSeconds(delay);
        DealCardPlayer(indexP, 0, true, cardValue);
    }


    private async UniTask AsyncDelayedAction(float time, Action callback)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(time));
        callback?.Invoke();
    }


    private void ViewIng(JArray listPlayer, JObject data)
    {
        potHkPoker.setValue((int)data["pot"]);
        for (var i = 0; i < listPlayer.Count(); i++)
        {
            Player player = getPlayer((string)listPlayer[i]["N"]);
            var indexP = getIndexOfPlayer(player);
            var dynaIndex = player._indexDynamic;
            if (player.is_ready)
            {
                JArray jArray = (JArray)listPlayer[i]["Arr"];
                var countArr = jArray?.Count() ?? 0;
                if ((string)listPlayer[i]["playerStatus"] == "Fold")
                {
                    for (var t = 0; t < countArr; t++)
                    {
                        DealCardPlayer(indexP, 0, false, 0);
                    }

                    FoldPlayer(player, 0);
                    player.playerView.setDark(true);
                    InstantiateBoxBet(dynaIndex, 0, "Fold");
                }
                else
                {
                    for (var t = 0; t < countArr; t++)
                    {
                        DealCardPlayer(indexP, 0, false, (int)jArray?[t]);
                    }

                    isTurn = isTurn < countArr ? countArr : isTurn;
                }
            }
        }
    }

    private void InstantiateResultText(int indexP, int typeCard)
    {
        GameObject imageObject = new GameObject("TextImage");

        Image image = imageObject.AddComponent<Image>();

        imageObject.transform.SetParent(transform, false);
        image.sprite = listImgWinlose[typeCard];
        image.preserveAspect = true;
        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(listImgWinlose[typeCard].rect.width, listImgWinlose[typeCard].rect.height);
        var player = players[indexP];
        var length = player.vectorCardP1.Count;
        var dynamicIndex = player._indexDynamic;

        var cardScale = 0.5f;
        var widthCardNode = (149 * cardScale) / 2;
        var cardBegin = listPosCard[dynamicIndex];
        float offsetX = ((length - 2) / 2.0f) * widthCardNode;
        float posX = dynamicIndex > 2 ? cardBegin.x - offsetX : cardBegin.x + offsetX;
        float posY = cardBegin.y - (200 * cardScale) / 4;

        imageObject.transform.localPosition = new Vector2(posX, posY);

        DOTween.Sequence().AppendInterval(4f).AppendCallback(() =>
        {
            Destroy(imageObject.gameObject);
        });
    }

    private IEnumerator AnimateArrowSwapLoop(int repeatCount)
    {
        for (int i = 0; i < repeatCount; i++)
        {
            if (bgArrowSwap == null) yield break;

            Vector2 startPos = new Vector2(0, 0);
            Vector2 endPos = new Vector2(0, -25);

            yield return bgArrowSwap.transform.DOLocalMove(startPos, 0.4f).SetEase(Ease.OutCubic).WaitForCompletion();

            yield return new WaitForSeconds(0.05f);

            yield return bgArrowSwap.transform.DOLocalMove(endPos, 0.15f).SetEase(Ease.OutCubic).WaitForCompletion();

        }
    }

    private void FoldDown(int index, Card card, float delay)
    {
        float sk1 = (index <= 2) ? -15f : 15f;
        float sk2 = (index <= 2) ? 15f : -15f;
        StartCoroutine(FoldDownRoutine(card, delay, sk1, sk2));
    }

    private IEnumerator FoldDownRoutine(Card card, float delay, float sk1, float sk2)
    {
        yield return new WaitForSeconds(delay);

        Sequence foldSequence = DOTween.Sequence();
        foldSequence.Append(card.transform.DOScale(new Vector3(0f, 0.55f, 1f), 0.15f).SetEase(Ease.OutCubic))
            .AppendCallback(() =>
            {
                if (card != null)
                {
                    card.setTextureWithCode(0);
                    card.setDark(true, spriteFrameMask);
                }
            })
            .Append(card.transform.DOScale(new Vector3(0.45f, 0.45f, 1f), 0.15f).SetEase(Ease.OutCubic));

        Sequence skewSequence = DOTween.Sequence();
        skewSequence.Append(card.transform.DORotate(new Vector3(0, sk1, 0), 0.15f).SetEase(Ease.OutCubic))
            .AppendCallback(() => card.transform.rotation = Quaternion.Euler(0, sk2, 0))
            .Append(card.transform.DORotate(Vector3.zero, 0.15f).SetEase(Ease.OutCubic));
    }

    private void FoldUp(int index, Card card, int code)
    {
        float sk1 = (index <= 2) ? -15f : 15f;
        float sk2 = (index <= 2) ? 15f : -15f;

        // Scale animation
        Sequence foldSequence = DOTween.Sequence();
        foldSequence.Append(card.transform.DOScale(new Vector3(0f, 0.65f, 1f), 0.2f).SetEase(Ease.OutCubic))
            .AppendCallback(() =>
            {
                if (card != null)
                {
                    card.setTextureWithCode(code);
                }
            })
            .Append(card.transform.DOScale(new Vector3(0.45f, 0.45f, 1f), 0.2f).SetEase(Ease.OutCubic));

        // Skew animation (simulated via rotation)
        Sequence skewSequence = DOTween.Sequence();
        skewSequence.Append(card.transform.DORotate(new Vector3(0, sk1, 0), 0.2f).SetEase(Ease.OutCubic))
            .AppendCallback(() => card.transform.rotation = Quaternion.Euler(0, sk2, 0))
            .Append(card.transform.DORotate(Vector3.zero, 0.2f).SetEase(Ease.OutCubic));
    }

    private void FoldPlayer(Player player, int index)
    {
        playSound(SOUND_GAME.FOLD);
        player.isFold = true;
        player.playerView.setDark(true);
        for (int i = 0; i < player.vectorCardP1.Count; i++)
        {
            // player.vectorCardP1[i].setTextureWithCode(0);
            // player.vectorCardP1[i].setDark(true, this.spriteFrameMask);
            FoldDown(index, player.vectorCardP1[i], i * 0.1f);
        }
    }

    private void SetCurrentTurn(JObject data)
    {
        if (data["status"].ToString() == "")
        {
            return;
        }

        if (!string.IsNullOrEmpty(pnameBm))
        {
            Player player = getPlayer(pnameBm);
            InstantiateBoxBet(player._indexDynamic, agTable / 2, "");
            player.playerView.effectFlyMoney(-agTable / 2);
            // EffectMoneyChange(-agTable / 2, player.ag, player.playerView.lb);
            player.ag -= agTable / 2;
            player.setAg();


            if (player == thisPlayer)
            {
                // GameManager.GetInstance().User.Ag -= this.agTable / 2;
                User.userMain.AG -= agTable / 2;
            }

            pnameBm = "";

            if (!btnBetContainer.gameObject.activeSelf)
            {
                btnBetContainer.gameObject.SetActive(true);
                btnBetContainer.update_slider(player.ag);
            }
        }

        string namePlayer = data["userName"].ToString();
        Player currentPlayer = getPlayer(namePlayer);
        int indexDynamic = currentPlayer._indexDynamic;
        currentPlayer.playerView.setTurn(false, 0);
        InstantiateBoxBet(indexDynamic, (int)data["chipBet"], data["status"].ToString());

        if (data["status"].ToString() == "Fold")
        {
            FoldPlayer(currentPlayer, indexDynamic);
        }

        int valueBoxBet = listBoxBet[indexDynamic]?.chip ?? 0;
        Debug.Log("money bet " + namePlayer + " " + (string)data["chipStack"] + " " + preNextStack);
        if ((int)data["chipStack"] - preNextStack != 0)
        {
            currentPlayer.playerView.effectFlyMoney(-valueBoxBet);
        }

        int numberChip = 0;
        if (valueBoxBet > 0)
        {
            if (valueBoxBet <= agTable)
                numberChip = 1;
            else if (valueBoxBet <= 2 * agTable)
                numberChip = 2;
            else if (valueBoxBet <= 3 * agTable)
                numberChip = 3;
            else
                numberChip = 4;
        }

        Vector2 target = listPosBoxBet[indexDynamic];
        Vector2 vPos = listPosView[indexDynamic];

        for (int i = 0; i < numberChip; i++)
        {
            Vector2 vTemp = new Vector2(target.x, target.y);
            GameObject temp;
            if (chipBetPool.Count < 1)
            {
                ChipBet go = Instantiate(chipReturnHkPoker, layerChip);
                go.init(1, 0.4f);
                temp = go.gameObject;
            }
            else
            {
                temp = chipBetPool[0];
                temp.SetActive(true);
                temp.transform.localScale = Vector3.one * 0.4f;
                chipBetPool.RemoveAt(0);
            }

            temp.transform.localPosition = vPos;

            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(i * 0.1f)
                .Append(temp.transform.DOLocalMove(vTemp, 0.5f).SetEase(Ease.OutCubic))
                .Join(temp.GetComponent<SpriteRenderer>().DOFade(0, 0.8f).SetEase(Ease.InExpo))
                .OnComplete(() =>
                {
                    temp.SetActive(false);
                    chipBetPool.Add(temp);
                });
        }

        if (currentPlayer == thisPlayer)
        {
            User.userMain.AG += (int)data["chipStack"] - preNextStack;
        }

        // EffectMoneyChange((int)data["chipStack"] - this.preNextStack, this.preNextStack,
        //     currentPlayer.playerView.LbAg);
        currentPlayer.ag = (int)data["chipStack"];
        currentPlayer.setAg();

    }

    private void SetNextTurn(JObject data)
    {
        string namePlayer = (string)data["nextName"];
        if (string.IsNullOrEmpty(namePlayer))
        {
            potValue = (int)data["pot"];
            ResetNewTurn();
            return;
        }

        Player player = getPlayer(namePlayer);
        int indexDynamic = player._indexDynamic;
        int timeAction = (int)data["timeAction"] / 1000;
        int chipForCallInt = (int)data["chipForCall"];
        int valueAgPlayerInt = (int)data["nextStack"] - chipForCallInt;
        int valueBoxBex = listBoxBet[0]?.chip ?? 0;

        foreach (var p in players)
        {
            p.playerView?.setTurn(false, 0);
        }

        player.playerView.setTurn(true, timeAction);

        if (player == thisPlayer)
        {
            if (!btnCheckToggle.readInfoToggle())
            {
                btnBetContainer.gameObject.SetActive(true);
                lbChangeCard.SetActive(false);
                bgArrowSwap.SetActive(false);
                btnBetContainer.AutoBetIfClickRaise(timeAction);
                btnCheckToggle.gameObject.SetActive(false);

                bool isFirstTurn = (countTurn == 1);
                bool hasChipForCall = (chipForCallInt != 0);

                string secondButton = hasChipForCall ? "Call" : "Check";
                string thirdButton = isFirstTurn ? "Bet" : "Raise";

                btnBetContainer.setInfoBtn("Fold", secondButton, thirdButton, chipForCallInt - valueBoxBex);
                btnBetContainer.setValueInfo(valueAgPlayerInt, agTable, (int)data["pot"]);
            }
        }
        else if (btnBetContainer.gameObject.activeSelf)
        {
            btnBetContainer.SetFalseIsCountDown();
            btnBetContainer.gameObject.SetActive(false);
        }
    }

    private void ResetNewTurn()
    {
        // lay chip cho vao pot
        isTurn++;
        countTurn = 0;
        btnCheckToggle.gameObject.SetActive(false);

        if (isTurn < 6 && CheckNextTurnFail() > 1)
        {
            DealCardsAtEndOfTurn();
        }

        for (var j = 0; j < players.Count; j++)
        {
            // effect thu tien vao pot
            var indexDynamic = players[j]._indexDynamic;
            if (listBoxBet[indexDynamic] != null)
            {
                if (listBoxBet[indexDynamic].chip > 0)
                {
                    Debug.Log("indexDynamic la " + j);
                    Debug.Log("chip boxBet la " + listBoxBet[indexDynamic].chip);
                    for (var i = 0; i < 4; i++)
                    {
                        GameObject temp;
                        if (chipBetPool.Count < 1)
                        {
                            ChipBet goChipBet = Instantiate(chipReturnHkPoker, layerChip);
                            goChipBet.init(1, 0.4f);
                            temp = goChipBet.gameObject;
                        }
                        else
                        {
                            temp = chipBetPool[0];
                            temp.SetActive(true);
                            temp.transform.localScale = Vector3.one * 0.4f;
                            chipBetPool.RemoveAt(0);
                        }

                        temp.transform.localPosition = listPosBoxBet[indexDynamic];

                        Vector3 targetPos = potHkPoker.transform.localPosition + new Vector3(
                            Random.Range(-7f, 7f),
                            -40 + Random.Range(-7f, 7f),
                            0
                        );

                        Sequence seq = DOTween.Sequence();

                        seq.Append(temp.transform
                            .DOLocalMove(targetPos, 0.8f)
                            .SetEase(Ease.InBack));

                        seq.AppendInterval(0.6f);

                        seq.Append(temp.transform
                            .DOLocalMove(potHkPoker.transform.localPosition, 0.5f)
                            .SetEase(Ease.InElastic));

                        seq.PrependInterval(i * 0.02f);

                        seq.OnComplete(() =>
                        {
                            temp.gameObject.SetActive(false);
                            chipBetPool.Add(temp.gameObject);
                        });

                        seq.Play();
                    }
                }
            }
        }

        Invoke(nameof(PlaySoundCustom), 2f);
        Debug.Log("Gia tri cua potValue1 la " + potValue);

        potHkPoker.setValue(potValue, 2.0f);
        for (var i = 0; i < listBoxBet.Count; i++)
        {
            if (listBoxBet[i] != null)
            {
                if (listBoxBet[i].status == "Allin")
                {
                    listBoxBet[i].offSpriteAll();
                }

                if (
                    listBoxBet[i].status != "Allin" &&
                    listBoxBet[i].status != "Fold"
                )
                {
                    boxBetPool.Add(listBoxBet[i].gameObject);
                    listBoxBet[i].gameObject.SetActive(false);
                    listBoxBet[i] = null;
                }
                else
                {
                    listBoxBet[i].chip = 0;
                }
            }
        }
    }

    private void PlaySoundCustom()
    {
        playSound(SOUND_GAME.THROW_CHIP);
    }

    private void DealCardsAtEndOfTurn()
    {
        playSound(SOUND_GAME.DISPATCH_CARD);
        for (var j = 0; j < players.Count; j++)
        {
            if (!players[j].isFold && players[j].is_ready && players[j].vectorCardP1.Count < 5)
            {
                DealCardPlayer(j, 0, true, 0);
            }
        }
    }

    private int CheckNextTurnFail()
    {
        return Enumerable.Count(players, t => !t.isFold && t.is_ready);
    }

    private void InstantiateBoxBet(int indexDynamic, int chip, string status)
    {
        BoxBetShowHkPoker item = listBoxBet[indexDynamic];
        if (item == null)
        {
            if (boxBetPool.Count < 1)
            {
                item = Instantiate(boxBetPrefab, boxBetContainer);
            }
            else
            {
                item = boxBetPool[0].GetComponent<BoxBetShowHkPoker>();
                boxBetPool.RemoveAt(0);
                item.gameObject.SetActive(true);
            }
            listBoxBet[indexDynamic] = item;
            item.transform.localPosition = listPosBoxBet[indexDynamic];
        }

        if (indexDynamic == 0 && status == "Allin") isAllIn = true;
        item.setInfo(status, indexDynamic, chip);
    }

    private void DealCardPlayer(int index, float delay, bool isAction = true, int cardCode = 0)
    {
        Player player = players[index];
        int indexDynamic = player._indexDynamic;
        Vector2 vTemp = listPosCard[indexDynamic];

        bool isLeftTable = !(vTemp.x > 0);
        Vector2 vTarget;

        Card cardTemp = getCard();
        cardTemp.setTextureWithCode(0);

        cardTemp.transform.localPosition = new Vector2(-3, 175);
        cardTemp.transform.localScale = new Vector3(0.45f, 0.45f, 1);
        cardTemp.transform.localRotation = Quaternion.Euler(0, 0, 0);
        cardTemp.transform.SetParent(cardContainer, false);

        playerCards[indexDynamic].Add(cardTemp);

        if (!isLeftTable)
        {
            cardTemp.transform.SetSiblingIndex(20);
        }

        // Xác định vị trí đích của lá bài
        float cardWidth = cardTemp.GetComponent<RectTransform>().rect.width;
        float offset = (cardWidth / 2) * 0.38f * player.vectorCardP1.Count;

        if (isLeftTable)
        {
            vTarget = new Vector2(vTemp.x + offset, vTemp.y);
            if (player.vectorCardP1.Count > 0)
            {
                cardTemp.transform.SetSiblingIndex(player.vectorCardP1[^1].transform.GetSiblingIndex() + 1);
            }
        }
        else
        {
            vTarget = new Vector2(vTemp.x - offset, vTemp.y);
            if (player.vectorCardP1.Count > 0)
            {
                cardTemp.transform.SetSiblingIndex(player.vectorCardP1[^1].transform.GetSiblingIndex() - 1);
            }
        }

        if (isAction)
        {
            if (cardCode != 0)
            {
                // SoundManager.Instance.PlaySound("sound_cardFlipBJ");
                playSound(SOUND_GAME.CARD_FLIP_2);
                cardTemp.setTextureWithCode(cardCode);
            }

            cardTemp.transform.localRotation = Quaternion.Euler(0, 0, -90);
            cardTemp.transform.localScale = new Vector3(0, 0.45f, 1);

            cardTemp.transform.DOLocalMove(vTarget, 0.7f).SetEase(Ease.OutCubic).SetDelay(indexDynamic * 0.1f + delay);
            cardTemp.transform.DOLocalRotate(Vector3.zero, 0.6f).SetEase(Ease.OutCubic)
                .SetDelay(indexDynamic * 0.1f + delay);
            cardTemp.transform.DOScale(new Vector3(0.45f, 0.45f, 1), 0.6f).SetEase(Ease.OutCubic)
                .SetDelay(indexDynamic * 0.1f + delay);
        }
        else
        {
            cardTemp.setTextureWithCode(cardCode);
            cardTemp.transform.localPosition = vTarget;
        }

        player.vectorCardP1.Add(cardTemp);
    }

    private void CheckCardAndPut()
    {
        for (var i = 0; i < 5; i++)
        {
            for (var j = playerCards[i].Count - 1; j >= 0; j--)
            {
                Card card = playerCards[i][j];
                playerCards[i].RemoveAt(j);
                cardPool.Add(card);
            }
        }

        foreach (var player in players)
        {
            player.vectorCardP1 = new List<Card>();
        }
    }

    private async UniTask MoveCardsFinish(Card card, float delay, int zView)
    {
        await UniTask.Delay(TimeSpan.FromMilliseconds(delay));
        card.transform.SetSiblingIndex((int)GAME_ZORDER.Z_CARD + zView);

        await UniTask.WhenAll(
            card.transform.DOMove(dealerHkPoker.transform.position, 0.4f)
                .SetEase(Ease.OutCubic)
                .ToUniTask(),
            card.transform.DOScale(new Vector3(0.04f, 0.4f, 1f), 0.15f)
                .OnComplete(() => card.transform.DOScale(new Vector3(0.4f, 0.4f, 1f), 0.15f))
                .ToUniTask(),
            card.transform.DOLocalRotate(new Vector3(0, -15, 0), 0.15f)
                .OnComplete(() =>
                {
                    card.setTextureWithCode(0);
                    card.transform.localRotation = Quaternion.Euler(0, 15, 0);
                    card.transform.DOLocalRotate(Vector3.zero, 0.15f);
                })
                .ToUniTask()
        );

        await UniTask.Delay(TimeSpan.FromSeconds(0.6f));
        cardPool.Add(card);
    }

    private async UniTask RecallCards()
    {
        var delay = 0;
        var zView = 0;
        List<UniTask> tasks = new List<UniTask>();
        for (var i = 0; i < 5; i++)
        {
            for (var j = playerCards[i].Count() - 1; j >= 0; j--)
            {
                var card = playerCards[i][j];
                playerCards[i].RemoveAt(j);
                tasks.Add(MoveCardsFinish(card, delay, zView));
                zView++;
                delay += 100;
            }
        }

        await UniTask.WhenAll(tasks);
        // await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        foreach (Transform child in cardContainer.transform)
        {
            child.gameObject.SetActive(false);
        }

        for (var i = 0; i < players.Count(); i++)
        {
            players[i].vectorCardP1 = new List<Card>();
        }

        await UniTask.Delay((delay + 700) / 1000);
        HandleGame.nextEvt();
        checkAutoExit();
    }

    private void HandleCardsFinish(JObject data)
    {
        var array = data["declarePacketsTrans"];
        for (var i = 0; i < array.Count(); i++)
        {
            var player = getPlayer((string)array[i]?["userName"]);
            var indexDynamic = player._indexDynamic;
            var startCard = 1;
            if (array[i]?["arr"] != null)
            {
                startCard = 0;
            }
            // this.showCardResultAnimattion(indexDynamic, startCard, 1);
        }

        RecallCards();
    }

    private void ResetViewGame(JObject data)
    {
        Debug.Log("chay vao ham resetGameVIew");
        HandleCardsFinish(data);
        foreach (var player in players)
        {
            if (player.isFold)
            {
                player.isFold = false;
                player.playerView.setDark(false);
            }
        }

        foreach (var item in listBoxBet)
        {
            if (item != null)
            {
                boxBetPool.Add(item.gameObject);
                item.gameObject.SetActive(false);
            }
        }

        listBoxBet = new List<BoxBetShowHkPoker>() { null, null, null, null, null };
    }

    private string ShowAnimOnBegin(bool isStart = true)
    {
        string animName = isStart ? "startgame" : "startgame2";
        animStart.gameObject.SetActive(true);
        animStart.AnimationState.SetAnimation(0, animName, false);
        return animName;
    }

    private async void CallAsyncFunction(Awaitable function)
    {
        try
        {
            await function;
        }
        catch (Exception e)
        {
            if (e.GetType() != typeof(MissingReferenceException))
                Debug.LogError("Error on calling async function: " + e.Message);
        }
    }

    public void OnClickSwapCard()
    {
        SocketSend.sendChangeCard(true);
        (thisPlayer.vectorCardP1[0], thisPlayer.vectorCardP1[3]) =
            (thisPlayer.vectorCardP1[3], thisPlayer.vectorCardP1[0]);
        lbChangeCard.SetActive(false);
        bgArrowSwap.SetActive(false);
        bgArrowSwap.transform.DOKill();
        thisPlayer.vectorCardP1[3].setDark(false, spriteFrameMask);
        var vTemp = thisPlayer.vectorCardP1[3].transform.position;
        var zindexTemp = thisPlayer.vectorCardP1[3].transform.GetSiblingIndex();
        thisPlayer.vectorCardP1[3].transform.position = thisPlayer.vectorCardP1[0].transform.position;
        thisPlayer.vectorCardP1[3].transform.SetSiblingIndex(thisPlayer.vectorCardP1[0].transform.GetSiblingIndex());
        thisPlayer.vectorCardP1[0].transform.position = vTemp;
        thisPlayer.vectorCardP1[0].transform.SetSiblingIndex(zindexTemp);
        SocketIOManager.getInstance()
            .emitSIOCCCNew(Config.formatStr("ClickSwapCard_%s", CURRENT_VIEW.getCurrentSceneName()));
        (playerCards[0][0], playerCards[0][3]) = (playerCards[0][3], playerCards[0][0]);
    }

    public void OnClickCancel()
    {
        thisPlayer.vectorCardP1[3].setDark(false, spriteFrameMask);
        SocketSend.sendChangeCard(false);
        lbChangeCard.SetActive(false);
        bgArrowSwap.SetActive(false);
        bgArrowSwap.transform.DOKill();
        SocketIOManager.getInstance()
            .emitSIOCCCNew(Config.formatStr("ClickNotSwapCard_%s", CURRENT_VIEW.getCurrentSceneName()));
    }
    public void OnClickTip()
    {
        SocketSend.sendTip();
    }
}