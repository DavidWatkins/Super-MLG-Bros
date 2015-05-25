using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Main game manager that handles all the smarts for the entire game
//m_ variable prefix designates that this is a member variable of this class
public class GameManagerScript : MonoBehaviour {

    private const int Width = 400, Height = 300;
    private const int MAX_SIZE = 16, OFFSET = 8;
    private const int MAX_SIZE_Y = 12, OFFSET_Y = 6;


    public static string audio_dir = "Music/";
    public static string reaction_dir = "Reactions/";
    //Need a way to get those dank wubz
    public static string[] audio_clips = new string[] {
        "AIRHORN SONATA",
        "Darude Dankstorm", 
        "Digeridubstep",
        "Faster",
        "Forsen",
        "Frozen",
        "He Man",
        "Inception",
        "Meatball Parade",
        "Nyan Cat",
        "Rick Roll'd",
        "SELFIE",
        "Skeletons",
        "Skrillex",
        "Trololo"
    };

    public static string[] reactions = new string[] {
          "GET_NO-SCOPED!",
          "Oh_Baby_A_Triple",
          "OMG_NO_WAY",
          "Random_Screaming",
          "Sick_Reaction_1",
          "Sick_Reaction_2",
          "skrillex_yes_oh_my_gosh",
          "SMOKE_WEEK_EVERYDAY",
          "WHAT_THE_FU__BOOM!!!",
          "WOMBO_COMBO!!!!"
    };

    private static AudioClip[] ReactionClips;
    private AudioClip SadAirHorn;
    private AudioClip prevMusic;

    //For drop down menu
    private Vector2 scrollViewVector = Vector2.zero;
    public Rect dropDownRect = new Rect(250,50,125,300);
    bool show = false;


    public AudioClip ClickSound;
    public AudioClip HitMarkerSound;
    public LayerMask GroundMask;

    //We need to have a variable controlling the active menu
    private MenuTypes m_ActiveMenu { get; set; }
    private MenuTypes m_SourceMenu { get; set; }

    //We need to know when to display the menu or play the game
    private bool IsMenuActive { get; set; }
    private bool IsPlaying { get; set; }
    public static bool IsDead = false;

    private GameObject m_Player;
    private Animator m_PlayerAnimator;

    private System.Random r;

    private Settings m_Settings = new Settings();
    private AudioSource m_SoundSource, m_MusicSource, m_ReactionSource;
    private float prevMusicSlider;
    private bool isNiceTry;

    private int Score = 0, numIters = 0;
    private ArrayList platforms;
    private LinkedList<GameObject> DankMemes;

    private GameObject MainPlatform,
                       MainMeme;

    //Constructor defined at runtime for GameManagerScript
    public GameManagerScript() {
        //Assign the inner static fields of MenuTypes to the corresponding values needed for menu creation
        MenuTypes.MainMenu     = new MenuTypes(CreateMainMenu, "Main Menu");
        MenuTypes.OptionsMenu  = new MenuTypes(CreateOptionsMenu, "Options");
        MenuTypes.GameOverMenu = new MenuTypes(CreateGameOverMenu, "Game Over");

        platforms = new ArrayList();
        DankMemes = new LinkedList<GameObject>();

        r = new System.Random();
    }

	void Awake() {
        IsMenuActive = true;
        m_ActiveMenu = MenuTypes.MainMenu;

		//Allow the game to run in the background
		Application.runInBackground = true;

		//If we were to add more scenes, we want this script which runs the smarts to keep state
		DontDestroyOnLoad (this.gameObject);

        //This gets the main audio output from the main camera and assigns it to m_SoundSource
        m_SoundSource = Camera.main.transform.FindChild("Sound").GetComponent<AudioSource>();
        m_MusicSource = Camera.main.transform.FindChild("Music").GetComponent<AudioSource>();
        m_ReactionSource = Camera.main.transform.FindChild("Reaction").GetComponent<AudioSource>();

        m_Settings.Load(
            m_MusicSource,
            m_SoundSource,
            m_ReactionSource
        );
      
        //Play a random annoying song. This is going to be fun :)
        ClickSound = (AudioClip)Resources.Load("Sounds/click");
        System.Random r = new System.Random();
        m_MusicSource.clip = (AudioClip)Resources.Load(audio_dir + audio_clips[r.Next(audio_clips.Length)]);
        m_MusicSource.Play();
        m_MusicSource.loop = true;

        //LOL you aren't lowering the volume
        m_Settings.MusicVolume = 1.0f; 
        prevMusicSlider = (float)m_Settings.MusicVolume;
        isNiceTry = false;

        MainPlatform = GameObject.Find("Main Platform");
        MainMeme = GameObject.Find("Main Meme");

        m_Player = GameObject.Find("Mario");
        m_PlayerAnimator = m_Player.GetComponent<Animator>();
        ReactionClips = new AudioClip[reactions.Length];
        for (int index = 0; index < reactions.Length; index++)
        {
            ReactionClips[index] = (AudioClip) Resources.Load(reaction_dir + reactions[index]);
        }

        SadAirHorn = (AudioClip)Resources.Load("Sounds/Iamkill");
        HitMarkerSound = (AudioClip)Resources.Load("Sounds/Hit Marker");

        numIters = 0;
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        if (IsMenuActive && m_ActiveMenu != MenuTypes.GameOverMenu)
        {
            numIters++;
            if (numIters % 5 == 0)
            {
                GameObject temp2 = Instantiate(MainMeme);
                temp2.GetComponent<AnimatedGifDrawer>().drawPosition = new Vector2(-r.Next(Screen.width) + Screen.width, -r.Next(Screen.height) + Screen.height);
                DankMemes.AddLast(temp2);
                
            }

            if (DankMemes.Count > 20)
            {
                Destroy(DankMemes.First.Value);
                DankMemes.RemoveFirst();
                m_SoundSource.Stop();
            }
        }
	}

