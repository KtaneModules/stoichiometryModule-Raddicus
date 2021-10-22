
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;

public class stoichiometryModule : MonoBehaviour {
    
    public KMAudio Audio;
    public KMBombModule Module;
    public KMBombInfo Info;
    public KMColorblindMode Colorblind;
    public KMSelectable titrateButton, precipitateButton, baseToggle, rVent, rFilter, lVent, lFilter;
    public GameObject centralVial, leftLight, rightLight;
    public TextMesh baseDisplay, toggleDisplay, cbText, redOn, blueOn;
    public TextMesh[] dropDisplays = new TextMesh[2]; //0 is left, 1 is right
    public KMSelectable[] baseTravel, tenDropTravel, oneDropTravel;
    public Material red, blue, green, cyan, yellow, magenta, black, white, grey, lightOn, lightOff;
    public GameObject[] leftFlaps, rightFlaps;
    public float flapRotationAngle;
    
    //basetravel: 0 is down, 1 is up
    //drop travels: 0 is down, 1 is up

    private delegate bool thisCondition(); //this delegate does not require input since it can access the KMBombInfo object
    //List<thisCondition> conditionalArray = new List<thisCondition>();
    private int _moduleId = 0, _moduleIdCounter = 1;
    private int drops=0, baseOneIndex, baseTwoIndex, saltOneIndex, saltTwoIndex, timeDif,
        leftBaseDrops, rightBaseDrops;
    private bool _lightsOn = false, _isSolved = false, whichBase = false, currentDisplay = true,
        leftVent = false, rightVent = false, leftFilter = false, rightFilter = false, unicorn = false, 
        rightToxic, leftToxic, rightGas, leftGas, halfSolved = false, amogus = false;
    //currentDisplay = (0 for Bases, 1 for Salts)
    //private int[] leftRGB = new int[3], rightRBG = new int[3], centreRBG= new int[3];
    private Color lightRed = new Color(1, 0.796f, 0.796f, 1), lightBlue = new Color(0.796f, 0.796f, 1,1), darkRed = new Color(.25f, 0.199f, 0.199f, 1), darkBlue = new Color(0.199f, 0.199f, .25f, 1);
    private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ", numerals = "0123456789", solveFluff;
    private int[] acidIndexes = { 2,0,6,8,1,3,7,9,4,5};
    private string[] colorCodes = { "r","g","b","gb","rb","rg","k","rgb"};
    private Mixture[] acids = { new Mixture ("H2SO4", 98.072,"","Sulfuric Acid"),// acid, B
        new Mixture("HCl",36.458,"","Hydrochloric Acid"),//E
        new Mixture("HF",20.006,"","Hydroflouric Acid"),//A
        new Mixture("HBr",80.912,"","Hydrogen Bromide"),//F
        new Mixture("HC₂H₃O₂",60.052,"","Acetic Acid"),//I
        new Mixture("CF₃SO₃H",150.07,"","Triflic Acid"),//J
        new Mixture("HNO₃",63.012,"","Nitric Acid"),//C
        new Mixture("H₂CO₃",62.024,"","Carbonic Acid"),//G
        new Mixture("H₃PO₄",97.994,"","Phosphoric Acid"),//D
        new Mixture("HBF₄",87.81,"","Fluoroboric Acid")//H
    };
    private Mixture[] bases = { new Mixture("NaOH", 39.997,"r","Sodium Hydroxide"),
        new Mixture("NaHCO₃", 84.006,"g","Sodium Bicarbonate"),
        new Mixture("KOH", 56.105,"b","Potassium Hydroxide"),
        new Mixture ("NH₃",17.031,"gb","Ammonia"),
        new Mixture("LiOH",23.947,"rb","Lithium Hydroxide"),
        new Mixture("LiC₄H₉",64.056,"rg","ButyLithium"),
        new Mixture("NaH",23.998,"k","Sodium Hydride"), //k denotes that the mixture is entirely black, if the string is ever a size of 0 it should default to k
        new Mixture("Mg(OH)₂",58.319,"rgb","Magnesium Hydroxide")
    };
    

    
    #region Reactions Matrix
    private Reaction[,] reactions = new Reaction[,] {
        //Sodium Hydroxide, 1
        { new Reaction("Na₂SO₄",true,true,false,(double) 2/1),new Reaction("NaCl",true,true,false,1/1),new Reaction("NaF",true,true,true,1/1),new Reaction("NaBr",true,false,false,1/1),new Reaction("NaC₂H₃O₂",true,true,true,1/1),new Reaction("NaOTf",true,true,false,1/1),new Reaction("NaNO₃",true,false,false,1/1),new Reaction("Na₂CO₃",true,true,false,2/1),new Reaction("Na₃PO₄",true,true,true,3/1),new Reaction("NaBF₄",true,false,false,1/1)},
        //Sodium Bicarbonate, 2
        { new Reaction("Na₂SO₄",true,true,false,(double) 2/1),new Reaction("NaCl",true,true,true,1/1),new Reaction("NaF",true,false,false,1/1),new Reaction("NaBr",true,false,false,1/1),new Reaction("NaC₂H₃O₂",true,true,false,1/1),new Reaction("NaOTf",true,false,false,1/1),new Reaction("NaNO₃",true,true,false,1/1),new Reaction("Na₂CO₃",true,false,false,(double) 1/1),new Reaction("Na₃PO₄",true,true,true,3/1),new Reaction("NaBF₄",true,false,false,1/1)},
        //Potassium Hydroxide, 3
        { new Reaction("K₂SO₄",true,true,false,(double) 2/1),new Reaction("KCl",true,false,false,1/1),new Reaction("KF",true,true,false,1/1),new Reaction("KBr",true,false,false,1/1),new Reaction("KC₂H₃O₂",true,true,true,1/1),new Reaction("KOTf",true,true,true,1/1),new Reaction("KNO₃",true,true,false,1/1),new Reaction("K₂CO₃",true,false,false,2/1),new Reaction("K₃PO₄",true,true,false,3/1),new Reaction("KBF₄",true,true,true,1/1)},
        //Ammonia, 4
        { new Reaction("(NH₄)₂SO₄",true,true,true,(double) 2/1),new Reaction("NH₄Cl",true,true,true,1/1),new Reaction("NH₄F",true,false,false,1/1),new Reaction("NH₄Br",true,false,false,1/1),new Reaction("NH₄C₂H₃O₂",true,true,false,1/1),new Reaction("NH₄OTf",true,false,false,1/1),new Reaction("NH₄NO₃",true,true,false,1/1),new Reaction("(NH₄)₂CO₃",true,true,false,2/1),new Reaction("(NH₄)₃PO₄",true,true,true,3/1),new Reaction("NH₄BF₄",true,true,true,1/1)},
        //Lithium Hydroxide, 5
        { new Reaction("Li₂SO₄",true,false,false,(double) 2/1),new Reaction("LiCl",true,false,false,1/1),new Reaction("LiF",true,true,true,1/1),new Reaction("LiBr",true,true,true,1/1),new Reaction("LiC₂H₃O₂",true,true,true,1/1),new Reaction("LiOTf",true,true,true,1/1),new Reaction("LiNO₃",true,true,true,1/1),new Reaction("Li₂CO₃",true,false,false,2/1),new Reaction("Li₃PO₄",true,false,false,3/1),new Reaction("LiBF₄",true,false,false,1/1)},
        //ButyLithium, 6
        { new Reaction("Li₂SO₄",true,false,false,(double) 2/1),new Reaction("LiCl",true,true,true,1/1),new Reaction("LiF",true,true,true,1/1),new Reaction("LiBr",true,false,false,1/1),new Reaction("LiC₂H₃O₂",true,true,true,1/1),new Reaction("LiOTf",true,false,false,1/1),new Reaction("LiNO₃",true,false,false,1/1),new Reaction("Li₂CO₃",true,true,true,2/1),new Reaction("Li₃PO₄",true,false,false,3/1),new Reaction("LiBF₄",true,true,false,1/1)},
        //Sodium Amide, 7
        { new Reaction("Na₂SO₄",true,false,false,(double) 2/1),new Reaction("NaCl",true,true,true,1/1),new Reaction("NaF",true,false,false,1/1),new Reaction("NaBr",true,true,false,1/1),new Reaction("NaC₂H₃O₂",true,false,false,1/1),new Reaction("NaOTf",true,true,false,1/1),new Reaction("NaNO₃",true,true,false,1/1),new Reaction("Na₂CO₃",true,true,true,2/1),new Reaction("Na₃PO₄",true,true,true,3/1),new Reaction("NaBF₄",true,true,false,1/1)},
        //Magnesium Hydroxide, 8
        { new Reaction("MgSO₄",true,true,true,(double) 1/1),new Reaction("MgCl₂",true,true,true,(double) 0.5),new Reaction("MgF₂",true,false,false,(double) 0.5),new Reaction("MgBr₂",true,false,false,(double)0.5),new Reaction("Mg(C₂H₃O₂)₂",true,true,false,(double)0.5),new Reaction("Mg(OTf)₂",true,true,true,(double) 0.5),new Reaction("Mg(NO₃)₂",true,false,false,(double) 0.5),new Reaction("MgCO₃",true,true,false,1/1),new Reaction("Mg₃(PO₄)₂",true,true,true,(double) 1.5),new Reaction("Mg(BF₄)₂",true,true,false,(double)0.5)},
    };
    #endregion

