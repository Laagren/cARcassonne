using System.Linq;
using Carcassonne.AR;
using Carcassonne.Models;
using Carcassonne.State;
using Carcassonne.Controllers;
using MRTK.Tutorials.MultiUserCapabilities;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class MeepleCountScript : MonoBehaviourPun
{
    public GameState state;
    public GameControllerScript controller;
    public MeepleController meepleController;

    private TextMeshPro meepleCountText => transform.GetComponentsInChildren<TextMeshPro>()[1];
    
    private void Start()
    {
        state = FindObjectOfType<GameState>();
        controller = state.GetComponent<GameControllerScript>();
    }
    
    public void UpdateMeepleCount()
    {
        Debug.Assert(state != null, "State is null");

        Debug.Log($"Meeple Count Board updated");
        meepleCountText.text = "" + meepleController.getMeepleCount();
    }
}
