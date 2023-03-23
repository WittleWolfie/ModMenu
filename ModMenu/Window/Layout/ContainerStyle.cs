using Kingmaker.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ModMenu.Window.Layout
{
  public class GridStyle
  {
    private readonly GridLayoutGroup.Axis Axis;
    private readonly GridLayoutGroup.Constraint Constraint;
    private readonly int ConstraintCount;

    private readonly int Height;
    private readonly int Width;

    private readonly int CellHeight;
    private readonly int CellWidth;

    public static GridStyle FixedColumns(
      int height,
      int width,
      int columnCount,
      int cellHeight = 50,
      int cellWidth = 200)
    {
      return new(
        GridLayoutGroup.Axis.Horizontal,
        GridLayoutGroup.Constraint.FixedColumnCount,
        columnCount,
        height,
        width,
        cellHeight,
        cellWidth);
    }

    public static GridStyle FixedRows(int height, int width, int rowCount, int cellHeight = 50, int cellWidth = 200)
    {
      return new(
        GridLayoutGroup.Axis.Vertical,
        GridLayoutGroup.Constraint.FixedRowCount,
        rowCount,
        height,
        width,
        cellHeight,
        cellWidth);
    }

    private GridStyle(
      GridLayoutGroup.Axis axis,
      GridLayoutGroup.Constraint constraint,
      int constraintCount,
      int height,
      int width,
      int cellHeight,
      int cellWidth)
    {
      Axis = axis;
      Constraint = constraint;
      ConstraintCount = constraintCount;
      CellHeight = cellHeight;
      CellWidth = cellWidth;
    }

    internal void Apply(GridLayoutGroupWorkaround grid)
    {
      grid.startAxis = Axis;
      grid.constraint = Constraint;
      grid.constraintCount = ConstraintCount;
      grid.cellSize = new(CellWidth, CellHeight);

      var viewport = grid.transform.parent;
      viewport.GetComponent<RectTransform>().sizeDelta = new(Width, Height);
    }
  }
}
