using UnityEngine.UI;
using UnityEngine;
using System.ComponentModel.DataAnnotations;
using System;
public class HangryMeter : MonoBehaviour
{
    [SerializeField] Image hangryRedLevel;


    public void setHangryRedLevel(float hangryAmount, float maxHangry = 100.0f)
    {
        hangryAmount = Mathf.Min(hangryAmount, maxHangry);
        hangryRedLevel.fillAmount = hangryAmount / maxHangry;
    }
}
