using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MovingObject
{

    public int wallDamage = 1;
    public int pointsPerFood = 10;
    public int pointsPerSoda = 20;
    public float restartLevelDelay = 1f;
    public Text foodText;
    //public AudioClip moveSound1;
    //public AudioClip moveSound2;
    //public AudioClip eatSound1;
    //public AudioClip eatSound2;
    //public AudioClip drinkSound1;
    //public AudioClip drinkSound2;
    //public AudioClip gameOverSound;

    private Animator animator;
    private int food;
    //Adding variables for touchscreen in iOS
    private Vector2 touchOrigin = -Vector2.one;

    // Use this for initialization
    protected override void Start()
    {
        animator = GetComponent<Animator>();


        food = GameManager.instance.playerFoodPoints;

        foodText.text = "Food: " + food;

        base.Start();
    }

    //To show how the food vary throughout the levels, and so on
    private void OnDisable()
    {
        GameManager.instance.playerFoodPoints = food;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.playersTurn) return;

        //To store the position in the game, as we move
        int horizontal = 0;
        int vertical = 0;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER

        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");

        if (horizontal != 0)
            vertical = 0;

#else
        if(Input.TouchCount>0){
            Touch myTouch = Input.touches[0];

            if(myTouch.phase == TouchPhase.Began){
                touchOrigin = myTouch.position;
            }

            elseif(myTouch.phase == TouchPhase.Ended && touchOrigin.x>=0){
                Vector2 touchEnd = myTouch.position;
                float x = touchEnd.x - touchOrigin.x;
                float y = touchEnd.y - touchOrigin.y;
                touchOrigin.x = -1;

                if (Mathf.Abs(x) > Mathf.Abs(y))
                        //If x is greater than zero, set horizontal to 1, otherwise set it to -1
                        horizontal = x > 0 ? 1 : -1;   
                else
                        //If y is greater than zero, set horizontal to 1, otherwise set it to -1
                        vertical = y > 0 ? 1 : -1;
            }
        }

#endif
        if (horizontal != 0 || vertical != 0)
            AttemptMove<Wall>(horizontal, vertical);
    }

    protected override void AttemptMove<T>(int xDir, int yDir)
    {
        //Everytime the player moves, he will lose 1 point of food
        food--;
        foodText.text = "Food: " + food;
        base.AttemptMove<T>(xDir, yDir);
        RaycastHit2D hit;
        if (Move(xDir, yDir, out hit))
        {
            //Call RandomizeSfx of SoundManager to play the move sound, passing in two audio clips to choose from
            //SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
        }
        CheckIfGameOver();
        GameManager.instance.playersTurn = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        switch (other.tag)
        {
            case "Exit":
                Invoke("Restart", restartLevelDelay);
                enabled = false;
                break;
            case "Food":
                food = food + pointsPerFood;
                foodText.text = "+" + pointsPerFood + "Food: " + food;
                //SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
                other.gameObject.SetActive(false);
                break;
            case "Soda":
                food = food + pointsPerSoda;
                foodText.text = "+" + pointsPerSoda + "Food: " + food;
                //SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
                other.gameObject.SetActive(false);
                break;
        }
    }

    private void CheckIfGameOver()
    {
        if (food <= 0)
        {
           // SoundManager.instance.RandomizeSfx(gameOverSound);
            //SoundManager.instance.musicSource.Stop();
            GameManager.instance.GameOver();
        }
    }

    protected override void OnCantMove<T>(T component)
    {
        //Casting to a Wall (as)
        Wall hitWall = component as Wall;
        hitWall.DamageWall(wallDamage);
        //In Animator we declared two Trigger paramteres, called playerHit and playerChop
        animator.SetTrigger("playerChop");
    }

    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoseFood(int loss)
    {
        //In Animator we declared two Trigger paramteres, called playerHit and playerChop
        animator.SetTrigger("playerHit");
        food = food - loss;
        foodText.text = "-" + loss + "Food: " + food;
        CheckIfGameOver();
    }
}