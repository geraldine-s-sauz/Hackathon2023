using Azure;
using Azure.AI.Language.QuestionAnswering;

namespace QuestionAnsweringSampleCode
{
    class Program
    {
        private static readonly Uri endpoint = new Uri("https://eastasialanguageservice.cognitiveservices.azure.com/");
        private static readonly AzureKeyCredential credential = new AzureKeyCredential("385f523ff29c4903bf421af38eff988f");

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

                        Console.WriteLine($"Q:{options.Question}");
                        Console.WriteLine($"A:{BestAnswer}");
                        Console.WriteLine($"Confidence Score: ({response.Value.Answers[0].Confidence:P2})"); //:P2 converts the result to a percentage with 2 decimals of accuracy. 
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