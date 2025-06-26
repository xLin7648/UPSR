using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    public GameObject comboObj;
    public TMP_Text comboTex;

    public int combo;

    private void Awake()
    {
        instance = this;
    }

    public void Hit()
    {
        if (++combo >= 3)
        {
            comboObj.SetActive(true);
        }

        comboTex.text = combo.ToString();
    }

    public void Miss()
    {
        comboObj.SetActive(false);
        combo = 0;

        comboTex.text = combo.ToString();
    }
}
