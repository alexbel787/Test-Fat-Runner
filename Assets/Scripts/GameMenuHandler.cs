using UnityEngine;
using UnityEngine.UI;

public class GameMenuHandler : MonoBehaviour
{
    private GameManagerScript GMS;
    public GameObject gameOverObj;
    public GameObject nextLevelObj;
    public Text resultText;

    private void Start()
    {
        GMS = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
        gameOverObj = GameObject.Find("Canvas/GameOver");
        gameOverObj.SetActive(false);
        gameOverObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, gameOverObj.GetComponent<RectTransform>().anchoredPosition.y);
        nextLevelObj = GameObject.Find("Canvas/NextLevel");
        resultText = GameObject.Find("Canvas/NextLevel/Text").GetComponent<Text>();
        nextLevelObj.SetActive(false);
        nextLevelObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, nextLevelObj.GetComponent<RectTransform>().anchoredPosition.y);
    }

    public void NextLevelBotton()
    {
        GMS.NextLevel();
    }

    public void GameOverBotton()
    {
        GMS.overallResult = 0;
        GMS.level = 1;
        GMS.centerReached = false;
        GMS.NextLevel();
    }

}