    private Mixture leftBase, rightBase, leftAcid, rightAcid, nextAcid;
    private Reaction leftSalt, rightSalt;
    private List<Node> flowchart;
    private Node startingNode, thisNode, antiNode;
    private string dayOfWeek = DateTime.Today.DayOfWeek.ToString();
    private string[] daysOfTheWeek = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
    private double[] modifiers = { (double)1/2,(double)3/2,(double)7/5,(double)40/20, (double)9/4, 1, (double)13/16};
    
    private double startOne, startTwo;

    string submittingBase; 
    string submittingSalt;
    int correctBaseIndex;       //These variables are all for the TP autosolver
    int correctSaltIndex;
    int correctDrops;

    bool CBON;

    // Use this for initialization
    #region Start, Awake, and Activate
    void Start ()
    {
        _moduleId = _moduleIdCounter++;
        Module.OnActivate += Activate;
        if (Colorblind.ColorblindModeActive) CBON = true;
	}

    void Awake()
    {
        tenDropTravel[0].OnInteract += delegate ()
        {
            handleDrop(-10);
            return false;
        };
        tenDropTravel[1].OnInteract += delegate ()
        {
            handleDrop(10);
            return false;
        };
        oneDropTravel[0].OnInteract += delegate ()
        {
            handleDrop(-1);
            return false;
        };
        oneDropTravel[1].OnInteract += delegate ()
        {
            handleDrop(1);
            return false;
        };
        baseTravel[0].OnInteract += delegate ()
        {
            handleBaseTravel(-1);//subtract one from current index
            return false;
        };
        baseTravel[1].OnInteract += delegate ()
        {
            handleBaseTravel(1);//add one from current index
            return false;
        };
        precipitateButton.OnInteract += delegate () 
        {
            precipitateButton.AddInteractionPunch();
            handlePrecipitate();
            return false;
        };
        baseToggle.OnInteract += delegate ()
        {
            baseToggle.AddInteractionPunch();
            handleBaseToggle();
            return false;
        };
        titrateButton.OnInteract += delegate ()
        {
            titrateButton.AddInteractionPunch();
            handleTitrate();
            return false;
        };
        rVent.OnInteract += delegate ()
        {
            handleRightVent();
            return false;
        };
        lVent.OnInteract += delegate ()
        {
            handleLeftVent();
            return false;
        };
        rFilter.OnInteract += delegate ()
        {
            handleRightFilter();
            return false;
        };
        lFilter.OnInteract += delegate ()
        {
            handleLeftFilter();
            return false;
        };
    }

    void Activate()
    {
        Init();
        _lightsOn = true;
    }
    #endregion

