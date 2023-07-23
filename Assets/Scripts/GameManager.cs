using Figure;
using TMPro;
using UnityEngine;

namespace SimUnity.Management
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;

        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var gameObject = new GameObject("Game manager", typeof(GameManager));
                    _instance = gameObject.GetComponent<GameManager>();
                    _instance.Awake();
                }

                return _instance;
            }
            private set => _instance = value;
        }
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance ??= this;
            DontDestroyOnLoad(this);
        }
        public GameFigure SelectedFigure { get; set; }

        public TextMeshProUGUI WinnerText;

        private void Start()
        {
            // Instantiate(CollectionPrefab);

            // Operators = new[]
            // {
            //     SchemeManager.gameObject,
            //     SchemeEditor.gameObject
            // };
        }

        private void DeactivateAll()
        {
        }

        public void Win(Team figureTeam)
        {
            Debug.Log(figureTeam.TeamName +" won");
            WinnerText.text = figureTeam.TeamName + " won";
            GameField.Instance.Restart();
        }

        public void Quit()
        {
            Application.Quit(0);
        }
    }
}