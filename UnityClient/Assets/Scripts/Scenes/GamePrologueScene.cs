using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class GamePrologueScene : MonoBehaviour
{
    private VideoPlayer videoPlayer = null;
    private int PlayerStatus = 0;


    // Start is called before the first frame update
    void Start()
    {
        GameObject camera = GameObject.Find("MainCamera");
        videoPlayer = camera.GetComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.loopPointReached += EndReached;
        // videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraNearPlane;
        // videoPlayer.targetCameraAlpha = 0.5F;

        PlayerStatus = 0;
        PlayVideo("Videos/Game/3dologo.mp4");
    }

    void UpdateStatus()
    {
        if (PlayerStatus == 0)
        {
            PlayerStatus = 1;
            PlayVideo("Videos/Game/nwclogo.mp4");
            return;
        }

        if (PlayerStatus == 1)
        {
            PlayerStatus = 2;
            PlayVideo("Videos/Game/h3x1intr.mp4");
            return;
        }

        if (PlayerStatus == 2)
        {
            // Exit this scene
            SceneManager.LoadScene("GameMenuScene");
        }
    }

    void PlayVideo(string videoFileName)
    {
        videoPlayer.Stop();


        videoPlayer.url = Path.Combine(Application.streamingAssetsPath, videoFileName);
        videoPlayer.frame = 0;
        videoPlayer.isLooping = false;

        videoPlayer.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            UpdateStatus();
        }

    }

    void EndReached(VideoPlayer vp)
    {
        UpdateStatus();
    }
}
