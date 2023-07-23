using Figure;
using JetBrains.Annotations;
using SimUnity.Management;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameFieldCell : MonoBehaviour, IPointerClickHandler
{
    private Material _material;

    private Material Material
    {
        get
        {
            if (_material == null)
            {
                _material = GetComponent<MeshRenderer>().material;
            }

            return _material;
        }
    }

    [CanBeNull] public GameFigure Figure;
    public GameField Field { get; set; }
    private Team _team;

    public Team Team
    {
        get => _team;
        set
        {
            _team = value;
            Material.SetColor("_TeamColor", value.Color);
        }
    }

    public bool IsWinCell = false;

    public Vector2 Position;
    private int _cellLayer;

    void Start()
    {
        _cellLayer = LayerMask.GetMask("FieldCell");
    }

    void Update()
    {
        // if (Input.GetMouseButtonDown(0))
        // {
        //     if (MousePosition.TryGet(Camera.current, _cellLayer, out var position, out var hits))
        //     {
        //         if (hits.Contains(gameObject))
        //         {
        //             MoveCurrentFigure();
        //         }
        //     }
        // }
    }

    public void Select()
    {
        Material.SetFloat("_Selected", 1);
        IsPossibleMove = true;
    }

    public void Deselect()
    {
        Material.SetFloat("_Selected", 0);
        IsPossibleMove = false;
    }

    [CanBeNull]
    public GameFieldCell GetCell(Vector2 direction)
    {
        var pos = Position + direction;
        return Field.GetCell(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        MoveCurrentFigure();
    }

    public bool IsPossibleMove { get; set; }

    private void MoveCurrentFigure()
    {
        if (GameManager.Instance.SelectedFigure != null && IsPossibleMove)
        {
            GameManager.Instance.SelectedFigure.Move(this);
            GameManager.Instance.SelectedFigure.DeselectFigure();
        }
    }
}