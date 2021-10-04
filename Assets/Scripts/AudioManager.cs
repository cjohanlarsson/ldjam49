using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [RequireComponent(typeof(HangryController))]
[RequireComponent(typeof(AudioSource))]

public class AudioManager : MonoBehaviour
{
    // HangryController hc;
    private float currentHangriness = 0;

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
        if (currentHangriness == Hangriness)
        {
            print("Hangriness unchanged");
            print(Hangriness);
        }
        else
        {
            currentHangriness = Hangriness;
            if (currentHangriness == 0)
            {
                audioSource.clip = layers[0];
                audioSource.Play();
            }
            else if (currentHangriness == 13)
            {
                audioSource.clip = layers[1];
                audioSource.Play();
            }
            else if (currentHangriness == 25)
            {
                audioSource.clip = layers[2];
                audioSource.Play();
            }
            else if (currentHangriness == 38)
            {
                audioSource.clip = layers[3];
                audioSource.Play();
            }
            else if (currentHangriness == 50)
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
