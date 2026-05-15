using System.Collections;
using UnityEngine;


public class TutorialCinematic : MonoBehaviour
{
    [Header("Progress")]
    public int statuesRemaining = 2;

    [Header("Visual Elements")]
    public GameObject fogCube;
    public CanvasGroup titleGroup;

    [Header("Timings (Seconds)")]
    public float fogFadeSpeed = 0.5f;
    public float titleFadeSpeed = 1f;
    public float titleDisplayTime = 4f;

    private void Start()
    {
        if (titleGroup != null)
        {
            titleGroup.alpha = 0f;
        }
    }

    public void RegisterStatueDeath()
    {
        statuesRemaining--;

        if (statuesRemaining <= 0)
        {
            if (fogCube != null)
            {
                Collider fogCollider = fogCube.GetComponent<Collider>();
                if (fogCollider != null)
                {
                    fogCollider.enabled = false;
                }
            }

            StartCoroutine(RevealSequence());
        }
    }

    private IEnumerator RevealSequence()
    {
        if (fogCube != null)
        {
            Renderer fogRenderer = fogCube.GetComponent<Renderer>();
            if (fogRenderer != null)
            {
                Material fogMat = fogRenderer.material;
                Color fogColor = fogMat.color;

                while (fogColor.a > 0f)
                {
                    fogColor.a -= Time.deltaTime * fogFadeSpeed;
                    fogMat.color = fogColor;
                    yield return null;
                }
            }
            fogCube.SetActive(false);
        }

        if (titleGroup != null)
        {
            while (titleGroup.alpha < 1f)
            {
                titleGroup.alpha += Time.deltaTime * titleFadeSpeed;
                yield return null;
            }

            yield return new WaitForSeconds(titleDisplayTime);

            while (titleGroup.alpha > 0f)
            {
                titleGroup.alpha -= Time.deltaTime * titleFadeSpeed;
                yield return null;
            }
        }
    }
}