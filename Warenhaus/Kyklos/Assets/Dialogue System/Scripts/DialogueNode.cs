using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Option
{
    public int nextId { get; private set; }
    public string optionText { get; private set; }

    public int highestPrioWC { get; private set; }

    public int currentSkill { get; private set; }

    public int bossAlive { get; private set; }

    public int locationHist { get; private set; }

    public Option(int id, string text, int prioWC, int skill, int boss, int location)
    {
        nextId = id;
        optionText = text;
        highestPrioWC = prioWC;
        currentSkill = skill;
        bossAlive = boss;
        locationHist = location;
    }
}

public class DialogueNode
{
    public int id { get; set; }
    public int nextID { get; set; }
    public string speakerName { get; set; }
    public string background { get; set;}
    public List<string> mainText { get; set; }
    public List<Option> options { get; set; }

    public DialogueNode()
    {
        id = 0;
        nextID = 0;
        speakerName = "";
        background = "";
        mainText = new List<string>();
        options = new List<Option>();
    }

}
