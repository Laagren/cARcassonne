using Carcassonne.Controllers;
using Carcassonne.Models;
using Carcassonne.State;
using ExitGames.Client.Photon.StructWrapping;
using Microsoft.MixedReality.Toolkit;
using Newtonsoft.Json;
using QuikGraph.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class Cell
{
    // public float TotalGazeTime;
    // For future SAAG
    //public bool CanPlaceMeeple;
    //public bool HasTile;
    //public Tile tile;

    public Vector2Int CellPos;
    public float GazeDuration;
    public float TotalGazeTime;
    public TileData tileData;
    public bool hasTile;


    [System.NonSerialized]
    public bool ShouldTrackTime;

    public Cell(Vector2Int cellPos)
    {
        CellPos = cellPos; 
        GazeDuration = 0;
        ShouldTrackTime = false;
        //CanPlaceMeeple = false;
        //TotalGazeTime = 0;
    }
}


[System.Serializable]
public class TileData
{
    public Dictionary<Vector2Int, Geography> SubTileDictionary;
    public Dictionary<Vector2Int, Geography> Sides;
    public IDictionary<Vector2Int, Geography> Geographies;
    public Geography[,] Matrix;
    public Geography? Center;
    public Geography South;
    public Geography North;
    public Geography East;
    public Geography West;  
    public int m_id;
    public int m_rotations;
    public bool IsReady;
    public bool Shield;
    public Tile.TileSet set;
    // Include static members?
}



public class TestEyeTrackData : MonoBehaviour
{
    [SerializeField] private  GameObject viewObject;
    [SerializeField] private GameObject suggestObject;
    [SerializeField] private float minGazeTime = 0.5f;

    private Dictionary<Vector2Int, Cell> gridCells = new Dictionary<Vector2Int, Cell>();
    private List<Cell> cellViewOrderList = new List<Cell>();
    private Dictionary<Vector2Int, Vector2Int> uniqueCellsViewed = new Dictionary<Vector2Int, Vector2Int>();
    private GameObject gameController;
    private TileController tileController;
    private GameState gameState;
    private UnityEngine.Grid grid;
    private const int gridSize = 14;
    private Vector2Int currentCell = Vector2Int.zero;
    private Vector2Int prevCell;
    private int roundID = 1;
    private int playerID;
    private float roundTimer;


    void Start()
    {
        grid = GameObject.FindWithTag("TileGrid").GetComponent<UnityEngine.Grid>();
        gameController = GameObject.Find("GameController");
        playerID = GetComponent<Player>().id;
        prevCell = currentCell;  
        viewObject = Instantiate(viewObject, Vector3.zero, Quaternion.identity, grid.transform);
        suggestObject = Instantiate(suggestObject, Vector3.zero, Quaternion.identity, grid.transform);

        for(int i = (gridSize * -1); i <= gridSize; i++) {
            for(int j = (gridSize * -1); j <= gridSize; j++) {
                Vector2Int pos = new Vector2Int(i, j);
                gridCells.Add(pos, new Cell(pos));                   
            }
        }

        gameController.GetComponent<GameController>().OnTurnEnd.AddListener(RegisterRound);
        //gameController.GetComponent<TileController>().ActivateCellEvent += OnActivateCell;
        tileController = gameController.GetComponent<TileController>();
        gameState = tileController.GetComponent<GameState>();
        tileController.ActivateCellEvent += OnActivateCell;
        OnActivateCell(Vector2Int.zero, gameController.GetComponent<TileController>().FirstTile);
    }

    private Cell PredictPlacement()
    {
        // Find most viewed cell
        Cell mostViewed;
        float gazeTime = 0;
        Vector2Int cellKey = Vector2Int.zero;
        foreach (Cell cell in cellViewOrderList)
        {
            if (cell.TotalGazeTime > gazeTime && !cell.hasTile)
            {
                gazeTime = cell.TotalGazeTime;
                cellKey = cell.CellPos;
            }
        }

        //if (gazeTime == 0) return false; // Should not occur, only if player has not looked at any empty cells.
        return gridCells[cellKey];
    }

