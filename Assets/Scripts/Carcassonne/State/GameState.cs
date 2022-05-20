using System;
using System.Linq;
using Carcassonne.Models;
using Carcassonne.State.Features;
using UnityEngine;

namespace Carcassonne.State
{
    // [CreateAssetMenu(fileName = "GameState", menuName = "States/GameState")]
    public class GameState : MonoBehaviour
    {
        public DateTime Timestamp;
        
        public GameRules Rules;

        /// <summary>
        /// Describes what is happening currently in the game.
        /// </summary>
        public Phase phase;

        public TileState Tiles;
        public MeepleState Meeples;
        public FeatureState Features;
        public PlayerState Players;

        public GridMapper grid;

        public Guid GameID = Guid.NewGuid();

        // private void Awake()
        // {
        //     Reset();
        // }

        //TODO Resetting game state.
        public void Reset()
        {
            Debug.Log("Resetting Game State...");
            
            Rules = new GameRules();
            Tiles = new TileState();
            Meeples = new MeepleState();
            Features = new FeatureState(Meeples, grid);
            Players = new PlayerState();
            
            Timestamp = DateTime.Now;
            GameID = Guid.NewGuid();
        }

        // private void OnEnable()
        // {
        //     Reset();
        // }


        #region Utilities
        /// <summary>
        /// Not every cell is represented in the graph. For example, the centre of a tile is often unrepresented,
        /// so one cannot query the graph by cell. This returns a represented cell on the Meeple's tile in the graph
        /// for any placed Meeple.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public Vector2Int GetGraphLocationForMeeple(Meeple m)
        {
            return GetGraphVertexForMeeple(m).location;
        }
        
        public SubTile GetGraphVertexForMeeple(Meeple m)
        {
            var meepleCell = Meeples.Placement.Single(kvp => kvp.Value == m).Key;
            var feature = Features.GetFeatureAt(meepleCell);
            var vertex = feature.Vertices.First(tile => grid.MeepleToTile(tile.location) == grid.MeepleToTile(meepleCell));

            return vertex;
        }

        #endregion
    }
}