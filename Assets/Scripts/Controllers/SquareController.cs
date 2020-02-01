using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum BoundaryState
{
  INSIDE,
  LEFT,
  RIGHT,
  BOTTOM
}

public class SquareController : MonoBehaviour
{
  public Transform lockParent;
  public List<Vector2Int> outs = new List<Vector2Int>();
  public bool isActive = false;
  public SpriteRenderer[] cables;
  public SquareController next;
  public SquareController prev;
  public bool isBlocker = true;

  public Vector3 GetRotatePos()
  {
    return new Vector3(
      transform.localPosition.y,
      -transform.localPosition.x,
      0
    );
  }

  public virtual void SetActivated(bool newValue)
  {
    isActive = newValue;
    foreach (var cable in cables)
    {
      cable.color = newValue ? Color.yellow : Color.white;
    }

    if (newValue)
    {
      PlaySceneManager.Instance.root = this;
      ActivateNextSquare();
    }
    else
    {
      if (next != null)
      {
        next.prev = null;
        next.SetActivated(false);
        this.next = null;
      }

      if (prev != null)
      {
        PlaySceneManager.Instance.root = prev;
      }
    }
  }

  public List<int> GetOutIndexes()
  {

    var result = new List<int>();
    foreach (var output in outs)
    {
      result.Add(PlaySceneManager.Instance.TransformToGridIndex((Vector2)transform.position + output));
    }

    return result;
  }

  public void ActivateNextSquare()
  {
    foreach (var index in GetOutIndexes())
    {
      ActivateNextSquareFromOut(index);
    }
  }

  public void ActivateNextSquareFromOut(int targetIndex)
  {
    var thisIndex = PlaySceneManager.Instance.TransformToGridIndex(transform.position);
    var target = PlaySceneManager.Instance.GetStaticSquareAt(targetIndex);
    if (target == null) return;
    if (target.isActive) return;
    if (target.GetOutIndexes().Contains(thisIndex))
    {
      target.prev = this;
      this.next = target;
      target.SetActivated(true);
    }
  }

  public void Rotate()
  {
    transform.localPosition = GetRotatePos();
    transform.Rotate(0, 0, -90);
    for (int i = 0; i < outs.Count; i++)
    {
      var output = outs[i];
      outs[i] = new Vector2Int(output.y, -output.x);
    }
  }

  public void LockInPlace()
  {
    PlaySceneManager.Instance.LockInPlace(this);
  }

  public void Clear()
  {
    if (isActive) SetActivated(false);
    PlaySceneManager.Instance.RemoveFromGrid(transform.position);
    GameObject.Destroy(gameObject);
  }
}
