using Ink;
using Ink.Runtime;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

class InkTurn
{
    public string id;
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
        var prx = new Regex(@"p:(\d\.?\d+)", RegexOptions.IgnoreCase);
        var allTagStr = String.Join(" ", story.currentTags);
        var match = prx.Match(allTagStr);

        if (match.Success)
        {
            Group g = match.Groups[1];
            return Double.Parse(g.Value);
        }
        else
        {
            return 1.0;
        }
    }

    static InkTurn CollectUserTurn(Story story, Random rng, string lastUserText = "")
    {
        int index = rng.Next(story.currentChoices.Count);
        var choice = story.currentChoices[index];
        story.ChooseChoiceIndex(index);
        story.Continue();
        double p = TurnProbability(story);

        // TODO: Try if tag can be picked using choiceToChoose.targetPath
        string currentText;
        if (rng.NextDouble() <= p)
        {
            if (choice.text.Trim() == "null")
            {
                currentText = lastUserText.Trim();
            }
            else
            {
                currentText = (lastUserText + " " + choice.text).Trim();
            }
            if (AtUserTurn(story))
            {
                if (currentText == "") {
                    currentText = "null";
                }
                return CollectUserTurn(story, rng, currentText);
            }
            else
            {
                return new InkTurn("U", currentText);
            }
        }
        else
        {
            return null;
        }
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
            bool dialogFailed = false;

            while (!AtStoryEnd(story))
            {
                if (AtMachineTurn(story))
                {
                    turn = CollectMachineTurn(story);
                    turn.id = did + ":" + turnIndex;
                    dialogs[did].Add(turn);
                    turnIndex++;
                }
                else
                {
                    if (AtUserTurn(story))
                    {
                        turn = CollectUserTurn(story, rng);
                        // The stars didn't align. This mechanism needs to
                        // improve though.
                        if (turn == null) {
                            dialogFailed = true;
                            break;
                        }
                        // We skip null turns and consider them as passes by the
                        // user.
                        if (turn.text != "null")
                        {
                            turn.id = did + ":" + turnIndex;
                            dialogs[did].Add(turn);
                            turnIndex++;
                        }
                    }
                }
            }
            story.ResetState();
            if (dialogFailed)
            {
                dialogs.Remove(did);
            }
            else
            {
                collectedDialogs++;
            }
        }

        var serializer = new JavaScriptSerializer();
        Console.WriteLine(serializer.Serialize(dialogs));
    }
}
