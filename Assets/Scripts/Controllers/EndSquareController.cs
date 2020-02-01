using UnityEngine;

public class EndSquareController : SquareController
{

  private void Start()
  {
    PlaySceneManager.Instance.LockInPlace(this);
  }

  public override void SetActivated(bool newValue)
  {
    isActive = newValue;
    foreach (var cable in cables)
    {
      cable.color = newValue ? Color.yellow : Color.white;
    }

    if (newValue)
    {
      ActivateNextSquare();
      PlaySceneManager.Instance.CheckWin();
    }
  }
}