	void OnGUI() {
        GUI.Label(new Rect(0, 0, 100, 100), "Score: " + Score);
        GUI.Label(new Rect(Screen.width - 100, 0, 100, 100), "Highscore: " + m_Settings.HighScore);

        if (IsMenuActive) //Want to draw menu when it is active
        {
			//Draw a menu rectangle at the middle of the current screen
			//(Screen.width - width)/2 will always be in the middle of the screen
			//Same for height
			Rect WindowRect = new Rect((Screen.width - Width)/2, 
			                           (Screen.height - Height)/2, 
			                           Width, Height);

            //We pass the currently active menu function into the Window function so that the correct menu is rendered
			//id parameter is unimportant, therefore 0xBAE
			GUILayout.Window(0xBAE, WindowRect, m_ActiveMenu.windowFunction, m_ActiveMenu.menustring);
        }
	}

    private void CreateMainMenu(int id)
    {
        //If the player presses the start game button play a sound and remove the menu
        if (GUILayout.Button("Start Game"))
        {
            m_SoundSource.PlayOneShot(ClickSound);
            m_PlayerAnimator.SetBool("IsDead", false);
            IsPlaying = true;
            IsMenuActive = false;
            m_Player.transform.position = new Vector2(0, 0);
            m_Player.layer = 8;
            m_Player.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
            numIters = 0;

            for (LinkedListNode<GameObject> j = DankMemes.First; DankMemes.Count > 0; j = DankMemes.First)
            {
                Destroy((GameObject)j.Value);
                DankMemes.RemoveFirst();
            }
        }

        //Change the menu context to Options
        if (GUILayout.Button("Options"))
        {
            m_SoundSource.PlayOneShot(ClickSound);
            m_SourceMenu = MenuTypes.MainMenu;
            m_ActiveMenu = MenuTypes.OptionsMenu;
        }

        //You cannot exit a unity application or call quit if they are in the web player or editor
        //Only good for client side games
        if (!Application.isWebPlayer && !Application.isEditor)
        {
            if (GUILayout.Button("Exit"))
            {
                m_SoundSource.PlayOneShot(ClickSound);
                Application.Quit();
            }
        }
    }

    private void CreateOptionsMenu(int id)
    {
        if (isNiceTry)
        {
            GUILayout.Label("THE SLIDER, IT DOES NOTHING!", GUILayout.Width(300));
        }
        else if (prevMusicSlider != m_Settings.MusicVolume)
        {
            isNiceTry = true;
        }

        //Add a horizontal music volume slider
        GUILayout.BeginHorizontal();
        GUILayout.Label("Music Volume: ", GUILayout.Width(90));
        /* m_Settings.MusicVolume = */prevMusicSlider = GUILayout.HorizontalSlider(prevMusicSlider, 0.0f, 1.0f);
        GUILayout.EndHorizontal();

        //Add a horizontal slider for sound
        GUILayout.BeginHorizontal();
        GUILayout.Label("Sound Volume: ", GUILayout.Width(90));
        m_Settings.SoundVolume = GUILayout.HorizontalSlider(m_Settings.SoundVolume, 0.0f, 1.0f);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Reset High Score"))
        {
            m_SoundSource.PlayOneShot(ClickSound);
            m_Settings.HighScore = 0;
        }

        if(show) {
            scrollViewVector = GUILayout.BeginScrollView(scrollViewVector, "");
            for(int index = 0; index < audio_clips.Length; index++) {
                if(GUILayout.Button(audio_clips[index])) {
                    show = false;
                    m_SoundSource.PlayOneShot(ClickSound);

                    m_MusicSource.clip = (AudioClip)Resources.Load(audio_dir + audio_clips[index]);
                    m_MusicSource.Play();
                    m_MusicSource.loop = true;
                }
            }
            GUILayout.EndScrollView();
        } else if(GUILayout.Button("Change Song")) {
            show = true;
            m_SoundSource.PlayOneShot(ClickSound);
        }
       
   
        //B
        if (GUILayout.Button("Back"))
        {
            m_SoundSource.PlayOneShot(ClickSound);
            m_Settings.Save();
            m_ActiveMenu = m_SourceMenu;
        }
    }

