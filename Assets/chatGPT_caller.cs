using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using OpenAI;
using System.Collections;

public class ChatGPTCCaller : MonoBehaviour
{
    private Animator animator;
    private OpenAIApi openai = new OpenAIApi();
    public Button chatButton;
    public InputField userInput;
    public Text outputText;
    private ElevenLabsTTS elevenlabsTTS;
    Coroutine idleTimer;

    private List<ChatMessage> messages = new List<ChatMessage>();
    private string prompt = "Act as a helpful robot assistant at a conference hall. Answer questions and provide directions. Use keywords like 'idle' or 'nodding' to trigger animations.";
    public string mostRecentlyParsedAnimation;

    void Start()
    {
        // Get the Animator component attached to this GameObject
        animator = GetComponent<Animator>();

        // Add listener to the chat button
        chatButton.onClick.AddListener(OnChatButtonClicked);
        elevenlabsTTS = FindObjectOfType<ElevenLabsTTS>();
        if (elevenlabsTTS == null)
        {
            Debug.LogError("ElevenLabsTTS script not found.");
        }

    }

    public void StartTimerForReturningToIdle(float audioClipLength)
    {
        if(idleTimer != null)
        {
            StopCoroutine(idleTimer);
        }
        idleTimer = StartCoroutine(StopTalkingAfterClip(audioClipLength));
    }

    IEnumerator StopTalkingAfterClip(float clipLength)
    {
        Debug.Log("starting timer for " + clipLength + " seconds");
        yield return new WaitForSeconds(clipLength);
        TriggerIdle();
    }

    // Method to trigger the 'idle' animation
    public void TriggerIdle()
    {
        animator.SetTrigger("ReturnToIdle");
    }

    public void TriggerTalkingAnimation()
    {
        //using this for now to keep things simple.
        animator.Play("Talking");
        //when we want to get more complicated animations,
        //we'll likely want to store the most recently parsed animation instruction
        //and use it to play the correct and specific animation.
        //for example
        if(mostRecentlyParsedAnimation == "idle")
        {
            //animator.SetTrigger("ReturnToIdle")
        }
        else if(mostRecentlyParsedAnimation == "nodding")
        {
            //animator.Play("Nodding");
            //or whatever the relevant animation state is called.
        }
    }

    // Method to trigger the 'nodding' animation
    public void TriggerNodding()
    {
        //animator.SetTrigger("trigger-ph-nodding");
    }

    // Method called when the chat button is clicked
    private async void OnChatButtonClicked()
    {
        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content = userInput.text
        };

        if (messages.Count == 0) newMessage.Content = prompt + "\n" + userInput.text;

        messages.Add(newMessage);

        // Disable input while waiting for response
        chatButton.enabled = false;
        userInput.text = "";
        userInput.enabled = false;

        //start thinking animation
        animator.SetTrigger("StartThinking");

        // Get response from OpenAI
        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-4o-mini",
            Messages = messages
        });

        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            var message = completionResponse.Choices[0].Message;
            message.Content = message.Content.Trim();

            messages.Add(message);
            ParseAndTriggerAnimation(message.Content);

            // Display response in output text field
            outputText.text = message.Content;

            await elevenlabsTTS.PlayResponseAudio(message.Content);
        }
        else
        {
            Debug.LogWarning("No text was generated from this prompt.");
        }

        // Re-enable input
        chatButton.enabled = true;
        userInput.enabled = true;
    }

    // Parse the response from ChatGPT and trigger the corresponding animation
    private void ParseAndTriggerAnimation(string response)
    {

        //example of how we would store the chat response.
        //we don't want to trigger it immediately so that we can process the text-to-speech
        //we want to wait until the audio clip is processed before triggering the speech animation,
        //but when that happens we want to know which animation to play
        if (response.Contains("idle"))
        {
            mostRecentlyParsedAnimation = "idle";
        }
        else if (response.Contains("nodding"))
        {
            mostRecentlyParsedAnimation = "nodding";
        }
        else
        {
            Debug.Log("No matching trigger word found in response: " + response);
        }
        
    }
}
