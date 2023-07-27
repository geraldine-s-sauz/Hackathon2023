using Azure;
using Azure.AI.Language.QuestionAnswering;
using Azure.AI.TextAnalytics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QuestionAnsweringSampleCode
{
    class Program
    {
        private static readonly Uri endpoint = new Uri("https://voxscribelanguageservice.cognitiveservices.azure.com/");
        private static readonly AzureKeyCredential credential = new AzureKeyCredential("f9912326eba444d3a4eec9858baed7c4");

        static void Main(string[] args)
        {
            QuestionAnsweringClient qaClient = new QuestionAnsweringClient(endpoint, credential);
            string currentDirectory = Environment.CurrentDirectory;
            string transcriptPath = Path.Combine(currentDirectory, @"SampleFiles\SampleTranscript.txt");
            string questionPath = Path.Combine(currentDirectory, @"SampleFiles\SampleQuestions.txt");
            string readTranscriptText = File.ReadAllText(Path.GetFullPath(transcriptPath));
            string[] readQuestionText = File.ReadAllLines(Path.GetFullPath(questionPath));

            IEnumerable<TextDocument> records = new[]
            {
                new TextDocument("doc", readTranscriptText)
            };

            foreach (string question in readQuestionText)
            {
                AnswersFromTextOptions options = new AnswersFromTextOptions(question, records);
                Response<AnswersFromTextResult> response = qaClient.GetAnswersFromText(options);

                foreach (TextAnswer answer in response.Value.Answers)
                {
                    if (answer.Confidence > .1)
                    {
                        string BestAnswer = response.Value.Answers[0].ShortAnswer.Text;

                        Console.WriteLine($"Q{question.Count()}:{options.Question}");
                        Console.WriteLine($"A{question.Count()}:{BestAnswer}");
                        Console.WriteLine($"Confidence Score: ({response.Value.Answers[0].Confidence:P2})"); //:P2 converts the result to a percentage with 2 decimals of accuracy.
                        Console.WriteLine("\b");
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"Q:{options.Question}");
                        Console.WriteLine("No answers met the requested confidence score.");
                        break;
                    }
                }
            }
        }      
    }
}