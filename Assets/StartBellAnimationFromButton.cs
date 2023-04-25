using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PunTabletop
{
    public class StartBellAnimationFromButton : MonoBehaviour
    {
        public GameObject gameController;
        public GameObject bellObject;

        public void StartAni()
        {
            Animation animation = bellObject.GetComponent<Animation>();
            AudioSource bellAudio = bellObject.GetComponent<AudioSource>();
            animation.Play();
            bellAudio.Play();
        }
    }
}

