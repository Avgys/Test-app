using System;
using System.Collections.Generic;
using System.Linq;
using Figure;
using SimUnity.Management;
using SimUnity.Models;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Management
{
    public class Collection : MonoBehaviour
    {
        private static Collection _instance;

        public static Collection Instance
        {
            get
            {
                if (_instance != null) return _instance;

                var prefab = Resources.Load<Collection>("Managers/Collection");
                _instance = Instantiate(prefab);
                _instance.Awake();

                return _instance;
            }

            private set => _instance = value;
        }

        public Dictionary<int, IModel> Models { get; private set; }
        [SerializeField] private GameObject[] ModelPrefabs;
        private GameFigure[] GameFigures;
        [SerializeField] private GameFigure[] GameFigurePrefabs;

        public Team[] Teams;

        [SerializeField] private FigureType[] GameFigureTypes;
        private GameManager _gameManager;

        // [SerializeField] private AvailableAction[] DefaultActions;

        // [SerializeField] public PopupWindow PopupWindowPrefab;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance ??= this;
            DontDestroyOnLoad(this);

            _gameManager = GameManager.Instance;

            LoadDefault();
        }

        private void LoadDefault()
        {
            LoadDefaultModels();
            LoadDefaultGameFigures();
        }

        public void LoadDefaultModels()
        {
            Models = new Dictionary<int, IModel>();

            var prefabDict = ModelPrefabs
                .ToDictionary(x => x.GetInstanceID(), x => x.GetComponent<IModel>());

            foreach (var item in prefabDict)
            {
                Models.TryAdd(item.Key, item.Value);
            }
        }

        public void LoadDefaultGameFigures()
        {
            GameFigures = new GameFigure[GameFigureTypes.Length];

            for (int i = 0; i < GameFigureTypes.Length; i++)
            {
                //Make more models
                GameFigures[i] = CreateGameFigure(GameFigureTypes[i], Models.First().Value);
            }
        }

        private GameFigure CreateGameFigure(FigureType gameFigureType, IModel model)
        {
            var prefab = GameFigurePrefabs.First();
            var newObj = Instantiate(prefab);

            newObj.Mesh.mesh = model.Mesh.sharedMesh;

            newObj.Renderer.material = new Material(model.Renderer.sharedMaterial);

            var figure = newObj.GetComponent<GameFigure>();
            figure.transform.localScale = model.Transform.localScale;
            figure.Type = gameFigureType;
            figure.Color = gameFigureType.Color;

            figure.gameObject.SetActive(false);
            return figure;
        }
        
        private GameFigure CreateClearGameFigure(FigureType gameFigureType, IModel model)
        {
            var newObj =  new GameObject("Figure", typeof(GameFigure), typeof(Interactable));

            var meshFilter = newObj.AddComponent<MeshFilter>();
            meshFilter.mesh = model.Mesh.sharedMesh;

            var renderer = newObj.AddComponent<MeshRenderer>();
            renderer.material = new Material(model.Renderer.sharedMaterial);

            newObj.AddComponent<BoxCollider>();

            var figure = newObj.GetComponent<GameFigure>();
            figure.transform.localScale = model.Transform.localScale;
            figure.Type = gameFigureType;
            figure.Color = gameFigureType.Color;

            // var interactable = newObj.GetComponent<Interactable>();
            // interactable.GameManager = _gameManager;

            figure.gameObject.SetActive(false);
            return figure;
        }

        // private void LoadModels()
        // {
        //     var modelPrefabs = Resources.LoadAll<GameObject>("ObjectModels"!);
        //
        //     var models = modelPrefabs
        //         .Select(x => x.GetComponent<GameFigure>() ?? x.AddComponent<GameFigure>())
        //         .ToList();
        //
        //     var idsJson = PlayerPrefs.GetString("ModelIds");
        //
        //     var ids = JsonConvert.DeserializeObject<(string name, int id)[]>(idsJson);
        //
        //     var dict = models.ToDictionary(x =>
        //     {
        //         var id = ids?.FirstOrDefault(y => y.name == x.name).id ?? x.GetInstanceID();
        //         id = id == default ? x.GetInstanceID() : id;
        //         x.Id = id;
        //         return id;
        //     }, x => (IModel)x);
        //
        //     LoadModels(dict);
        // }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        public GameFigure GetRandomFigure()
        {
            var prototype = GameFigures[Random.Range(0, GameFigures.Length)].Clone();
            prototype.gameObject.SetActive(true);
            return prototype;
        }
    }
}