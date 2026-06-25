using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class ScreenshotTaker : MonoBehaviour
{
    [SerializeField] private KeyCode screenshotKey = KeyCode.F12;
    [SerializeField] private string filePrefix = "FishBalatro";
    [SerializeField] private int superSize = 1;

    private bool isCapturing;

    private void Update()
    {
        if (Input.GetKeyDown(screenshotKey) && !isCapturing)
        {
            StartCoroutine(CaptureAtEndOfFrame());
        }
    }

    private IEnumerator CaptureAtEndOfFrame()
    {
        isCapturing = true;
        yield return new WaitForEndOfFrame();

        string picturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        string fileName = filePrefix + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        string fullPath = Path.Combine(picturesPath, fileName);

        ScreenCapture.CaptureScreenshot(fullPath, Mathf.Max(1, superSize));
        Debug.Log("Screenshot saved to: " + fullPath);

        isCapturing = false;
    }
}
