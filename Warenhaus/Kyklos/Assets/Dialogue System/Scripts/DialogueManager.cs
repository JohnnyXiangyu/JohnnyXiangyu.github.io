using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Linq;


//Format for the option lines:
// winCondition;skill;boss;locationHistory;nextNodeID;Text
// Where winCondition is the win condition tree the edge will head towards
// Where skill the the player's currently equipped skill
// Where boss is ONLY really for the part when an npc asks you about which bosses to know about. Set this to the number of the boss the NPC will talk about when the dialogue option is chosen
// Where locationHistory is the next location that the player will travel to based on whether or not its been visited before during gameplay and during the day. (if already visited during the day, it wont show the option)
//      values 0, 1, 2 correspond to training grounds, tavern, castle walls respectively where the player has not visited ever before and will take the player to the intro scene
//      values 3, 4, 5 correspond to the same 3 locaitons respectively where the player has visited the location before during gameplay it will take the player to the non-intro scene dialogue
public class DialogueManager : MonoBehaviour
{
    [SerializeField] private DialogueEnvironment env;
    [SerializeField] private DialogueGlobals globals;
    private GameController gameController;

    [SerializeField] private TextAsset textFile;
    [SerializeField] private TextMeshProUGUI dialogueBox;
    [SerializeField] private TextMeshProUGUI nameBox;
    [SerializeField] private Transform dialogueContainer;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject dialogueWindow;
    [SerializeField] private GameObject HB_canvas;
    [SerializeField] private GameObject Dialogue_canvas;

    [SerializeField] private Image background;

    [SerializeField] private DialogueNode[] nodes;
    [Range(0.001f, 0.01f)]
    [SerializeField] private float textDelay;

    [SerializeField] private int cur = 0;
    [SerializeField] private int next = 0;

    private int textIndex = 0;
    private bool print = true;
    private bool optionsDisplayed = false;
    private string playerName = "Dorian";
    private string lastKnownBG = "";

    //DEBUG ONLY
    public bool skill0;
    public bool skill1;
    public bool skill2;
    public bool skill3;

    public bool start;
    public bool win = false;

    // Start is called before the first frame update
    void Start()
    {
        gameController = FindObjectOfType<GameController>();
        env = FindObjectOfType<DialogueEnvironment>();
        globals = FindObjectOfType<DialogueGlobals>();
        HB_canvas = this.transform.parent.GetChild(2).gameObject;

        //start =  false;
        
        globals.LocationHistory.Clear();

        
        Parse();

        
        LoadText("PH");
        globals.ChangeDayCounter();
    }

    public void LoadText(string background)
    {
        cur = 0;
        textIndex = 0;
        lastKnownBG = background;
        ChangeSpeaker();
        StartCoroutine(Speak(nodes[cur].mainText[textIndex]));
        //start = true;
    }

