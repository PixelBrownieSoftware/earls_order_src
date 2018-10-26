using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class s_sound : MonoBehaviour {

    static AudioSource audio;

    private void Awake()
    {
        audio = GetComponent<AudioSource>();
    }

    public static void PlaySound(AudioClip sound)
    {
        audio.PlayOneShot(sound);
    }

}
