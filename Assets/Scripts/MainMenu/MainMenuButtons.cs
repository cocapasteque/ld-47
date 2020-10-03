using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MainMenu
{
    public class MainMenuButtons : MonoBehaviour
    {
        private CarSpawner _spawner;
        public Transform[] _cars;
        public GameObject buttonPrefab;
        public Transform buttonTarget;
        private GameObject[] _buttons;
        private bool _ready;
        private string[] _buttonTexts = new[] {"Start", "Credits", "Exit"};

        private void Start()
        {
            _spawner = GetComponent<CarSpawner>();
            _spawner.CarSpawned += OnCarSpawned;
        }

        public void OnCarSpawned()
        {
            Debug.Log("Car spawned");
            _cars = new Transform[3];

            for (var i = 0; i < 3; i++)
            {
                Transform car = null;
                while (car == null || _cars.Contains(car))
                {
                    car = GetCar();
                }

                _cars[i] = car;
            }

            _buttons = new GameObject[3];

            for (var i = 0; i < 3; i++)
            {
                var button = Instantiate(buttonPrefab, _cars[i]);
                button.transform.localPosition = Vector3.up * 2;
                var text = button.GetComponentInChildren<Text>();
                text.text = _buttonTexts[i];
                button.GetComponentInChildren<Canvas>().worldCamera = Camera.main;
                _buttons[i] = button;
            }

            _ready = true;
        }

        private void Update()
        {
            if (!_ready) return;
            foreach (var button in _buttons)
            {
                button.transform.LookAt(buttonTarget);
            }
        }

        public Transform GetCar()
        {
            return _spawner.CarParent.GetChild(Random.Range(0, _spawner.CarParent.childCount));
        }
    }
}