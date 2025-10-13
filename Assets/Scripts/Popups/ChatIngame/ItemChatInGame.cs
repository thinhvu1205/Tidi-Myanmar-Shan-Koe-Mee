using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;
using System.Linq;
using System;
using DG.Tweening;
using System.Collections;

public class ItemChatInGame : MonoBehaviour
{
    [SerializeField]
    Image bkg;

    [SerializeField]
    TextMeshProUGUI txtMsg;

    [SerializeField]
    SkeletonGraphic anim;
    [SerializeField] GameObject m_Speaker;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void setSpeak(PlayerView playerView)
    {
        StartCoroutine(SpeakRoutine(playerView));
    }

    private IEnumerator SpeakRoutine(PlayerView playerView)
    {
        // Ẩn các phần không cần
        bkg.gameObject.SetActive(false);
        txtMsg.gameObject.SetActive(false);
        anim.gameObject.SetActive(false);

        // Lấy thông tin vị trí nhân vật
        var pos = playerView.transform.localPosition;
        var sizePl = playerView.GetComponent<RectTransform>().sizeDelta.x;
        var scale = playerView.transform.localScale;
        float bubbleWidth = 50f;
        Vector3 finalPos = pos;
        finalPos.x += sizePl * 0.5f * scale.x + bubbleWidth * 0.5f + 10 + (pos.x < 0 ? 0 : -170);
        finalPos.y += 50;
        m_Speaker.transform.localPosition = finalPos;
        float scaleFactor = 0.5f;
        m_Speaker.transform.localScale = new Vector3((pos.x < 0 ? 1 : -1) * scaleFactor, scaleFactor, 1);
        m_Speaker.SetActive(true);
        m_Speaker.transform.DOShakePosition(
            duration: 2f,
            strength: new Vector3(5, 5, 0),
            vibrato: 10,
            randomness: 90f,
            snapping: false,
            fadeOut: true
        );
        yield return new WaitForSeconds(2f);
        m_Speaker.SetActive(false);
        DOVirtual.DelayedCall(0.1f, () =>
        {
            Destroy(gameObject);
        });
    }




    public void setMsg(string msg, string type, PlayerView playerView)
    {
        var pos = playerView.transform.localPosition;
        var sizePl = playerView.GetComponent<RectTransform>().sizeDelta.x;
        var scale = playerView.transform.localScale;

        if (!msg.Equals(""))
        {
            bkg.gameObject.SetActive(true);
            txtMsg.gameObject.SetActive(true);
            anim.gameObject.SetActive(false);

            txtMsg.text = msg;
            var sizeBg = bkg.rectTransform.sizeDelta;

            if (msg.Length > 35)
            {
                sizeBg.x = 300;
            }
            else
            {
                sizeBg.x = 200;
            }
            bkg.rectTransform.sizeDelta = sizeBg;
            bkg.transform.localScale = new Vector3(pos.x < 0 ? 1 : -1, 1, 1);
            txtMsg.transform.localScale = new Vector3(pos.x < 0 ? 1 : -1, 1, 1);
            pos.x += sizePl * .5f * scale.x + sizeBg.x * .5f + 10 + (pos.x < 0 ? 0 : -300);
            pos.y += 50;
        }
        else if (type != "")
        {
            SkeletonDataAsset skeEmo = null;
            if (type.Contains("*e"))
            {
                var idAnimation = string.Join("", type.ToCharArray().Where(Char.IsDigit));
                //Globals.Logging.Log("-=idAnimation   " + idAnimation);
                skeEmo = BundleHandler.LoadSkeletonDataAsset("GameView/Emo/emoticon/e" + idAnimation + "/skeleton_SkeletonData");
            }

            if (skeEmo != null)
            {
                bkg.gameObject.SetActive(false);
                txtMsg.gameObject.SetActive(false);
                anim.gameObject.SetActive(true);
                anim.skeletonDataAsset = skeEmo;
                anim.startingAnimation = "animation";
                anim.startingLoop = true;
                anim.Initialize(true);
            }
        }

        transform.localPosition = pos;
        Globals.Logging.Log("Pos chat ==" + pos.x + "," + pos.y);
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        DOTween.Sequence().Append(canvasGroup.DOFade(1, 0.5f)).AppendInterval(2).AppendCallback(() =>
        {
            Destroy(gameObject);
        });
    }
}
