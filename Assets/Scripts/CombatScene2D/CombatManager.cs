using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class CombatManager : MonoBehaviour
{
    public static CombatManager instance = null;

    [Header("Player Characters")]
    public GameObject[] playerCharacters = new GameObject[4];

    [Header("Enemy Characters")]
    public GameObject[] enemyCharacters = new GameObject[4];

    [Header("Action Panel")]
    public Animator actionPanelAnimator = null;
    public TMP_Text nameLabel = null;
    public Slider HPSlider = null;
    public Slider MPSlider = null;

    [Header("Selected Enemy Panel")]
    public Animator enemyInfoPanelAnimator = null;
    public Image enemyProfileImageDisplay = null;
    public TMP_Text enemyNameDisplay = null;
    public TMP_Text enemyActionsDisplay = null;
    public Slider enemyHealthDisplay = null;
    public TMP_Text enemyHealthValueDisplay = null;

    [Header("End Scene Panels")]
    public GameObject touchBlockerPanel = null;
    public Animator victoryPanelAnimator = null;
    public Animator defeatPanelAnimator = null;

    [Header("Time Sliders")]
    // player character sliders
    public Slider aldenTimeSlider = null;

    // enemy sliders
    public List<Slider> enemySliders = new List<Slider>();

    // enemy slider markers
    public List<Sprite> enemySliderMarkers = new List<Sprite>();

    // Storing script files for all characters on field
    public PlayerCombat2D[] playerCombatControllers = new PlayerCombat2D[4];
    public EnemyCombat2D[] enemyCombatControllers = new EnemyCombat2D[4];

    private bool timeStart = true;

    private bool isAldenTurn = false;
    private bool isValricTurn = false;
    private bool isOsmirTurn = false;
    private bool isAssassinTurn = false;

    public int aldenIndex = -1;
    public int valricIndex = -1;
    public int osmirIndex = -1;
    public int assassinIndex = -1;

    private int currentSelectedTarget = 0;
    private int enemiesAlive = 0;
    private int playersAlive = 0;

    private void Awake()
    {
        if (instance)
        {
            Debug.LogError("Trying to create more than one CombatManager instance!");
            Destroy(gameObject);
        }
        instance = this;
    }

    private void Start()
    {
        // Initialising player & enemy controller scripts
        for(int i=0; i < 4; i++)
        {
            if(playerCharacters[i].gameObject != null)
            {
                playerCombatControllers[i] = playerCharacters[i].gameObject.GetComponent<PlayerCombat2D>();
            }

            if (enemyCharacters[i].gameObject != null)
            {
                enemyCombatControllers[i] = enemyCharacters[i].gameObject.GetComponent<EnemyCombat2D>();
            }
        }

        // Assigning Index values for Player Characters (to save time later by skipping a linear search every time we reference)
        for(int i = 0; i < 4; i++)
        {
            if(playerCombatControllers[i] != null)
            {
                if (playerCombatControllers[i].characterName == "Alden")
                {
                    aldenIndex = i;
                }
                if (playerCombatControllers[i].characterName == "Valric")
                {
                    valricIndex = i;
                }
                if (playerCombatControllers[i].characterName == "Osmir")
                {
                    osmirIndex = i;
                }
                if (playerCombatControllers[i].characterName == "Assassin")
                {
                    assassinIndex = i;
                }
            }
        }

        // Counting alive enemies and players
        for(int i = 0; i < 4; i++)
        {
            if(enemyCombatControllers[i] != null)
            {
                enemiesAlive++;
            }

            if(playerCombatControllers[i] != null)
            {
                playersAlive++;
            }
        }

        // Updating enemy markers on the time sliders
        UpdateEnemySliderMarkers();

        // Start a function that ticks turn count for all characters
        StartCoroutine(TickTime());
    }

    public IEnumerator TickTime()
    {
        while(timeStart == true)
        {
            // Check is all players are dead, DEFEAT
            if(playersAlive <= 0)
            {
                Debug.Log("Defeat");
                timeStart = false;
                StartCoroutine(DisplayDefeatPanel());
            }

            // Check if all enemies are dead, VICTORY
            if (enemiesAlive <= 0)
            {
                Debug.Log("Victory");
                timeStart = false;
                StartCoroutine(DisplayVictoryPanel());
            }

            // Check if a character has reached turn cap
            for (int i = 0; i < 4; i++)
            {
                if(timeStart == true)
                {
                    if (playerCombatControllers[i] != null)
                    {
                        if(playerCombatControllers[i].isDefeated == false)
                        {
                            if (playerCombatControllers[i].turnCounter >= 100)
                            {
                                // Turn starts for ith character
                                // stop time
                                timeStart = false;

                                // setting the active character
                                if (playerCombatControllers[i].characterName == "Alden")
                                {
                                    isAldenTurn = true;
                                }
                                else if (playerCombatControllers[i].characterName == "Valric")
                                {
                                    isValricTurn = true;
                                }
                                else if (playerCombatControllers[i].characterName == "Osmir")
                                {
                                    isOsmirTurn = true;
                                }
                                else if (playerCombatControllers[i].characterName == "Assassin")
                                {
                                    isAssassinTurn = true;
                                }

                                // show action panel & update for active character
                                ShowUpdatedActionPanel();

                                break;
                            }
                        }
                        
                    }
                }
                
                if(timeStart == true)
                {
                    // Check if an enemy has reached turn cap
                    if (enemyCombatControllers[i] != null)
                    {
                        if(enemyCombatControllers[i].isDefeated == false)
                        {
                            if (enemyCombatControllers[i].turnCounter >= 100)
                            {
                                // Turn starts for ith enemy
                                // stop time
                                timeStart = false;

                                // Make the enemy attack a player character
                                enemyCombatControllers[i].Action_PlayTurn();

                                break;
                            }
                        }
                    }
                }
            }

            // Tick time for all characters on field
            if(timeStart == true)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (playerCombatControllers[i] != null)
                    {
                        if(playerCombatControllers[i].isDefeated == false)
                        {
                            playerCombatControllers[i].turnCounter += playerCombatControllers[i].turnSpeed;
                        }
                        else
                        {
                            playerCombatControllers[i].turnCounter = 0;
                        }
                    }

                    if (enemyCombatControllers[i] != null)
                    {
                        if(enemyCombatControllers[i].isDefeated == false)
                        {
                            enemyCombatControllers[i].turnCounter += enemyCombatControllers[i].turnSpeed;
                        }
                        else
                        {
                            enemyCombatControllers[i].turnCounter = 0;
                        }
                    }

                }
            }
            

            // Update time slider position for all characters
            UpdateTimeSliders();

            yield return new WaitForSeconds(0.1f);
        }
    }

    public void ResumeTime()
    {
        timeStart = true;
        StartCoroutine(TickTime());
    }

    public IEnumerator DisplayVictoryPanel()
    {
        touchBlockerPanel.gameObject.SetActive(true);
        AudioManager.instance.PlayVictoryMusic();
        yield return new WaitForSeconds(0.8f);
        victoryPanelAnimator.SetTrigger("Enter");
    }

    public IEnumerator DisplayDefeatPanel()
    {
        touchBlockerPanel.gameObject.SetActive(true);
        AudioManager.instance.PlayDefeatMusic();
        yield return new WaitForSeconds(0.8f);
        defeatPanelAnimator.SetTrigger("Enter");
    }

    private void ShowUpdatedActionPanel()
    {
        // Updating action panel with active character's stats
        if(isAldenTurn == true)
        {
            // updating HP and MP bars
            HPSlider.maxValue = playerCombatControllers[aldenIndex].GetMaxHealthValue();
            HPSlider.value = playerCombatControllers[aldenIndex].GetHealthValue();
            MPSlider.value = playerCombatControllers[aldenIndex].GetManaValue();

            // updating name label
            nameLabel.text = playerCombatControllers[aldenIndex].characterName;
        }

        // Setting initial selected target
        int activeEnemies = CheckActiveEnemies();
        for(int i = 0; i < activeEnemies; i++)
        {
            if(enemyCombatControllers[i].isDefeated == false)
            {
                enemyCombatControllers[i].selectMarker.gameObject.SetActive(true);
                currentSelectedTarget = i;
                break;
            }
        }

        // Updating & Showing the Selected Enemy Info Panel
        UpdateInfoPanel();
        enemyInfoPanelAnimator.SetTrigger("SlideIn");

        actionPanelAnimator.SetTrigger("SlideUp");
    }

    private void UpdateInfoPanel()
    {
        if(enemyCombatControllers[currentSelectedTarget] != null)
        {
            // Update the profile image
            if(enemyCombatControllers[currentSelectedTarget].enemyProfileSprite != null)
            {
                enemyProfileImageDisplay.sprite = enemyCombatControllers[currentSelectedTarget].enemyProfileSprite;
            }
            
            // Update the enemy name
            enemyNameDisplay.text = enemyCombatControllers[currentSelectedTarget].enemyName;

            // Update the enemy actions
            enemyActionsDisplay.text = enemyCombatControllers[currentSelectedTarget].enemyActions;

            // Update the enemy health slider
            enemyHealthDisplay.maxValue = enemyCombatControllers[currentSelectedTarget].maxHealth;
            enemyHealthDisplay.value = enemyCombatControllers[currentSelectedTarget].health;

            // Update the health text
            enemyHealthValueDisplay.text = enemyCombatControllers[currentSelectedTarget].health.ToString() + " / " + enemyCombatControllers[currentSelectedTarget].maxHealth.ToString();
        }
    }

    private int CheckActiveEnemies()
    {
        int activeEnemies = 0;
        for(int i = 0; i < 4; i++)
        {
            if(enemyCombatControllers[i] != null)
            {
                activeEnemies++;
            }
        }
        return activeEnemies;
    }

    private void UpdateTimeSliders()
    {
        for(int i = 0; i < 4; i++)
        {
            // update player sliders
            if(playerCombatControllers[i] != null)
            {
                if(playerCombatControllers[i].isDefeated == false)
                {
                    aldenTimeSlider.value = playerCombatControllers[aldenIndex].turnCounter;
                    // similar for valric
                    // similar for osmir
                    // similar for assassin girl
                }
                // player slider turn off on defeat managed in the ReportDeath() function.
            }

            // update enemy sliders
            if (enemyCombatControllers[i] != null)
            {
                if(enemyCombatControllers[i].isDefeated == false)
                {
                    enemySliders[i].value = enemyCombatControllers[i].turnCounter;
                }
                else
                {
                    enemySliders[i].gameObject.SetActive(false);
                }
            }
        }
    }

    // Use to inform combatmanager about dead characters
    public void ReportDeath()
    {
        enemiesAlive--;
    }

    public void ReportDeath(string charName)
    {
        // handle player char death
        if(charName == "Alden")
        {
            playerCombatControllers[aldenIndex].isDefeated = true;
            aldenTimeSlider.gameObject.SetActive(false);
        }
        else if (charName == "Valric")
        {
            playerCombatControllers[valricIndex].isDefeated = true;
            // update this for valric too
        }
        else if (charName == "Osmir")
        {
            playerCombatControllers[osmirIndex].isDefeated = true;
        }
        else if (charName == "Assassin")
        {
            playerCombatControllers[assassinIndex].isDefeated = true;
        }

        playersAlive--;
    }

    // Updating enemy markers on the sliders
    private void UpdateEnemySliderMarkers()
    {
        for(int i = 0; i < 4; i++)
        {
            if(enemyCombatControllers[i] != null)
            {
                if(enemyCombatControllers[i].enemyMarkerSprite != null)
                {
                    enemySliderMarkers[i] = enemyCombatControllers[i].enemyMarkerSprite;
                }
            }
        }
    }

    // Overloaded HandleDealtDamage function
    public void HandleDealtDamage(int dealtDamage, bool isCritical)  // this one is used by player char to deal damage to enemy
    {
        enemyCombatControllers[currentSelectedTarget].Action_TakeDamage(dealtDamage, isCritical);
    }

    public void HandleDealtDamage(int dealtDamage, int selectedTarget) // this one is used by enemy char to deal damage to player
    {
        playerCombatControllers[selectedTarget].Action_TakeDamage(dealtDamage);
    }

    // Handling Action Panel Commands
    public void OnAttackButton()
    {
        // When attack button is pressed during Alden's turn
        if (isAldenTurn)
        {
            // Play attack animation according to selected target
            playerCombatControllers[aldenIndex].DoAction("Attack" ,currentSelectedTarget);
        }

        // When attack button is pressed during Valric's turn
        if (isValricTurn)
        {

        }

        // When attack button is pressed during Osmir's turn
        if (isOsmirTurn)
        {

        }

        // When attack button is pressed during Assassin's Turn
        if (isAssassinTurn)
        {

        }


        AfterButtonPressHandler();
    }

    public void OnSkillButton()
    {
        actionPanelAnimator.SetTrigger("SkillShow");
    }

    public void OnBackButton()
    {
        actionPanelAnimator.SetTrigger("SkillHide");
    }

    public void OnSkill1Button()
    {

    }

    public void OnSkill2Button()
    {

    }

    public void OnDefendButton()
    {
        // When defend button is pressed during Alden's turn
        if (isAldenTurn)
        {
            // Play defend animation
            playerCombatControllers[aldenIndex].DoAction("Defend", currentSelectedTarget);
        }

        // When attack button is pressed during Valric's turn
        if (isValricTurn)
        {

        }

        // When attack button is pressed during Osmir's turn
        if (isOsmirTurn)
        {

        }

        // When attack button is pressed during Assassin's Turn
        if (isAssassinTurn)
        {

        }


        AfterButtonPressHandler();
    }

    public void OnSkill3Button()
    {

    }

    private void AfterButtonPressHandler()
    {
        enemyCombatControllers[currentSelectedTarget].selectMarker.gameObject.SetActive(false);
        actionPanelAnimator.SetTrigger("SlideDown");
        enemyInfoPanelAnimator.SetTrigger("SlideOut");
    }

    public void OnLeftButton()
    {
        if(currentSelectedTarget == 0) // Selecting next target when current target is 0
        {
            if(enemyCombatControllers[3] != null)
            {
                if(enemyCombatControllers[3].isDefeated == false)
                {
                    enemyCombatControllers[3].selectMarker.gameObject.SetActive(true);
                    enemyCombatControllers[0].selectMarker.gameObject.SetActive(false);
                    currentSelectedTarget = 3;
                }
                else if(enemyCombatControllers[2].isDefeated == false)
                {
                    enemyCombatControllers[2].selectMarker.gameObject.SetActive(true);
                    enemyCombatControllers[0].selectMarker.gameObject.SetActive(false);
                    currentSelectedTarget = 2;
                }
                else if(enemyCombatControllers[1].isDefeated == false)
                {
                    enemyCombatControllers[1].selectMarker.gameObject.SetActive(true);
                    enemyCombatControllers[0].selectMarker.gameObject.SetActive(false);
                    currentSelectedTarget = 1;
                }
            }
            else if(enemyCombatControllers[2] != null)
            {
                if(enemyCombatControllers[2].isDefeated == false)
                {
                    enemyCombatControllers[2].selectMarker.gameObject.SetActive(true);
                    enemyCombatControllers[0].selectMarker.gameObject.SetActive(false);
                    currentSelectedTarget = 2;
                }
                else if(enemyCombatControllers[1].isDefeated == false)
                {
                    enemyCombatControllers[1].selectMarker.gameObject.SetActive(true);
                    enemyCombatControllers[0].selectMarker.gameObject.SetActive(false);
                    currentSelectedTarget = 1;
                }
            }
            else if(enemyCombatControllers[1] != null)
            {
                if(enemyCombatControllers[1].isDefeated == false)
                {
                    enemyCombatControllers[1].selectMarker.gameObject.SetActive(true);
                    enemyCombatControllers[0].selectMarker.gameObject.SetActive(false);
                    currentSelectedTarget = 1;
                }
            }
        }
        else if(currentSelectedTarget == 1) // Selecting next target when current target is 1
        {
            if (enemyCombatControllers[0].isDefeated == false)
            {
                enemyCombatControllers[0].selectMarker.gameObject.SetActive(true);
                enemyCombatControllers[1].selectMarker.gameObject.SetActive(false);
                currentSelectedTarget = 0;
            }
            else if (enemyCombatControllers[3] != null)
            {
                if (enemyCombatControllers[3].isDefeated == false)
                {
                    enemyCombatControllers[3].selectMarker.gameObject.SetActive(true);
                    enemyCombatControllers[1].selectMarker.gameObject.SetActive(false);
                    currentSelectedTarget = 3;
                }
                else if (enemyCombatControllers[2].isDefeated == false)
                {
                    enemyCombatControllers[2].selectMarker.gameObject.SetActive(true);
                    enemyCombatControllers[1].selectMarker.gameObject.SetActive(false);
                    currentSelectedTarget = 2;
                }
            }
            else if (enemyCombatControllers[2] != null)
            {
                if (enemyCombatControllers[2].isDefeated == false)
                {
                    enemyCombatControllers[2].selectMarker.gameObject.SetActive(true);
                    enemyCombatControllers[1].selectMarker.gameObject.SetActive(false);
                    currentSelectedTarget = 2;
                }
            }
        }
        else if(currentSelectedTarget == 2) // Selecting next target when current target is 2
        {
            if(enemyCombatControllers[1].isDefeated == false)
            {
                enemyCombatControllers[1].selectMarker.gameObject.SetActive(true);
                enemyCombatControllers[2].selectMarker.gameObject.SetActive(false);
                currentSelectedTarget = 1;
            }
            else if(enemyCombatControllers[0].isDefeated == false)
            {
                enemyCombatControllers[0].selectMarker.gameObject.SetActive(true);
                enemyCombatControllers[2].selectMarker.gameObject.SetActive(false);
                currentSelectedTarget = 0;
            }
            else if(enemyCombatControllers[3] != null)
            {
                if(enemyCombatControllers[3].isDefeated == false)
                {
                    enemyCombatControllers[3].selectMarker.gameObject.SetActive(true);
                    enemyCombatControllers[2].selectMarker.gameObject.SetActive(false);
                    currentSelectedTarget = 3;
                }
            }
        }
        else if(currentSelectedTarget == 3) // Selecting next target when current target is 3
        {
            if(enemyCombatControllers[2].isDefeated == false)
            {
                enemyCombatControllers[2].selectMarker.gameObject.SetActive(true);
                enemyCombatControllers[3].selectMarker.gameObject.SetActive(false);
                currentSelectedTarget = 2;
            }
            else if(enemyCombatControllers[1].isDefeated == false)
            {
                enemyCombatControllers[1].selectMarker.gameObject.SetActive(true);
                enemyCombatControllers[3].selectMarker.gameObject.SetActive(false);
                currentSelectedTarget = 1;
            }
            else if(enemyCombatControllers[0].isDefeated == false)
            {
                enemyCombatControllers[0].selectMarker.gameObject.SetActive(true);
                enemyCombatControllers[3].selectMarker.gameObject.SetActive(false);
                currentSelectedTarget = 0;
            }
        }

        UpdateInfoPanel();
    }

    public void OnRightButton()
    {
        if (currentSelectedTarget == 0) // Selecting next target when current target is 0
        {
            if (enemyCombatControllers[1] != null)
            {
                if (enemyCombatControllers[1].isDefeated == false)
                {
                    enemyCombatControllers[1].selectMarker.gameObject.SetActive(true);
                    enemyCombatControllers[0].selectMarker.gameObject.SetActive(false);
                    currentSelectedTarget = 1;
                }
                else if(enemyCombatControllers[2] != null)
                {
                    if(enemyCombatControllers[2].isDefeated == false)
                    {
                        enemyCombatControllers[2].selectMarker.gameObject.SetActive(true);
                        enemyCombatControllers[0].selectMarker.gameObject.SetActive(false);
                        currentSelectedTarget = 2;
                    }
                    else if(enemyCombatControllers[3] != null)
                    {
                        if(enemyCombatControllers[3].isDefeated == false)
                        {
                            enemyCombatControllers[3].selectMarker.gameObject.SetActive(true);
                            enemyCombatControllers[0].selectMarker.gameObject.SetActive(false);
                            currentSelectedTarget = 3;
                        }
                    }
                }
            }
        }
        else if(currentSelectedTarget == 1) // Selecting next target when current target is 1
        {
            if (enemyCombatControllers[2] != null)
            {
                if (enemyCombatControllers[2].isDefeated == false)
                {
                    enemyCombatControllers[2].selectMarker.gameObject.SetActive(true);
                    enemyCombatControllers[1].selectMarker.gameObject.SetActive(false);
                    currentSelectedTarget = 2;
                }
                else if (enemyCombatControllers[3] != null)
                {
                    if (enemyCombatControllers[3].isDefeated == false)
                    {
                        enemyCombatControllers[3].selectMarker.gameObject.SetActive(true);
                        enemyCombatControllers[1].selectMarker.gameObject.SetActive(false);
                        currentSelectedTarget = 3;
                    }
                    else if (enemyCombatControllers[0].isDefeated == false)
                    {
                        enemyCombatControllers[0].selectMarker.gameObject.SetActive(true);
                        enemyCombatControllers[1].selectMarker.gameObject.SetActive(false);
                        currentSelectedTarget = 0;
                    }
                }
                else if (enemyCombatControllers[0].isDefeated == false)
                {
                    enemyCombatControllers[0].selectMarker.gameObject.SetActive(true);
                    enemyCombatControllers[1].selectMarker.gameObject.SetActive(false);
                    currentSelectedTarget = 0;
                }
            }
        }
        else if(currentSelectedTarget == 2) // Selecting next target when current target is 2
        {
            if (enemyCombatControllers[3] != null)
            {
                if (enemyCombatControllers[3].isDefeated == false)
                {
                    enemyCombatControllers[3].selectMarker.gameObject.SetActive(true);
                    enemyCombatControllers[2].selectMarker.gameObject.SetActive(false);
                    currentSelectedTarget = 3;
                }
                else if (enemyCombatControllers[0].isDefeated == false)
                {
                    enemyCombatControllers[0].selectMarker.gameObject.SetActive(true);
                    enemyCombatControllers[2].selectMarker.gameObject.SetActive(false);
                    currentSelectedTarget = 0;
                }
                else if (enemyCombatControllers[1].isDefeated == false)
                {
                    enemyCombatControllers[1].selectMarker.gameObject.SetActive(true);
                    enemyCombatControllers[2].selectMarker.gameObject.SetActive(false);
                    currentSelectedTarget = 1;
                }
            }
            else if (enemyCombatControllers[0].isDefeated == false)
            {
                enemyCombatControllers[0].selectMarker.gameObject.SetActive(true);
                enemyCombatControllers[2].selectMarker.gameObject.SetActive(false);
                currentSelectedTarget = 0;
            }
            else if (enemyCombatControllers[1].isDefeated == false)
            {
                enemyCombatControllers[1].selectMarker.gameObject.SetActive(true);
                enemyCombatControllers[2].selectMarker.gameObject.SetActive(false);
                currentSelectedTarget = 1;
            }
        }
        else if(currentSelectedTarget == 3) // Selecting next target when current target is 3
        {
            if (enemyCombatControllers[0].isDefeated == false)
            {
                enemyCombatControllers[0].selectMarker.gameObject.SetActive(true);
                enemyCombatControllers[3].selectMarker.gameObject.SetActive(false);
                currentSelectedTarget = 0;
            }
            else if (enemyCombatControllers[1].isDefeated == false)
            {
                enemyCombatControllers[1].selectMarker.gameObject.SetActive(true);
                enemyCombatControllers[3].selectMarker.gameObject.SetActive(false);
                currentSelectedTarget = 1;
            }
            else if (enemyCombatControllers[2].isDefeated == false)
            {
                enemyCombatControllers[2].selectMarker.gameObject.SetActive(true);
                enemyCombatControllers[3].selectMarker.gameObject.SetActive(false);
                currentSelectedTarget = 2;
            }
        }

        UpdateInfoPanel();
    }
}
