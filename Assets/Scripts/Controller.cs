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
    public float secTextFadeOut = 2.0f;
    public float secNumsFadeOut = 1.0f;
    public float secAnswer = 1.0f;  //Seconds for wait before FadeOut

    [Header("Counters Texts")]
    public Text rightText;
    public Text wrongText;
    int rightCount = 0;
    int wrongCount = 0;

    [Space]
    //Needs to change the delta Size for have enought space for all text 
    public int sizeDeltaX = 0;

    int correctNumber = -1;
    int correctIndex = -1;
    int lastIndex = -1;
    bool waitingAnswer = false;

    bool canTry = true;

    //Save the start Delta to avoid to pick out of the number and detect it
    Vector2 startSize;

    // Start is called before the first frame update
    void Start()
    {
        startSize = numbers[0].GetComponent<RectTransform>().sizeDelta;
        StartCoroutine(GenerateQuestion());
    }

    public void TextClicked(int index)
    {
        //Texts Can not be clicked unless the animations will finish;
        if (!waitingAnswer || index == lastIndex)
            return;

        //Right Answer
        if (index == correctIndex)
        {
            //Green number
            numbers[index].GetComponent<Text>().color = Color.green;
            //Avoid Pick more numbers
            waitingAnswer = false;

            //Set true to start new question
            for (int i = 0; i < numbers.Length; i++)
            {
                //FadeOut the others numbers if they still in the screen
                if (i != index && i != lastIndex)
                    StartCoroutine(FadeOutAnim(numbers[i], secNumsFadeOut, secAnswer));
            }
            StartCoroutine(FadeOutAnim(numbers[index], secNumsFadeOut, secAnswer, true));

            rightCount++;
            rightText.text = "Encerts: " + rightCount.ToString();
        }
        else
        {
            //Red Wrong Answer
            numbers[index].GetComponent<Text>().color = Color.red;
            if (canTry)
            {
                StartCoroutine(FadeOutAnim(numbers[index], secNumsFadeOut, secAnswer));
                lastIndex = index;
            }
            else
            {
                waitingAnswer = false;
                //Green correct number
                numbers[correctIndex].GetComponent<Text>().color = Color.green;
                StartCoroutine(FadeOutAnim(numbers[index], secNumsFadeOut,secAnswer));
                StartCoroutine(FadeOutAnim(numbers[correctIndex], secNumsFadeOut, secAnswer, true));
            }

            canTry = false;
            wrongCount++;
            wrongText.text = "Errades: " + wrongCount.ToString();
        }

    }

    IEnumerator GenerateQuestion()
    {
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
        yield return FadeOutAnim(numbers[0], secTextFadeOut);

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
    /*
    Velocity animation -> secFadeOut
    Delay if we want to wait for something before starts the animation -> waitBefore
    Create a new question after the anim ends (if "isLastAnim" is true)
    */
    IEnumerator FadeOutAnim(GameObject number, float secFadeOut, float waitBefore = 0.0f, bool isLastAnim = false)
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
        number.GetComponent<Text>().color = Color.black;
        number.SetActive(false);

        if (isLastAnim)
            ResetQuestion();
    }
    //Set the Start Values before Generate again a question
    private void ResetQuestion()
    {
        numbers[0].SetActive(true);
        canTry = true;
        lastIndex = -1;
        StartCoroutine(GenerateQuestion());
    }
}
