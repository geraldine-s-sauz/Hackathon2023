using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Speaker;
using Microsoft.CognitiveServices.Speech;

namespace SpeechToTextSampleCode
{
    internal class SpeakerRecognition
    {
        static string currentDirectory = Environment.CurrentDirectory;
        static string wavFile = Path.Combine(currentDirectory, @"SampleFiles\Call1_separated_16k_health_insurance.wav");

        public static async Task VerificationEnrollTextDependent(SpeechConfig config, Dictionary<string, string> profileMapping)
        {
            using (var client = new VoiceProfileClient(config))
            using (var profile = await client.CreateProfileAsync(VoiceProfileType.TextDependentVerification, "en-us"))
            {
                var phraseResult = await client.GetActivationPhrasesAsync(VoiceProfileType.TextDependentVerification, "en-us");
                using (var audioInput = AudioConfig.FromWavFileInput(wavFile))
                {
                    Console.WriteLine($"Enrolling profile id {profile.Id}.");
                    // give the profile a human-readable display name
                    profileMapping.Add(profile.Id, "Your Name");

                    VoiceProfileEnrollmentResult result = null;
                    while (result is null || result.RemainingEnrollmentsCount > 0)
                    {
                        Console.WriteLine($"Speak the passphrase, \"${phraseResult.Phrases[0]}\"");
                        result = await client.EnrollProfileAsync(profile, audioInput);
                        Console.WriteLine($"Remaining enrollments needed: {result.RemainingEnrollmentsCount}");
                        Console.WriteLine("");
                    }

                    if (result.Reason == ResultReason.EnrolledVoiceProfile)
                    {
                        await SpeakerVerify(config, profile, profileMapping);
                    }
                    else if (result.Reason == ResultReason.Canceled)
                    {
                        var cancellation = VoiceProfileEnrollmentCancellationDetails.FromResult(result);
                        Console.WriteLine($"CANCELED {profile.Id}: ErrorCode={cancellation.ErrorCode} ErrorDetails={cancellation.ErrorDetails}");
                    }
                }
            }
        }
        public static async Task VerificationEnrollTextIndependent(SpeechConfig config, Dictionary<string, string> profileMapping)
        {
            using (var client = new VoiceProfileClient(config))
            using (var profile = await client.CreateProfileAsync(VoiceProfileType.TextIndependentVerification, "en-us"))
            {
                var phraseResult = await client.GetActivationPhrasesAsync(VoiceProfileType.TextIndependentVerification, "en-us");
                using (var audioInput = AudioConfig.FromWavFileInput(wavFile))
                {
                    Console.WriteLine($"Enrolling profile id {profile.Id}.");
                    // give the profile a human-readable display name
                    profileMapping.Add(profile.Id, "Your Name");

                    VoiceProfileEnrollmentResult result = null;
                    while (result is null || result.RemainingEnrollmentsSpeechLength > TimeSpan.Zero)
                    {
                        Console.WriteLine($"Speak the activation phrase, \"${phraseResult.Phrases[0]}\"");
                        result = await client.EnrollProfileAsync(profile, audioInput);
                        Console.WriteLine($"Remaining enrollment audio time needed: {result.RemainingEnrollmentsSpeechLength}");
                        Console.WriteLine("");
                    }

                    if (result.Reason == ResultReason.EnrolledVoiceProfile)
                    {
                        await SpeakerVerify(config, profile, profileMapping);
                    }
                    else if (result.Reason == ResultReason.Canceled)
                    {
                        var cancellation = VoiceProfileEnrollmentCancellationDetails.FromResult(result);
                        Console.WriteLine($"CANCELED {profile.Id}: ErrorCode={cancellation.ErrorCode} ErrorDetails={cancellation.ErrorDetails}");
                    }
                }
            }
        }

        public static async Task SpeakerVerify(SpeechConfig config, VoiceProfile profile, Dictionary<string, string> profileMapping)
        {
            var speakerRecognizer = new SpeakerRecognizer(config, AudioConfig.FromWavFileInput(wavFile));
            var model = SpeakerVerificationModel.FromProfile(profile);

            Console.WriteLine("Speak the passphrase to verify: \"My voice is my passport, please verify me.\"");
            var result = await speakerRecognizer.RecognizeOnceAsync(model);
            Console.WriteLine($"Verified voice profile for speaker {profileMapping[result.ProfileId]}, score is {result.Score}");
        }

        public static async Task<List<VoiceProfile>> IdentificationEnroll(SpeechConfig config, List<string> profileNames, Dictionary<string, string> profileMapping)
        {
            List<VoiceProfile> voiceProfiles = new List<VoiceProfile>();
            using (var client = new VoiceProfileClient(config))
            {
                var phraseResult = await client.GetActivationPhrasesAsync(VoiceProfileType.TextIndependentVerification, "en-us");
                foreach (string name in profileNames)
                {
                    var speakerRecognizer = new SpeakerRecognizer(config, AudioConfig.FromWavFileInput(wavFile));

                    using (var audioInput = AudioConfig.FromWavFileInput(wavFile))
                    {
                        var profile = await client.CreateProfileAsync(VoiceProfileType.TextIndependentIdentification, "en-us");
                        Console.WriteLine($"Creating voice profile for {name}.");
                        profileMapping.Add(profile.Id, name);

                        VoiceProfileEnrollmentResult result = null;
                        while (result is null || result.RemainingEnrollmentsSpeechLength > TimeSpan.Zero)
                        {
                            Console.WriteLine($"Speak the activation phrase, \"${phraseResult.Phrases[0]}\" to add to the profile enrollment sample for {name}.");
                            result = await client.EnrollProfileAsync(profile, audioInput);
                            Console.WriteLine($"Remaining enrollment audio time needed: {result.RemainingEnrollmentsSpeechLength}");
                            Console.WriteLine("");
                        }
                        voiceProfiles.Add(profile);
                    }
                }
            }
            return voiceProfiles;
        }

        public static async Task SpeakerIdentification(SpeechConfig config, List<VoiceProfile> voiceProfiles, Dictionary<string, string> profileMapping)
        {
            var speakerRecognizer = new SpeakerRecognizer(config, AudioConfig.FromWavFileInput(wavFile));
            var model = SpeakerIdentificationModel.FromProfiles(voiceProfiles);

            Console.WriteLine("Speak some text to identify who it is from your list of enrolled speakers.");
            var result = await speakerRecognizer.RecognizeOnceAsync(model);
            Console.WriteLine($"The most similar voice profile is {profileMapping[result.ProfileId]} with similarity score {result.Score}");
        }
    }
}