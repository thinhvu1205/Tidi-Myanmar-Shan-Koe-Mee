using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
enum TYPEINPUT
{
    NONE,
    MONEY,
    NUMBER
}
public class EdbControllerOpen : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    TYPEINPUT TYPE_EDB = TYPEINPUT.NONE;
    [SerializeField]
    TMP_InputField edb;

    [SerializeField]
    bool isCheckWithAg = true;
    //[SerializeField]
    //public bool isCheckWithAgSafe = false;
    long number_input = 0;
    void Start()
    {
        SetCheckNumber(isCheckWithAg);
    }

    public void SetCheckNumber(bool isAg)
    {
        if (TYPE_EDB == TYPEINPUT.NONE) return;
        isCheckWithAg = isAg;
        edb.onValueChanged.RemoveAllListeners();
        edb.onValueChanged.AddListener(onEdbChange);
    }

    public void onEdbChange(string value)
    {
        if (TYPE_EDB == TYPEINPUT.NONE) return;

        string textNumber = edb.text;

        if (string.IsNullOrEmpty(textNumber))
        {
            number_input = 0;
            return;
        }

        // Loại bỏ mọi ký tự không phải số (chặn luôn dấu '-')
        string cleanText = "";
        foreach (char c in textNumber)
        {
            if (char.IsDigit(c))
            {
                cleanText += c;
            }
        }

        number_input = Globals.Config.splitToLong(cleanText);

        if (isCheckWithAg && Globals.User.userMain.AG < number_input)
        {
            number_input = Globals.User.userMain.AG;
        }
        else if (!isCheckWithAg && Globals.User.userMain.agSafe < number_input)
        {
            number_input = Globals.User.userMain.agSafe;
        }

        if (TYPE_EDB == TYPEINPUT.NUMBER)
        {
            edb.text = Globals.Config.FormatNumber(number_input);
        }
        else
        {
            edb.text = Globals.Config.FormatMoney(number_input);
        }
    }


    public long getLong()
    {
        return number_input;
    }
}
