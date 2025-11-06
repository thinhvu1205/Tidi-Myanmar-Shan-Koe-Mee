using Newtonsoft.Json.Linq;
using UnityEngine;

public class HandleBaucua
{
    public static void processData(JObject jData) // class nay dung de viet them cac evt rieng cua game binh a nhe. Con may cai chung nhu stable,ctable o ben handleGame co r/
    {
        var gameView = (BaucuaGameView)UIManager.instance.gameView;
        if (gameView == null) return;
        string evt = (string)jData["evt"];
        switch (evt)
        {
            case "startgame":
                Debug.Log($"Tinh=))startgame: {jData.ToString(Newtonsoft.Json.Formatting.None)}");
                gameView.handleStart(jData);
                break;
            case "bet":
                gameView.handleBetGame(jData);
                break;
            case "finish":
                gameView.handleFinish(jData);
                break;
            case "history":
                gameView.handleHistory(jData);
                break;
        }
    }
}