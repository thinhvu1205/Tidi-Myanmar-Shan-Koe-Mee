using UnityEngine;
using UnityEngine.UI;

public class HistoryManager : MonoBehaviour
{
    public Image chip;

    public Sprite[] chipSprites;

    // Set sprite for the chip by index
    public void SetTexture(int state)
    {
        if (state >= 0 && state < chipSprites.Length)
        {
            chip.sprite = chipSprites[state];
        }
        else
        {
            Debug.LogWarning("SetTexture: Invalid state index " + state);
        }
    }
}
