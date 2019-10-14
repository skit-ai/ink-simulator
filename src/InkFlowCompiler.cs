using System;
using System.IO;

namespace Ink
{
    class InkFlowCompiler : Ink.IFileHandler
    {
        public string CompileFile(string filePath)
        {
            try
            {
                string flowString = File.ReadAllText(filePath);
                Runtime.Story story;
                Compiler compiler = new Compiler(flowString, new Compiler.Options {
                        sourceFilename = filePath,
                        errorHandler = OnError,
                        fileHandler = this
                    });
                story = compiler.Compile();
                return story.ToJson();
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not compile file '" + filePath + "'");
                Console.WriteLine(e.ToString());
                return "";
            }
        }

        public string ResolveInkFilename (string includeName)
        {
            var workingDir = Directory.GetCurrentDirectory();
            var fullRootInkPath = Path.Combine(workingDir, includeName);
            return fullRootInkPath;
        }

        public string LoadInkFileContents (string fullFilename)
        {
            return File.ReadAllText(fullFilename);
        }

        void OnError(string message, ErrorType errorType)
        {
            Console.WriteLine(message);
        }
    }
}
