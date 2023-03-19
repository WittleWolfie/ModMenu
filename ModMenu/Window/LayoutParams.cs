using UnityEngine;

namespace ModMenu.Window
{
  /// <summary>
  /// Called when a view is ready for display.
  /// </summary>
  /// <param name="transform">The Unity <see cref="Transform"/> ready for display</param>
  /// <param name="id">An identifier used to identify a specific view</param>
  public delegate void OnLayout(Transform transform, string id);

  public abstract class LayoutParams
  {
    /// <summary>
    /// Identifier for a specific view.
    /// </summary>
    public string ID { get; private set; }

    /// <summary>
    /// Callback when the view is ready to be displayed
    /// </summary>
    public OnLayout LayoutHandler { get; private set; }

    protected LayoutParams(string id, OnLayout layoutHandler)
    {
      ID = id;
      LayoutHandler = layoutHandler;
    }

    internal void Apply(Transform transform)
    {
      if (LayoutHandler is not null)
        LayoutHandler(transform, ID);
      ApplyInternal(transform);
    }

    protected abstract void ApplyInternal(Transform transform);
  }

  /// <summary>
  /// Layout params for the root level, uses absolute positioning.
  /// </summary>
  public class AbsoluteLayoutParams : LayoutParams
  {
    private readonly Vector3? Position;
    private readonly Vector3? Scale;
    private readonly Quaternion? Rotation;

    public AbsoluteLayoutParams(
      string id,
      Vector3? position = null,
      Vector3? scale = null,
      Quaternion? rotation = null,
      OnLayout layoutHandler = null) : base(id, layoutHandler)
    {
      Position = position;
      Scale = scale;
      Rotation = rotation;
    }

    protected override void ApplyInternal(Transform transform)
    {
      transform.localPosition = Position ?? Vector3.zero;
      transform.localScale = Scale ?? Vector3.one;
      transform.localRotation = Rotation ?? Quaternion.identity;
    }
  }
}