    private bool PredictedCellIsValid(Cell cell)
    {
        Tile tile = gameState.Tiles.Current;
        // Find adjacent cells
        foreach (var side in Tile.Directions)
        {
            Vector2Int neighbour = cell.CellPos + side;
            Cell neighbourCell = gridCells[neighbour];

            if (neighbourCell.tileData != null)
            {
               
                for (var rotation = 0; rotation < 4; rotation++)
                {
                    // Check if tile placement with current rotation is valid
                    if (tileController.IsPlacementValid(cell.CellPos))
                    {
                        Debug.Log($"Found a valid position at ({neighbour.x},{neighbour.y}) with rotation {rotation}.");

                        // Randomly rotate tile to not bias positioning
                        tile.RotateTo(0); //TODO Switch this once there is a way of syncing. 

                        return true;
                    }

                    tile.Rotate();
                }
            }
        }
        tile.RotateTo(0);
        return false;
    }

    private void Algoritm()
    {
        // Steps
        // 1 - Check if most viewed cell is valid for current tile
        // 2 - if valid, do nothing and break
        // 3 - if invalid, highlight possible options, start over from step 1

        Cell mostViewed = PredictPlacement();
        if (!PredictedCellIsValid(mostViewed))
        { 
            // Loop over all? possible empty cells
            // Check if valid
            //    if valid, hightlight this cell
            //    if not, continue to iterate over cells and check next

            foreach(var cell in gridCells)
            {
                if(cell.Value.ShouldTrackTime 
                    && cell.Value.hasTile == false) // if cell is activated and is not occupied
                {
                    if (PredictedCellIsValid(cell.Value))
                    {
                        // TODO: Highlight
                        suggestObject.transform.position = grid.GetCellCenterWorld(new Vector3Int(cell.Value.CellPos.x, cell.Value.CellPos.y, 0));
                        roundTimer = 0;
                        break;
                    }
                }
            }
        }
    }

    //public override bool CanBePlaced()
    //{
    //    // Log the cells that have been visited
    //    HashSet<Vector2Int> visitedTiles = new HashSet<Vector2Int>();

    //    // Check the cells adjacent to each placed tile
    //    foreach (var kvp in state.Tiles.Placement)
    //    {
    //        var c = kvp.Key;
    //        var t = kvp.Value;
    //        foreach (var side in Tile.Directions) // Every neighbouring cell to a placed tile
    //        {
    //            var neighbour = c + side;
    //            if (!visitedTiles.Contains(neighbour))
    //            {
    //                for (var rotation = 0; rotation < 4; rotation++)
    //                {
    //                    if (IsPlacementValid(neighbour))
    //                    {
    //                        Debug.Log($"Found a valid position at ({neighbour.x},{neighbour.y}) with rotation {rotation}.");

    //                        // Randomly rotate tile to not bias positioning
    //                        // tile.Rotate(Random.Range(0,4));
    //                        tile.RotateTo(0); //TODO Switch this once there is a way of syncing. 

    //                        return true;
    //                    }

    //                    tile.Rotate();
    //                }
    //                visitedTiles.Add(neighbour);
    //            }
    //        }
    //    }

    //    Debug.LogWarning($"Tile ID {tile.ID} cannot be placed.");
    //    return false;
    //}


    void RegisterRound()
    {     
        string path = "Assets/EyeTrackData/round_" + roundID + ".json";
        if(roundID++ == 1)
            File.Create(path).Close();

        foreach (Cell cell in cellViewOrderList){
            string str = JsonUtility.ToJson(cell);
            Debug.Log("SAVING TO FILE: " + str);
            File.AppendAllText(path, str + "\n");
        }

        cellViewOrderList.Clear(); // Should it clear after every round?
        // TODO: clear total gaze time from cellGrids.
        roundTimer = 0;
    }

    TileData ParseTileData(Tile tile)
    {
        return new TileData
        {
            SubTileDictionary = tile.SubTileDictionary,
            Sides             = tile.Sides,
            Geographies       = tile.Geographies,
            Matrix            = tile.Matrix,
            Center            = tile.Center,
            South             = tile.South,
            North             = tile.North,
            East              = tile.East,
            West              = tile.West,
            m_id              = tile.ID,
            m_rotations       = tile.Rotations,
            IsReady           = tile.IsReady,
            Shield            = tile.Shield,
            set               = tile.set
        };
    }


