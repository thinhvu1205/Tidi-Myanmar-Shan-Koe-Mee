using System;
using System.Collections;
using DG.Tweening;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChipReturnHkPoker : ChipBet
{
    [SerializeField] private TextMeshProUGUI lbText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject containerBet;

    public float SetInfo(Vector2 target1, Vector2 target2, float delay, int numChip)
    {
        init(0, 0.8f);
        gameObject.GetComponent<Image>().enabled = false;
        canvasGroup.alpha = 0;
        containerBet.SetActive(true);

        Sequence seq = DOTween.Sequence();

        seq.AppendInterval(0.5f)
            .Append(transform.DOLocalMove(new Vector3(target1.x, target1.y - 40, 0), 0.6f).SetEase(Ease.OutCubic))
            .Join(transform.DOScale(1.4f, 0.3f))
            .Append(transform.DOScale(1.0f, 0.3f).SetEase(Ease.InCubic))
            .AppendCallback(() =>
            {
                lbText.text = Config.FormatMoney(numChip);
                canvasGroup.DOFade(1, 0.4f);
            })
            .AppendInterval(delay)
            .AppendCallback(() =>
            {
                lbText.transform.parent.gameObject.SetActive(false);
                StartCoroutine(PlaySoundWithDelay());
            })
            .Append(transform.DOLocalMove(target2, 1.0f).SetEase(Ease.OutCubic))
            .Join(transform.DOScale(0.5f, 1.0f).SetEase(Ease.OutCubic))
            .AppendCallback(() => Destroy(gameObject));


        float duration = seq.Duration();
        return duration;
    }

    private IEnumerator PlaySoundWithDelay(float delay = 1f)
    {
        yield return new WaitForSeconds(delay);
        SoundManager.instance.playEffectFromPath(SOUND_GAME.THROW_CHIP);
    }

}