    void Init() //generate module and solution (haha gotem)
    {
        //because the acids list doesn't have any color notations, we will use the base array to initialize. once all acids are colored, we can simply replace bases with acids

        #region Base and Color Init

        int[] centreColor = new int[3];

        for (int i = 0; i < centreColor.Length; i++)
        {
            centreColor[i] = Random.Range(0, 2);
        }
        //Debug.Log("Color Array is " + holder);
        Material centreFinal = stringColor(calcColor(centreColor));
        centralVial.GetComponent<Renderer>().material = centreFinal;
        string centralColor = calcColor(centreColor);
        SetCB();
        int centralDegree = colorDegree(centralColor);
        int firstBase, secondBase;

        redOn.color = lightRed;
        blueOn.color = darkBlue;
        #endregion

        #region Acid Determining

        constructGraph();
        //evaluate the bomb and determine the ending
        thisNode = startingNode;
        antiNode = startingNode;
        List<bool> path = new List<bool>();//a list of booleans that dictates the order of paths taken, either yes (1) or no (0)
        string thisPath = "", antiPath = "";
        while (thisNode.yesNode() != null) //first run, normal
        {
            path.Add(thisNode.nodeCondition());
            if (thisNode.nodeCondition())//if this node's condition returns true
            {
                Debug.LogFormat("[Stoichiometry #{0}] Normal Run: {1} is True.", _moduleId, thisNode.getLabel());
                thisNode = thisNode.yesNode();
                thisPath = thisPath + "Yes ";
            }
            else//if this node's condition returns false
            {
                Debug.LogFormat("[Stoichiometry #{0}] Normal Run: {1} is False.", _moduleId, thisNode.getLabel());
                thisNode = thisNode.noNode();
                thisPath = thisPath + "No ";
            }
        }

        while(antiNode.yesNode() != null)
        {
            path.Add(antiNode.nodeCondition());
            if (antiNode.nodeCondition())
            {
                Debug.LogFormat("[Stoichiometry #{0}] Reverse Run: {1} is True.", _moduleId, antiNode.getLabel());
                antiNode = antiNode.noNode();
                antiPath = antiPath + "No ";
            }
            else
            {
                Debug.LogFormat("[Stoichiometry #{0}] Reverse Run: {1} is False.", _moduleId, antiNode.getLabel());
                antiNode = antiNode.yesNode();
                antiPath = antiPath + "Yes ";
            }
        }

        leftAcid = thisNode.getMix();
        rightAcid = antiNode.getMix();
        thisPath = thisPath.Substring(0,thisPath.Length - 1); antiPath = antiPath.Substring(0, antiPath.Length - 1);
        Debug.LogFormat("[Stoichiometry #{0}] Paths Taken are {1}, {2}.", _moduleId, thisPath, antiPath);
        Debug.LogFormat("[Stoichiometry #{0}] Acids are {1} and {2}.", _moduleId, thisNode.getMix().getName(), antiNode.getMix().getName());
        #endregion

        unicorn = assessUnicorn();

        startOne = (modifiers[Array.IndexOf(daysOfTheWeek, dayOfWeek)] * 16);
        startTwo = (modifiers[Array.IndexOf(daysOfTheWeek, dayOfWeek)] * 20);
        //Debug.Log("Acid Quantities are: " + startOne);

        #region Base Determining

        bool labcoatIndicator = false;
        //Debug.Log("Offset1 Init= " + offset1);
        foreach (string ind in Info.GetIndicators())
        {
            
            if (ind.Any("GEALBCOT".Contains))
            {
                labcoatIndicator = true;
            }
        }

        if (centralDegree == 3)
        {
            firstBase = 7; secondBase = 6; //if there are no Lab Coat indicators
            if (labcoatIndicator) {//if there ARE Lab Coat indicators
                firstBase = 6; secondBase = 7;
            }
        }
        else if (centralDegree == 2)
        {
            List<int> secondaries = new List<int>();
            secondaries.Add(3);
            secondaries.Add(4);
            secondaries.Add(5);
            secondaries.Remove(Array.IndexOf(colorCodes, centralColor));//removes the color in the middle from the Secondaries List

            
            firstBase = secondaries.ElementAt(0);
            secondBase = secondaries.ElementAt(1);

            if (!labcoatIndicator) { firstBase = secondaries.ElementAt(1); secondBase = secondaries.ElementAt(0); }

        }
        else
        {
            List<int> primaries = new List<int>();
            primaries.Add(0);
            primaries.Add(1);
            primaries.Add(2);
            primaries.Remove(Array.IndexOf(colorCodes, centralColor));//removes the color in the middle from the Primaries List


            
            firstBase = primaries.ElementAt(0);
            secondBase = primaries.ElementAt(1);

            if (!labcoatIndicator) { firstBase = primaries.ElementAt(1); secondBase = primaries.ElementAt(0); }
        }

        string baseLogOne = bases[firstBase].getName();
        string baseLogTwo = bases[secondBase].getName();

        Debug.LogFormat("[Stoichiometry #{0}] Left Base Position {1} ({2}), Right Base Position {3} ({4}).", _moduleId, firstBase, baseLogOne, secondBase, baseLogTwo);

        leftBase = bases[firstBase];
        rightBase = bases[secondBase];

        #endregion

        double firstAcidMoles = startOne / leftAcid.getMass(), secondAcidMoles = startTwo / rightAcid.getMass();

        leftSalt = reactions[Array.IndexOf(bases, leftBase), Array.IndexOf(acids, leftAcid)];
        rightSalt = reactions[Array.IndexOf(bases, rightBase), Array.IndexOf(acids, rightAcid)];
        rightToxic = rightSalt.isToxic(); rightGas = rightSalt.containsGas();
        leftToxic = leftSalt.isToxic(); leftGas = leftSalt.containsGas();

        double leftBaseGrams = leftSalt.getRatio() * firstAcidMoles * leftBase.getMass(),
            rightBaseGrams = rightSalt.getRatio() * secondAcidMoles * rightBase.getMass();
        
        leftBaseDrops = (int) Math.Ceiling(leftBaseGrams); rightBaseDrops = (int)Math.Ceiling(rightBaseGrams);
        if (leftBaseDrops == 0) leftBaseDrops++; if (rightBaseDrops == 0) rightBaseDrops++;
        if (leftBaseDrops > 99) leftBaseDrops = 99; if (rightBaseDrops > 99) rightBaseDrops = 99;
        //Debug.LogFormat("{0} | {1}",leftBaseGrams,rightBaseGrams);

        string leftLogOne, leftLogTwo, rightLogOne, rightLogTwo;
        leftLogOne = (leftGas) ? "opened" : "closed"; leftLogTwo = (leftToxic) ? "On" : "Off";
        rightLogOne = (rightGas) ? "opened" : "closed"; rightLogTwo = (rightToxic) ? "On" : "Off";

        Debug.LogFormat("[Stoichiometry #{0}] {1} / {2} = {3} mols of {4}", _moduleId, startOne, leftAcid.getMass(), firstAcidMoles, leftAcid.getName());
        Debug.LogFormat("[Stoichiometry #{0}] {1} mols of {2} * molar ratio of {3} * molar mass of {4} ({5}) = {6} grams of {4}", _moduleId, firstAcidMoles, leftAcid.getName(),
            leftSalt.getRatio(),leftBase.getName(),leftBase.getMass(),leftBaseGrams);

        Debug.LogFormat("[Stoichiometry #{0}] {1} / {2} = {3} mols of {4}", _moduleId, startTwo, rightAcid.getMass(), secondAcidMoles,rightAcid.getName());
        Debug.LogFormat("[Stoichiometry #{0}] {1} mols of {2} * molar ratio of {3} * molar mass of {4} ({5}) = {6} grams of {4}", _moduleId, secondAcidMoles, rightAcid.getName(),
            rightSalt.getRatio(),rightBase.getName(),rightBase.getMass(),rightBaseGrams);

        Debug.LogFormat("[Stoichiometry #{0}] First Combo: {1} grams of {2}, prep for {3} with vents {4} and filter {5}.", _moduleId, leftBaseDrops, leftBase.getSymbol(), leftSalt.getSalt(), leftLogOne, leftLogTwo);
        Debug.LogFormat("[Stoichiometry #{0}] Second Combo: {1} grams of {2}, prep for {3} with vents {4} and filter {5}.", _moduleId, rightBaseDrops, rightBase.getSymbol(), rightSalt.getSalt(), rightLogOne, rightLogTwo);
        if (unicorn)
        {
            int uniLog = ((int)thisNode.getMix().getMass() >= 60) ? digitalRoot((int)thisNode.getMix().getMass()) : (int)thisNode.getMix().getMass();
            Debug.LogFormat("[Stoichiometry #{0}] ...except not really! You are dealing with AzidoAzideAzide! Just submit when the timer displays {1} seconds!", _moduleId, uniLog);
        }
        switch (dayOfWeek)
        {
            case "Monday":
                solveFluff = "See you tomorrow!";
                break;
            case "Tuesday":
                solveFluff = "Enjoy your Tacos!";
                break;
            case "Wednesday":
                solveFluff = "Keep on going, the week's halfway done!";
                break;
            case "Thursday":
                solveFluff = "The week is almost over, make sure to not blow up before it ends!";
                break;
            case "Friday":
                solveFluff = "Enjoy your weekend!";
                break;
            case "Saturday":
                solveFluff = "Don't cope with the chemicals TOO much, y'hear!";
                break;
            case "Sunday":
                solveFluff = "What a way to start the week, huh?";
                break;
            default:
                break;
        }

        //possible Souvenir answer generation
        int[] notDrops = new int[3];
        int f = Random.Range(1, 100); while (f == leftBaseDrops || f == rightBaseDrops) { f = Random.Range(1, 100); }
        int j = Random.Range(1, 100); while (j == leftBaseDrops || j == rightBaseDrops || j == f) { j = Random.Range(1, 100); }
        int k = Random.Range(1, 100); while (k == leftBaseDrops || k == rightBaseDrops || k == f || k == j) { j = Random.Range(1, 100); }
        notDrops[0] = f; notDrops[1] = f; notDrops[2] = k;
    }

