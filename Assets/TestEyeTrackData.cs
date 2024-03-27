using Carcassonne.Controllers;
using Carcassonne.Models;
using Microsoft.MixedReality.Toolkit;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;
using UnityEngine;

public class Cell
{
    // public float TotalGazeTime;
    // For future SAAG
    //public bool CanPlaceMeeple;
    //public bool HasTile;

    public Vector2Int CellPos;
    public float CurrentGazeTime;
    public Tile tile;

    [JsonIgnore]
    public bool ShouldTrackTime;

    public Cell(Vector2Int cellPos)
    {
        CellPos = cellPos;
        //TotalGazeTime = 0;
        CurrentGazeTime = 0;
        ShouldTrackTime = false;
        //CanPlaceMeeple = false;
    }
}



public class TestEyeTrackData : MonoBehaviour
{
    [SerializeField] private  GameObject viewObject;

    private Dictionary<Vector2Int, Cell> gridCells = new Dictionary<Vector2Int, Cell>();
    //private Stack<Cell> cellStack = new Stack<Cell>();
    private List<Cell> cellStack = new List<Cell>();
    private GameObject gameController;
    private UnityEngine.Grid grid;
    private const int gridSize = 14;
    private Vector2Int currentCell = Vector2Int.zero;
    private Vector2Int prevCell;
    private int roundID = 1;


    void Start()
    {
        grid = GameObject.FindWithTag("TileGrid").GetComponent<UnityEngine.Grid>();
        gameController = GameObject.Find("GameController");

        prevCell = currentCell;  
        viewObject = Instantiate(viewObject, Vector3.zero, Quaternion.identity, grid.transform);
    
        for(int i = (gridSize * -1); i <= gridSize; i++) {
            for(int j = (gridSize * -1); j <= gridSize; j++) 
            {
                Vector2Int pos = new Vector2Int(i, j);
                gridCells.Add(pos, new Cell(pos));                   
            }
        }

        // Activate first default tile and its neighbours
        //gridCells[new Vector2Int(0, 0)].ShouldTrackTime  = true;
        //gridCells[new Vector2Int(-1, 0)].ShouldTrackTime = true;
        //gridCells[new Vector2Int(1, 0)].ShouldTrackTime = true;
        //gridCells[new Vector2Int(0, 1)].ShouldTrackTime = true;
        //gridCells[new Vector2Int(0, -1)].ShouldTrackTime = true;
        //for (int i = -1; i <= 1; i += 2)
        //{
        //    gridCells[new Vector2Int(i, 0)].ShouldTrackTime = true; // 1: (-1, 0)  2: (1, 0)
        //    gridCells[new Vector2Int(0, i)].ShouldTrackTime = true; // 1: (0, -1)  2: (0, 1)
        //}

        //OnActivateCell(Vector2Int.zero, );

        gameController.GetComponent<GameController>().OnTurnEnd.AddListener(RegisterRound);
        gameController.GetComponent<TileController>().ActivateCellEvent += OnActivateCell;
        OnActivateCell(Vector2Int.zero, gameController.GetComponent<TileController>().FirstTile);
    }


    void RegisterRound()
    {
       
        // TODO: Save gaze info into JSON file
        //string path = Application.persistentDataPath + "/cellStack.json";
        string path = "Assets/EyeTrackData/round_" + roundID + ".json";
        if(roundID++ == 1)
            File.Create(path).Close();
        

        foreach (Cell cell in cellStack)
        {
            string str = JsonUtility.ToJson(cell);
            File.AppendAllText(path, str + "\n");
        }
        //string JsonCellStack = JsonUtility.ToJson(cellStack);
        //string str;
        //str = JsonSerializer.Serialize(cellStack);
        //System.IO.File.AppendAllText(Application.persistentDataPath + "/cellStack.json", JsonCellStack);

    }

    void OnActivateCell(Vector2Int cell, Tile tile /*, List<Vector2Int> neighbours*/)
    {
        gridCells[cell].tile = tile;
        //gridCells[cell].HasTile = true;

        // TODO: Neigbour bounds check
        gridCells[cell].ShouldTrackTime = true;
        for (int i = -1; i <= 1; i += 2)
        {
            gridCells[new Vector2Int(cell.x - i, cell.y)].ShouldTrackTime = true; // 1: (x+1, y)  2: (x-1, y)
            gridCells[new Vector2Int(cell.x, cell.y - i)].ShouldTrackTime = true; // 1: (x, y+1)  2: (x, y-1)
        }
        //gridCells[new Vector2Int(cell.x - 1, cell.y)].ShouldTrackTime = true;
        //gridCells[new Vector2Int(cell.x + 1, cell.y)].ShouldTrackTime = true;
        //gridCells[new Vector2Int(cell.x, cell.y - 1)].ShouldTrackTime = true;
        //gridCells[new Vector2Int(cell.x, cell.y + 1)].ShouldTrackTime = true;
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
            // Increase gaze time
            //gridCells[currentCell].TotalGazeTime += Time.deltaTime;
            gridCells[currentCell].CurrentGazeTime += Time.deltaTime;     
        }

        // Switched gaze to another cell
        if (currentCell != prevCell && gridCells[currentCell].ShouldTrackTime)
        {
            //Debug.Log("Looking at new cell: " + currentCell.ToString() + ", total gaze time: " + gridCells[currentCell].TotalGazeTime);
            viewObject.transform.position = grid.GetCellCenterWorld(new Vector3Int(currentCell.x, currentCell.y, 0));


            Debug.Log("Previous cell was: " + prevCell.ToString() + ", current gaze time was: " + 
                gridCells[prevCell].CurrentGazeTime/* + ", total gaze time: " + gridCells[prevCell].TotalGazeTime*/);

            prevCell = currentCell;
            //cellStack.Push(gridCells[prevCell]);
            cellStack.Add(gridCells[prevCell]);
            gridCells[prevCell].CurrentGazeTime = 0;       
        }     
    }


    private void Update()
    {
        TrackCell();
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
