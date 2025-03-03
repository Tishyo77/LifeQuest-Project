using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEditor;
using UnityEditorInternal;

public class VN_Manager : MonoBehaviour
{
    [Header("Character Prefabs")]
    public List<GameObject> characters = new List<GameObject>();
    [Header("Background Sprites")]
    public List<Sprite> backgrounds = new List<Sprite>();
    [Header("SFX Audioclips")]
    public AudioClip buttonClickSound = null;
    public List<AudioClip> soundEffects = new List<AudioClip>();
    [Header("Background Musics")]
    public AudioClip initialBackgroundMusic = null;
    public List<AudioClip> backgroundMusic = new List<AudioClip>();
    [Header("Text Display Speed")]
    public float textSpeed = 0.01f;
    [Header("Typing Sound Settings")]
    public AudioClip[] textSounds = new AudioClip[2];
    public float soundPitchMin = 0.9f;
    public float soundPitchMax = 1.1f;
    public int soundInterval = 1;

    [Header("Plug in these components:")]
    public Image background = null;
    public GameObject backgroundTransition = null;
    public GameObject nextButton = null;
    public AudioSource BGM_Source = null;
    public AudioSource SFX_Source = null;
    public AudioSource textSoundSource = null;
    public GameObject nameTab = null;
    public TMP_Text nameTabText;
    public TMP_Text dialogueBoxText = null;

    [Header("Create Dialogues:")]
    public List<DialogueEntry> dialogues = new List<DialogueEntry>();

    // Internal Variables
    private Animator nameTabAnimator;
    private float nextDialogue = 0;
    private int activeCharacter = -1;

    public void Start()
    {
        // Triggering the next button to set up scene with first dialogue
        OnNextButton();
        characters[activeCharacter].gameObject.GetComponent<Animator>().SetTrigger("SlideInstant");  // using a faster version of animation to spawn character sooner

        // Playing the initial background music
        BGM_Source.clip = initialBackgroundMusic;
        BGM_Source.Play();

        nameTabAnimator = nameTab.gameObject.GetComponent<Animator>();
    }

    public void OnNextButton() // Loads the scene with the next dialogue's data.
    {
        StartCoroutine(UpdateSceneData());
    }

    public IEnumerator UpdateSceneData()
    {
        // Play button clicked SFX
        SFX_Source.clip = buttonClickSound;
        SFX_Source.Play();

        // Fetching the appropriate dialogue data for next step
        DialogueEntry currentDialogue = null;
        for (int i = 0; i < dialogues.Count; i++)
        {
            if (dialogues[i].dialogueID == nextDialogue)
            {
                currentDialogue = dialogues[i];
                break;
            }
        }

        // Checking if BG transition is needed; -1 = Not Needed.
        if (currentDialogue != null && currentDialogue.background != -1)
        {
            StartCoroutine(SwapBackground(currentDialogue.background));
            nextButton.gameObject.SetActive(false);                                 // to prevent user from skipping dialogue mid transition
            yield return new WaitForSeconds(1.5f);
        }

        // Making the active character slide into frame
        if (currentDialogue != null && currentDialogue.character != -1)
        {
            if (currentDialogue.background != -1)
            {
                StartCoroutine(SlideAfterTransition(currentDialogue));
            }
            else
            {
                characters[currentDialogue.character].gameObject.SetActive(true);
            }
        }

        // Checking if the next dialogue is made by a different character.

        // First initialising the active character
        if (activeCharacter == -1)
        {
            activeCharacter = currentDialogue.character;
        }
        else // If already initialised, checking to see if active character needs to be changed.
        {
            if (currentDialogue.character != activeCharacter) // character needs to be changed
            {
                characters[activeCharacter].gameObject.GetComponent<Animator>().SetTrigger("SlideOut");  // Removing the old active character

                characters[currentDialogue.character].gameObject.SetActive(true);  //spawning in the new character after short delay
                activeCharacter = currentDialogue.character;  // Refreshing the current active character
            }
        }

        // Moving the name tab if needed
        if (currentDialogue.tabRight == true)
        {
            nameTabAnimator.SetTrigger("MoveRight");
        }
        if (currentDialogue.tabLeft == true)
        {
            nameTabAnimator.SetTrigger("MoveLeft");
        }

        // Inserting the character name in the name tab
        if (currentDialogue.tabLeft == true || currentDialogue.tabRight == true)
        {
            StartCoroutine(InsertCharacterName(currentDialogue.characterName));   // with delay when name tab moves
        }
        else
        {
            nameTabText.text = currentDialogue.characterName;  // instantly when static
        }

        // Inserting character dialogue
        StartCoroutine(AnimateText(currentDialogue.dialogueText));

        // Playing dialogue SFX if it exists
        if (currentDialogue.SFX != -1)
        {
            SFX_Source.clip = soundEffects[currentDialogue.SFX];
            SFX_Source.Play();
        }

        // Changing BGM if needed
        if (currentDialogue.BGM != -1)
        {
            StartCoroutine(BGMFadeTransition(currentDialogue.BGM));
        }

        // Moving to next dialogue
        nextDialogue = currentDialogue.nextDialogue;
    }

