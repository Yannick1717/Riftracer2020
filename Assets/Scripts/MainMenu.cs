using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour {

    public GameObject loadingScreen;
    public Slider slider;
    public TextMeshProUGUI progressText;

    public void StartGame() {
        loadingScreen.SetActive(true);
        StartCoroutine(LoadAsynchronously("prototype"));
    }

    IEnumerator LoadAsynchronously(string scene) {
        AsyncOperation operation = SceneManager.LoadSceneAsync("prototype");

        while (!operation.isDone) {
            //Ladebalken bis 100% gehen lassen.
            slider.value = Mathf.Clamp01(operation.progress / .9f);
            progressText.text = (Mathf.Clamp01(operation.progress / .9f) * 100).ToString("F0") + "%";

            //Auf nächsten Frame Warten.
            yield return null;
        }
    }
}
