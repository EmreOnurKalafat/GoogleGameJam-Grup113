using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Sound Source")]
   [SerializeField] AudioSource musicSource;

    [Header("Sound Clips")]
   public AudioClip backgroundMusic;

   private void Start()
   {
       musicSource.clip = backgroundMusic;
       musicSource.Play();
   }
}
