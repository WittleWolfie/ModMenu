using UnityEngine;

namespace ModMenu.Window
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
      if (Binder is not null)
        Binder(transform, ID);
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
