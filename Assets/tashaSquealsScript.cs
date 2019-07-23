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
    public KMSelectable[] btnSelectables;
    public Light[] Lights;
    public MeshRenderer[] btnRenderers;

    private static int _moduleIdCounter = 1;
    private int _moduleId;
    private bool solved;
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
                if (!solved)
                    ButtonPress(j);
                btnSelectables[j].AddInteractionPunch(2);
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
                Audio.PlaySoundAtTransform(soundNames[j], Module.transform);
                return false;
            };
        }
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

        StartCoroutine(DoTheFlashyFlash());
    }

    void ButtonPress(int btnNum)
    {
        anyBtnPressed = true;

        StopAllCoroutines();

        for (int i = 0; i < 4; i++)
        {
            Lights[i].enabled = false;
        }

        StartCoroutine(SingleFlash(btnNum));

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
                    Audio.PlaySoundAtTransform(soundNames[Array.IndexOf(btnColors, flashing[flash])], Module.transform);
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

    public string TwitchHelpMessage = "Use !{0} press pink blue green yellow to press buttons. You can also use the first letters, or positions.";
    IEnumerator ProcessTwitchCommand(string cmd)
    {
        string[] acceptableWords = { "top", "right", "bottom", "left", "pink", "green", "yellow", "blue", "p", "g", "y", "b" };

        if (cmd.ToLowerInvariant().StartsWith("press "))
        {
            string btns = cmd.Substring(6).ToLowerInvariant();
            string[] btnSequence = btns.Split(' ');

            if (btnSequence.Length > 5)
            {
                yield return "sendtochaterror That's more than 5 buttons :/";
                yield break;
            }

            foreach (var btn in btnSequence)
            {
                if (!acceptableWords.Contains(btn))
                {
                    yield return "sendtochaterror One of those buttons isn't a valid button...";
                    yield break;
                }
            }

            foreach (var btn in btnSequence)
            {
                yield return null;

                if (btn == "top")
                {
                    yield return new KMSelectable[] { btnSelectables[0] };
                }

                else if (btn == "right")
                {
                    yield return new KMSelectable[] { btnSelectables[1] };
                }

                else if (btn == "bottom")
                {
                    yield return new KMSelectable[] { btnSelectables[2] };
                }

                else if (btn == "right")
                {
                    yield return new KMSelectable[] { btnSelectables[3] };
                }

                else if (btn == "pink" || btn == "p")
                {
                    yield return new KMSelectable[] { btnSelectables[Array.IndexOf(btnColors, 0)] };
                }

                else if (btn == "green" || btn == "g")
                {
                    yield return new KMSelectable[] { btnSelectables[Array.IndexOf(btnColors, 1)] };
                }
                
                else if (btn == "yellow" || btn == "y")
                {
                    yield return new KMSelectable[] { btnSelectables[Array.IndexOf(btnColors, 2)] };
                }
                
                else if (btn == "blue" || btn == "b")
                {
                    yield return new KMSelectable[] { btnSelectables[Array.IndexOf(btnColors, 3)] };
                }
            }
        }

        else
        {
            yield break;
        }
    }
}
