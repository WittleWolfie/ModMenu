using Kingmaker.UI;
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

    public static GridStyle FixedColumns(int columnCount, int height = 50, int width = 200)
    {
      return new(
        GridLayoutGroup.Axis.Horizontal, GridLayoutGroup.Constraint.FixedColumnCount, columnCount, height, width);
    }

    public static GridStyle FixedRows(int rowCount, int height = 50, int width = 200)
    {
      return new(GridLayoutGroup.Axis.Vertical, GridLayoutGroup.Constraint.FixedRowCount, rowCount, height, width);
    }

    private GridStyle(
      GridLayoutGroup.Axis axis,
      GridLayoutGroup.Constraint constraint,
      int constraintCount,
      int height,
      int width)
    {
      Axis = axis;
      Constraint = constraint;
      ConstraintCount = constraintCount;
      Height = height;
      Width = width;
    }

    internal void Apply(GridLayoutGroupWorkaround grid)
    {
      grid.startAxis = Axis;
      grid.constraint = Constraint;
      grid.constraintCount = ConstraintCount;
      grid.cellSize = new(Width, Height);
    }
  }
}
