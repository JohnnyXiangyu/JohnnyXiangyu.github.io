using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class DialogueGlobals : MonoBehaviour
{
    public static DialogueGlobals instance;

    GameController gameController;
    public Dictionary<DialogueEnvironment.Environment, bool> IntroducitonHistory = new Dictionary<DialogueEnvironment.Environment, bool>();
    public Dictionary<DialogueEnvironment.Environment, bool> LocationHistory = new Dictionary<DialogueEnvironment.Environment, bool>();

    public Dictionary<string, int> BackgroundDict = new Dictionary<string, int> { { "TG", 0}, { "TV", 1 }, { "CW", 2 }, { "PH", 3 }, { "HB", 4 } };

    public List<int> remainingWC = new List<int>();

    public DialogueEnvironment.Environment[] IntroList;

    

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(this);

        gameController = FindObjectOfType<GameController>();
    }

    private void Start()
    {
        //gameController = FindObjectOfType<GameController>();
        Header.Skills[] wc = gameController.getNeededSkills();
        remainingWC.Add((int)wc[0]);
        remainingWC.Add((int)wc[1]);

    }


    public void ResetRemainingWC()
    {
        remainingWC.Clear();
        Header.Skills[] wc = gameController.getNeededSkills();
        remainingWC.Add((int)wc[0]);
        remainingWC.Add((int)wc[1]);
    }

    public void Update()
    {
        /*
        if(Input.GetKeyDown(KeyCode.W))
        {
            IntroPrintKeys();
        }
        if(Input.GetKeyDown(KeyCode.S))
        {
            LocPrintKeys();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log(GetHighestWC());
        }
        */

    }

    public void IntroPrintKeys()
    {
       foreach(var key in IntroducitonHistory.Keys)
        {
            Debug.Log(key);
        }
    }

    public void LocPrintKeys()
    {
        foreach (var key in LocationHistory.Keys)
        {
            Debug.Log(key);
        }
    }

    public void DayReset()
    {
        Debug.Log("Day has been reset");
        gameController.actions = 0;
        gameController.AdvanceDay();
        LocationHistory.Clear();
        ChangeDayCounter();
        if(gameController.day >= 4)
        {
            gameController.Restart();
        }
    }

    public void ChangeDayCounter()
    {
        //TextMeshProUGUI dayCount = GameObject.Find("Canvas").transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI dayCount = GameObject.Find("Canvas").transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        Debug.Log(dayCount);
        dayCount.text = "Day: " + gameController.day.ToString();
    }

    public void ManageHB()
    {
        List<GameObject> bossList = new List<GameObject>();
        bossList.Add(GameObject.Find("Canvas").transform.GetChild(2).Find("Zombie").gameObject);
        bossList.Add(GameObject.Find("Canvas").transform.GetChild(2).Find("Kid").gameObject);
        bossList.Add(GameObject.Find("Canvas").transform.GetChild(2).Find("Hound").gameObject);
        bossList.Add(GameObject.Find("Canvas").transform.GetChild(2).Find("Shark").gameObject);
        for (int i = 0; i < bossList.Count; i++)
        {
            if(gameController.IsDead((Header.Bosses)i))
            {
                bossList[i].SetActive(false);
            }
        }
    }

    public void ResetHB()
    {
        List<GameObject> bossList = new List<GameObject>();
        bossList.Add(GameObject.Find("Canvas").transform.GetChild(2).Find("Zombie").gameObject);
        bossList.Add(GameObject.Find("Canvas").transform.GetChild(2).Find("Kid").gameObject);
        bossList.Add(GameObject.Find("Canvas").transform.GetChild(2).Find("Hound").gameObject);
        bossList.Add(GameObject.Find("Canvas").transform.GetChild(2).Find("Shark").gameObject);

        for (int i = 0; i < bossList.Count; i++)
        {
            bossList[i].SetActive(true);
        }
    }

   


    public int GetHighestWC()
    {
        switch (remainingWC.Count)
        {
            case 2:
                if (remainingWC[0] > remainingWC[1])
                    return remainingWC[0];
                else
                    return remainingWC[1];
            case 1:
                return remainingWC[0];
            case 0:
                return -1;
            default:
                return -2;
        }
    }

    public void RemoveHighestWC()
    {
        remainingWC.Remove(GetHighestWC());
    }


    

}
