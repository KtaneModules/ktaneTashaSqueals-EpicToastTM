using UnityEngine;
using KModkit;
using System;
using System.Linq;
using System.Collections;
using Random = UnityEngine.Random;

public class tashaSquealsScript : MonoBehaviour {

    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMColorblindMode Colorblind;
    public KMSelectable[] btnSelectables;
    public Light[] Lights;
    public MeshRenderer[] btnRenderers;
    public GameObject[] colorblindTexts;

    private static int _moduleIdCounter = 1;
    private int _moduleId;
    private bool solved;
    private bool colorblindActive = false;
    private int[] btnColors = { 0, 1, 2, 3 };
    private string[] soundNames = { "High", "NotAsHigh", "NotAsHighAsNotAsHigh", "NotHigh" };
    private static readonly Color32[] materialColors = { new Color32(236, 0, 220, 255), new Color32(0, 182, 0, 255), new Color32(223, 226, 0, 255), new Color32(21, 21, 161, 255) };
    private static readonly string[] positionNames = { "top", "right", "bottom", "left" };
    private static readonly string[] colorNames = { "pink", "green", "yellow", "blue" };
    private int[] flashing = new int[5];
    private int[] answers = { 99, 99, 99, 99, 99 };
    private int[] currentColumn = new int[4]; // holds the missing color in the column, and the three colors in the column.
    private int stageNum = 0, flash = 0, pressed = 0;
    private bool anyBtnPressed = false;
    
    void Start () {
        float scalar = transform.lossyScale.x;
        for (var i = 0; i < Lights.Length; i++)
        {
            Lights[i].range *= scalar;
            Lights[i].enabled = false;
        }

        colorblindActive = Colorblind.ColorblindModeActive;
        _moduleId = _moduleIdCounter++;
        Module.OnActivate += Activate;

        GenerateModule();
    }

