using Kingmaker.UI;
using UnityEngine.UI;

namespace ModMenu.Window.Layout
{
  public class GridStyle
  {
    private readonly GridLayoutGroup.Axis Axis;
    private readonly GridLayoutGroup.Constraint Constraint;
    private readonly int ConstraintCount;

    private readonly int CellHeight;
    private readonly int CellWidth;

    public static GridStyle FixedColumns(
      int columnCount,
      int cellHeight = 60,
      int cellWidth = 346)
    {
      return new(
        GridLayoutGroup.Axis.Horizontal,
        GridLayoutGroup.Constraint.FixedColumnCount,
        columnCount,
        cellHeight,
        cellWidth);
    }

    public static GridStyle FixedRows(
      int rowCount,
      int cellHeight = 60,
      int cellWidth = 346)
    {
      return new(
        GridLayoutGroup.Axis.Vertical,
        GridLayoutGroup.Constraint.FixedRowCount,
        rowCount,
        cellHeight,
        cellWidth);
    }

    private GridStyle(
      GridLayoutGroup.Axis axis,
      GridLayoutGroup.Constraint constraint,
      int constraintCount,
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
    }
  }
}
