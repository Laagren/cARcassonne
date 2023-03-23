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
    public class TileCountScript : MonoBehaviourPun
    {
        public GameObject baseGameController;
        public TileController tileController;

        [SerializeField] private TextMeshPro tileCountText => transform.GetComponentsInChildren<TextMeshPro>()[1];

        private void Start()
        {
            tileController = baseGameController.GetComponent<TileController>();
        }

        public void UpdateTileCount()
        {
            Debug.Log($"Tile Count Board updated");
            tileCountText.text = "" + tileController.getTileCount();
        }
    }
}
