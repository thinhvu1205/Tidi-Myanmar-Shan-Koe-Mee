using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Spine;

public class ItemChatAction : MonoBehaviour
{
    // [SerializeField] Image img;
    [SerializeField] List<Sprite> lsAction = new List<Sprite>();
    [SerializeField] SkeletonGraphic skeletonGraphic;
    [SerializeField] List<SkeletonDataAsset> lsAnimData = new List<SkeletonDataAsset>();
    public IEnumerator setData(int idAnimation, Vector3 targetV3)
    {
        Globals.Logging.Log("-=-=idAnimation " + idAnimation);

        if (idAnimation < 0 || idAnimation >= lsAnimData.Count)
            yield break;

        // ================== SHOW IMAGE ==================
        // if (idAnimation < lsAction.Count && lsAction[idAnimation] != null)
        // {
        //     img.gameObject.SetActive(true);
        //     img.sprite = lsAction[idAnimation];
        //     img.SetNativeSize();
        // }

        yield return new WaitForSeconds(0.1f);

        // ================== MOVE ==================
        Tween moveTween = transform.DOMove(targetV3, 1f)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);

        yield return moveTween.WaitForCompletion();

        // ================== PLAY SPINE ==================
        // img.gameObject.SetActive(false);
        skeletonGraphic.gameObject.SetActive(true);

        skeletonGraphic.skeletonDataAsset = lsAnimData[idAnimation];
        skeletonGraphic.Initialize(true);
        skeletonGraphic.AnimationState.Complete -= OnSpineComplete;

        TrackEntry trackEntry = skeletonGraphic.AnimationState.SetAnimation(0, "animation", false);
        skeletonGraphic.AnimationState.Complete += OnSpineComplete;

        // ================== SOUND ==================
        PlaySound(idAnimation);

        // ================== FAILSAFE DESTROY ==================
        float duration = trackEntry.Animation.Duration;
        Destroy(gameObject, duration + 0.5f);
    }
    private void OnSpineComplete(TrackEntry trackEntry)
    {
        Destroy(gameObject);
    }
    private void PlaySound(int idAnimation)
    {
        string sound = idAnimation switch
        {
            0 => Globals.SOUND_CHAT.BOOM,
            1 => Globals.SOUND_CHAT.KISS,
            2 => Globals.SOUND_CHAT.ROSE,
            3 => Globals.SOUND_CHAT.BEER,
            4 => Globals.SOUND_CHAT.TOMATO,
            5 => Globals.SOUND_CHAT.WATER,
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(sound))
        {
            SoundManager.instance.playEffectFromPath(sound);
        }
    }
}