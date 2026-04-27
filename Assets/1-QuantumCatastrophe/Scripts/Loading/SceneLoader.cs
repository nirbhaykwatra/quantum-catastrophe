using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [SerializeField]
    public string sceneName;
    
    [SerializeField]
    public string bankFolderURL;

    [FMODUnity.BankRef]
    public List<string> banks;

    private void Awake()
    {
        LoadBanks();
    }

    public void LoadBanks()
    {
        foreach (string b in banks)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            FMODUnity.RuntimeManager.LoadBank(b, bankFolderURL, true);
#else
            FMODUnity.RuntimeManager.LoadBank(b, true);
#endif
            Debug.Log("Loaded bank " + b);
        }
        /*
            For Chrome / Safari browsers / WebGL.  Reset audio on response to user interaction (LoadBanks is called from a button press), to allow audio to be heard.
        */
        FMODUnity.RuntimeManager.CoreSystem.mixerSuspend();
        FMODUnity.RuntimeManager.CoreSystem.mixerResume();
        
        StartCoroutine(CheckBanksLoaded());
    }

    private IEnumerator CheckBanksLoaded()
    {
        while (!FMODUnity.RuntimeManager.HaveAllBanksLoaded)
        {
            yield return null;
        }
        
        LoadNextScene();
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}