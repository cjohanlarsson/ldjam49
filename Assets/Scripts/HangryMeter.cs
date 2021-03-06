using UnityEngine.UI;
using UnityEngine;
//using System.ComponentModel.DataAnnotations;
using System;
public class HangryMeter : MonoBehaviour
{
    [SerializeField] Image hangryRedLevel = null;

	private HangryController _toddlerToWatch = null;
	public HangryController toddlerToWatch
	{
		set
		{
			if(_toddlerToWatch != null)
			{
				_toddlerToWatch.OnMeterChanged -= setHangryRedLevel;
			}
			_toddlerToWatch = value;
			_toddlerToWatch.OnMeterChanged += setHangryRedLevel;
			setHangryRedLevel(_toddlerToWatch.Hangriness, _toddlerToWatch.maxHangry);
		}
	}

	public void setHangryRedLevel(float hangryAmount, float maxHangry = 100.0f)
    {
        hangryAmount = Mathf.Min(hangryAmount, maxHangry);
        hangryRedLevel.fillAmount = hangryAmount / maxHangry;
		hangryRedLevel.color = Color.Lerp(Color.yellow, Color.red, hangryRedLevel.fillAmount);
    }
}
