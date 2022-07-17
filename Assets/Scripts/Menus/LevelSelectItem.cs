using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectItem : MonoBehaviour
{
    public int buildIndex;
    public Sprite screenshot;

    [Space]

    public Image image;
    public TextMeshProUGUI text;

    private void Awake() {
        image.sprite = screenshot;
        text.SetText(buildIndex.ToString());
    }

    public void LoadLevel() {
        LevelManager.LoadLevel(buildIndex);
    }
}
