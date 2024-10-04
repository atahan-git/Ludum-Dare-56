using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {
    public static SceneLoader s;

    public Camera ScreenSpaceCanvasCamera;
    
    [SerializeField]
    private SceneReference mainMenu;
    
    public SceneReference playScene;
    
    [SerializeField]
    private SceneReference initialScene;

    public bool isLevelStarted = false;
    public bool isLevelFinished = false;

    public bool isLevelInProgress {
        get {
            return isLevelStarted && !isLevelFinished;
        }
    }

    public bool isProfileMenu = true;
    
    private void Awake() {
        if (s == null) {
            s = this;
            DontDestroyOnLoad(gameObject);

            if (SceneManager.GetActiveScene().path == initialScene.ScenePath) {
                LoadScene(mainMenu);
            } else {
                loadingScreen.SetActive(false);
            }
        } else if(s!= this){
            loadingScreen.SetActive(false);
            Destroy(gameObject);
        }
    }


    public GameObject loadingScreen;
    public CanvasGroup canvasGroup;


    
    public void LoadMenuScene() {
        isLevelFinished = false;
        isLevelStarted = false;
        isProfileMenu = false;
        LoadScene(mainMenu);
    }

    public void LoadPlayScene() {
        LoadScene(playScene);
    }

    public static float loadingProgress;
    public bool isLoading = false;
    private void LoadScene(SceneReference sceneReference) {
        if(!isLoading)
            StartCoroutine(StartLoad(sceneReference));
    }

    IEnumerator StartLoad(SceneReference sceneReference) {
        isLoading = true;
        loadingProgress = 0;
        loadingScreen.SetActive(true);
        yield return StartCoroutine(FadeLoadingScreen(0,1, 0.5f));

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneReference.ScenePath);
        while (!operation.isDone)
        {
            loadingProgress = Mathf.Clamp01(operation.progress / 0.9f);
            yield return null;
        }

        yield return StartCoroutine(FadeLoadingScreen(1,0, 0.5f));
        loadingScreen.SetActive(false);
        isLoading = false;
    }

    IEnumerator FadeLoadingScreen(float startValue, float targetValue, float duration)
    {
        float time = 0;

        while (time < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startValue, targetValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = targetValue;
    }
}
