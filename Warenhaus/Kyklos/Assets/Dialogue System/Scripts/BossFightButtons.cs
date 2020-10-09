using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossFightButtons : MonoBehaviour
{
    public void FightKid()
    {
        SceneManager.LoadSceneAsync("Kid");
    }

    public void FightZombie()
    {
        SceneManager.LoadSceneAsync("Zombie");
    }

    public void FightHound()
    {
        SceneManager.LoadSceneAsync("Hound");
    }

    public void FightShark()
    {
        SceneManager.LoadSceneAsync("Shark");
    }
}
