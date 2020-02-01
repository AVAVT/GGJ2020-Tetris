using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlaySceneManager : MonoBehaviour
{
  public static PlaySceneManager Instance { get; private set; }
  public string levelName;
  public TMPro.TMP_Text notificationText;
  public TMPro.TMP_Text tutorialText;
  public BlockController[] prefabs;
  public List<SquareController> roots;
  public EndSquareController[] targets;
  public string nextScene;
  Dictionary<int, SquareController> staticBlocks = new Dictionary<int, SquareController>();
  List<BlockController> nextBlocks = new List<BlockController>();
  public Transform[] previewPositions;
  public SpriteRenderer[] destroyLights;
  public AudioClip moveSound;
  public AudioClip lockSound;
  public AudioClip clearSound;
  BlockController currentBlock;
  bool acceptUserInput = false;
  bool shouldShowGameOver = false;
  bool isWin = false;
  int destroyCount = 5;

  private void Awake()
  {
    Instance = this;
  }

  private void Start()
  {
    PrepareNextBlock();
    PrepareNextBlock();
    StartCoroutine(StartGame());
    tutorialText.text = $"L,R,D: Move\nUp: Drop\nSpace: Rotate\nShift: Destroy ({destroyCount.ToString()})";
  }

  IEnumerator StartGame()
  {
    yield return new WaitForSeconds(2);
    acceptUserInput = true;
    SpawnNextBlock();
  }

  private void Update()
  {
    if (shouldShowGameOver)
    {
      if (isWin) StartCoroutine(ToNextLevel());
      else
      {
        notificationText.text = "Game Over\nPress Escape to replay";
        notificationText.transform.parent.gameObject.SetActive(true);
      }
      shouldShowGameOver = false;
      return;
    }
    if (Input.GetKeyDown(KeyCode.Escape)) SceneManager.LoadScene("Level1");
    if (!acceptUserInput) return;

    if (Input.GetKeyDown(KeyCode.LeftArrow)) currentBlock.MoveLeft();
    else if (Input.GetKeyDown(KeyCode.RightArrow)) currentBlock.MoveRight();
    else if (Input.GetKeyDown(KeyCode.DownArrow)) currentBlock.MoveDown();
    else if (Input.GetKeyDown(KeyCode.UpArrow)) currentBlock.Drop();

    else if (Input.GetKeyDown(KeyCode.Space)) currentBlock.Rotate();
    else if (Input.GetKeyDown(KeyCode.LeftShift)) DestroyCurrentBlock();
  }

  IEnumerator ToNextLevel()
  {
    if (!string.IsNullOrEmpty(nextScene))
    {
      notificationText.text = "You win!!";
      notificationText.transform.parent.gameObject.SetActive(true);
      yield return new WaitForSeconds(2);
      SceneManager.LoadScene(nextScene);
    }
    else
    {
      notificationText.text = "CONGRATULATION YOU WON THE GAME!!";
      notificationText.transform.parent.gameObject.SetActive(true);
    }
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
        if (!staticBlocks.ContainsKey(12 * y + x) || !staticBlocks[12 * y + x].isBlocker)
        {
          lineCleared = false;
          continue;
        }
      }
      if (!lineCleared) continue;

      if (startLine == -9999) startLine = y;
      endLine = y;
    }

    AudioSource.PlayClipAtPoint(lockSound, Camera.main.transform.position, 1);

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
        if (staticBlocks.ContainsKey(12 * y + x)) staticBlocks[12 * y + x].Clear();
      }
    }

    yield return new WaitForSeconds(0.3f);

    AudioSource.PlayClipAtPoint(clearSound, Camera.main.transform.position, 1);

    var diff = end - start;
    for (int y = end; y < 14; y++)
    {
      for (int x = -4; x < 6; x++)
      {
        var index = 12 * y + x;
        if (staticBlocks.ContainsKey(index) && staticBlocks[index].isBlocker)
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

  void DestroyCurrentBlock()
  {
    if (destroyCount <= 0) return;
    destroyCount--;
    tutorialText.text = $"L,R,D: Move\nUp: Drop\nSpace: Rotate\nShift: Destroy ({destroyCount.ToString()})";
    for (int i = 0; i < 5; i++)
    {
      destroyLights[i].color = i < destroyCount ? Color.red : Color.gray;
    }
    GameObject.Destroy(currentBlock.gameObject);
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
    roots.ForEach(root => root.ActivateNextSquare());
    if (shouldShowGameOver) return;

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
      block.transform.SetParent(transform);
      staticBlocks.Add(gridIndex, block);
      if (gridIndex >= 12 * 12 - 4) GameOver();
    }
    catch
    {
      GameObject.Destroy(block.gameObject);
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
    shouldShowGameOver = true;
    acceptUserInput = false;
  }

  public void CheckWin()
  {
    foreach (var square in targets)
    {
      if (!square.isActive) return;
    }

    shouldShowGameOver = true;
    isWin = true;
    acceptUserInput = false;
  }
}