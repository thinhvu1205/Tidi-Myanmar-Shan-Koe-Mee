using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class HandleDataHongKongView
{
    public static void processData(JObject jData)
    {
        var gameView = (HongKongPokerView)UIManager.instance.gameView;
        if (gameView == null) return;
        string evt = (string)jData["evt"];
        switch (evt)
        {
            case "timeToStart":
                gameView.HandleTimeToStart(jData);
                break;
            case "show_card":
                gameView.HandleShowCard(jData);
                break;
            case "lc":
                gameView.HandleLc(jData);
                break;
            case "bm":
                gameView.HandleBm(jData);
                break;
            case "cab":
                gameView.HandleCab(jData);
                break;
            case "buyin":
                gameView.HandleBuyIn(jData);
                break;
            case "bc":
                gameView.HandleBc(jData);
                break;
            case "finish":
                gameView.HandleFinish(jData);
                break;
            case "tip":
                gameView.HandlerTip(jData);
                break;
        }
    }
}