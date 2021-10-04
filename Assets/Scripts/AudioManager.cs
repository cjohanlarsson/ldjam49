using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [RequireComponent(typeof(HangryController))]
[RequireComponent(typeof(AudioSource))]

public class AudioManager : MonoBehaviour
{
    // HangryController hc;
    int currentHangrinessTier = 0;
    int HangrinessTier = 0;

    [Header("Audio")]
    [SerializeField] AudioClip[] layers;
    [SerializeField] ToddlerManager toddlerManager;
    private AudioSource audioSource;

    private void Awake()
    {
        // hc = GetComponent<HangryController>();
        audioSource = this.GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource.clip = layers[0];
        audioSource.Play();
        StartCoroutine(hangryTimer());
    }

    private IEnumerator hangryTimer()
    {
        while(true)
        {
            yield return new WaitForSeconds(1);
            MusicLayerSwitcher();
        }
    }

    void MusicLayerSwitcher()
    {
        float Hangriness = toddlerManager.Hangriness;
        if (Hangriness < 13)
        {
            HangrinessTier = 0;
        }
        else if (Hangriness < 25)
        {
            HangrinessTier = 1;
        }
        else if (Hangriness < 38)
        {
            HangrinessTier = 2;
        }
        else if (Hangriness < 50)
        {
            HangrinessTier = 3;
        }
        else if (Hangriness >= 50)
        {
            HangrinessTier = 4;
        }

        if (currentHangrinessTier == HangrinessTier)
        {
            print("Hangriness Tier Unchanged");
        }
        else
        {
            currentHangrinessTier = HangrinessTier;
            if (currentHangrinessTier == 0)
            {
                audioSource.clip = layers[0];
                audioSource.Play();
            }
            else if (currentHangrinessTier == 1)
            {
                audioSource.clip = layers[1];
                audioSource.Play();
            }
            else if (currentHangrinessTier == 2)
            {
                audioSource.clip = layers[2];
                audioSource.Play();
            }
            else if (currentHangrinessTier == 3)
            {
                audioSource.clip = layers[3];
                audioSource.Play();
            }
            else if (currentHangrinessTier == 4)
            {
                audioSource.Stop();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
