using System;
using System.Collections.Generic;
using System.Linq;
using Figure;
using JetBrains.Annotations;
using Management;
using UnityEngine;

public class GameField : MonoBehaviour
{
    [Range(1, 40)] public int RowCount = 7;
    [Range(1, 40)] public int ColunmCount = 7;

    private GameFieldCell[,] Cells;
    private List<GameFigure> Figures = new();
    private Vector3 _cellSize;
    [SerializeField] private GameFieldCell CellPrefab;
    [SerializeField] private Collection Collection;

    private static GameField _instance;

    public static GameField Instance
    {
        get
        {
            if (_instance == null)
            {
                var gameObject = new GameObject("Game Field", typeof(GameField));
                _instance = gameObject.GetComponent<GameField>();
                _instance.Awake();
            }

            return _instance;
        }
        private set => _instance = value;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance ??= this;
        DontDestroyOnLoad(this);
    }

    public void Restart()
    {
        RemoveAllFigures();
        RecreateField();
        SpawnFigures();
    }

    private void RemoveAllFigures()
    {
        while (Figures.Any())
        {
            var figure = Figures.First();
            if (figure != null)
            {
                figure.RemoveObject();
            }

            Figures.Remove(figure);
        }
    }

    void Start()
    {
        _cellSize = CellPrefab.GetComponent<MeshRenderer>().bounds.size;
        Restart();
    }

    void Update()
    {
        CheckFieldSize();
    }

    private void CheckFieldSize()
    {
        if (Cells.GetLength(0) != RowCount || Cells.GetLength(1) != ColunmCount)
        {
            RecreateField();
            SpawnFigures();
        }
    }

    private void RecreateField()
    {
        var oldCells = Cells?.Cast<GameFieldCell>().GetEnumerator();
        Cells = new GameFieldCell[RowCount, ColunmCount];

        bool IsLastLine(int i)
        {
            return i == 0 || i == Cells.GetLength(0) - 1;
        }

        for (int i = 0; i < Cells.GetLength(0); i++)
        {
            for (int j = 0; j < Cells.GetLength(1); j++)
            {
                GameFieldCell cell;
                if (oldCells?.MoveNext() ?? false)
                {
                    cell = oldCells.Current;
                }
                else
                {
                    cell = Instantiate(CellPrefab, transform, false);
                    cell.Field = this;
                }

                cell!.transform.position =
                    new Vector3((-RowCount / 2f + i) * _cellSize.x, 0, (-ColunmCount / 2f + j) * _cellSize.z);
                cell.Position = new Vector2(i, j);
                cell.Team = GetTeam(i);

                if (IsLastLine(i))
                {
                    cell.IsWinCell = true;
                }

                Cells[i, j] = cell;
            }
        }

        while (oldCells?.MoveNext() ?? false)
        {
            Destroy(oldCells.Current!.gameObject);
        }

        oldCells?.Dispose();
    }

    [CanBeNull]
    public GameFieldCell GetCell(int x, int y)
    {
        if (0 <= x && x < RowCount &&
            0 <= y && y < ColunmCount)
        {
            return Cells[x, y];
        }

        return null;
    }

    bool IsFreeLine(int i)
    {
        var isOdd = Cells.GetLength(0) % 2 == 1;
        var lineSize = FreeLineSize + (isOdd ? 0 : 1) - 1;
        
        var midLine = Mathf.FloorToInt(Cells.GetLength(0) / 2f);

        return i >= (midLine - lineSize) && i <= (midLine + lineSize - (!isOdd ? 1 : 0));
    }

    Team GetTeam(int i)
    {
        var teams = Collection.Instance.Teams;

        if (IsFreeLine(i))
        {
            return teams.First(x => x.Id == 0);
        }

        return Math.Sign(i - Cells.GetLength(0) / 2) < 0 ? teams.First(x => x.Id == 1) : teams.First(x => x.Id == 2);
    }

    void SpawnFigures()
    {
        //Row
        for (int i = 0; i < Cells.GetLength(0); i++)
        {
            //Column
            if (IsFreeLine(i)) continue;

            for (int j = 0; j < Cells.GetLength(1); j++)
            {
                var figure = Collection.GetRandomFigure();

                Figures.Add(figure);
                figure.Team = GetTeam(i);
                figure.Assign(Cells[i, j], false);
                figure.MoveLineBehind = MoveLine;
                figure.OnDestroy += OnFigureDestroy;
            }
        }
    }

    private void MoveLine(GameFieldCell cell, Vector2 direction, bool fillLastFreeCell, Team team)
    {
        var dir = -direction.normalized;
        var currentCell = cell;
        var nextCell = currentCell.GetCell(dir);

        while (nextCell != null && nextCell.Team == team)
        {
            if (currentCell.Figure == null && nextCell.Figure != null)
                nextCell.Figure.Assign(currentCell, true);

            currentCell = nextCell;
            nextCell = currentCell.GetCell(dir);
        }

        if (fillLastFreeCell && currentCell.Figure == null && currentCell.Team == team)
        {
            var figure = Collection.GetRandomFigure();

            Figures.Add(figure);
            figure.Team = team;
            figure.Assign(currentCell, false);
            figure.MoveLineBehind = MoveLine;
            figure.OnDestroy += OnFigureDestroy;
        }
    }

    public void OnFigureDestroy(GameFigure figure)
    {
        Figures.Remove(figure);
        figure.OnDestroy -= OnFigureDestroy;
    }

    public void SetRowCount(float value)
    {
        RowCount = Mathf.RoundToInt(value);
        Restart();
    }

    public void SetColumnCount(float value)
    {
        ColunmCount = Mathf.RoundToInt(value);
        Restart();
    }

    public void SetFreeLineSize(float value)
    {
        FreeLineSize = Mathf.RoundToInt(value);
        Restart();
    }

    public int FreeLineSize { get; set; } = 1;
}