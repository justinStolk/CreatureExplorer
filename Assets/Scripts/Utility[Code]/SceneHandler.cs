using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    public System.Action<AsyncOperation> onLoadCompleted;
    public System.Action<AsyncOperation> onUnloadCompleted;
    public System.Action<float> onProgressChanged;

    private void OnDestroy()
    {
        onLoadCompleted = null;
        onUnloadCompleted = null;
        onProgressChanged = null;
    }

    public void UnloadSceneAsync(int buildIndex, UnloadSceneOptions unloadOptions = UnloadSceneOptions.None)
    {
        StartCoroutine(UnloadAsync(buildIndex, unloadOptions));
    }

    public void UnloadSceneAsync(string name)
    {
        StartCoroutine(UnloadAsync(name));
    }
    
    public void LoadSceneAsync(int buildIndex, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        StartCoroutine(LoadAsync(buildIndex, loadSceneMode));
    }
    
    public  void LoadSceneAsync(string name, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        StartCoroutine(LoadAsync(name, loadSceneMode));
    }

    public void LoadScene(int index, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        SceneManager.LoadScene(index, loadSceneMode);
    }

    public void LoadScene(string name, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        SceneManager.LoadScene(name, loadSceneMode);
    }

    private IEnumerator LoadAsync(int index, LoadSceneMode loadSceneMode)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(index, loadSceneMode);
        asyncLoad.completed += onLoadCompleted;

        while (!asyncLoad.isDone)
        {
            onProgressChanged?.Invoke(asyncLoad.progress);
            yield return null;
        }
        onProgressChanged?.Invoke(1);
    }

    private IEnumerator LoadAsync(string name, LoadSceneMode loadSceneMode)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name, loadSceneMode);
        asyncLoad.completed += onLoadCompleted;

        while (!asyncLoad.isDone)
        {
            onProgressChanged?.Invoke(asyncLoad.progress);
            yield return null;
        }
        onProgressChanged?.Invoke(1);
    }

    private IEnumerator UnloadAsync(int index, UnloadSceneOptions unloadOptions)
    {
        AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync(index, unloadOptions);
        asyncLoad.completed += onUnloadCompleted;
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        System.GC.Collect();
    }

    private IEnumerator UnloadAsync(string name)
    {
        AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync(name);
        asyncLoad.completed += onUnloadCompleted;
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        System.GC.Collect();
    }
}
