using UnityEngine;
using TMPro;
using DG.Tweening; // Đảm bảo rằng bạn đã cài DoTween

public class FrameChipView : MonoBehaviour
{
    public bool isRunEffect = false;
    public float isCurMoney = 0;
    public float isLastMoney = 0;
    public float isDeltaMoney = 0;
    public float isDeltaTime = 0;
    public float totalBet = 0;
    public TextMeshProUGUI lbAg; // Thẻ UI hiển thị số tiền cược
    public GameObject nodeArrow; // GameObject chứa hiệu ứng mũi tên

    public void OnBet(long money, bool isEffect = true)
    {
        if (money == 0) return;
        SetupEffectChangeMoney(totalBet, totalBet + money);
        totalBet += money;

        if (isEffect)
        {
            GameObject effectUp = Instantiate(nodeArrow, transform);
            effectUp.transform.localScale = new Vector3(1, 0, 1); // Khởi tạo scale Y bằng 0 (ẩn)
            effectUp.SetActive(true); // Bật hiệu ứng

            // Sử dụng DoTween để thay đổi scaleY từ 0 lên 1
            effectUp.transform.DOKill(); // Hủy bỏ các tween cũ nếu có
            effectUp.transform.DOScaleY(1f, 0.4f).SetEase(Ease.OutBack).OnKill(() =>
            {
                Destroy(effectUp); // Hủy hiệu ứng khi hoàn thành
            });
        }
    }

    public void ResetValue()
    {
        lbAg.text = "0";
        totalBet = 0;
    }

    public void OnHide()
    {
        Destroy(gameObject);
    }

    private void SetupEffectChangeMoney(float start, float end)
    {
        isCurMoney = start;
        isLastMoney = end;
        float delta = end - start;
        float around = 0;

        if (delta == 0) return;

        if (delta > 0)
        {
            if (delta < 10) around = delta / 2;
            else if (delta < 300) around = delta / 5;
            else around = delta / 10;
            if (around < 1) around = 1;
        }
        else
        {
            if (delta > -10) around = delta / 2;
            else if (delta > -300) around = delta / 5;
            else around = delta / 10;
        }

        isDeltaMoney = Mathf.FloorToInt(around);
        isRunEffect = true;
    }

    void Update()
    {
        if (isRunEffect)
        {
            isDeltaTime += Time.deltaTime;
            if (isDeltaTime >= 0.01f)
            {
                isDeltaTime = 0;
                isCurMoney += isDeltaMoney;

                if ((isCurMoney >= isLastMoney && isDeltaMoney > 0) || (isCurMoney <= isLastMoney && isDeltaMoney < 0))
                {
                    isCurMoney = isLastMoney;
                    isRunEffect = false;
                }

                lbAg.text = FormatMoney(isCurMoney);
            }
        }
    }

    private string FormatMoney(float money)
    {
        return string.Format("{0:N0}", money); // Định dạng số tiền với dấu phẩy phân cách (ví dụ: 1,000)
    }
}
