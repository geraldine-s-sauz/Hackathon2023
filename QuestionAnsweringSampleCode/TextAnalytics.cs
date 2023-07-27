using Azure.AI.TextAnalytics;
using Azure;

namespace QuestionAnsweringSampleCode
{
    internal class TextAnalytics
    {
        public static void AnalyzeText(TextAnalyticsClient client, string document)
        {
            try
            {
                Response<KeyPhraseCollection> response = client.ExtractKeyPhrases(document);
                KeyPhraseCollection keyPhrases = response.Value;

                Console.WriteLine($"Extracted {keyPhrases.Count} key phrases:");
                foreach (string keyPhrase in keyPhrases)
                {
                    Console.WriteLine($"  {keyPhrase}");
                }
            }
            catch (RequestFailedException exception)
            {
                Console.WriteLine($"Error Code: {exception.ErrorCode}");
                Console.WriteLine($"Message: {exception.Message}");
            }
        }
    }
}