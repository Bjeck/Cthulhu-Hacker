using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Sound : MonoBehaviour {

    public GameManager gm;


    public AudioSource slam;
    public AudioSource slam_low;
    public AudioSource reverse_bell;
    public AudioSource noise;

    public AudioSource[] writings;



    public void PlaySound(AudioSource sc)
    {
        sc.Play();
    }

    public void PlayWriting()
    {
        int r = Random.Range(0, writings.Length);
        writings[r].Play();
    }

    public void StopSound(AudioSource sc)
    {
        sc.Stop();
    }


}
