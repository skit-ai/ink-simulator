using Ink.Runtime;
using System;

class InkSimulator {

    static void runStory(Story story) {
        while (story.canContinue) {
            Console.WriteLine(story.Continue() + " :tags " + String.Join(" ", story.currentTags));
        }
    }

    static void Main(string[] args) {
        string text = System.IO.File.ReadAllText(args[0]);
        Story story = new Story(text);

        runStory(story);

        for (int i = 0; i < story.currentChoices.Count; ++i) {
            Choice choice = story.currentChoices [i];
            Console.WriteLine("Choice " + (i + 1) + ". " + choice.text);
        }
        story.ChooseChoiceIndex(0);
        runStory(story);
    }
}
