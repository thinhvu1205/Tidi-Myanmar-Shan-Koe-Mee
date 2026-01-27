using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class MenuFriend : BaseView
{
    public override void OnDestroy()
    {

        Destroy(gameObject);
    }
    void Awake()
    {
        // SocketSend.sendChatFriends(11770614, "hello đại ca");
        // SocketSend.deleteFriend(new List<int> { });
       // SocketSend.sendRequestAddFriend(8352532);
    }

    public void OpenListFriendView()
    {
        UIManager.instance.showListFriendView();

    }
    public void Rule()
    {
        UIManager.instance.showWebView(Globals.Config.linkRuleFriend);
    }
    public void commingsoon()
    {
        UIManager.instance.showComingsoon();
    }

}