    public IEnumerator BGMFadeTransition(int BGM_Index)
    {
        // Storing the initial volume level of BGM
        float setVolume = BGM_Source.volume;

        // Reducing volume of current track
        while(BGM_Source.volume > 0)
        {
            BGM_Source.volume -= 0.05f;
            yield return new WaitForSeconds(0.1f);
        }

        // Changing tracks
        BGM_Source.clip = backgroundMusic[BGM_Index];
        BGM_Source.Play();

        // Restoring volume of new track
        while(BGM_Source.volume < setVolume)
        {
            BGM_Source.volume += 0.05f;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public IEnumerator AnimateText(string characterDialogue)
    {
        for(int i=0; i < characterDialogue.Length + 1; i++)
        {
            dialogueBoxText.text = characterDialogue.Substring(0, i);

            // Play typing sound
            if (i > 0 && (i % soundInterval == 0))
            {
                if (textSoundSource != null && textSounds != null && textSounds.Length > 0)
                {
                    int soundIndex = UnityEngine.Random.Range(0, textSounds.Length);
                    textSoundSource.clip = textSounds[soundIndex];
                    textSoundSource.pitch = UnityEngine.Random.Range(soundPitchMin, soundPitchMax);
                    textSoundSource.Play();

                    yield return null; //enable or disable this incase tweaking text sound frequency, could probably save some performance
                }
            }

            yield return new WaitForSeconds(textSpeed);
        }
    }

    public IEnumerator InsertCharacterName(string characterName)
    {
        yield return new WaitForSeconds(0.25f);
        nameTabText.text = characterName;
    }

    public IEnumerator SwapBackground(int nextBackground)
    {
        backgroundTransition.gameObject.GetComponent<Animator>().SetTrigger("StartBlackout");
        Debug.Log("Trigger Started.");
        yield return new WaitForSeconds(1.1f);
        background.sprite = backgrounds[nextBackground];
        yield return new WaitForSeconds(0.4f);
        NextButtonActivator();
    }

    public void NextButtonActivator()
    {
        nextButton.gameObject.SetActive(true);
    }

    public IEnumerator SlideAfterTransition(DialogueEntry currentDialogue)
    {
        yield return new WaitForSeconds(1.5f);
        characters[currentDialogue.character].gameObject.SetActive(true);
    }

    [ContextMenu("Auto Index")]
    public void AutoIndexDialogues()
    {
        for(int i = 0; i < dialogues.Count; i++)
        {
            dialogues[i].dialogueID = i;
            dialogues[i].nextDialogue = i + 1;
        }
    }
}

[Serializable] 
public class DialogueEntry
{
    public float dialogueID = -1;
    public float nextDialogue = -1;
    [TextArea(1, 5)]
    public string characterName = null;
    [TextArea(5, 10)]
    public string dialogueText = null;
    public int character = -1;
    public int background = -1;
    public int SFX = -1;
    public int BGM = -1;
    public bool tabRight = false;
    public bool tabLeft = false;
    public bool lastDialogue = false;
    [TextArea(1, 5)]
    public String loadCombat = null;
}
   