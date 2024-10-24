using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class OtherStatusPanel : MonoBehaviour
{
    [SerializeField] Image classIcon;
    [SerializeField] TMP_Text className;
    [SerializeField] Image hpForeground;
    public TestBuffIndicator buffIndicator;
    public PlayerControllerNetworked player;

    [SerializeField] List<Image> lifeForeground;
    [SerializeField] Sprite heart;
    [SerializeField] Sprite brokenHeart;
    [SerializeField] Sprite darkHeart;

    public void Start()
    {
        float[] yPoses = {60f, -60f};
        OtherStatusPanel[] panels = FindObjectsOfType<OtherStatusPanel>();
        
        for (int i = 0; i < panels.Length; i++)
        {
            Transform rt = panels[i].GetComponent<Transform>();
            Vector3 pos = new (rt.localPosition.x, yPoses[i], rt.localPosition.z);
            rt.localPosition = pos;
        }
    }

    public void SetClass(int classTypeInt)
    {
        CharacterClassEnum classType = (CharacterClassEnum)classTypeInt;
        classIcon.sprite = ClassIcons.GetIcon(classType);
    }

    public void SetName(string name)
    {
        className.text = name;
    }

    public void SetHP(float hp, float maxHP)
    {
        hpForeground.fillAmount = hp / maxHP;
    }

    public void SetLife(int life)
    {
        for (int i = 0; i < lifeForeground.Count; i++)
        {
            if (life == 0)
            {
                lifeForeground[i].sprite = brokenHeart;
            }
            else if (life - 1 < i) 
            {
                lifeForeground[i].sprite = darkHeart;
            }
            else 
            {
                lifeForeground[i].sprite = heart;
            }
        }
    }
}