    // Update is called once per frame
    void Update()
    {
       /* 
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            gameController.playerSkills[Header.Skills.SHIELD] = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            gameController.playerSkills[Header.Skills.HEAL] = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            gameController.playerSkills[Header.Skills.BLOCK] = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            gameController.playerSkills[Header.Skills.JUMP] = true;
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Debug.Log((bool)gameController.playerSkills[Header.Skills.SHIELD]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            Debug.Log((bool)gameController.playerSkills[Header.Skills.HEAL]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            Debug.Log((bool)gameController.playerSkills[Header.Skills.BLOCK]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            Debug.Log((bool)gameController.playerSkills[Header.Skills.JUMP]);
        }*/

        if (cur == 7 && gameController.skillsUsed <= 0 && win == false)
        {
            win = true;
            SceneManager.LoadSceneAsync("Win");
        }


        if (cur == -1)
        {
            //Remove Dialogue Window and switch to hunting board
            dialogueWindow.SetActive(false);
            HB_canvas.SetActive(true);
            globals.ResetHB();
            globals.ManageHB();
            
        }
        else
        {
            if (nodes[cur].background != "" )
            {
                if (nodes[cur].background == "HB")
                {
                    dialogueWindow.SetActive(false);
                    if (!globals.LocationHistory.ContainsKey(DialogueEnvironment.Environment.HuntingBoard))
                        background.sprite = env.bgList[(int)DialogueEnvironment.Environment.HuntingBoard];
                    lastKnownBG = "HB";
                }
                else if (nodes[cur].background == "TV")
                {
                    background.sprite = env.bgList[(int)DialogueEnvironment.Environment.Tavern];
                    if (!globals.LocationHistory.ContainsKey(DialogueEnvironment.Environment.Tavern))
                        globals.LocationHistory.Add(DialogueEnvironment.Environment.Tavern, true);
                    lastKnownBG = "TV";
                }
                else if (nodes[cur].background == "PH")
                {
                    background.sprite = env.bgList[(int)DialogueEnvironment.Environment.PlayerHouse];
                    if (!globals.LocationHistory.ContainsKey(DialogueEnvironment.Environment.PlayerHouse))
                        globals.LocationHistory.Add(DialogueEnvironment.Environment.PlayerHouse, true);
                    lastKnownBG = "PH";
                }
                else if (nodes[cur].background == "CW")
                {
                    background.sprite = env.bgList[(int)DialogueEnvironment.Environment.CastleWalls];
                    if (!globals.LocationHistory.ContainsKey(DialogueEnvironment.Environment.CastleWalls))
                        globals.LocationHistory.Add(DialogueEnvironment.Environment.CastleWalls, true);
                    lastKnownBG = "CW";
                }
                else if (nodes[cur].background == "TG")
                {
                    background.sprite = env.bgList[(int)DialogueEnvironment.Environment.TrainingGrounds];
                    if (!globals.LocationHistory.ContainsKey(DialogueEnvironment.Environment.TrainingGrounds))
                        globals.LocationHistory.Add(DialogueEnvironment.Environment.TrainingGrounds, true);
                    lastKnownBG = "TG";
                }

            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                //If all the text has been printed to screen, proceed with the next chunk of text
                if (print == false && dialogueBox.IsActive() == true)
                {
                    //Speak the next chunk of main text
                    textIndex++;
                    if (textIndex < nodes[cur].mainText.Count && nodes[cur].mainText.Count != 0)
                    {
                        print = true;
                        StartCoroutine(Speak(nodes[cur].mainText[textIndex]));
                    }
                    else if (nodes[cur].options.Count > 0 && !optionsDisplayed)
                    {
                        //Display Dialogue Choices
                        //Selecting the choice will decide which node is gone to next
                        //Do not update cur in this case, it will be handled by the option
                        CreateDialogueChoices();
                        optionsDisplayed = true;
                    }
                    else if (optionsDisplayed && dialogueBox.IsActive() == false)
                    {

                    }
                    else if (optionsDisplayed && dialogueBox.IsActive() == true)
                    {
                        optionsDisplayed = false;
                        cur = next;
                        textIndex = 0;
                        print = true;
                        if (cur > 0)
                        {
                            if (nodes[cur].mainText.Count == 0)
                            {
                                CreateDialogueChoices();
                                optionsDisplayed = true;
                            }
                            else
                            {
                                ChangeSpeaker();
                                StartCoroutine(Speak(nodes[cur].mainText[textIndex]));
                            }
                        }
                    }
                    else
                    {
                        //Move onto the next default node
                        //cur will be updated to the default next node
                        next = nodes[cur].nextID;
                        if (next > 0)
                        {
                            cur = next;
                            if (nodes[cur].mainText.Count == 0)
                            {
                                CreateDialogueChoices();
                                optionsDisplayed = true;
                            }
                            else
                            {
                                textIndex = 0;
                                print = true;
                                ChangeSpeaker();
                                StartCoroutine(Speak(nodes[cur].mainText[textIndex]));
                            }
                        }
                        else
                        {
                            //Remove Dialogue Window and switch to hunting board
                            dialogueWindow.SetActive(false);
                            HB_canvas.SetActive(true);
                            globals.ResetHB();
                            globals.ManageHB();

                            //background.sprite = env.bgList[(int)DialogueEnvironment.Environment.HuntingBoard];
                        }
                    }
                }
                //If all the text has not been printed, jump to printing the rest
                else
                {
                    print = false;
                }
            }
        }
    }
    
    
    /*
     * Sequence of actions that result in the flow of dialogue text
     * 
     * 1) Show dialogue option iff mainText list is exhausted and optionList is greater than 0
     * 
     * 
     */

