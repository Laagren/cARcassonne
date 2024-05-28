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
    public Vector2Int CellPos;
    public float GazeDuration;
    public float TotalGazeTime;
    public TileData tileData;
    public bool hasTile;


    [System.NonSerialized]
    public bool ShouldTrackTime;

    public Cell()
    {
        GazeDuration = 0;
        ShouldTrackTime = false;
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
    [SerializeField] private float minAlgorithmTime = 15.0f;
    [SerializeField] private float suggestionUpTime = 5.0f;
    [SerializeField] private bool showTrackedCell = false;
    [SerializeField] private bool shouldSaveData = false;

    private Dictionary<Vector2Int, Vector2Int> uniqueCellsViewed = new Dictionary<Vector2Int, Vector2Int>();
    private Dictionary<Vector2Int, Cell> gridCells = new Dictionary<Vector2Int, Cell>();
    private List<Cell> cellViewOrderList = new List<Cell>();
    private GameObject gameController;
    private TileController tileController;
    private GameState gameState;
    private UnityEngine.Grid grid;
    private Vector2Int currentCell = Vector2Int.zero;
    private Vector2Int suggestedCellPos;
    private  Vector2Int prevMostViewed;
    private Vector2Int prevCell;

    private const int gridSize = 14;
    private int roundID = 1;
    private int playerID;
    private float roundTimer;
    private bool prevWasInvalid;
    private bool shouldSuggestPlacement = false;



    void Start()
    {
        grid = GameObject.FindWithTag("TileGrid").GetComponent<UnityEngine.Grid>();
        gameController = GameObject.Find("GameController");
        playerID = GetComponent<Player>().id;
        prevCell = currentCell;  
        viewObject = Instantiate(viewObject, Vector3.zero, Quaternion.identity, grid.transform);
        suggestObject = Instantiate(suggestObject, Vector3.zero, Quaternion.identity, grid.transform);
        prevMostViewed = new Vector2Int(100, 100);

        for (int i = (gridSize * -1); i <= gridSize; i++) {
            for(int j = (gridSize * -1); j <= gridSize; j++) {
                Vector2Int pos = new Vector2Int(i, j);
                Cell cell = new Cell { CellPos = pos };
                gridCells.Add(pos, cell);                   
            }
        }

        gameController.GetComponent<GameController>().OnTurnEnd.AddListener(RegisterRound);
        tileController = gameController.GetComponent<TileController>();
        gameState = tileController.GetComponent<GameState>();
        tileController.ActivateCellEvent += OnTilePlacement;
        tileController.OnDraw.AddListener(PlayerStartedRound);
        OnTilePlacement(Vector2Int.zero, gameController.GetComponent<TileController>().FirstTile);
    }

    public void PlayerStartedRound(Tile t)
    {
        shouldSuggestPlacement = true;
    }

    private Cell PredictPlacement()
    {
        // Find most viewed cell
        float gazeTime = 0;
        Vector2Int cellKey = Vector2Int.zero;
        foreach(var cell in uniqueCellsViewed)
        {
            if (gridCells[cell.Key].TotalGazeTime > gazeTime &&
                !gridCells[cell.Key].hasTile)
            {
                gazeTime = gridCells[cell.Key].TotalGazeTime;
                cellKey = gridCells[cell.Key].CellPos;
            }
        }

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
                        tile.RotateTo(0);
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
        roundTimer = 0;
        Cell mostViewed = PredictPlacement();

        if (mostViewed.CellPos == prevMostViewed && prevWasInvalid) 
        {
            // Highlight
            suggestObject.gameObject.SetActive(true);
            suggestObject.transform.position = grid.GetCellCenterWorld(new Vector3Int(suggestedCellPos.x, suggestedCellPos.y, 0));
        }    
        else if (!PredictedCellIsValid(mostViewed))
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
                        // Highlight
                        suggestObject.gameObject.SetActive(true);
                        suggestObject.transform.position = grid.GetCellCenterWorld(new Vector3Int(cell.Value.CellPos.x, cell.Value.CellPos.y, 0));
                        
                        suggestedCellPos = cell.Value.CellPos;
                        prevWasInvalid = true;
                        break;
                    }
                }
            }
        }
        else{
            prevWasInvalid = false;
        }
        prevMostViewed = mostViewed.CellPos;
        // Reset GazeTimers
        ResetCellGazeData();

        StartCoroutine(StartDeactivateSuggestObject(suggestionUpTime));
    }

    void RegisterRound()
    {     
        if(shouldSaveData)
        {
            string path = "Assets/EyeTrackData/round_" + roundID + ".json";
            if (roundID++ == 1)
                File.Create(path).Close();

            foreach (Cell cell in cellViewOrderList)
            {
                string str = JsonUtility.ToJson(cell);
                Debug.Log("SAVING TO FILE: " + str);
                File.AppendAllText(path, str + "\n");
            }
        }

        // Clear total gaze time from cellGrids.
        ResetCellGazeData();
        cellViewOrderList.Clear();     
        roundTimer = 0;
        prevMostViewed = new Vector2Int(100, 100);
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

    void OnTilePlacement(Vector2Int cell, Tile tile)
    {
        gridCells[cell].tileData = ParseTileData(tile);
        gridCells[cell].hasTile = true;

        // Neigbour bounds check
        gridCells[cell].ShouldTrackTime = true;
        for (int i = -1; i <= 1; i += 2){
            gridCells[new Vector2Int(cell.x - i, cell.y)].ShouldTrackTime = true; // 1: (x+1, y)  2: (x-1, y)
            gridCells[new Vector2Int(cell.x, cell.y - i)].ShouldTrackTime = true; // 1: (x, y+1)  2: (x, y-1)
        }

        shouldSuggestPlacement = false;
        suggestObject.gameObject.SetActive(false);
        StartDeactivateSuggestObject(0);
        roundTimer = 0;
    }


    void TrackCell()
    {
        // Get gaze point and transform it into coordinates on the grid.
        Vector3 hitPoint = CoreServices.InputSystem.EyeGazeProvider.HitPosition;
        Vector3Int local3DPos = grid.WorldToCell(hitPoint);
        prevCell = currentCell;
        currentCell = new Vector2Int(local3DPos.x, local3DPos.y);

        // If looking at cell outside board, abort.
        if (!gridCells.ContainsKey(currentCell) || !gridCells[currentCell].ShouldTrackTime){
            viewObject.transform.position = Vector3.zero;
            //Debug.Log("LOOKING ON NON ACTIVE CELLS");
            return;
        }  

        // Switched gaze to another cell
        if (currentCell != prevCell)
        {
            if(showTrackedCell){
                viewObject.transform.position = grid.GetCellCenterWorld(new Vector3Int(currentCell.x, currentCell.y, 0));
            }

            if (gridCells[prevCell].GazeDuration >= minGazeTime)
            {
                Cell temp = new Cell
                {
                    CellPos = gridCells[prevCell].CellPos,
                    GazeDuration = gridCells[prevCell].GazeDuration,
                    TotalGazeTime = gridCells[prevCell].TotalGazeTime,
                    tileData = gridCells[prevCell].tileData,
                    hasTile = gridCells[prevCell].hasTile,
                    ShouldTrackTime = gridCells[prevCell].ShouldTrackTime
                };

                Debug.Log("Previous cell was: " + temp.CellPos.ToString() + ", current gaze time was: " + temp.GazeDuration);
                cellViewOrderList.Add(temp);
                if (!uniqueCellsViewed.ContainsKey(prevCell))
                {
                    uniqueCellsViewed.Add(prevCell, prevCell);
                }
                gridCells[prevCell].GazeDuration = 0;
            }
            else{
                gridCells[prevCell].GazeDuration = 0;
                Debug.Log("Looked at new cell. Did not look at prev cel for enough time. Reset gaze duration. PREV: " + prevCell + ", CURRENT: " + currentCell);
            }
        }

        //Debug.Log("Current cell: " + currentCell + ", gaze: " + gridCells[currentCell].GazeDuration + ", total: " + gridCells[currentCell].TotalGazeTime);

        gridCells[currentCell].GazeDuration += Time.deltaTime;
        gridCells[currentCell].TotalGazeTime += Time.deltaTime;
    }

    private void ResetCellGazeData()
    {
        foreach (var cell in gridCells)
        {
            cell.Value.GazeDuration = 0;
            cell.Value.TotalGazeTime = 0;
        }
    }

    IEnumerator StartDeactivateSuggestObject(float time)
    {
        yield return new WaitForSeconds(time);
        // After timer is done, execute
        suggestObject.gameObject.SetActive(false);
    }

    private void Update()
    {
        TrackCell();

        if(shouldSuggestPlacement){
            roundTimer += Time.deltaTime;
            if (roundTimer > minAlgorithmTime)
                Algoritm();
        }
        else{
            roundTimer = 0;
        }
    }
}
