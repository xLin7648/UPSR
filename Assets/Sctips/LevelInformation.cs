using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelInformation : MonoBehaviour
{
    public static float speed = 7.5f; // 0x0
    public float offset; // 0x18
    public float noteScale; // 0x1C
    public float scale; // 0x20
    public int numOfNotes; // 0x24
    public List<JudgeLine> judgeLineList; // 0x28
    public List<ChartNote> chartNoteSortByTime; // 0x30
    public GameObject judgeLineOfCutIn; // 0x38
    public Image backgroundBlack; // 0x40
    public List<GameObject> judgeLines; // 0x48
    public bool aPfCisOn; // 0x51
    public float[] floorPositions; // 0x58
    public static bool chartLoaded; // 0x60
    public bool levelBegan; // 0x61
    public bool levelOver; // 0x62
    public bool hitFxIsOn; // 0x63
    public float musicVol; // 0x64
    private bool _judgeLineColorSetOver; // 0x68

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
