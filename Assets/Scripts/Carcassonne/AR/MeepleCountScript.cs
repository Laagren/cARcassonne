using System.Linq;
using Carcassonne.AR;
using Carcassonne.Models;
using Carcassonne.State;
using Carcassonne.Controllers;
using MRTK.Tutorials.MultiUserCapabilities;
using Photon.Pun;
using TMPro;
using UnityEngine;


namespace Carcassonne.Players
{
    public class MeepleCountScript : MonoBehaviourPun
    {
        public MeepleController meepleController;

        private TextMeshPro meepleCountText => transform.GetComponentsInChildren<TextMeshPro>()[1];

        private void Start()
        {
            meepleController = GetComponent<MeepleController>();
        }

        public void UpdateMeepleCount()
        {
            Debug.Log($"Meeple Count Board updated");
            meepleCountText.text = "" + meepleController.getMeepleCount();
        }
    }
}
