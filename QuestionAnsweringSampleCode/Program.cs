using Azure;
using Azure.AI.Language.QuestionAnswering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QuestionAnsweringSampleCode
{
    internal class Program
    {
        private static void Main()
        {
            Uri endpoint = new Uri("https://voxscribelanguageservice.cognitiveservices.azure.com/");
            AzureKeyCredential credential = new AzureKeyCredential("f9912326eba444d3a4eec9858baed7c4");
            QuestionAnsweringClient client = new QuestionAnsweringClient(endpoint, credential);
            string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string transcriptPath = Path.Combine(sCurrentDirectory, @"..\..\SampleFiles\SampleTranscript.txt");
            string questionPath = Path.Combine(sCurrentDirectory, @"..\..\SampleFiles\SampleQuestions.txt");
            string readTranscriptText = File.ReadAllText(Path.GetFullPath(transcriptPath));
            string[] readQuestionText = File.ReadAllLines(Path.GetFullPath(questionPath));

            IEnumerable<TextDocument> records = new[]
            {
                new TextDocument("doc1", readTranscriptText)
            };

            foreach (string question in readQuestionText)
            {
                AnswersFromTextOptions options = new AnswersFromTextOptions(question, records);
                Response<AnswersFromTextResult> response = client.GetAnswersFromText(options);

                foreach (TextAnswer answer in response.Value.Answers)
                {
                    if (answer.Confidence > .1)
                    {
                        string BestAnswer = response.Value.Answers[0].Answer;

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