    void Activate()
    {
        for (int i = 0; i < 4; i++)
        {
            int j = i;
            btnSelectables[i].OnInteract += delegate ()
            {
                StopAllCoroutines();
                if (!solved)
                    ButtonPress(j);
                btnSelectables[j].AddInteractionPunch(2);
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, btnSelectables[j].transform);
                Audio.PlaySoundAtTransform(soundNames[j], btnSelectables[j].transform);
                for (int k = 0; k < 4; k++)
                {
                    Lights[k].enabled = false;
                }
                StartCoroutine(SingleFlash(j));
                return false;
            };
        }
        StartCoroutine(DoTheFlashyFlash());
    }
    
    void GenerateModule()
    {
        btnColors = btnColors.Shuffle().ToArray();
        soundNames = soundNames.Shuffle().ToArray();
        for (int i = 0; i < 4; i++)
        {
            DebugMsg("The " + positionNames[i] + " button is " + colorNames[btnColors[i]] + ".");
            btnRenderers[i].material.color = materialColors[btnColors[i]];
            Lights[i].color = materialColors[btnColors[i]];
            if (colorblindActive)
            {
                colorblindTexts[i].GetComponent<TextMesh>().text = colorNames[btnColors[i]];
            }
        }

        for (int i = 0; i < 5; i++)
        {
            flashing[i] = Random.Range(0, 4);
            DebugMsg("Stage #" + (i + 1) + " of the flashing sequence is " + colorNames[flashing[i]] + ".");
        }

        for (int i = 0; i < 5; i++)
        {
            // currentColumn[0] = not present, ...[1] = press green, ...[2] = press yellow, ...[3] = press blue

            if (((i == 1 || i == 4) && btnColors[0] != 0) || ((i != 1 && i != 4) && btnColors[0] == 0))
            {
                DebugMsg("Using Table 1...");
                if (Array.IndexOf(btnColors, flashing[i]) == 0)
                {
                    currentColumn[0] = 0;
                    currentColumn[1] = 2;
                    currentColumn[2] = 1;
                    currentColumn[3] = 3;
                }

                else if (Array.IndexOf(btnColors, flashing[i]) == 1)
                {
                    currentColumn[0] = 2;
                    currentColumn[1] = 3;
                    currentColumn[2] = 1;
                    currentColumn[3] = 0;
                }

                else if (Array.IndexOf(btnColors, flashing[i]) == 3)
                {
                    currentColumn[0] = 3;
                    currentColumn[1] = 0;
                    currentColumn[2] = 2;
                    currentColumn[3] = 1;
                }

                else
                    answers[i] = 0;
            }

            else if (((i == 3) && btnColors[3] != 0) || ((i != 3) && btnColors[3] == 0))
            {
                DebugMsg("Using Table 2...");
                if (Array.IndexOf(btnColors, flashing[i]) == 0)
                {
                    currentColumn[0] = 1;
                    currentColumn[1] = 2;
                    currentColumn[2] = 3;
                    currentColumn[3] = 0;
                }

                else if (Array.IndexOf(btnColors, flashing[i]) == 1)
                {
                    currentColumn[0] = 0;
                    currentColumn[1] = 3;
                    currentColumn[2] = 2;
                    currentColumn[3] = 1;
                }

                else if (Array.IndexOf(btnColors, flashing[i]) == 2)
                {
                    currentColumn[0] = 3;
                    currentColumn[1] = 1;
                    currentColumn[2] = 0;
                    currentColumn[3] = 2;
                }

                else
                    answers[i] = 0;
            }

            else if (((i == 1 || i == 2 || i == 4) && Bomb.GetBatteryCount() % 2 != 0) || (i != 1 && i != 2 && i != 4 && Bomb.GetBatteryCount() % 2 == 0))
            {
                DebugMsg("Using Table 3...");
                if (Array.IndexOf(btnColors, flashing[i]) == 0)
                {
                    currentColumn[0] = 2;
                    currentColumn[1] = 1;
                    currentColumn[2] = 3;
                    currentColumn[3] = 0;
                }

                else if (Array.IndexOf(btnColors, flashing[i]) == 2)
                {
                    currentColumn[0] = 1;
                    currentColumn[1] = 2;
                    currentColumn[2] = 0;
                    currentColumn[3] = 3;
                }

                else if (Array.IndexOf(btnColors, flashing[i]) == 3)
                {
                    currentColumn[0] = 3;
                    currentColumn[1] = 1;
                    currentColumn[2] = 0;
                    currentColumn[3] = 2;
                }

                else
                    answers[i] = 0;
            }

            else
            {
                DebugMsg("Using Table 4...");
                if (Array.IndexOf(btnColors, flashing[i]) == 1)
                {
                    currentColumn[0] = 3;
                    currentColumn[1] = 0;
                    currentColumn[2] = 2;
                    currentColumn[3] = 1;
                }

                else if (Array.IndexOf(btnColors, flashing[i]) == 2)
                {
                    currentColumn[0] = 0;
                    currentColumn[1] = 1;
                    currentColumn[2] = 3;
                    currentColumn[3] = 2;
                }

                else if (Array.IndexOf(btnColors, flashing[i]) == 3)
                {
                    currentColumn[0] = 2;
                    currentColumn[1] = 0;
                    currentColumn[2] = 1;
                    currentColumn[3] = 3;
                }

                else
                    answers[i] = 0;
            }

            for (int x = 0; x < 4; x++)
            {
                if (x == 0)
                {
                    DebugMsg("The color not present in this column is " + colorNames[currentColumn[x]]);
                }

                else
                    DebugMsg("Row #" + (x + 1) + " is " + colorNames[currentColumn[x]]);
            }
            if (answers[i] == 99)
                answers[i] = Array.IndexOf(currentColumn, flashing[i]);
            DebugMsg("The color you should press for Stage #" + (i + 1) + " is " + colorNames[answers[i]] + ".");
        }
    }

    void ButtonPress(int btnNum)
    {
        anyBtnPressed = true;

        DebugMsg("You pressed the " + colorNames[btnColors[btnNum]] + " button.");
        if (btnNum == Array.IndexOf(btnColors, answers[pressed]))
        {
            pressed++;

            if (pressed > stageNum)
            {
                DebugMsg("Moving on to next stage...");
                stageNum++;
                pressed = 0;
                flash = 0;

                if (stageNum == 5)
                {
                    Module.HandlePass();
                    solved = true;

                    for (int i = 0; i < 4; i++)
                    {
                        Lights[i].enabled = false;
                    }
                }

                else
                {
                    StartCoroutine(DoTheFlashyFlash());
                }
            }
        }

        else
        {
            DebugMsg("That was wrong.");
            Module.HandleStrike();
            flash = 0;
            pressed = 0;

            StartCoroutine(DoTheFlashyFlash());
        }
    }

    void DebugMsg(string msg)
    {
        Debug.LogFormat("[Tasha Squeals #{0}] {1}", _moduleId, msg);
    }

    IEnumerator DoTheFlashyFlash()
    {
        yield return new WaitForSeconds(1.5f);
        while (!solved)
        {
            if (flash <= stageNum)
            {
                Lights[Array.IndexOf(btnColors, flashing[flash])].enabled = true;
                
                if (anyBtnPressed)
                {
                    Audio.PlaySoundAtTransform(soundNames[Array.IndexOf(btnColors, flashing[flash])], btnSelectables[Array.IndexOf(btnColors, flashing[flash])].transform);
                }

                yield return new WaitForSeconds(1f);

                Lights[Array.IndexOf(btnColors, flashing[flash])].enabled = false;
                yield return new WaitForSeconds(.5f);

                flash++;
            }
            
            else
            {
                yield return new WaitForSeconds(2f);
                flash = 0;
            }
        }
    }

    IEnumerator SingleFlash(int btnNum)
    {
        Lights[btnNum].enabled = true;

        yield return new WaitForSeconds(.75f);

        Lights[btnNum].enabled = false;
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = "Use !{0} press pink blue green yellow to press buttons. You can also use the first letters, or positions. Use !{0} colorblind to toggle colorblind mode.";
    #pragma warning disable 414
    IEnumerator ProcessTwitchCommand(string cmd)
    {
        string[] acceptableWords = { "top", "right", "bottom", "left", "pink", "green", "yellow", "blue", "p", "g", "y", "b" };

        if (cmd.ToLowerInvariant().StartsWith("press "))
        {
            string btns = cmd.Substring(6).ToLowerInvariant();
            string[] btnSequence = btns.Split(' ');

            yield return null;
            if (btnSequence.Length > 5)
            {
                yield return "sendtochaterror That's more than 5 buttons :/";
                yield break;
            }

            for (int i = 0; i < btnSequence.Length; i++)
            {
                if (!acceptableWords.Contains(btnSequence[i]))
                {
                    yield return "sendtochaterror One of those buttons isn't a valid button...";
                    yield break;
                }
            }

            for (int i = 0; i < btnSequence.Length; i++)
            {
                if (btnSequence[i].Equals("top"))
                {
                    btnSelectables[0].OnInteract();
                }

                else if (btnSequence[i].Equals("right"))
                {
                    btnSelectables[1].OnInteract();
                }

                else if (btnSequence[i].Equals("bottom"))
                {
                    btnSelectables[2].OnInteract();
                }

                else if (btnSequence[i].Equals("left"))
                {
                    btnSelectables[3].OnInteract();
                }

                else if (btnSequence[i].Equals("pink") || btnSequence[i].Equals("p"))
                {
                    btnSelectables[Array.IndexOf(btnColors, 0)].OnInteract();
                }

                else if (btnSequence[i].Equals("green") || btnSequence[i].Equals("g"))
                {
                    btnSelectables[Array.IndexOf(btnColors, 1)].OnInteract();
                }
                
                else if (btnSequence[i].Equals("yellow") || btnSequence[i].Equals("y"))
                {
                    btnSelectables[Array.IndexOf(btnColors, 2)].OnInteract();
                }
                
                else if (btnSequence[i].Equals("blue") || btnSequence[i].Equals("b"))
                {
                    btnSelectables[Array.IndexOf(btnColors, 3)].OnInteract();
                }

                yield return new WaitForSeconds(0.1f);
            }
        }
        else if (cmd.EqualsIgnoreCase("colorblind"))
        {
            yield return null;
            if (colorblindActive)
            {
                colorblindActive = false;
                for (int i = 0; i < 4; i++)
                {
                    colorblindTexts[i].GetComponent<TextMesh>().text = "";
                }
            }
            else
            {
                colorblindActive = true;
                for (int i = 0; i < 4; i++)
                {
                    colorblindTexts[i].GetComponent<TextMesh>().text = colorNames[btnColors[i]];
                }
            }
        }
        else
        {
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        int start = stageNum;
        for (int i = start; i < 5; i++)
        {
            int start2 = pressed;
            for (int j = start2; j < i+1; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    if (colorNames[answers[j]] == colorNames[btnColors[k]])
                    {
                        btnSelectables[k].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                        break;
                    }
                }
            }
        }
    }
}
