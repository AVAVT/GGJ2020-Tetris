using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySceneManager : MonoBehaviour
{
  public static PlaySceneManager Instance { get; private set; }
  public BlockController[] prefabs;
  public SquareController root;
  Dictionary<int, SquareController> staticBlocks = new Dictionary<int, SquareController>();
  List<BlockController> nextBlocks = new List<BlockController>();
  public Transform[] previewPositions;
  BlockController currentBlock;
  bool acceptUserInput = true;
  bool gameOver = false;

  private void Awake()
  {
    Instance = this;
  }

  private void Start()
  {
    PrepareNextBlock();
    PrepareNextBlock();
    SpawnNextBlock();
  }

  private void Update()
  {
    if (!acceptUserInput) return;

    if (Input.GetKeyDown(KeyCode.LeftArrow)) currentBlock.MoveLeft();
    if (Input.GetKeyDown(KeyCode.RightArrow)) currentBlock.MoveRight();
    if (Input.GetKeyDown(KeyCode.DownArrow)) currentBlock.MoveDown();
    if (Input.GetKeyDown(KeyCode.UpArrow)) currentBlock.Drop();
    if (Input.GetKeyDown(KeyCode.Space)) currentBlock.Rotate();
  }

  public void OnBlockTouchedGround()
  {
    int startLine = -9999;
    int endLine = -9999;
    for (int y = -10; y < 10; y++)
    {
      bool lineCleared = true;
      for (int x = -4; x < 6; x++)
      {
        if (!staticBlocks.ContainsKey(12 * y + x))
        {
          lineCleared = false;
          continue;
        }
      }
      if (!lineCleared) continue;

      if (startLine == -9999) startLine = y;
      endLine = y;
    }

    if (startLine == -9999) SpawnNextBlock();
    else StartCoroutine(AnimateDestroy(startLine, endLine + 1));
  }

  IEnumerator AnimateDestroy(int start, int end)
  {
    acceptUserInput = false;
    yield return new WaitForSeconds(0.3f);
    for (int y = start; y < end; y++)
    {
      for (int x = -4; x < 6; x++)
      {
        staticBlocks[12 * y + x].Clear();
      }
    }

    yield return new WaitForSeconds(0.3f);

    var diff = end - start;
    for (int y = end; y < 10; y++)
    {
      for (int x = -4; x < 6; x++)
      {
        var index = 12 * y + x;
        if (staticBlocks.ContainsKey(index))
        {
          var block = staticBlocks[index];
          staticBlocks.Remove(index);
          staticBlocks[index - 12 * diff] = block;
          block.transform.position = block.transform.position + Vector3.down * diff;
        }
      }
    }
    acceptUserInput = true;
    SpawnNextBlock();
  }

  void PrepareNextBlock()
  {
    var newBlock = Instantiate(prefabs.RandomItem());
    nextBlocks.Add(newBlock);
    newBlock.transform.position = previewPositions[nextBlocks.Count - 1].position;
    newBlock.Init(transform);
  }

  public void SpawnNextBlock()
  {
    root.ActivateNextSquare();
    if (gameOver) return;

    currentBlock = nextBlocks[0];
    currentBlock.falling = true;
    currentBlock.transform.position = transform.position;

    nextBlocks[0] = nextBlocks[1];
    nextBlocks[0].transform.position = previewPositions[0].position;
    nextBlocks.RemoveAt(1);
    PrepareNextBlock();
  }

  public bool IsPositionValid(Vector3 pos)
  {
    if (pos.x < -5) return false;
    if (pos.x > 5) return false;
    if (pos.y < -11) return false;
    var gridIndex = TransformToGridIndex(pos);
    if (!staticBlocks.ContainsKey(gridIndex)) return true;
    if (!staticBlocks[gridIndex].isBlocker) return true;
    return false;
  }

  public int TransformToGridIndex(Vector2 pos)
  {
    var gridPos = Vector2Int.RoundToInt(Vector2.one * 0.5f + pos);
    return gridPos.y * 12 + gridPos.x;
  }

  public SquareController GetStaticSquareAt(int index)
  {
    return staticBlocks.ContainsKey(index) ? staticBlocks[index] : null;
  }

  public void LockInPlace(SquareController block)
  {
    try
    {
      var gridIndex = TransformToGridIndex(block.transform.position);
      staticBlocks.Add(gridIndex, block);
      block.transform.SetParent(transform);
      if (gridIndex >= 9 * 12 - 4) GameOver();
    }
    catch
    {
      GameOver();
    }
  }
  public void RemoveFromGrid(Vector2 pos)
  {
    var index = TransformToGridIndex(pos);
    if (staticBlocks.ContainsKey(index)) staticBlocks.Remove(index);
  }
  void GameOver()
  {
    gameOver = true;
    acceptUserInput = false;
    Debug.Log("Game Over");
  }

  public void Win()
  {
    gameOver = true;
    acceptUserInput = false;
    Debug.Log("Yay!!");
  }
}