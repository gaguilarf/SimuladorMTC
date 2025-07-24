using System.Collections;
using UnityEngine;
using TMPro;
using UnityEditor;

public class Dialogo1 : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel1;
    [SerializeField] private TMP_Text dialogueText1;
    [SerializeField, TextArea(4, 6)] private string[] dialogueLines;
    private bool isPlayerInRange;
    private bool didDialogueStart;
    private int lineIndex;
    private float typingTime=0.05f;
    void Start()
    {
        
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetButtonDown("Fire1"))
        {
            if (!didDialogueStart)
            {
                StartDialogue();
            }
            else if(dialogueText1.text == dialogueLines[lineIndex])
            {
                NextDialogueLine();
            }
            else
            {
                StopAllCoroutines();
                dialogueText1.text = dialogueLines[lineIndex];
            }
            
        }
        
    }

    private void StartDialogue()
    {
        didDialogueStart = true;
        dialoguePanel1.SetActive(true);
        lineIndex = 0;
        Time.timeScale = 0f;
        StartCoroutine(ShowLine());

    }

    private void NextDialogueLine()
    {
        lineIndex++;
        if (lineIndex < dialogueLines.Length)
        {
            StartCoroutine(ShowLine());
        }
        else
        {
            didDialogueStart= false;
            dialoguePanel1.SetActive(false);
            Time.timeScale = 1f;

        }
    }
    private IEnumerator ShowLine()
    {
        dialogueText1.text = string.Empty;
        foreach (char ch in dialogueLines[lineIndex])
        {
            dialogueText1.text += ch;
            yield return new WaitForSecondsRealtime(typingTime);
        }
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerInRange = true;
            
            
        }
       
    }
    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log("No se puede inicializar un dialogo");
        }

    }
}
