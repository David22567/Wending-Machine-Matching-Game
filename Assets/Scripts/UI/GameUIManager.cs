using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private string nextlevel;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Slider scoreSlider;
    [SerializeField] private int maxScore = 100;

    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image[] stars;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);

        ClearStars();
        UpdateCombo(0);
        UpdateScore(0);
    }

    public void UpdateScore(int score)
    {
        if (scoreText) scoreText.text = score.ToString();
        if (scoreSlider) scoreSlider.value = (float)score / maxScore;
    }

    public void UpdateCombo(int combo)
    {
        if (!comboText) return;
        comboText.text = combo >= 2 ? $"COMBO x{combo}" : "";
    }

    public void UpdateTimer(float remainingTime)
    {
        if (!timerText) return;
        int min = Mathf.FloorToInt(remainingTime / 60f);
        int sec = Mathf.FloorToInt(remainingTime % 60f);
        timerText.text = $"{min:00}:{sec:00}";
    }

    public void UpdateStars(float elapsedTime)
    {
        int starCount = elapsedTime <= 60f ? 3 : elapsedTime <= 180f ? 2 : 1;
        for (int i = 0; i < stars.Length; i++)
            stars[i].enabled = i < starCount;
    }

    public void ClearStars()
    {
        foreach (var star in stars) star.enabled = false;
    }

    public void ShowWin()
    {
        if (winPanel) winPanel.SetActive(true);
    }

    public void ShowLose()
    {
        if (losePanel) losePanel.SetActive(true);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextlevel);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
