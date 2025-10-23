using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using static UnityEngine.ParticleSystem;

public class VideoScript : MonoBehaviour
{

    public VideoPlayer video;

    [SerializeField]
    private GameObject screen;

    private bool isDone;

    private VideoManager vm;

    private void Start()
    {
        vm = VideoManager.Instance;
        video.waitForFirstFrame = true;

        isDone = false;
        video.loopPointReached += OnLoopPointReached;
    }

    // Update is called once per frame
    void Update()
    {
        if (video.isPlaying && !vm.videoHasPlayed)
        {
            Time.timeScale = 0;
        }
        else if (vm.videoHasPlayed)
        {
            screen.SetActive(false);
            Time.timeScale = 1; 
        }

    }

    void OnLoopPointReached(VideoPlayer vp)
    {
        // Play the particle effect when the video reaches the end.

        isDone = true;
        vm.videoHasPlayed = true;
    }
}
