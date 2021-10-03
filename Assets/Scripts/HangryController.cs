using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangryController : MonoBehaviour
{
    public event System.Action<float, float> OnMeterChanged;

    [SerializeField] float hangryRateOfIncrease = 1.0f;
    [SerializeField] public float maxHangry = 50.0f;
    [SerializeField] float _hangryLevel = 0;

    [Header("Audio")]
    [SerializeField] AudioClip[] hangrySounds;
    
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
	}

	private void OnDestroy()
	{
	}

	// Start is called before the first frame update
	void Start()
    {
        BottleManager.Current.OnScoreChanged += (s) =>
        {
            resetHangry();
        };

        StartCoroutine(hangryTimer());
    }

    void PlayHangrySoundAt(int index)
	{
        if (hangrySounds.Length > index)
        {
            var hangrySound = hangrySounds[index];
            AudioSource.PlayClipAtPoint(hangrySound, this.transform.position);
        }
	}

    private IEnumerator hangryTimer()
    {
        while (true)
        {
            Hangriness += 1;
            if (100 * Hangriness / maxHangry == 26)
            {
                PlayHangrySoundAt(0);
            }
            else if (100 * Hangriness / maxHangry == 50)
            {
                PlayHangrySoundAt(1);
            }
            else if (100 * Hangriness / maxHangry == 76)
            {
                PlayHangrySoundAt(2);
            }
            if (Hangriness >= maxHangry) {
                GetComponent<ToddlerController>().throwTantrum();
                break; }
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


    public void reduceHangry()
    {
        Hangriness -= 10;
    }

    public void reduceHangryAlot()
    {
        Hangriness -= 25;
    }

    public void resetHangry()
    {
        StopAllCoroutines();
        Hangriness = 0;
        StartCoroutine(hangryTimer());
    }
}
