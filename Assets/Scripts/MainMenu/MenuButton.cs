using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    public Button.ButtonClickedEvent OnClick;

    public void Trigger()
    {
        OnClick?.Invoke();
    }
}
