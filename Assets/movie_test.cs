using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class movie_test : MonoBehaviour
{
    public MovieTexture movie;              //定义视频文件
    // Use this for initialization
    void Start()
    {

        GetComponent<AudioSource>().clip = movie.audioClip;       //获取音频文件
    }

    void update() { OnGUI(); }
    void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), movie, ScaleMode.ScaleToFit);

        if (GUILayout.Button("Play"))        //如果播放按键被按下
        {
            if (!movie.isPlaying )//&& !audio.isPlaying)        //如果视频和音频没有正在被播放
            {
                movie.Play();                   //播放视频文件
                GetComponent<AudioSource>().Play();                   //播放音频文件
            }

        }

        if (GUILayout.Button("pause"))        //如果暂停按键被按下
        {
            if (movie.isPlaying)// && audio.isPlaying)      //如果视频和音频正在被播放
            {
                movie.Pause();                   //暂停播放视频文件
                GetComponent<AudioSource>().Pause();                   //暂停播放音频文件
            }

        }

        if (GUILayout.Button("stop"))        //如果停止按键被按下
        {
            if (movie.isPlaying )//&& audio.isPlaying)      //如果视频和音频正在被播放
            {
                movie.Stop();                   //停止播放视频频文件
                GetComponent<AudioSource>().Pause();                  //停止播放音频频文件
            }

        }

    }
}