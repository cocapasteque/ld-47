using System;
using System.Collections;
using System.Linq;
using Doozy.Engine.UI;
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
        private bool _nameReady = false;
        private string[] _buttonTexts = new[] {"Start", "Exit", "Fun mode\noff"};
        public Button.ButtonClickedEvent[] buttonActions;
        public RawImage fadeImage;

        public UIView keyboardView;
        public TMP_InputField input;

        private bool funMode;

        private void Awake()
        {
            funMode = PlayerPrefs.HasKey("FunMode") ? Convert.ToBoolean(PlayerPrefs.GetInt("FunMode")) : false;
            _spawner = GetComponent<CarSpawner>();
            _spawner.CarSpawned += () =>
            {
                StartCoroutine(OnCarSpawned());
                if (string.IsNullOrEmpty(PlayerPrefs.GetString("Playername")))
                {
                    _nameReady = false;
                    keyboardView.Show();
                }
                else
                {
                    _nameReady = true;
                }
            };
        }

        public void ResetData()
        {
            PlayerPrefs.SetString("Playername", string.Empty);
            SceneManager.LoadScene("Main Menu");
        }

        public void SaveName()
        {
            if (!string.IsNullOrWhiteSpace(input.text))
            {
                PlayerPrefs.SetString("Playername", input.text);
                keyboardView.Hide();
                _nameReady = true;
            }
        }

        public IEnumerator OnCarSpawned()
        {
            StartCoroutine(Fade(false));
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
            if (_buttons != null)
            {
                foreach (var button in _buttons)
                {
                    button.transform.LookAt(buttonTarget);
                }
            }

            if (!_ready || !_nameReady) return;

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
            _buttons[2].GetComponentInChildren<TMP_Text>().text = "Fun Mode\n" + (funMode ? "On" : "Off");
        }

        public Transform GetCar()
        {
            var car = _spawner.CarParent.GetChild(Random.Range(0, _spawner.CarParent.childCount));
            if (Vector3.Distance(car.position, Camera.main.transform.position) > 20)
            {
                return GetCar();
            }

            return car;
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
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void ToggleFunMode()
        {
            funMode = !funMode;
            PlayerPrefs.SetInt("FunMode", funMode ? 1 : 0);
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