    //Create Dialogue Choices
    //There may be greater than 3 dialogue choices present in a single DialogueNode
    //This function should create dialogue options based on the data present in gameController
    private void CreateDialogueChoices()
    {
        dialogueBox.gameObject.SetActive(false);

        foreach (Option option in nodes[cur].options)
        {
            if (option.bossAlive >= 0)
            {
                //Only print out these boss options if boss is alive
                if (!gameController.IsDead((Header.Bosses)option.bossAlive))
                {
                    var dialogueChoice = Instantiate(buttonPrefab, dialogueContainer);

                    dialogueChoice.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = option.optionText;
                    dialogueChoice.GetComponent<Button>().onClick.AddListener(() => { DisplayDialogueChoice(option.optionText); next = option.nextId; Debug.Log("Boss filter"); });
                }

            }
            else
            {
                if (option.locationHist < 6 && option.locationHist >= 0)
                {
                    if (globals.BackgroundDict[lastKnownBG] == option.locationHist % 3)
                    {
                        continue;
                    }
                }
                else if(option.locationHist >= 6)
                {
                    if (globals.BackgroundDict[lastKnownBG] == 3 && option.locationHist == 6)
                        continue;
                    if (globals.BackgroundDict[lastKnownBG] == 4 && option.locationHist == 7)
                        continue;
                }

                if (option.highestPrioWC >= 0)
                {
                    //Check if the game Controller has that wc set
                    //if not continue;
                    //if(option.locationHist)
                    if (gameController.actions == 2 && option.locationHist % 3 != globals.BackgroundDict[lastKnownBG] && option.locationHist < 6 && option.locationHist >= 0)
                        continue;

                    if (globals.LocationHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)) && option.locationHist != -1)
                        continue;

                    if (globals.IntroducitonHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)) && option.locationHist < 3 && option.locationHist != -1)
                        continue;