    private void CreateGameOverMenu(int id)
    {
        GUILayout.Label("YOU DIED", GUILayout.Width(300));
        GUILayout.Label("Your Score: " + Score, GUILayout.Width(300));
        GUILayout.Label("High Score: " + m_Settings.HighScore, GUILayout.Width(300));
        if (GUILayout.Button("Restart"))
        {
            m_SoundSource.PlayOneShot(ClickSound);
            m_PlayerAnimator.SetBool("IsDead", false);
            IsPlaying = true;
            IsMenuActive = false;
            m_Player.transform.position = new Vector2(0, 0);
            m_Player.layer = 8;
            m_Player.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
            Score = 0;

            m_MusicSource.clip = prevMusic;
            m_MusicSource.Play();
            m_MusicSource.loop = true;

            m_ReactionSource.Stop();
        }

        if (GUILayout.Button("Go to Main Menu"))
        {
            m_SoundSource.PlayOneShot(ClickSound);
            m_ActiveMenu = MenuTypes.MainMenu;
            Score = 0;

            m_MusicSource.clip = prevMusic;
            m_MusicSource.Play();
            m_MusicSource.loop = true;

            m_ReactionSource.Stop();
        }
    }

    void FixedUpdate() 
    {
        if (IsPlaying)
        {
            if (m_PlayerAnimator.GetBool("IsDead"))
            {
                numIters = 0;
                if (Score > m_Settings.HighScore)
                    m_Settings.HighScore = Score;
                for (int i = 0; i < platforms.Count; i++)
                {
                    Destroy((GameObject) platforms[i]);
                    platforms.Remove(platforms[i]);
                    i--;
                }
                for (LinkedListNode<GameObject> j = DankMemes.First; DankMemes.Count > 0; j = DankMemes.First)
                {
                    Destroy((GameObject)j.Value);
                    DankMemes.RemoveFirst();
                }
                IsPlaying = false;
                IsMenuActive = true;
                m_ActiveMenu = MenuTypes.GameOverMenu;

                prevMusic = m_MusicSource.clip;

                m_SoundSource.Stop();

                m_MusicSource.clip = SadAirHorn;
                m_MusicSource.Play();
                m_MusicSource.loop = true;

                return;
            }
            numIters++;
            if (numIters % 5 == 0)
            {
                GameObject platform = Instantiate(MainPlatform);
                platform.transform.position = new Vector2(-r.Next(MAX_SIZE) + OFFSET, 10);
                platforms.Add(platform);
            }

            for (int i = 0; i < platforms.Count; ++i)
            {
                GameObject e = (GameObject)platforms[i];
                e.transform.position = new Vector2(e.transform.position.x, e.transform.position.y - 0.08f);
                if (e.transform.position.y < -5)
                {

                    Destroy(e);
                    platforms.Remove(e);
                    m_SoundSource.PlayOneShot(HitMarkerSound);
                    i--;
                    Score++;

                    if (Score % 10 == 5)
                    {
                        GameObject temp2 = Instantiate(MainMeme);
                        temp2.GetComponent<AnimatedGifDrawer>().drawPosition = new Vector2(-r.Next(Screen.width) + Screen.width, -r.Next(Screen.height) + Screen.height);
                        DankMemes.AddLast(temp2);
                    }

                    if (Score % 15 == 3 && !m_SoundSource.isPlaying)
                    {
                        m_ReactionSource.PlayOneShot(ReactionClips[r.Next(reactions.Length)]);
                    }
                }
            }

            if (DankMemes.Count > 10)
            {
                Destroy(DankMemes.First.Value);
                DankMemes.RemoveFirst();
                m_SoundSource.Stop();
            }

        }

        
    }
	
	//We want an easy way to create additional menu items
	//Using a class allows for more variables to handle state, such as the title for the menu
	public class MenuTypes
	{
		public static MenuTypes
			MainMenu = null,
			OptionsMenu = null,
			PauseMenu = null,
			GameOverMenu = null;
		
		public string menustring { get; set; }
		public GUI.WindowFunction windowFunction { get; set; }
		
		public MenuTypes(GUI.WindowFunction windowFunction, string menustring) {
			this.windowFunction = windowFunction;
			this.menustring = menustring;
		}
	}
}