    void handleDrop(int adder)
    {
        if (!_lightsOn | _isSolved) return;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        drops += adder;
        if (drops < 0) drops = 0;
        if (drops > 99) drops = 99;
        //update displays
        dropDisplays[0].text = "" + (int)(drops / 10);
        dropDisplays[1].text = "" + drops % 10;
    }
    void handleTitrate()
    {
        if (!_lightsOn | _isSolved) return;
        //checking for Unicorn
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress,Module.transform);
        titrateButton.AddInteractionPunch();
        int seconds = ((int)Info.GetTime()) % 60;

        if (unicorn)
        {
            int molMass = (int) thisNode.getMix().getMass();
            if (molMass >= 60) { molMass = digitalRoot(molMass); }
            if (seconds == molMass)
            {
                Module.HandlePass();
                Audio.PlaySoundAtTransform("solve", Module.transform);
                Debug.LogFormat("[Stoichiometry #{0}] AzidoAzideAzide neutralized! That was pretty easy! Solving Module.",_moduleId);
                Audio.PlaySoundAtTransform("solve", Module.transform);
                centralVial.GetComponent<Renderer>().material = grey;
                cbText.text = string.Empty;
                baseDisplay.color = Color.black;
                _isSolved = true;
            }
            else { Module.HandleStrike();
                Debug.LogFormat("[Stoichiometry #{0}] Did not submit on {1}. Issuing Strike.",_moduleId, molMass);
            }
            return;
        }

        Mixture currentBase; Reaction currentSalt;
        int varInd;//variable Index
        if (!whichBase) { varInd = baseOneIndex; currentSalt = reactions[baseOneIndex, saltOneIndex]; }
        else { varInd = baseTwoIndex; currentSalt = reactions[baseTwoIndex, saltTwoIndex]; }
        currentBase = bases[varInd];

