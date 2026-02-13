using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DialogGridLayout : MonoBehaviour
{
    [SerializeField] private Vector2 cellSize = new Vector2(220f, 220f);
    [SerializeField] private Vector2 spacing = new Vector2(16f, 16f);
    [SerializeField] private GridLayoutGroup.Constraint constraint = GridLayoutGroup.Constraint.FixedColumnCount;
    [SerializeField] private int constraintCount = 3;
    [SerializeField] private TextAnchor childAlignment = TextAnchor.UpperLeft;

    private GridLayoutGroup _grid;

    private void Awake()
    {
        EnsureGrid();
        ApplySettings();
    }

    private void OnValidate()
    {
        EnsureGrid();
        ApplySettings();
    }

    private void EnsureGrid()
    {
        if (_grid == null)
        {
            _grid = GetComponent<GridLayoutGroup>();
        }

        if (_grid == null)
        {
            _grid = gameObject.AddComponent<GridLayoutGroup>();
        }
    }

    private void ApplySettings()
    {
        if (_grid == null)
        {
            return;
        }

        _grid.cellSize = cellSize;
        _grid.spacing = spacing;
        _grid.constraint = constraint;
        _grid.constraintCount = Mathf.Max(1, constraintCount);
        _grid.childAlignment = childAlignment;
    }
}
