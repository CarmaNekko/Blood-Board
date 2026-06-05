using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public GameObject movementPanel;
    public CanvasGroup movementCanvasGroup;
    public float fadeSpeed = 1.5f;

    public TextMeshProUGUI textW;
    public TextMeshProUGUI textS;
    public TextMeshProUGUI textA;
    public TextMeshProUGUI textD;
    public TextMeshProUGUI textShift;
    public TextMeshProUGUI textSpace;

    public GameObject combatPanel;
    public CanvasGroup combatCanvasGroup;
    public TextMeshProUGUI textLeftClick;
    public TextMeshProUGUI textRightClick;

    private bool completedW = false;
    private bool completedS = false;
    private bool completedA = false;
    private bool completedD = false;
    private bool completedShift = false;
    private bool completedSpace = false;
    private bool movementCompleted = false;

    private bool combatActive = false;
    private bool completedLeftClick = false;
    private bool completedRightClick = false;
    private bool combatCompleted = false;

    void Start()
    {
        UpdateText(textW, "Presionar \"W\" para avanzar. (0/1)", Color.yellow);
        UpdateText(textS, "Presiona \"S\" para retroceder. (0/1)", Color.yellow);
        UpdateText(textA, "Presiona \"A\" para moverte a la izquierda. (0/1)", Color.yellow);
        UpdateText(textD, "Presiona \"D\" para moverte a la derecha. (0/1)", Color.yellow);
        UpdateText(textShift, "Presiona \"Shift\" para correr. (0/1)", Color.yellow);
        UpdateText(textSpace, "Presiona \"Space\" para saltar. (0/1)", Color.yellow);

        UpdateText(textLeftClick, "Presionar \"Click Izquierdo\" para disparar magia blanca. (0/1)", Color.yellow);
        UpdateText(textRightClick, "Presionar \"Click Derecho\" para disparar magia negra. (0/1)", Color.yellow);

        if (combatPanel != null)
        {
            combatPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (!movementCompleted)
        {
            if (!completedW && Input.GetKeyDown(KeyCode.W))
            {
                completedW = true;
                UpdateText(textW, "Presionar \"W\" para avanzar. (1/1)", Color.green);
            }

            if (!completedS && Input.GetKeyDown(KeyCode.S))
            {
                completedS = true;
                UpdateText(textS, "Presiona \"S\" para retroceder. (1/1)", Color.green);
            }

            if (!completedA && Input.GetKeyDown(KeyCode.A))
            {
                completedA = true;
                UpdateText(textA, "Presiona \"A\" para moverte a la izquierda. (1/1)", Color.green);
            }

            if (!completedD && Input.GetKeyDown(KeyCode.D))
            {
                completedD = true;
                UpdateText(textD, "Presiona \"D\" para moverte a la derecha. (1/1)", Color.green);
            }

            if (!completedShift && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)))
            {
                completedShift = true;
                UpdateText(textShift, "Presiona \"Shift\" para correr. (1/1)", Color.green);
            }

            if (!completedSpace && Input.GetKeyDown(KeyCode.Space))
            {
                completedSpace = true;
                UpdateText(textSpace, "Presiona \"Space\" para saltar. (1/1)", Color.green);
            }

            if (completedW && completedS && completedA && completedD && completedShift && completedSpace)
            {
                movementCompleted = true;
                StartCoroutine(FadeMovementAndStartCombat());
            }
        }
        else if (combatActive && !combatCompleted)
        {
            if (!completedLeftClick && Input.GetMouseButtonDown(0))
            {
                completedLeftClick = true;
                UpdateText(textLeftClick, "Presionar \"Click Izquierdo\" para disparar magia blanca. (1/1)", Color.green);
            }

            if (!completedRightClick && Input.GetMouseButtonDown(1))
            {
                completedRightClick = true;
                UpdateText(textRightClick, "Presionar \"Click Derecho\" para disparar magia negra. (1/1)", Color.green);
            }

            if (completedLeftClick && completedRightClick)
            {
                combatCompleted = true;
                StartCoroutine(FadeCombatAndDisable());
            }
        }
    }

    void UpdateText(TextMeshProUGUI textComponent, string content, Color color)
    {
        textComponent.text = content;
        textComponent.color = color;
    }

    IEnumerator FadeMovementAndStartCombat()
    {
        yield return new WaitForSeconds(1f);

        while (movementCanvasGroup.alpha > 0)
        {
            movementCanvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        movementPanel.SetActive(false);

        yield return new WaitForSeconds(2f);

        combatPanel.SetActive(true);
        combatCanvasGroup.alpha = 1f;
        combatActive = true;
    }

    IEnumerator FadeCombatAndDisable()
    {
        yield return new WaitForSeconds(1f);

        while (combatCanvasGroup.alpha > 0)
        {
            combatCanvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        combatPanel.SetActive(false);
    }
}