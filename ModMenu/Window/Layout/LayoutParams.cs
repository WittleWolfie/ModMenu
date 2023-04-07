using ModMenu.Utils;
using UnityEngine;

namespace ModMenu.Window.Layout
{
  /// <summary>
  /// Called when a view is ready for display.
  /// </summary>
  /// <param name="transform">The Unity <see cref="Transform"/> ready for display</param>
  /// <param name="id">An identifier used to identify a specific view</param>
  public delegate void OnBind(Transform transform, string id);

  public abstract class LayoutParams
  {
    internal readonly string ID;
    private readonly OnBind Binder;

    protected LayoutParams(string id, OnBind binder)
    {
      ID = id;
      Binder = binder;
    }

    internal void Apply(Transform transform)
    {
      ApplyInternal(transform);
      if (Binder is not null)
        Binder(transform, ID);
    }

    protected abstract void ApplyInternal(Transform transform);
  }

  /// <summary>
  /// Layout params which anchor themselves inside of the parent.
  /// </summary>
  public class RelativeLayoutParams : LayoutParams
  {
    private readonly Vector2? AnchorMin;
    private readonly Vector2? AnchorMax;

    public RelativeLayoutParams(
      string id,
      Vector2? anchorMin = null,
      Vector2? anchorMax = null,
      OnBind binder = null) : base(id, binder)
    {
      AnchorMin = anchorMin;
      AnchorMax = anchorMax;
    }

    protected override void ApplyInternal(Transform transform)
    {
      var rect = transform.Rect();
      rect.anchorMin = AnchorMin ?? new(0.05f, 0.05f);
      rect.anchorMax = AnchorMax ?? new(0.95f, 0.95f);
      rect.sizeDelta = Vector2.zero;
    }
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
      OnBind binder = null) : base(id, binder)
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