                    if (!globals.IntroducitonHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)) && option.locationHist >= 3)
                        continue;

                    if (globals.GetHighestWC() == option.highestPrioWC)
                    {
                        var _dialogueChoice = Instantiate(buttonPrefab, dialogueContainer);

                        _dialogueChoice.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = option.optionText;
                        _dialogueChoice.GetComponent<Button>().onClick.AddListener(() => { DisplayDialogueChoice(option.optionText); next = option.nextId;
                                                                                            if (!globals.IntroducitonHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)))
                                                                                            { globals.IntroducitonHistory.Add((DialogueEnvironment.Environment)(option.locationHist % 3), true); }
                                                                                            if (!globals.LocationHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)))
                                                                                            { globals.LocationHistory.Add((DialogueEnvironment.Environment)(option.locationHist % 3), true); }
                                                                                            if(option.locationHist % 3 != globals.BackgroundDict[lastKnownBG] && option.locationHist < 6 && option.locationHist >= 0)
                                                                                                gameController.doAction();
                                                                                            Debug.Log("WC filter"); 
                        });
                        continue;
                    }
                    else
                        continue;
                }

                if (globals.GetHighestWC() != 1 && globals.GetHighestWC() != 2 && (option.locationHist == 0 || option.locationHist == 3) && option.highestPrioWC == -2)
                {
                    //If two actions have already been taken today skip
                    if (gameController.actions == 2 && option.locationHist % 3 != globals.BackgroundDict[lastKnownBG] && option.locationHist < 6 && option.locationHist >= 0)
                        continue;

                    //If this location has already been visited today
                    if (globals.LocationHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)))
                        continue;

                    //If this location has been introduced to the player and the option is linked to an intro scene node, skip
                    if (globals.IntroducitonHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)) && option.locationHist < 3)
                        continue;

                    //If this location has not been introduced to the player and the option is linked to a post scene node, skip
                    if (!globals.IntroducitonHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)) && option.locationHist >= 3)
                        continue;

                    var _dialogueChoice = Instantiate(buttonPrefab, dialogueContainer);

                    _dialogueChoice.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = option.optionText;
                    _dialogueChoice.GetComponent<Button>().onClick.AddListener(() => { DisplayDialogueChoice(option.optionText); next = option.nextId;
                                                                                        if (!globals.IntroducitonHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)))
                                                                                        { globals.IntroducitonHistory.Add((DialogueEnvironment.Environment)(option.locationHist % 3), true); }
                                                                                        if (!globals.LocationHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)))
                                                                                        { globals.LocationHistory.Add((DialogueEnvironment.Environment)(option.locationHist % 3), true); }
                                                                                        if (option.locationHist % 3 != globals.BackgroundDict[lastKnownBG] && option.locationHist < 6 && option.locationHist >= 0)
                                                                                            gameController.doAction();
                                                                                        Debug.Log("WC filter filler node TG"); 
                    });
                    continue;
                }

                if ((globals.GetHighestWC() == 1 || globals.GetHighestWC() == 2) && (option.locationHist == 0 || option.locationHist == 3) && option.highestPrioWC == -2)
                    continue;

                if (globals.GetHighestWC() != 0 && globals.GetHighestWC() != 3 && (option.locationHist == 2 || option.locationHist == 5) && option.highestPrioWC == -2)
                {
                    if (gameController.actions == 2 && option.locationHist % 3 != globals.BackgroundDict[lastKnownBG] && option.locationHist < 6 && option.locationHist >= 0)
                        continue;

                    if (globals.LocationHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)))
                        continue;
                    
                    if (globals.IntroducitonHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)) && option.locationHist < 3)
                        continue;

                    if (!globals.IntroducitonHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)) && option.locationHist >= 3)
                        continue;

                    var _dialogueChoice = Instantiate(buttonPrefab, dialogueContainer);

                    _dialogueChoice.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = option.optionText;
                    _dialogueChoice.GetComponent<Button>().onClick.AddListener(() => { DisplayDialogueChoice(option.optionText); next = option.nextId;
                                                                                        if (!globals.IntroducitonHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)))
                                                                                        { globals.IntroducitonHistory.Add((DialogueEnvironment.Environment)(option.locationHist % 3), true); }
                                                                                        if (!globals.LocationHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)))
                                                                                        { globals.LocationHistory.Add((DialogueEnvironment.Environment)(option.locationHist % 3), true); }
                                                                                        if (option.locationHist % 3 != globals.BackgroundDict[lastKnownBG] && option.locationHist < 6 && option.locationHist >= 0)
                                                                                            gameController.doAction();
                                                                                        Debug.Log("WC filter filler node CW"); 
                    });
                    continue;
                }
                
                if ((globals.GetHighestWC() == 0 || globals.GetHighestWC() == 3) && (option.locationHist == 2 || option.locationHist == 5) && option.highestPrioWC == -2)
                    continue;

                if (option.currentSkill >= 0)
                {
                    //Check if the player has the current skill equipped
                    //if not continue;
                    if (
                         (bool)gameController.playerSkills[(Header.Skills)option.currentSkill]
                        /*(int)gameController.skill1 == option.currentSkill || (int)gameController.skill2 == option.currentSkill*/
                        )
                    {
                        var _dialogueChoice = Instantiate(buttonPrefab, dialogueContainer);

                        _dialogueChoice.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = option.optionText;
                        _dialogueChoice.GetComponent<Button>().onClick.AddListener(() => { DisplayDialogueChoice(option.optionText); next = option.nextId;
                            globals.RemoveHighestWC();
                            gameController.skillsUsed--;
                            Debug.Log("Skill filter"); });
                        continue;
                    }
                    else
                        continue;
                }

                if(option.locationHist >= 0)
                {
                    //Check if the player has already visited that place
                    //Ordering for both are Training Grounds, Tavern, CastleWall respectively
                    //0,1,2 are intro scenes
                    //3,4,5 are scenes where the player has already visited it before

                    //Check for the player's location history for the day by accessing DialogueGlobals.LocationHistory
                    // if DialogueGlobals.hasKey((DialogueEnvironment.Environment) option.locationHist % 3)
                    //      Then the player has been there before so instead of creating a dialogue choice continue

                    //if( locationHist >= 0 && < 3)
                    // Then create dialogue choice heading to the respective intro scene
                    //otherwise, if (locationHist > 3 && < 6)
                    // Then create dialogue choice heading to the respective scene where the player has already met the npc there
                    // otherwise if (locationHist >= 6)
                    // Then let locationHist = 6 correspond to PlayerHouse and locationHist = 7 correspond to the Huntint Board

                    //If the player is at home do not show an option to go home
                    if (globals.BackgroundDict[lastKnownBG] == 3 && option.locationHist == 6)
                        continue;

                    //If the player is at the hunting board do not show an option to go to the huning board
                    //This should never really be checked since the hunting board is displayed separately from the dialogue, but just for consistency
                    if (globals.BackgroundDict[lastKnownBG] == 4 && option.locationHist == 7)
                        continue;

                    
                    if (option.locationHist < 6)
                    {
                        //If the current the last known bg corresponds to the background that the option will take you to, skip that option
                        //NOTE: When you want to switch scenes, the first node of that scene must contain the name of the scene eg "HB", "CW", etc. and all succeeding nodes in the same scene must have  "" as its bg
                        // IOW, the entry node for a location must be labeled with the bg on the first line, and all nodes within that same location must have the bg labeled as ""
                        if (globals.BackgroundDict[lastKnownBG] == option.locationHist % 3)
                            continue;

                        if (gameController.actions == 2 && option.locationHist % 3 != globals.BackgroundDict[lastKnownBG] && option.locationHist >= 0)
                            continue;

                        //If the player has been to the location that the option is trying to take you to, don't show this option
                        if (globals.LocationHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)))
                            continue;

                        if (globals.IntroducitonHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)) && option.locationHist < 3)
                            continue;

                        if (!globals.IntroducitonHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)) && option.locationHist >= 3)
                            continue;

                        //If the player has gone through the introduction scene, and the dialogue option will take you to a node post-introduction, show that option
                        if (globals.IntroducitonHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)) && option.locationHist >= 3)
                        {
                            var _dialogueChoice = Instantiate(buttonPrefab, dialogueContainer);

                            _dialogueChoice.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = option.optionText;
                            _dialogueChoice.GetComponent<Button>().onClick.AddListener(() => { DisplayDialogueChoice(option.optionText); next = option.nextId;
                                                                                                if (!globals.IntroducitonHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)))
                                                                                                { globals.IntroducitonHistory.Add((DialogueEnvironment.Environment)(option.locationHist % 3), true); }
                                                                                                if (!globals.LocationHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)))
                                                                                                { globals.LocationHistory.Add((DialogueEnvironment.Environment)(option.locationHist % 3), true); }
                                                                                                if (option.locationHist % 3 != globals.BackgroundDict[lastKnownBG] && option.locationHist < 6 && option.locationHist >= 0)
                                                                                                    gameController.doAction();
                                                                                                Debug.Log("Location filter go to post intro scene");
                            });
                            continue;
                        }
                        //Otherwise if the player has not gone through the introduction scene and the dialogue option will take you to an introduction scene, show that option
                        else if (!globals.IntroducitonHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)) && option.locationHist < 3)
                        {
                            var _dialogueChoice = Instantiate(buttonPrefab, dialogueContainer);

                            _dialogueChoice.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = option.optionText;
                            _dialogueChoice.GetComponent<Button>().onClick.AddListener(() => { DisplayDialogueChoice(option.optionText); next = option.nextId;
                                                                                               if(!globals.IntroducitonHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)))
                                                                                                 { globals.IntroducitonHistory.Add((DialogueEnvironment.Environment)(option.locationHist % 3), true); }
                                                                                               if (!globals.LocationHistory.ContainsKey((DialogueEnvironment.Environment)(option.locationHist % 3)))
                                                                                                { globals.LocationHistory.Add((DialogueEnvironment.Environment)(option.locationHist % 3), true); }
                                                                                                if (option.locationHist % 3 != globals.BackgroundDict[lastKnownBG] && option.locationHist < 6 && option.locationHist >= 0)
                                                                                                    gameController.doAction();
                                                                                               Debug.Log("WC filter go to intro scene");
                            });
                            continue;
                        }
                        //Otherwise, since the option is taking the player to an intro scene while the player has completed it, or since the option is taking the player to a post-intro scene while the player hasn't completed it, skip option
                        else
                            continue;
                    }

                        

                }

                if(option.locationHist == 6 && gameController.actions == 2)
                {
                    var _dialogueChoice = Instantiate(buttonPrefab, dialogueContainer);

                    _dialogueChoice.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "<i>Goes home to rest for the day</i>";
                    _dialogueChoice.GetComponent<Button>().onClick.AddListener(() => {
                        DisplayDialogueChoice(option.optionText); next = option.nextId;
                        Debug.Log("Location filter actions completed");
                        globals.DayReset();
                    });
                    continue;
                }

                if (option.locationHist == 7 && gameController.actions == 2)
                    continue;

                if(option.locationHist == 7 && option.nextId == -1)
                {
                    var _dialogueChoice = Instantiate(buttonPrefab, dialogueContainer);

                    _dialogueChoice.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "<i>Move to the Hunting Board</i>";
                    _dialogueChoice.GetComponent<Button>().onClick.AddListener(() => {
                        globals.ResetHB();
                        globals.ManageHB();
                        this.gameObject.SetActive(false);
                        HB_canvas.SetActive(true);
                        Debug.Log("HB filter");
                      });
                    continue;
                }

                var dialogueChoice = Instantiate(buttonPrefab, dialogueContainer);

                dialogueChoice.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = option.optionText;
                dialogueChoice.GetComponent<Button>().onClick.AddListener(() => { DisplayDialogueChoice(option.optionText); next = option.nextId; Debug.Log("Default option made, No Filter"); });
            }
        }
    }

    private void DisplayDialogueChoice(string speech)
    {
        Transform button = EventSystem.current.currentSelectedGameObject.transform;

        print = true;
        nameBox.text = playerName;
        StartCoroutine(Speak(speech)); // Print the dialogue option that was chosen as regular text

        // Delete all of the buttons (including this one) generated by ShowDialogueOptions()
        foreach (Transform child in button.parent)
        {
            if (child.gameObject.name == "Dialogue") // Also reactivate the dialogue box
                child.gameObject.SetActive(true);
            else
                GameObject.Destroy(child.gameObject); // Destroy the button
        }
    }


    //Parses a specifically formatted text file and initializes an array of Dialogue nodes that are traversed during the course of dialogue
    private void Parse()
    {
        string[] nodeList = textFile.text.Split(new string[] { Environment.NewLine + Environment.NewLine }, StringSplitOptions.None);

        nodes = new DialogueNode[nodeList.Length];

        for(int i = 0; i < nodeList.Length; i++)
        {
            string[] fields = nodeList[i].Split(new string[] { Environment.NewLine + ":-" + Environment.NewLine }, StringSplitOptions.None);

            nodes[i] = new DialogueNode();
 

            for(int j = 0; j < fields.Length; j++)
            {
                switch(j)
                {
                    case 0:
                        string[] idName = fields[j].Split(';');
                        //Debug.Log(idName[0]);
                        nodes[i].id = int.Parse(idName[0]);
                        nodes[i].speakerName = idName[1];
                        nodes[i].background = idName[2];
                        break;
                    case 1:
                        if (fields[j] != "EMPTY")
                        {
                            string[] mainText = fields[j].Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                            for (int k = 0; k < mainText.Length; k++)
                            {
                                nodes[i].mainText.Add(mainText[k]);
                            }
                        }
                        break;
                    case 2:
                        if (fields[j] != "EMPTY")
                        {
                            string[] optionChoices = fields[j].Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                            for (int k = 0; k < optionChoices.Length; k++)
                            {
                                string[] option = optionChoices[k].Split(';');
                                //id, text, prioWC, currentSkill, boss, location
                                nodes[i].options.Add(new Option(int.Parse(option[4]), option[5], int.Parse(option[0]), int.Parse(option[1]), int.Parse(option[2]), int.Parse(option[3])));
                            }
                        }
                        break;
                    case 3:
                        nodes[i].nextID = int.Parse(fields[j]);
                        break;
                    default:
                        Debug.Log("Error: Incorrect formatting in the text file");
                        break;
                }
            }
        }       
    }


    //Debug Purposes Only
    //Prints out all of the info passed when parsing a given text file
   private void PrintNodesData()
    {
        for(int i = 0; i < nodes.Length; i++)
        {
            Debug.Log("Node ID: " + nodes[i].id + ", Speaker Name: " + nodes[i].speakerName);
            for(int j = 0; j < nodes[i].mainText.Count; j++)
            {
                Debug.Log("MainText" + j + ": " + nodes[i].mainText[j]);
            }

            for (int j = 0; j < nodes[i].options.Count; j++)
            {
                Debug.Log("Option" + j + ": " + "Next ID: " + nodes[i].options[j].nextId + ", Text: " + nodes[i].options[j].optionText);
            }

            Debug.Log("nextID: " + nodes[i].nextID);
        }
    }


    
    private void ChangeSpeaker()
    {
        nameBox.text = nodes[cur].speakerName;
    }



    private IEnumerator Speak(string speech)
    {
        
        ClearDialogue();
        for (int i = 0; i < speech.Length; i++)
        {
            if (print)
            {
                //If an HTML tag is encountered print out the whole tag as a chunk of character instead of individually
                //NOTE: This will cause an infinite loop if the tag is not closed, so make sure you close your tags
                //      Also under the assumption that these characters not being used for other purposes
                if (speech[i] == '<')
                {
                    while (speech[i] != '>')
                    {
                        dialogueBox.text += speech[i];
                        i++;
                    }
                    dialogueBox.text += speech[i];
                }
                else
                    dialogueBox.text += speech[i];
                yield return new WaitForSeconds(textDelay);
            }
            else
            {
                dialogueBox.text = speech;
                break;
            }
        }
        print = false;
    }



    private void ClearDialogue()
    {
        dialogueBox.text = "";
    }


}
