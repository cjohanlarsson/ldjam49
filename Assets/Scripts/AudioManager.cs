using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(HangryController))]
public class AudioManager : MonoBehaviour
{
    HangryController hc;
    private int currentHangrinessTier = 0;

    [Header("Audio")]
    [SerializeField] AudioClip[] layers;
    private AudioSource audioSource;

    private void Awake()
    {
        hc = GetComponent<HangryController>();
        audioSource = this.GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        print(layers[0]);
        audioSource.clip = layers[0];
        audioSource.Play();
        StartCoroutine(hangryTimer());
    }

    private IEnumerator hangryTimer()
    {
        while(true)
        {
            yield return new WaitForSeconds(1 / hc.hangryRateOfIncrease);
            MusicLayerSwitcher();
        }
    }

    void MusicLayerSwitcher()
    {
        if (currentHangrinessTier == hc.HangrinessTier)
        {
            print("Hangriness unchanged");
            print(currentHangrinessTier);
            print(hc.HangrinessTier);
            print(hc.getHangryLevel());
        }
        else
        {
            currentHangrinessTier = hc.HangrinessTier;
            if (currentHangrinessTier == 0)
            {
                print(currentHangrinessTier);
                print(hc.HangrinessTier);
                print(hc.getHangryLevel());
                audioSource.clip = layers[0];
                audioSource.Play();
            }
            else if (currentHangrinessTier == 1)
            {
                print(currentHangrinessTier);
                print(hc.HangrinessTier);
                print(hc.getHangryLevel());
                audioSource.clip = layers[1];
                audioSource.Play();
            }
            else if (currentHangrinessTier == 2)
            {
                print(currentHangrinessTier);
                print(hc.HangrinessTier);
                print(hc.getHangryLevel());
                audioSource.clip = layers[2];
                audioSource.Play();
            }
            else if (currentHangrinessTier == 3)
            {
                print(currentHangrinessTier);
                print(hc.HangrinessTier);
                print(hc.getHangryLevel());
                audioSource.clip = layers[3];
                audioSource.Play();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
