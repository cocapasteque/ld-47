﻿using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
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
        public Button.ButtonClickedEvent[] buttonActions;
        public RawImage fadeImage;

        private void Awake()
        {
            _spawner = GetComponent<CarSpawner>();
            _spawner.CarSpawned += () => { StartCoroutine(OnCarSpawned()); };
        }

        public IEnumerator OnCarSpawned()
        {
            yield return Fade(false);
            yield return null;
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
                var text = button.GetComponentInChildren<TMP_Text>();
                text.text = _buttonTexts[i];
                var menuButton = button.GetComponentInChildren<MenuButton>();
                menuButton.OnClick = buttonActions[i];

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

            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100))
                {
                    if (hit.collider.CompareTag("MenuButton"))
                    {
                        var button = hit.collider.GetComponent<MenuButton>();
                        button.Trigger();
                    }
                }
            }
        }

        public Transform GetCar()
        {
            return _spawner.CarParent.GetChild(Random.Range(0, _spawner.CarParent.childCount));
        }

        public void StartGame()
        {
            StartCoroutine(Work());

            IEnumerator Work()
            {
                yield return Fade(true);
                SceneManager.LoadScene("Test Scene");
            }
        }

        public void Credits()
        {
            Debug.Log("Credits");
        }

        public void Exit()
        {
            Debug.Log("Exit");
        }

        IEnumerator Fade(bool fadeIn = false)
        {
            var t = 0f;
            while (t < 1)
            {
                fadeImage.color = Color.Lerp(fadeIn ? Color.clear : Color.black, fadeIn ? Color.black : Color.clear, t);
                t += Time.deltaTime / 2;
                yield return null;
            }
        }
    }
}