        if (halfSolved)//if the first acid has already been neutralized
        {
            Reaction nextSalt; Mixture nextBase;
            int nextDrops;
            bool nextVent;
            string logFirst, logSecond;

            if (nextAcid == rightAcid)
            {
                nextSalt = rightSalt; nextBase = rightBase;
                nextDrops = rightBaseDrops;
                nextVent = (rightGas == rightVent && rightToxic == rightFilter);
                logFirst = (rightGas) ? "open" : "closed"; logSecond = (rightToxic) ? "On" : "Off";
            }
            else//acid is left acid
            {
                nextSalt = leftSalt; nextBase = leftBase;
                nextDrops = leftBaseDrops;
                nextVent = (leftGas == rightVent && leftToxic == rightFilter);
                logFirst = (leftGas) ? "open" : "closed"; logSecond = (leftToxic) ? "On" : "Off";
            }

            if (currentBase.isEqual(nextBase))
            {
                if (currentSalt.isEqual(nextSalt))
                {
                    if(drops == nextDrops)
                    {
                        if (nextVent)
                        {
                            Debug.LogFormat("[Stoichiometry #{0}] Second Acid Neutralized. The Module is solved. {1}",_moduleId,solveFluff);
                            Module.HandlePass();
                            cbText.text = string.Empty;
                            Audio.PlaySoundAtTransform("solve",Module.transform);
                            centralVial.GetComponent<Renderer>().material = grey;
                            baseDisplay.color = Color.black;
                            _isSolved = true;
                            assessAmogus();
                        }
                        else
                        {
                            Debug.LogFormat("[Stoichiometry #{0}] Second Base and Salt matches with the correct Base amount, but the Vent should be {1} and the Filter should be {2}. Issuing Strike.", _moduleId, logFirst, logSecond);
                            Module.HandleStrike();
                        }
                    }
                    else
                    {
                        Debug.LogFormat("[Stoichiometry #{0}] Second Base ({1}) and Salt ({2}) match, but incorrect Base amount ({3} =/= {4}). Issuing Strike.", _moduleId, currentBase.getName(), currentSalt.getSalt(), drops, nextDrops);
                        Module.HandleStrike();
                    }
                }
                else
                {
                    Debug.LogFormat("[Stoichiometry #{0}] Second Base ({1}) does not match it's Salt({2} =/= {3}). Issuing Strike.", _moduleId, nextBase.getName(), currentSalt.getSalt(), nextSalt.getSalt());
                    Module.HandleStrike();
                }
            }
            else
            {
                Debug.LogFormat("[Stoichiometry #{0}] Second Acid ({1}) does not match it's Base ({2} =/= {3}). Issuing Strike.", _moduleId, nextAcid.getName(), currentBase.getName(), nextBase.getName());
                Module.HandleStrike();
            }



        }
        else if (currentBase.isEqual(leftBase)) //current base is left base
        {
            nextAcid = rightAcid;
            if (currentSalt.isEqual(leftSalt)) {//salt is correct
                if (leftBaseDrops == drops) {//drop amount is correct
                    if(leftGas == leftVent && leftToxic == leftFilter)//gas and filter states match, aka Vent Correct
                    {
                        Debug.LogFormat("[Stoichiometry #{0}] First acid successfully Neutralized!",_moduleId);
                        handleBaseToggle();
                        halfSolved = true; //indicates the module is half-solved.
                        Audio.PlaySoundAtTransform("firstSound",Module.transform);
                    }
                    else
                    {
                        string logFirst = (leftGas) ? "open" : "closed";
                        string logSecond = (leftToxic) ? "On" : "Off";
                        Debug.LogFormat("[Stoichiometry #{0}] First Base and Salt matches with the correct Base amount, but the Vent should be {1} and the Filter should be {2}. Issuing Strike.", _moduleId, logFirst, logSecond);
                        Module.HandleStrike();
                    }
                }
                else
                {
                    Debug.LogFormat("[Stoichiometry #{0}] First Base ({1}) and Salt ({2}) match, but incorrect Base amount ({3} =/= {4}). Issuing Strike.", _moduleId,currentBase.getName(), currentSalt.getSalt(), drops,leftBaseDrops);
                    Module.HandleStrike();
                }

            }
            else {
                Debug.LogFormat("[Stoichiometry #{0}] First Base ({1}) does not match it's Salt({2} =/= {3}). Issuing Strike.",_moduleId,currentBase.getName(), currentSalt.getSalt(),leftSalt.getSalt());
                Module.HandleStrike();
            }
        }
        else if (currentBase.isEqual(rightBase))
        {
            nextAcid = leftAcid;
            if (currentSalt.isEqual(rightSalt))
            {//salt is correct
                if (rightBaseDrops == drops)
                {//drop amount is correct
                    if (rightGas == leftVent && rightToxic == leftFilter)//gas and filter states match, aka Vent Correct
                    {
                        Debug.LogFormat("[Stoichiometry #{0}] First Acid successfully Neutralized! ",_moduleId);
                        handleBaseToggle();
                        halfSolved = true; //indicates the module is half-solved.
                    }
                    else
                    {
                        string logFirst = (rightGas) ? "open" : "closed";
                        string logSecond = (rightToxic) ? "On" : "Off";
                        Debug.LogFormat("[Stoichiometry #{0}] First Base and Salt matches with the correct Base amount, but the Right Vent should be {1} and the Filter should be {2}. Issuing Strike.", _moduleId, logFirst, logSecond);
                        Module.HandleStrike();
                    }
                }
                else
                {
                    Debug.LogFormat("[Stoichiometry #{0}] First Base ({1}) and Salt ({2}) match, but incorrect Base amount ({3} =/= {4}). Issuing Strike.", _moduleId, currentBase.getName(), currentSalt.getSalt(), drops, rightBaseDrops);
                    Module.HandleStrike();
                }

            }
            else
            {
                Debug.LogFormat("[Stoichiometry #{0}] First Base ({1}) does not match it's Salt({2} =/= {3}). Issuing Strike.", _moduleId, currentBase.getName(), currentSalt.getSalt(), rightSalt.getSalt());
                Module.HandleStrike();
            }
        }
        else
        {
            Module.HandleStrike();
            Debug.LogFormat("[Stoichiometry #{0}] Submitted Base ({1}) was not relevant to either solution. Issuing Strike.",_moduleId, currentBase.getName());
        }
    }
    void SetCB()
    {
        string currentCol = centralVial.GetComponent<Renderer>().material.name.Split(' ').First(); //If I don't include the .Split and .First, the text ends up with "(Instance)" after it.
        if (currentCol == "lightOn" || currentCol == "lightOff")                                   //I am very mildly peeved that you don't use an int[] for the colors. Expect several angry work emails >:(                                
            return;
        if (CBON && currentCol != "black" && currentCol != "white" && currentCol != "grey") //We don't need colorblind support for these colors! They aren't even real! Ha!
            cbText.text = currentCol;
        else
            cbText.text = string.Empty;
    }

    bool assessUnicorn() { return (Info.GetBatteryHolderCount() == 1 && Info.GetBatteryCount() == 2 && Info.IsIndicatorPresent("FRK") && Info.IsIndicatorOn("FRK")); }
    bool assessAmogus() { return (Info.GetSolvableModuleNames().Contains("The Imposter") || Info.GetSolvableModuleNames().Contains("amogus") 
            || Info.GetSolvableModuleNames().Contains("Among Us"));}

    void handlePrecipitate()
    {
        if (_isSolved | !_lightsOn) return;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        
        if (!currentDisplay) {
            toggleDisplay.text = "BASES";
            if (!whichBase)
            {
                baseDisplay.text = bases[baseOneIndex].getSymbol();
                baseDisplay.color = lightRed;
            }
            else
            {
                baseDisplay.text = bases[baseTwoIndex].getSymbol();
                baseDisplay.color = lightBlue;
            }
        }
        else {
            toggleDisplay.text = "SALTS";
            if (!whichBase)
            {
                baseDisplay.text = reactions[baseOneIndex, saltOneIndex].getSalt();
                baseDisplay.color = lightRed;
            }
            else
            {
                baseDisplay.text = reactions[baseTwoIndex, saltTwoIndex].getSalt();
                baseDisplay.color = lightBlue;
            }
        }
        currentDisplay = !currentDisplay;
    }
    void handleBaseTravel(int i)
    {
        if (!_lightsOn || _isSolved) return;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        //whichBase represents either a 0 or 1. 0 is base one, 1 is base two. I understand, it's weird, but it works.
        if (currentDisplay)//travel to bases
        {
            if (!whichBase)
            {
                baseOneIndex = (baseOneIndex + i) % bases.Length;
                if (baseOneIndex < 0) baseOneIndex += bases.Length;
                baseDisplay.text = bases[baseOneIndex].getSymbol();
            }
            else
            {
                baseTwoIndex = (baseTwoIndex + i) % bases.Length;
                if (baseTwoIndex < 0) baseTwoIndex += bases.Length;
                baseDisplay.text = bases[baseTwoIndex].getSymbol();
            }
        }
        else//travel along reactions array at the column specified
        {
            if(!whichBase)//salts along base1's array
            {
                saltOneIndex = (saltOneIndex + i) % 10;
                if (saltOneIndex < 0) saltOneIndex += 10;
                baseDisplay.text = reactions[baseOneIndex,saltOneIndex].getSalt();
            }
            else//salts along base2's array
            {
                saltTwoIndex = (saltTwoIndex + i) % 10;
                if (saltTwoIndex < 0) saltTwoIndex += 10;
                baseDisplay.text = reactions[baseTwoIndex, saltTwoIndex].getSalt();
            }
        }
        //Debug.LogFormat("Indexes: [{0}][{1}]",baseOneIndex,baseTwoIndex);
    }
    void handleBaseToggle()
    {
        if (!_lightsOn || _isSolved || halfSolved) return;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
        if (currentDisplay)
        {
            whichBase = !whichBase;
            if (!whichBase)
            {
                baseDisplay.text = bases[baseOneIndex].getSymbol();
                baseDisplay.color = lightRed;
                redOn.color = lightRed;
                blueOn.color = darkBlue;
            }
            else
            {
                baseDisplay.text = bases[baseTwoIndex].getSymbol();
                baseDisplay.color = lightBlue;
                redOn.color = darkRed;
                blueOn.color = lightBlue;
            }
        }
        else
        {
            
            whichBase = !whichBase;
            if(!whichBase)
            {
                baseDisplay.text = reactions[baseOneIndex, saltOneIndex].getSalt();
                baseDisplay.color = lightRed;
            }
            else
            {
                baseDisplay.text = reactions[baseTwoIndex, saltTwoIndex].getSalt();
                baseDisplay.color = lightBlue;
            }
        }
    }

    int digitalRoot(int i)
    {
        string rep = "" + i;
        if (rep.Length == 1) return i;
        int first, second;
        first = numerals.IndexOf(rep[0]);
        second = numerals.IndexOf(rep[1]);
        int thisRoot = first + second;
        if (thisRoot > 9) return digitalRoot(thisRoot);
        return thisRoot;
    }

    #region Filters and Vents
    void handleLeftVent()
    {
        if (amogus && _isSolved) {
            int fileNum = Random.Range(1, 4);
            string filename = "vent" + fileNum;
            Audio.PlaySoundAtTransform(filename,lVent.transform);
            return;
            }

        if (_isSolved || !_lightsOn) return;
        leftVent = !leftVent;
        float flapRotate = 0.0f;
        if(leftVent)//open
        {
            flapRotate = 360.0f - flapRotationAngle;
        }
        else//close
        {
            flapRotate = flapRotationAngle;
        }
        foreach (GameObject flap in leftFlaps)
        {
            flap.transform.Rotate(0f, flapRotate, 0f, Space.Self);
        }
    }
    void handleRightVent()
    {
        if (amogus && _isSolved)
        {
            int fileNum = Random.Range(1, 4);
            string filename = "vent" + fileNum;
            Audio.PlaySoundAtTransform(filename, lVent.transform);
            return;
        }
        if (_isSolved || !_lightsOn) return;
        rightVent = !rightVent;
        float flapRotate = 0.0f;
        if(rightVent)//open
        {
            flapRotate = flapRotationAngle;
        }
        else//close
        {
            flapRotate = 360.0f - flapRotationAngle;
        }
        foreach (GameObject flap in rightFlaps)
        {
            flap.transform.Rotate(0f, flapRotate, 0f, Space.Self);
        }
    }
    void handleLeftFilter()
    {
        if (_isSolved || !_lightsOn) return;
        leftFilter = !leftFilter;
        if(leftFilter)//On
        {
            leftLight.GetComponent<Renderer>().material = lightOn;
        }
        else//Off
        {
            leftLight.GetComponent<Renderer>().material = lightOff;
        }
    }
    void handleRightFilter()
    {
        if (_isSolved || !_lightsOn) return;
        rightFilter = !rightFilter;
        if(rightFilter)
        {
            rightLight.GetComponent<Renderer>().material = lightOn;
        }
        else
        {
            rightLight.GetComponent<Renderer>().material = lightOff;
        }
    }
    #endregion

    void constructGraph()
    {
        flowchart = new List<Node>();
        Node[] endPieces = new Node[10];
        //this for loop initializes all of the end nodes by using the Alphabet string and acidIndexes array.
        //rearranging the Acid Indexes array will change which acids correlate with which letter A-J.
        for(int i = 0; i < acidIndexes.Length; i++)
        {
            endPieces[i] = new Node(alphabet[i], acids[acidIndexes[i]]);//the end pieces are initialized with their labels and mixtures
        }
        
        flowchart.Add(new Node("Empty Port Plate",endPieces[0],endPieces[1],emptyPlatePresent));//traversal for A and B
        flowchart.Add(new Node("Ports <= Indicators",endPieces[4],endPieces[5],lessPortsThanInd));//traversal for E and F
        flowchart.Add(new Node("Ports > Last Serial # Digit",flowchart[0],flowchart[1],morePortsThanDig));//traversal towards 0 and 1
        flowchart.Add(new Node("More Lit than Unlit Indicators",endPieces[2],endPieces[3],moreOffThanOnInd));//traversal towards C and D
        flowchart.Add(new Node("Sum of Serial # Letters >= 20",endPieces[6],endPieces[7],lettersTwenty));//traversal towards G and H
        flowchart.Add(new Node("Odd Number of Batteries",flowchart[3],flowchart[4],oddBatt));//traversal towards 3 and 4
        flowchart.Add(new Node("Lit BOB",endPieces[9],endPieces[8],litBOBpresent));//traversal towards I and J
        flowchart.Add(new Node("Modules > Starting Time in Minutes",flowchart[0],flowchart[2],modMoreThanTime));
        flowchart.Add(new Node("D Battery",flowchart[2],flowchart[6],dBatPresent));
        flowchart.Add(new Node("Sum of Serial # Digits >= 12",flowchart[3],flowchart[5],digitsTwelve));
        flowchart.Add(new Node("Odd Number in Serial",flowchart[5],flowchart[6],serialOdd));
        flowchart.Add(new Node("Needy is Present",flowchart[9],flowchart[7],needyPresent));
        flowchart.Add(new Node("Serial # Digits >= Letters",flowchart[10],flowchart[8],moreDigThanLet));
        startingNode = new Node("Serial # contains a Vowel",flowchart[11],flowchart[12],vowelInSerial);
    }

    #region Mixture and Reaction Classes

    class Mixture
    {
        private string symbol, color, name;
        private double mass;

        public Mixture(string s, double m, string c, string n)
        {
            symbol = s;
            mass = m;
            color = c;
            name = n;
        }
        public string getSymbol()
        { return symbol; }
        public double getMass()
        { return mass; }
        public string getColor()
        { return color; }
        public string getName()
        { return name; }

        public override string ToString()
        {
            return "[" + name + ", " + symbol + "][M = " + mass + "]";
        }
        
        
        public bool isEqual(Mixture other)
        {
            if (other.getSymbol().Equals(symbol)) return true;
            return false;
        }
        
    }
    class Reaction
    {
        private string salt;
        private bool liquid, toxic, gas;
        private double ratio;

        public Reaction(string s, bool l, bool g, bool t, double r) { salt = s; liquid = l; gas = g; toxic = t; ratio = r; }
        public Reaction(string s, bool l, bool g, bool t) { salt = s; liquid = l; gas = g; toxic = t; ratio = 1; }
        public string getSalt() { return salt; }
        public bool containsLiquid() { return liquid; }
        public bool containsGas() { return gas; }
        public bool isToxic() { return toxic; }
        public double getRatio() { return ratio; }
        public bool isEqual(Reaction other) {return (other.getSalt().Equals(salt));}
    }

    #endregion

    #region Color Methods

    
    string calcColor(int[] rgb)
    {
        string s = "";
        if (rgb[0] == 1) s = s + "r";
        if (rgb[1] == 1) s = s + "g";
        if (rgb[2] == 1) s = s + "b";
        if (s.Length == 0) s = "k";
        return s;
    }
    Material stringColor(string rgb)
    {
        Material holder = grey;
        switch (rgb)
        {
            case "r":
                holder = red;
                break;
            case "g":
                holder = green;
                break;
            case "b":
                holder = blue;
                break;
            case "rg":
                holder = yellow;
                break;
            case "rb":
                holder = magenta;
                break;
            case "gb":
                holder = cyan;
                break;
            case "rgb":
                holder = white;
                break;
            case "k":
                holder = black;
                break;
            default: holder = black;
                break;
        }
        return holder;
    }
    Material colorToGlow(string rgb)
    {
        Material holder;
        switch (rgb)
        {
            case "r":
                holder = red;
                break;
            case "g":
                holder = green;
                break;
            case "b":
                holder = blue;
                break;
            case "rg":
                holder = yellow;
                break;
            case "rb":
                holder = magenta;
                break;
            case "gb":
                holder = cyan;
                break;
            case "rgb":
                holder = white;
                break;
            case "k":
                holder = black;
                break;
            default:
                holder = black;
                break;
        }

        return holder;
    }
    string colorSub(string b, string subtractor)
    {
        b.Remove(b.IndexOf(subtractor));
        if (b.Length == 0) b = "k";
        return b;
    }
    string colorAdd(string b, string subtractor)
    {
        return "";
    }
    int colorDegree(string c)
    {
        
         if(Array.IndexOf(colorCodes,c) <=2) return 1;
         else if (Array.IndexOf(colorCodes, c) <= 5) return 2;
         else if (Array.IndexOf(colorCodes, c) <= 7) return 3;
        return 0;
         
    }
    #endregion

    //the following data structure is used to traverse the graph/flowchart present on the module
    //each node will have two destinations: yes and no
    //each node will have two strings: one for logging, the other to check if it is an end piece
    //end pieces contain a mixture, non-end pieces have this data as null

    class Node
    {
        private string label;
        private char endLabel;
        private Node yes, no;
        private Mixture endMix;
        private thisCondition condition;

        public Node(char e, Mixture m) //end node, does not have a condition or any further branches
        {
            label = null;
            endLabel = e;
            yes = null;
            no = null;
            endMix = m;
            condition += delegate (){return false; };//will always return false
        }
        public Node(string s, Node y, Node n, thisCondition c) //traversal node, does not have a mixture attached or end label
        {
            label = s;
            endLabel = ' '; //the space indicates that the current node is not an end node, used for a while loop
            yes = y;
            no = n;
            endMix = null;
            condition += c;
            
        }
        public string getLabel()
        { return label; }
        public char getEndLabel()
        { return endLabel; }
        public Node yesNode()
        { return yes; }
        public Node noNode()
        { return no; }
        public Mixture getMix()
        { return endMix; }
        public bool nodeCondition()
        { return condition(); }
    }

    //the following methods are for initializing delegates
    #region Delegates
    bool moreBatsThanHold(){return (Info.GetBatteryCount()>Info.GetBatteryHolderCount());}
    bool moreOffThanOnInd(){return (Info.GetOnIndicators().Count() > Info.GetOffIndicators().Count());}
    bool morePortsThanPlates(){return (Info.GetPortCount() > Info.GetPortPlateCount());}
    bool dBatPresent(){return (Info.GetBatteryHolderCount() != (Info.GetBatteryCount()/2));}
    bool litBOBpresent(){return (Info.IsIndicatorPresent("BOB") && Info.IsIndicatorOn("BOB"));}
    bool vowelInSerial(){return (Info.GetSerialNumberLetters().Contains('A')|| Info.GetSerialNumberLetters().Contains('E')|| 
            Info.GetSerialNumberLetters().Contains('I')|| Info.GetSerialNumberLetters().Contains('O')|| 
            Info.GetSerialNumberLetters().Contains('U'));}
    bool digitsTwelve()
    {
        int adder = 0;
        foreach(int num in Info.GetSerialNumberNumbers()) {adder += num;}
        return (adder>=12);
    }
    bool serialOdd() {return (Int32.Parse(Info.GetSerialNumber().Substring(5,1))%2==1);}
    bool lettersTwenty()
    {
        int adder = 0;
        foreach (char letter in Info.GetSerialNumberLetters())
        {
            adder += (alphabet.LastIndexOf(letter) + 1);
        }
        return (adder >= 20);
    }
    bool oddModules(){return (Info.GetModuleNames().Count() % 2 == 0);}
    bool needyPresent(){return (Info.GetModuleNames().Count() != Info.GetSolvableModuleNames().Count());//counts how many modules are on the bomb and how many are solvable. If they are equal, there are no needys present.
    }
    bool oddBatt(){return (Info.GetBatteryCount() % 2 == 1);}
    bool moreDigThanLet() {return (Info.GetSerialNumberNumbers().Count() >= Info.GetSerialNumberLetters().Count());}
    bool modMoreThanTime(){return (Info.GetTime() / 60 < Info.GetSolvableModuleNames().Count());}
    bool forgetPresent()
    {
        bool presented = false;
        foreach (string name in Info.GetModuleNames())
        {
            if (name.Contains("Forget")) presented = true;
        }
        return presented;
    }
    bool morePortsThanDig(){return (Info.GetPortCount() > Int32.Parse(Info.GetSerialNumber().Substring(5, 1)));}
    bool lessPortsThanInd() { return (Info.GetPortCount() <= Info.GetIndicators().Count()); }
    bool emptyPlatePresent()
    {
        foreach(string[] ports in Info.GetPortPlates())
        {
            if (ports.Length == 0) return true;
        }

        return false;
    }
    #endregion

    #region Twitch Plays Code
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use [!{0} set base NaHCO3] to set NaHCO₃ as your base. Use [!{0} set salt Mg(OTf)2] to set Mg(OTf)₂ as the prepared salt. Use [!{0} set drops 12] to set the drop count to 12. Use [!{0} toggle] to switch the display between red and blue. Use [!{0} vent/filter left/right] to toggle the vent or filter on that side of the module. Use [!{0} cycle] to cycle the salts available. Use [!{0} titrate] to press the titrate button. Use [!{0} titrate at ##] to press the titrate when the seconds digits of the timer say  Use [!{0} colorblind] to toggle colorblind mode.";
#pragma warning restore 414

    string[] GenerateSalts() //Helper method used for getting the salt list for the selected base.
    {
        string[] output = new string[10];
        int chosenIndex = (!whichBase) ? baseOneIndex : baseTwoIndex;
        for (int i = 0; i < 10; i++)
            output[i] = reactions[chosenIndex, i].getSalt();
        return output;
    }
    string RemoveSubscripts(string input)
    {
        return input.ToUpperInvariant().Replace('₂', '2').Replace('₃', '3').Replace('₄', '4');
    }

    IEnumerator ProcessTwitchCommand(string input)
    {
        string[] bases = { "NAOH", "NAHCO3", "KOH", "NH3", "LIOH", "LIC4H9", "NAH", "MG(OH)2"};
        string command = input.Trim().ToUpperInvariant();
        List<string> parameters = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        if (parameters.First() == "TITRATE" || parameters.First() == "SUBMIT")
        {
            parameters.Remove(parameters.First());
            if (parameters.Count == 0)
            { //If it's just titrate, just press the button. If not, we need to do some additional parsing.
                yield return null;
                titrateButton.OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            else if (parameters.Count == 2 && parameters[0] == "AT" && parameters[1].Length == 2 && parameters[1].All(x => "0123456789".Contains(x)))
            {
                yield return null;
                while (((int)Info.GetTime() % 60).ToString().PadLeft(2, '0') == parameters[1])
                    yield return "trycancel"; //Prevents an obscure bug with the TP handler.
                while (((int)Info.GetTime() % 60).ToString().PadLeft(2, '0') != parameters[1])
                    yield return "trycancel";
                titrateButton.OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            else 
	    {
	    	yield return "sendtochaterror Improper formatting of timed titration command.";
	    	yield break;
	    }
    }
        else if (command == "COLORBLIND" || command == "COLOURBLIND" || command == "CB")
        {
            yield return null;
            CBON = !CBON;
            SetCB();
        }
        else if (command == "TOGGLE")
        {
            yield return null;
            baseToggle.OnInteract();
        }
        else if (command == "CYCLE")
        {
            bool startedOnBase = false;
            yield return null;
            if (currentDisplay)
            {
                startedOnBase = true;
                precipitateButton.OnInteract();
                yield return new WaitForSeconds(0.5f);
            }
            for (int i = 0; i < 10; i++)
            {
                baseTravel[1].OnInteract();
                yield return "trycancel";
                yield return new WaitForSeconds(0.75f);
            }
            if (startedOnBase)
                precipitateButton.OnInteract();
        }
        else if (parameters.Count == 2 && (parameters[0] == "VENT" || parameters[0] == "FILTER") && (parameters[1] == "LEFT" || parameters[1] == "RIGHT"))
        {
            yield return null;
            KMSelectable[,] safetyThings = new KMSelectable[,] //Probably one of the weirder ways I've accessed a bunch of buttons.
               { { lFilter, rFilter }, {lVent, rVent } };      //Since each of the KMSelectables is individually assigned, I'm pretty much putting them into their own private array here.
            safetyThings[(parameters[0] == "FILTER") ? 0 : 1, (parameters[1] == "LEFT") ? 0 : 1].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        else if (parameters.First() == "SET" && parameters.Count == 3)
        {
            parameters.Remove("SET");
            if (parameters.First() == "BASE" && bases.Contains(parameters.Last()))
            {
                yield return null;
                if (!currentDisplay)
                    precipitateButton.OnInteract();
                int targetIndex = Array.IndexOf(bases, RemoveSubscripts(parameters.Last()));
                KMSelectable whichButton = (Math.Abs((whichBase ? baseTwoIndex : baseOneIndex) - targetIndex) < 4) ? baseTravel[0] : baseTravel[1];
                while ((whichBase ? baseTwoIndex : baseOneIndex) != targetIndex)
                {
                    whichButton.OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
            }
            else if (parameters.First() == "SALT" && GenerateSalts().Select(x => RemoveSubscripts(x)).Contains(parameters.Last()))
            {
                yield return null;
                if (currentDisplay)
                    precipitateButton.OnInteract();
                int targetIndex = Array.IndexOf(GenerateSalts().Select(x => RemoveSubscripts(x)).ToArray(), RemoveSubscripts(parameters.Last()));
                if (targetIndex == -1)
		{	
                    yield return "sendtochaterror Invalid salt entered.";
	            yield break;
		}
	    	KMSelectable whichButton = (Math.Abs((whichBase ? baseTwoIndex : baseOneIndex) - targetIndex) < 5) ? baseTravel[0] : baseTravel[1];
                while ((whichBase ? saltTwoIndex : saltOneIndex) != targetIndex)
                {
                    whichButton.OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
            }
            else if (parameters.First() == "DROPS" && parameters.Last().All(x => "1234567890".Contains(x)) && parameters.Last().Length <= 2)
            {
                yield return null;
                int inputted = int.Parse(parameters.Last());
                KMSelectable whichTens = (drops / 10 > inputted / 10) ? tenDropTravel[0] : tenDropTravel[1];
                KMSelectable whichOnes = (drops % 10 > inputted % 10) ? oneDropTravel[0] : oneDropTravel[1];
                while (inputted / 10 != drops / 10)
                {
                    whichTens.OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
                while (inputted % 10 != drops % 10)
                {
                    whichOnes.OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
    }

    void SetAutosolverVars()
    {
        submittingBase = (halfSolved && nextAcid == rightAcid) ? rightBase.getSymbol() : leftBase.getSymbol();
        submittingSalt = (halfSolved && nextAcid == rightAcid) ? rightSalt.getSalt() : leftSalt.getSalt();
        correctBaseIndex = Array.IndexOf(bases.Select(x => x.getSymbol()).ToArray(), submittingBase);
        correctSaltIndex = Array.IndexOf(GenerateSalts(), submittingSalt);
        correctDrops = (halfSolved && nextAcid == rightAcid) ? rightBaseDrops : leftBaseDrops;
    }
    IEnumerator TwitchHandleForcedSolve()
    {
        if (unicorn)
        {
            int molMass = (int)thisNode.getMix().getMass();
            if (molMass >= 60) molMass = digitalRoot(molMass);
            while ((int)Info.GetTime() % 60 != molMass)
                yield return true;
            titrateButton.OnInteract();
            yield break;
        }
        SubmittingLeft:
        if (halfSolved && nextAcid == rightAcid)
            goto SubmittingRight;
        if (leftVent ^ leftGas)
        {
            lVent.OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        if (leftFilter ^ leftToxic)
        {
            lFilter.OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        SubmittingRight:
        if (halfSolved && nextAcid == leftAcid)
            goto SubmittingLeft;
        if (rightVent ^ rightGas)
        {
            rVent.OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        if (rightFilter ^ rightToxic)
        {
            rFilter.OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        yield return null;

        SetAutosolverVars();
        if (!currentDisplay) precipitateButton.OnInteract();
        KMSelectable whichBaseButton = (Math.Abs((whichBase ? baseOneIndex : baseTwoIndex) - correctBaseIndex) > 4) ? baseTravel[0] : baseTravel[1];
        while ((whichBase ? baseTwoIndex : baseOneIndex) != correctBaseIndex)
        {
            whichBaseButton.OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        SetAutosolverVars();
        if (currentDisplay) precipitateButton.OnInteract();
        KMSelectable whichSaltButton = (Math.Abs((whichBase ? baseOneIndex : saltTwoIndex) - correctSaltIndex) > 5) ? baseTravel[0] : baseTravel[1];
        Debug.Log(correctSaltIndex);
        Debug.Log((whichBase ? saltTwoIndex : saltOneIndex));
        while ((whichBase ? saltTwoIndex : saltOneIndex) != correctSaltIndex)
        {
            whichBaseButton.OnInteract();
            yield return new WaitForSeconds(0.1f);
        }

        KMSelectable whichTens = (drops / 10 > correctDrops / 10) ? tenDropTravel[0] : tenDropTravel[1];
        KMSelectable whichOnes = (drops % 10 > correctDrops % 10) ? oneDropTravel[0] : oneDropTravel[1];
        while (correctDrops / 10 != drops / 10)
        {
            whichTens.OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        while (correctDrops % 10 != drops % 10)
        {
            whichOnes.OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        titrateButton.OnInteract();
        yield return new WaitForSeconds(0.2f);
        if (_isSolved) yield break;
        if (halfSolved) goto SubmittingLeft;
    }
    #endregion
}
