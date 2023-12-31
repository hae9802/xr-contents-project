using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneCounter : MonoBehaviour
{
    public int CurState { get; set; }
    public bool IsStart { get; set; }
    
    public bool IsEndFirstAnim { get; set; }
    public bool IsEndSecondAnim { get; set; }
    public bool IsFade { get; set; }
    
    public bool IsEndingOn { get; set; }
    
    public string SceneName { get; set; }

    private static CutSceneCounter inst;

    public static CutSceneCounter Inst => inst;

    private void Awake()
    {
        if (inst == null)
        {
            inst = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        DontDestroyOnLoad(gameObject);

        CurState = -1;

        IsStart = false;
        IsEndFirstAnim = false;
        IsEndFirstAnim = false;
        IsFade = false;
    }

    public void SettingEndingScene()
    {
        IsFade = false;
        IsStart = false;
        IsEndFirstAnim = true;
        IsEndSecondAnim = true;
    }

    public void SettingRestartScene()
    {
        IsStart = true;
        IsFade = false;
        IsEndFirstAnim = true;
        IsEndSecondAnim = true;
    }

    public void SettingGameOver()
    {
        CurState = -1;
        IsFade = false;
        IsStart = false;
        IsEndFirstAnim = false;
        IsEndSecondAnim = false;
        IsEndingOn = true;
    }

    public void SettingSceneName()
    {
        
    }
}
