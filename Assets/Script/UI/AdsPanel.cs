using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AdsPanel : MonoBehaviour
{
    [Header("Refs")]
    public Slider progressBar;
    public GameObject exitButton;

    [Header("Settings")]
    public float adDuration = 5f;

    private void OnEnable()
    {
        exitButton.SetActive(false);
        progressBar.value = 0f;
        StartCoroutine(RunAd());
    }

    private IEnumerator RunAd()
    {
        float elapsed = 0f;
        while (elapsed < adDuration)
        {
            elapsed += Time.deltaTime;
            progressBar.value = elapsed / adDuration;
            yield return null;
        }

        progressBar.value = 1f;
        exitButton.SetActive(true);
    }

    public void OnExitClick()
    {
        StopAllCoroutines();
        LevelManager.Instance?.OnAdsFinished();
    }
}
