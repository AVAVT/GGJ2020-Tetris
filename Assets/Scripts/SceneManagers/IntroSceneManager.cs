using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroSceneManager : MonoBehaviour
{
  public GameObject music;
  public void Play()
  {
    GameObject.DontDestroyOnLoad(music);
    SceneManager.LoadScene("Level1");
  }
}
