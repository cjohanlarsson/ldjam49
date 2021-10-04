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
                // Lowest tier - Starts at the beginning, but should start where tier 1 track left off
                audioSource.clip = layers[0];
                audioSource.Play();
            }
            else if (currentHangrinessTier == 1)
            {
                // 2nd Lowest Tier - Should start where tier 0 or 2 track left off
                audioSource.clip = layers[1];
                audioSource.Play();
            }
            else if (currentHangrinessTier == 2)
            {
                // 2nd highest tier - Should start where tier 1 or 3 track left off
                audioSource.clip = layers[2];
                audioSource.Play();
            }
            else if (currentHangrinessTier == 3)
            {
                // Highest tier - Should start where tier 2 track left off
                audioSource.clip = layers[3];
                audioSource.Play();
            }
            else if (currentHangrinessTier == 4)
            {
                // Game Over Music - Should always start from the beginning
                audioSource.clip = layers[4];
                audioSource.Play();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
