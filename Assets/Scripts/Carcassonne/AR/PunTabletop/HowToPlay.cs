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
        if (!tutorial.activeInHierarchy)
        {
            tutorial.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator toggler()
    {
        yield return new WaitForSeconds(0.2f);
        tutorial.SetActive(!tutorial.activeInHierarchy);
    }

    //Called when how to play button is pressed.
    public void ToggleTutorial()
    {
        StartCoroutine(toggler());
    }
}
