using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    public string[] allNumbers;
    public GameObject[] numbers;

    [Header("Transitions Timers")]
    public float secInScreen = 4.0f; //2 animation secs + 2 seconds more
    public float secFadeOut = 2.0f;
    public float secAnswer = 1.0f;  //Seconds for wait before FadeOut

    [Space]
    //Needs to change the delta Size for have enought space for all text 
    public int sizeDeltaX;

    int correctNumber;
    int correctIndex;
    int lastIndex = -1;
    bool waitingAnswer = false;

    bool canTry = true;

    //Save the start Delta to avoid to pick out of the number and detect it
    Vector2 startSize;
    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        startSize = numbers[0].GetComponent<RectTransform>().sizeDelta;
        StartCoroutine(GenerateQuestion());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TextClicked(int index)
    {
        //Texts Can not be clicked unless the animations will finish;
        if (!waitingAnswer || index == lastIndex)
            return;

        if (index == correctIndex)
        {
            Debug.Log("Correct");
            numbers[index].GetComponent<Text>().color = Color.green;
            EndQuestion(true);
        }
        else if (canTry)
        {
            numbers[index].GetComponent<Text>().color = Color.red;
            StartCoroutine(FadeOutAnim(numbers[index], secAnswer));
            lastIndex = index;
            canTry = false;
        }
        else
        {
            waitingAnswer = false;
            EndQuestion(false);
        }
    }

    private void EndQuestion(bool isCorrect)
    {
        for (int i = 0; i < numbers.Length; i++)
        {
            if (numbers[i].activeSelf)
            {
                if (i == correctIndex)
                    numbers[i].GetComponent<Text>().color = Color.green;
                else if(!isCorrect)
                    numbers[i].GetComponent<Text>().color = Color.red;
                StartCoroutine(FadeOutAnim(numbers[i], secAnswer));
            }
        }
    }

    IEnumerator GenerateQuestion()
    {
        //Set it to true to avoid pick the same number twice
        waitingAnswer = false;
        //Generate number & print it
        correctNumber = Random.Range(0, allNumbers.Length);
        numbers[0].GetComponent<Text>().text = allNumbers[correctNumber];
        //Needs to change the delta Size for have enought space for all text 
        numbers[0].GetComponent<RectTransform>().sizeDelta = new Vector2 (sizeDeltaX, startSize.y);
        //Start Anim
        numbers[0].GetComponent<Animator>().Play("FadeIn");
        //Wait the enunciated seconds (4sec)
        yield return new WaitForSeconds(secInScreen);

        //Start Fade out Routine
        yield return FadeOutAnim(numbers[0]);

        //Set the correct number in one text
        correctIndex = Random.Range(0, numbers.Length);
        int lastNum = correctNumber;

        for (int i = 0; i < numbers.Length; i++)
        {
            int newNumber = correctNumber;
            //Generate the other two unique numbers 
            while (i != correctIndex && (newNumber == correctNumber || newNumber == lastNum))
            {
                newNumber = Random.Range(0, allNumbers.Length);
            }

            if (i != correctIndex)
                lastNum = newNumber;
            //Set the start Delta to avoid to pick out of the number and detect it
            else
                numbers[0].GetComponent<RectTransform>().sizeDelta = startSize;


            //Set active the three texts, set the numbers to text & start animation
            numbers[i].SetActive(true);
            numbers[i].GetComponent<Text>().text = newNumber.ToString();
            //We should check the Animator isn't null. But here it's imposible
            numbers[i].GetComponent<Animator>().Play("FadeIn");
        }

        yield return new WaitForSeconds(2.0f);
        //After the animation ends the user can chose the correct answer
        waitingAnswer = true;
    }

    //Create a fade out animation routine for all GO
    IEnumerator FadeOutAnim(GameObject number, float waitBefore = 0.0f)
    {
        yield return new WaitForSeconds(waitBefore);
        //Get parent for future resize
        Transform panel = number.transform.parent;
        float t = 0;
        //Animation with code (2sec) fading out with the panel size
        while (t < secFadeOut)
        {
            float scale = Mathf.Lerp(1, 0, t / secFadeOut);
            panel.localScale = new Vector3(scale, scale, scale);
            t += Time.deltaTime;
            yield return null;
        }
        //desactive object & set the parent with correct size
        panel.localScale = Vector3.one;
        number.SetActive(false);
    }
}
