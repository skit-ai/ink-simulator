using Ink;
using Ink.Runtime;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

class InkTurn
{
    public string tid;
    public string type;
    public string text;

    public InkTurn(string typeStr, string textStr)
    {
        type = typeStr;
        text = textStr;
    }
}

class InkSimulator
{
    static InkTurn CollectMachineTurn(Story story)
    {
        var text = story.ContinueMaximally().Trim();
        return new InkTurn("M", Regex.Replace(text, @"\s+", " "));
    }

    static double TurnProbability(Story story)
    {
        var regex = new Regex(@"p:(\d\.?\d+)");
        // story.currentTags;
        return 1.0;
    }

    static InkTurn CollectUserTurn(Story story, Random rng)
    {
        int index = rng.Next(story.currentChoices.Count);
        var choice = story.currentChoices[index];
        story.ChooseChoiceIndex(index);

        // TODO: Try if tag can be picked using choiceToChoose.targetPath

        // We skip user selected text for now.
        story.Continue();
        return new InkTurn("U", choice.text.Trim());
    }

    static bool AtMachineTurn(Story story)
    {
        return story.canContinue;
    }

    static bool AtUserTurn(Story story)
    {
        return (!story.canContinue) && (story.currentChoices.Count > 0);
    }

    static bool AtStoryEnd(Story story)
    {
        return (!story.canContinue) && (story.currentChoices.Count == 0);
    }

    static string GenerateDialogID()
    {
        return DateTime.UtcNow.Ticks.ToString();
    }

    // Usage: <ink-file> <n>
    static void Main(string[] args)
    {
        var ifc = new InkFlowCompiler();
        var text = ifc.CompileFile(args[0]);
        int n = Int32.Parse(args[1]);
        var story = new Story(text);
        var rng = new Random();

        // Map from dialog id to turns
        var dialogs = new Dictionary<string, List<InkTurn>>();

        InkTurn turn;
        int collectedDialogs = 0;
        while (collectedDialogs < n)
        {
            var did = GenerateDialogID();
            dialogs.Add(did, new List<InkTurn>());
            int turnIndex = 0;

            while (!AtStoryEnd(story))
            {
                if (AtMachineTurn(story))
                {
                    turn = CollectMachineTurn(story);
                    turn.tid = did + ":" + turnIndex;
                    dialogs[did].Add(turn);
                    turnIndex++;
                }
                else
                {
                    if (AtUserTurn(story))
                    {
                        turn = CollectUserTurn(story, rng);
                        // We skip null turns and consider them as passes by the
                        // user.
                        if (turn.text != "null")
                        {
                            turn.tid = did + ":" + turnIndex;
                            dialogs[did].Add(turn);
                            turnIndex++;
                        }
                    }
                }
            }
            story.ResetState();
            collectedDialogs++;
        }

        var serializer = new JavaScriptSerializer();
        Console.WriteLine(serializer.Serialize(dialogs));
    }
}
