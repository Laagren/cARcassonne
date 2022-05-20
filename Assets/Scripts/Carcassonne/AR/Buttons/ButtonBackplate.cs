using Carcassonne.Controllers;
using Carcassonne.Models;
using Photon.Pun;
using UnityEngine;

namespace Carcassonne.AR.Buttons
{
    public class ButtonBackplate : MonoBehaviourPun
    {
        public Materials materials;

        public Material newTurn;
        public Material tileDown;
        public Material meepleDown;

        public Material meepleValidAim;
        public Material meepleInvalidAim;

        private void Start()
        {
            var gc = FindObjectOfType<GameController>();
            var tc = FindObjectOfType<TileController>();
            var mc = FindObjectOfType<MeepleController>();
            var mcs = FindObjectOfType<MeepleControllerScript>();

            if(newTurn)
                gc.OnTurnStart.AddListener(HandleNewTurn);
            
            if(tileDown)
                tc.OnPlace.AddListener(HandleTileDown);
            
            if(meepleDown)
                mc.OnPlace.AddListener(HandleMeepleDown);
            
            if(meepleValidAim)
                mcs.OnValidAim.AddListener(HandleValidAim);
            
            if(meepleInvalidAim)
                mcs.OnInvalidAim.AddListener(HandleInvalidAim);
            
        }

        public void HandleNewTurn()
        {
            GetComponent<MeshRenderer>().material = newTurn;
        }

        public void HandleTileDown(Tile tile, Vector2Int cell)
        {
            GetComponent<MeshRenderer>().material = tileDown;
        }

        public void HandleMeepleDown(Meeple meeple, Vector2Int direction)
        {
            GetComponent<MeshRenderer>().material = meepleDown;
        }

        public void HandleValidAim(Vector2Int arg0)
        {
            GetComponent<MeshRenderer>().material = meepleValidAim;
        }

        public void HandleInvalidAim(Vector2Int arg0)
        {
            GetComponent<MeshRenderer>().material = meepleInvalidAim;
        }
    }
}