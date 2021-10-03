using UnityEngine.UI;
using UnityEngine;
using System.ComponentModel.DataAnnotations;
using System;
public class HangryMeter : MonoBehaviour
{
    [SerializeField] Image hangryRedLevel;
	[SerializeField] HangryController toddlerToWatch;

	private void Start()
	{
		if(toddlerToWatch != null)
		{
			toddlerToWatch.OnMeterChanged += setHangryRedLevel;
		}
	}

	public void setHangryRedLevel(float hangryAmount, float maxHangry = 100.0f)
    {
        hangryAmount = Mathf.Min(hangryAmount, maxHangry);
        hangryRedLevel.fillAmount = hangryAmount / maxHangry;
		hangryRedLevel.color = Color.Lerp(Color.yellow, Color.red, hangryRedLevel.fillAmount);
    }
}
