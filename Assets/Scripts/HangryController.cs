using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangryController : MonoBehaviour
{
    public static HangryController Current { get; private set; }
    public event System.Action<float, float> OnMeterChanged;

    [SerializeField] float hangryRateOfIncrease = 1.0f;
    [SerializeField] public float maxHangry = 50.0f;
    [SerializeField] float _hangryLevel = 0;
    
    private float Hangriness
    {
        get { return _hangryLevel; }
        set
        {
            _hangryLevel = value;
            if(OnMeterChanged != null)
                OnMeterChanged(_hangryLevel, maxHangry);
        }
    }

	private void Awake()
	{
        Current = this;
	}

	private void OnDestroy()
	{
        if (Current == this)
            Current = null;
	}

	// Start is called before the first frame update
	void Start()
    {
        StartCoroutine(hangryTimer());
    }

    private IEnumerator hangryTimer()
    {
        while (true)
        {
            Hangriness += 1;
            if (Hangriness >= maxHangry) { break; }
            yield return new WaitForSeconds(1 / hangryRateOfIncrease);
        }
    }

    public float getHangryLevel()
    {
        return Hangriness;
    }

    public void makeHangrier()
    {
        Hangriness += 5;
    }

    public void makeMuchHangrier()
    {
        Hangriness += 10;
    }

    public void makeSuperHangry()
    {
        Hangriness += 20;
    }

    public void resetHangry()
    {
        StopAllCoroutines();
        Hangriness = 0;
        StartCoroutine(hangryTimer());
    }
}
