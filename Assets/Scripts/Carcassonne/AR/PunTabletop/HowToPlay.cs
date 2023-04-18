using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HowToPlay : MonoBehaviourPun
{
    public GameObject tutorial;
    
    // Start is called before the first frame update
    void Start()
    {
        tutorial.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Called when how to play button is pressed.
    public void ToggleTutorial()
    {
        tutorial.SetActive(!tutorial.activeInHierarchy);
    }
}
