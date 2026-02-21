using UnityEngine;
using System.Collections;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [SerializeField] private float gameDurationSeconds = 300f;
    [SerializeField] private float comboResetTime = 5f;

    private bool gameEnded;
    private int score;
    private int comboMultiplier;

    private float lastComboTime;
    private float remainingTime;
    private float elapsedTime;

    private Coroutine timerRoutine;
    private Coroutine comboResetRoutine;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        ResetGame();
    }

    private void ResetGame()
    {
        score = 0;
        comboMultiplier = 0;
        gameEnded = false;
        remainingTime = gameDurationSeconds;
        elapsedTime = 0f;

        GameUIManager.Instance.UpdateScore(score);
        GameUIManager.Instance.UpdateCombo(comboMultiplier);
        GameUIManager.Instance.ClearStars();
        GameUIManager.Instance.UpdateTimer(remainingTime);

        if (timerRoutine != null) StopCoroutine(timerRoutine);
        timerRoutine = StartCoroutine(GameTimer());
    }

    public void RegisterCombo()
    {
        if (gameEnded) return;

        comboMultiplier = Time.time - lastComboTime <= comboResetTime ? comboMultiplier + 1 : 1;
        lastComboTime = Time.time;
        score = Mathf.Clamp(score + 3, 0, 100);

        GameUIManager.Instance.UpdateScore(score);
        GameUIManager.Instance.UpdateCombo(comboMultiplier);

        if (comboResetRoutine != null) StopCoroutine(comboResetRoutine);
        comboResetRoutine = StartCoroutine(ResetComboAfterDelay());
    }

    private IEnumerator ResetComboAfterDelay()
    {
        yield return new WaitForSeconds(comboResetTime);
        comboMultiplier = 0;
        GameUIManager.Instance.UpdateCombo(comboMultiplier);
    }

    private IEnumerator GameTimer()
    {
        while (remainingTime > 0f)
        {
            remainingTime -= Time.deltaTime;
            elapsedTime += Time.deltaTime;
            GameUIManager.Instance.UpdateTimer(remainingTime);
            yield return null;
        }
        Lose();
    }

    public void Win()
    {
        if (gameEnded) return;
        gameEnded = true;
        StopAllCoroutines();
        GameUIManager.Instance.UpdateStars(elapsedTime);
        GameUIManager.Instance.ShowWin();
        Time.timeScale = 0f;
    }

    public void Lose()
    {
        if (gameEnded) return;
        gameEnded = true;
        StopAllCoroutines();
        GameUIManager.Instance.ShowLose();
        Time.timeScale = 0f;
    }
}
