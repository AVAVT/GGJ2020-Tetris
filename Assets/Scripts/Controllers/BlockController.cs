using UnityEngine;

public class BlockController : MonoBehaviour
{
  public SquareController[] children;
  float cooldown = 0;
  Transform lockParent;
  public bool falling = false;

  private void Update()
  {
    if (!falling) return;
    cooldown += Time.deltaTime;
    if (cooldown > 1f)
    {
      MoveDown();
    }
  }

  public void Init(Transform lockParent)
  {
    this.lockParent = lockParent;
    foreach (var square in children) square.lockParent = lockParent;
  }

  public void Rotate()
  {
    foreach (var square in children)
    {
      var newLocalTransform = square.GetRotatePos();
      if (!PlaySceneManager.Instance.IsPositionValid(newLocalTransform + transform.position)) return;
    }

    foreach (var square in children) square.Rotate();
  }

  public void MoveLeft()
  {
    foreach (var square in children)
    {
      if (!PlaySceneManager.Instance.IsPositionValid(square.transform.position + Vector3.left)) return;
    }
    transform.position += Vector3.left;
  }

  public void MoveRight()
  {
    foreach (var square in children)
    {
      if (!PlaySceneManager.Instance.IsPositionValid(square.transform.position + Vector3.right)) return;
    }
    transform.position += Vector3.right;
  }

  public bool MoveDown()
  {
    foreach (var square in children)
    {
      if (!PlaySceneManager.Instance.IsPositionValid(square.transform.position + Vector3.down))
      {
        LockInPlace();
        return false;
      }
    }

    transform.position += Vector3.down;
    cooldown = 0;

    return true;
  }

  public void Drop()
  {
    while (MoveDown()) { }
  }

  void LockInPlace()
  {
    foreach (var square in children) square.LockInPlace();

    PlaySceneManager.Instance.OnBlockTouchedGround();

    GameObject.Destroy(gameObject);
  }
}