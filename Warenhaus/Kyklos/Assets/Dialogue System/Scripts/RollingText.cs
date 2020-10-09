using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RollingText : MonoBehaviour
{
    public GameObject nextButton;
    [SerializeField] private TextMeshProUGUI textBox;
    [SerializeField] private float textDelay = 0.001f;


    [TextArea (10,10)]
    [SerializeField] string introSpeech;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitForSpeak());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator Speak(string speech)
    {
        textBox.text = "";
        for (int i = 0; i < speech.Length; i++)
        {
                if (speech[i] == '<')
                {
                    while (speech[i] != '>')
                    {
                        textBox.text += speech[i];
                        i++;
                    }
                    textBox.text += speech[i];
                }
                else
                    textBox.text += speech[i];
                yield return new WaitForSeconds(textDelay);
        }
    }

    private IEnumerator WaitForSpeak()
    {
        yield return Speak(introSpeech);
        nextButton.SetActive(true);
    }

}