    void OnActivateCell(Vector2Int cell, Tile tile)
    {
        gridCells[cell].tileData = ParseTileData(tile);
        gridCells[cell].hasTile = true;

        // Neigbour bounds check
        gridCells[cell].ShouldTrackTime = true;
        for (int i = -1; i <= 1; i += 2){
            gridCells[new Vector2Int(cell.x - i, cell.y)].ShouldTrackTime = true; // 1: (x+1, y)  2: (x-1, y)
            gridCells[new Vector2Int(cell.x, cell.y - i)].ShouldTrackTime = true; // 1: (x, y+1)  2: (x, y-1)
        }
    }


    void TrackCell()
    {
        // Get gaze point and transform it into coordinates on the grid.
        Vector3 hitPoint = CoreServices.InputSystem.EyeGazeProvider.HitPosition;
        Vector3Int local3DPos = grid.WorldToCell(hitPoint);
        currentCell = new Vector2Int(local3DPos.x, local3DPos.y);

        // If looking at cell outside board, abort.
        if (!gridCells.ContainsKey(currentCell))
            return;

        // Check if currentCell is active and should update time 
        if (gridCells[currentCell].ShouldTrackTime){
            gridCells[currentCell].GazeDuration += Time.deltaTime;
            gridCells[currentCell].TotalGazeTime += Time.deltaTime;
        }
           
            
        // Switched gaze to another cell
        if (currentCell != prevCell && gridCells[currentCell].ShouldTrackTime
            && gridCells[currentCell].GazeDuration > minGazeTime)
        {
            viewObject.transform.position = grid.GetCellCenterWorld(new Vector3Int(currentCell.x, currentCell.y, 0));
            Debug.Log("Previous cell was: " + prevCell.ToString() + ", current gaze time was: " + gridCells[prevCell].GazeDuration);

            prevCell = currentCell;
            cellViewOrderList.Add(gridCells[prevCell]);
            uniqueCellsViewed.Add(prevCell, prevCell);
            gridCells[prevCell].GazeDuration = 0;       
        }     
    }


    private void Update()
    {
        TrackCell();

        roundTimer += Time.deltaTime;
        if (roundTimer > 10)
            Algoritm();

        #region legacy
        //Debug.Log(CoreServices.InputSystem.EyeGazeProvider.HitPosition);
        //gazeTime+= Time.deltaTime;
        //if(debugTimer > 0) 
        //{

        //___
        //Vector3 hitPoint = CoreServices.InputSystem.EyeGazeProvider.HitPosition;
        //hitPoint.Set(hitPoint.x - (0.0325f / 2), hitPoint.y, hitPoint.z - (0.0325f / 2));
        //// Calculate the position of the grid center
        //Vector3 gridCenter = grid.transform.position;

        //// Calculate the position of the hit point relative to the grid center
        //Vector3 hitPointRelativeToGrid = hitPoint - gridCenter;

        //// Calculate the column index based on the X position relative to the grid
        //float cellSizeX = 0.0325f;
        //int columnIndex = Mathf.RoundToInt(hitPointRelativeToGrid.x / cellSizeX);

        //// Calculate the row index based on the Z position relative to the grid
        //float cellSizeZ = 0.0325f;
        //int rowIndex = Mathf.RoundToInt(hitPointRelativeToGrid.z / cellSizeZ);
        //currentCell = new Vector2Int(columnIndex, rowIndex);
        //___

        // Return the grid indices as a Vector2Int
        //Debug.Log(new Vector2Int(columnIndex, rowIndex));
        //cellTimer = 0;

        //gazeTime += Time.deltaTime;
        //// if gazed on a new cell, add to dictionary
        //if (!cellGazeTimes.ContainsKey(currentCell))
        //{
        //    cellGazeTimes.Add(currentCell, 0.0f);
        //}
        //gridCells[currentCell] += Time.deltaTime;
        #endregion
    }
}
