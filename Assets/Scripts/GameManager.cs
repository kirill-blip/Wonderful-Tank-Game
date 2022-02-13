﻿using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public bool isStopped;
    public BaseScript baseGO;
    public Text countOfEnemiesText;
    public Text healthText;
    public int levelId;
    public Transform playerPoint;
    public Rigidbody2D playerPointRigid;
    private EnemySpawnManager enemySpawnManager;
    private PlayerController playerController;


    private void Start()
    {
        playerPointRigid = playerPoint.GetComponentInChildren<Rigidbody2D>();
        playerPointRigid.gameObject.SetActive(false);

        enemySpawnManager = GetComponent<EnemySpawnManager>();
        enemySpawnManager.spawnFinished += UpdateCountOfEnemies;
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        playerController.playerDestroyed += PlayerController_playerDestroyed;
        baseGO = GameObject.Find("Base").GetComponent<BaseScript>();
        baseGO.baseDestroyed += GameManager_baseDestroyed;
        healthText.text = "Health: " + playerController.GetHealth();
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            PlayerPrefs.DeleteKey("HaveBoat");
            PlayerPrefs.DeleteKey("TurboShooting");
        }
    }

    private void UpdateCountOfEnemies(object sender, int e)
    {
        countOfEnemiesText.text = "Count of enemies: " + e;
    }
    public void GameManager_onBonus(object sender, BonusType type)
    {
        switch (type)
        {
            case BonusType.shootingTime:
                playerController.turboShooting = true;
                //StartCoroutine(ChangeShootingTime());
                break;
            case BonusType.stopTimeForEnemy:
                StartCoroutine(StoppingEnemy());
                break;
            case BonusType.bomb:
                var enemies = GameObject.FindGameObjectsWithTag("Enemy");
                foreach (var enemy in enemies)
                {
                    enemy.GetComponent<Enemy>().DestroyTank();
                    Debug.Log("Enemy destoy");
                }
                break;
            case BonusType.boat:
                playerController.boatGO.SetActive(true);
                playerController.canMoveOnWater = true;
                break;
            case BonusType.shield:
                StartCoroutine(SetActiveShield());
                break;
            case BonusType.destroyBush:
                playerController.canDestroyBush = true;
                break;
            default:
                break;
        }
    }
    IEnumerator SetActiveShield()
    {
        playerController.hasShield = true;
        playerController.shieldGO.gameObject.SetActive(true);
        yield return new WaitForSeconds(10f);
        playerController.hasShield = false;
        playerController.shieldGO.gameObject.SetActive(false);
    }
    IEnumerator StoppingEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        isStopped = true;
        foreach (var enemy in enemies)
        {
            enemy.GetComponent<Enemy>().speed = 0;
            enemy.GetComponent<Enemy>().canShoot = false;
        }
        yield return new WaitForSeconds(15f);

        isStopped = false;
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            enemy.GetComponent<Enemy>().speed = 5;
            enemy.GetComponent<Enemy>().canShoot = true;
        }
    }
    private void PlayerController_playerDestroyed(object sender, GameObject e)
    {
        if (playerController.GetHealth() == 0)
        {
            StartCoroutine(LoadSceneInTime(0, 1));
            return;
        }
        healthText.text = "Health: " + playerController.GetHealth();
        StartCoroutine(SetActivePlayer());
    }
    IEnumerator SetActivePlayer()
    {
        playerPointRigid.gameObject.SetActive(true);
        playerPointRigid.AddTorque(200f);
        yield return new WaitForSeconds(.5f);

        playerController.particleSystem.Stop();
        playerController.gameObject.SetActive(false);
        playerController.transform.position = playerPoint.position;
        playerController.movePoint.position = playerPoint.position;
        yield return new WaitForSeconds(1.5f);

        playerController.GetComponent<Collider2D>().enabled = true;
        playerController.gameObject.SetActive(true);
        playerController.canMove = true;
        playerPointRigid.gameObject.SetActive(false);
    }

    int i;
    int i1;
    IEnumerator LoadSceneInTime(int id, float time)
    {
        if (id != 0)
        {
            if (playerController.canMoveOnWater)
                PlayerPrefs.SetInt("HaveBoat", 1);
            if (playerController.turboShooting)
                PlayerPrefs.SetInt("TurboShooting", 1);
            if (playerController.canDestroyBush)
                PlayerPrefs.SetInt("CanDestroyBush", 1);

        }
        yield return new WaitForSeconds(time);
        SceneManager.LoadScene(id);
    }

    private void GameManager_baseDestroyed(object sender, GameObject baseGO)
    {
        Destroy(baseGO);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Update()
    {
        int countEnemiesOnScene = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if (enemySpawnManager.GetCountEnemiesToSpawn() == 0 && countEnemiesOnScene == 0)
        {
            StartCoroutine(LoadSceneInTime(levelId, 5f));
        }
    }
}
