using UnityEngine.UI;
using UnityEngine;
using System.ComponentModel.DataAnnotations;
using System;
public class HangryMeter : MonoBehaviour
{
    [SerializeField] Image hangryRedLevel;


	private void Start()
	{
		if( HangryController.Current != null)
		{
			HangryController.Current.OnMeterChanged += setHangryRedLevel;
		}
	}

	public void setHangryRedLevel(float hangryAmount, float maxHangry = 100.0f)
    {
        hangryAmount = Mathf.Min(hangryAmount, maxHangry);
        hangryRedLevel.fillAmount = hangryAmount / maxHangry;
		if (hangryRedLevel.fillAmount > 0.0f)
        {
			hangryRedLevel.color = Color.Lerp(Color.yellow, Color.red, hangryRedLevel.fillAmount);
        }
    }
}
