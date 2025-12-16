using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
enum TYPE
{
    NONE,
    MONEY,
    NUMBER
}
public class EdbController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    TYPE TYPE_EDB = TYPE.NONE;
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
        if (TYPE_EDB == TYPE.NONE) return;
        isCheckWithAg = isAg;
        edb.onValueChanged.RemoveAllListeners();
        edb.onValueChanged.AddListener(onEdbChange);
        edb.onEndEdit.AddListener(onEdbEndEdit);
    }

    public void onEdbChange(string value)
    {
        if (TYPE_EDB == TYPE.NONE) return;

        if (string.IsNullOrEmpty(value))
        {
            number_input = 0;
            return;
        }

        if (!Globals.Config.TrySplitToLong(value, out long parsedValue))
            return;
        if (isCheckWithAg && Globals.User.userMain.AG < parsedValue)
        {
            parsedValue = Globals.User.userMain.AG;
        }
        else if (!isCheckWithAg && Globals.User.userMain.agSafe < parsedValue)
        {
            parsedValue = Globals.User.userMain.agSafe;
        }

        number_input = parsedValue;
    }
    public void onEdbEndEdit(string value)
    {
        if (TYPE_EDB == TYPE.NONE) return;

        if (!Globals.Config.TrySplitToLong(value, out long parsedValue))
        {
            edb.text = "0";
            number_input = 0;
            return;
        }

        if (isCheckWithAg && Globals.User.userMain.AG < parsedValue)
        {
            parsedValue = Globals.User.userMain.AG;
        }
        else if (!isCheckWithAg && Globals.User.userMain.agSafe < parsedValue)
        {
            parsedValue = Globals.User.userMain.agSafe;
        }

        number_input = parsedValue;

        edb.text = (TYPE_EDB == TYPE.NUMBER)
            ? Globals.Config.FormatNumber(parsedValue)
            : Globals.Config.FormatMoney(parsedValue);
    }

    public long getLong()
    {
        return number_input;
    }
}
