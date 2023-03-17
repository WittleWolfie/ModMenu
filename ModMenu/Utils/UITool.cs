using Kingmaker.Utility;
using System.Linq;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace ModMenu.Utils
{
  /// <summary>
  /// General utilities for working w/ Unity UI, partially copied from BubbleGauntlet:
  /// https://github.com/factubsio/BubbleGauntlet/blob/main/BubbleGauntlet/UIHelpers.cs
  /// </summary>
  public static class UITool
  {
    /// <summary>
    /// Returns the Transform matching the specified <paramref name="path"/>
    /// </summary>
    public static Transform ChildTransform(this GameObject obj, string path)
    {
      return obj.transform.Find(path);
    }

    /// <summary>
    /// Returns the GameObject matching the specified <paramref name="path"/>
    /// </summary>
    public static GameObject ChildObject(this GameObject obj, string path)
    {
      return obj.ChildTransform(path)?.gameObject;
    }

    /// <summary>
    /// Returns a list of child GameObjects matching the provided <paramref name="paths"/>.
    /// </summary>
    public static List<GameObject> ChildObjects(this GameObject obj, params string[] paths)
    {
      return paths.Select(p => obj.transform.Find(p)?.gameObject).ToList();
    }

    /// <summary>
    /// Calls <see cref="GameObject.Destroy"/> on all GameObjects matching the provided <paramref name="paths"/>
    /// </summary>
    public static void DestroyChildren(this GameObject obj, params string[] paths)
    {
      obj.ChildObjects(paths).ForEach(GameObject.Destroy);
    }

    /// <summary>
    /// Calls <see cref="GameObject.DestroyImmediate"/> on all GameObjects matching the provided <paramref name="paths"/>
    /// </summary>
    public static void DestroyChildrenImmediate(this GameObject obj, params string[] paths)
    {
      obj.ChildObjects(paths).ForEach(GameObject.DestroyImmediate);
    }

    /// <summary>
    /// Calls <see cref="GameObject.DestroyImmediate"/> on all components of type <typeparamref name="T"/>
    /// </summary>
    public static void DestroyComponents<T>(this GameObject obj) where T : UnityEngine.Object
    {
      obj.GetComponents<T>().ForEach(c => GameObject.DestroyImmediate(c));
    }

    /// <summary>
    /// Invokes <paramref name="build"/> on the first component of type <typeparamref name="T"/> and returns it
    /// </summary>
    public static T EditComponent<T>(this GameObject obj, Action<T> build) where T : Component
    {
      var component = obj.GetComponent<T>();
      build(component);
      return component;
    }

    /// <summary>
    /// Returns the <see cref="RectTransform"/> of the GameObject matching the provided <paramref name="path"/>
    /// </summary>
    public static RectTransform ChildRect(this GameObject obj, string path)
    {
      return obj.ChildTransform(path) as RectTransform;
    }

    /// <summary>
    /// Returns the <see cref="RectTransform"/> of the GameObject
    /// </summary>
    public static RectTransform Rect(this GameObject obj)
    {
      return obj.transform as RectTransform;
    }

    /// <summary>
    /// Returns the <see cref="RectTransform"/> of the Transform
    /// </summary>
    public static RectTransform Rect(this Transform obj)
    {
      return obj as RectTransform;
    }

    /// <summary>
    /// Adds <paramref name="obj"/> as a child of <paramref name="parent"/>.
    /// </summary>
    /// 
    /// <remarks>Resets the position, scale, and rotation.</remarks>
    public static void AddTo(this Transform obj, Transform parent)
    {
      obj.SetParent(parent);
      obj.localPosition = Vector3.zero;
      obj.localScale = Vector3.one;
      obj.localRotation = Quaternion.identity;
      obj.Rect().anchoredPosition = Vector3.zero;
    }
  }
}
