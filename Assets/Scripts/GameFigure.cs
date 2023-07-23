using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Figure;
using HelpScripts;
using Management;
using SimUnity.Management;
using SimUnity.Models;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameFigure : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IModel
{
    public int Id { get; set; }
    public Color TeamColor;
    [SerializeField] private TextMeshPro PowerText;

    public delegate void OnDestroyHandler(GameFigure figure);

    public event OnDestroyHandler OnDestroy;

    public Action<GameFieldCell, Vector2, bool, Team> MoveLineBehind;

    public GameFieldCell Cell { get; private set; }

    private Vector3 _figureSize;

    private Vector3 FigureSize
    {
        get
        {
            {
                if (_figureSize == Vector3.zero)
                {
                    _figureSize = GetComponent<MeshRenderer>().bounds.size;
                }

                return _figureSize;
            }
        }
        set => _figureSize = value;
    }

    // Start is called before the first frame update
    void Start()
    {
        Power = _power;
        // PowerText.transform.localEulerAngles = new Vector3(0, 180, 0);
        HiglithedCells = new GameFieldCell[_possibleDirections.Length + 1];
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            DeselectFigure();
        }

        RotatePowerTextTowardCamera();
    }

    private void RotatePowerTextTowardCamera()
    {
        // PowerText.transform.LookAt(Camera.main.transform);
        PowerText.transform.rotation = Camera.main.transform.rotation;
    }

    public void Move(Vector2 direction)
    {
        Move(Cell.GetCell(direction));
    }

    public void Move(GameFieldCell nextCell)
    {
        if (nextCell != null)
        {
            var startCell = Cell;
            var direction = (nextCell.Position - Cell.Position);
            if (nextCell.Figure == this)
                return;

            if (nextCell.Figure == null)
            {
                MoveToFreeCell(nextCell);
                MoveLineBehind(startCell, direction, true, Team);
            }
            else
            {
                var nextCellFigure = nextCell.Figure;
                if (Team == nextCellFigure.Team)
                {
                    if (Type == nextCellFigure.Type)
                    {
                        //Combine two same type objects
                        nextCellFigure.Power += Power;
                        RemoveObject();
                        MoveLineBehind(startCell, direction, true, Team);
                    }
                    else
                    {
                        Swap(this, nextCellFigure);
                    }
                }
                else
                {
                    var enemy = nextCell.Figure;

                    var startPower = Power;
                    Power -= enemy.Power;
                    enemy.Power -= startPower;

                    var enemyCell = enemy.Cell;

                    if (enemy.Power <= 0)
                    {
                        enemy.RemoveObject();
                    }

                    if (Power <= 0)
                    {
                        RemoveObject();
                    }
                    else
                    {
                        Assign(enemyCell, true);
                    }
                    
                    MoveLineBehind(startCell, direction, true, Team);
                }
            }

            DeselectHighlitedCells();
        }
    }

    private void DeselectHighlitedCells()
    {
        for (var i = 0; i < HiglithedCells.Length; i++)
        {
            HiglithedCells[i]?.Deselect();
        }
    }

    private void MoveToFreeCell(GameFieldCell nextCell)
    {
        Assign(nextCell, true);
    }

    void UpdatePosition(bool animate)
    {
        var position = Cell.transform.position;
        if (!animate)
        {
            position.y += FigureSize.y / 2;
            transform.position = position;
        }
        else
        {
            StartCoroutine(SmoothMove(position));
        }
    }

    private static void Swap(GameFigure first, GameFigure second)
    {
        var fCell = first.Cell!;
        var sCell = second.Cell!;

        second.Assign(fCell, true);
        first.Assign(sCell, true);
    }

    public void RemoveObject()
    {
        Cell.Figure = null;
        Cell = null;

        OnDestroy?.Invoke(this);
        Destroy(gameObject);
    }

    public FigureType Type { get; set; }

    public float _power = 1f;

    public float Power
    {
        get => _power;
        set
        {
            _power = value;
            if (_power > 1)
            {
                PowerText.color = Type.Color;
                PowerText.text = _power.ToString(CultureInfo.CurrentCulture);
            }
            else
            {
                PowerText.text = "";
            }
        }
    }

    public Team Team { get; set; }

    public GameFigure Clone()
    {
        var clone = Instantiate(this);
        clone.Type = Type;
        clone.Team = Team;
        return clone;
    }

    public void SelectFigure()
    {
        if (GameManager.Instance.SelectedFigure != null)
        {
            GameManager.Instance.SelectedFigure.DeselectFigure();
        }

        GameManager.Instance.SelectedFigure = this;
        Cell.Select();

        HiglithedCells[^1] = Cell;
        HighlightPossibleMoves();
    }

    private void HighlightPossibleMoves()
    {
        for (int i = 0; i < _possibleDirections.Length; i++)
        {
            HiglithedCells[i] = Cell.GetCell(_possibleDirections[i]);
            HiglithedCells[i]?.Select();
        }
    }

    private GameFieldCell[] HiglithedCells;

    private readonly Vector2[] _possibleDirections =
    {
        Vector2.down,
        Vector2.up,
        Vector2.left,
        Vector2.right
    };

    public void DeselectFigure()
    {
        if (GameManager.Instance.SelectedFigure == this)
            GameManager.Instance.SelectedFigure = null;

        DeselectHighlitedCells();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SelectFigure();
    }

    public string Name => gameObject.name;
    public Transform Transform => transform;

    public MeshFilter Mesh => GetComponent<MeshFilter>();
    public MeshRenderer Renderer => GetComponent<MeshRenderer>();

    public Color Color
    {
        get => Renderer.material.color;
        set
        {
            var newMat = new Material(Renderer.material)
            {
                color = value
            };

            Renderer.material = newMat;
        }
    }

    public Vector3 Scale => transform.localScale;

    public float ProgressSpeed = 1;

    private IEnumerator SmoothMove(Vector3 endPos)
    {
        var height = FigureSize.y / 2f;
        
        endPos.y += height;
        
        var startPos = transform.position;

        var progress = 0f;
        
        while ((endPos - transform.position).magnitude > 0.01f)
        {
            transform.position = new Vector3(Mathf.Lerp(startPos.x, endPos.x, progress), endPos.y,
                Mathf.Lerp(startPos.z, endPos.z, progress));

            progress += ProgressSpeed * Time.deltaTime;
            yield return new WaitForSeconds(0.01f);
        }

        var position = endPos;
        position.y += height;
        transform.position = position;
        yield break;
    }

    public void Assign(GameFieldCell nextCell, bool animate)
    {
        var figure = this;
        if (figure.Cell == nextCell)
            return;

        var isWon = figure.Team != nextCell.Team && nextCell.Team.TeamName != Team.TeamEnum.Zero && nextCell.IsWinCell;

        if (figure.Cell != null && figure.Cell.Figure == figure)
            figure.Cell.Figure = null;

        figure.Cell = nextCell;
        nextCell.Figure = figure;
        nextCell.Team = figure.Team;

        UpdatePosition(animate);
        if (isWon)
        {
            GameManager.Instance.Win(figure.Team);
        }
    }
}