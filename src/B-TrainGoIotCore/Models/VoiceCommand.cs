using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;

namespace B_TrainGoIotCore.Models
{
    enum EVoiceCommand
    {
        None,
        Start,
        Stop,
    }

    class VoiceCommandEventArgs : EventArgs
    {
        public EVoiceCommand Command { get; }

        public VoiceCommandEventArgs(EVoiceCommand command)
        {
            Command = command;
        }
    }

    class VoiceCommand
    {
        #region const

        private const string UriCommandGrammar = "Grammar\\CommandGrammar.xml";
        private const string SpeechTag = "cmd";
        private const string SpeechTagStart = "Start";

        #endregion

        #region field

        private readonly SpeechRecognizer _recognizer;

        private bool _isInitialized;

        private bool _isRunning;

        #endregion

        #region property

        private static CoreDispatcher Dispatcher => CoreApplication.MainView.Dispatcher;

        #endregion

        #region event

        public event EventHandler<VoiceCommandEventArgs> VoiceCommandReceived; 

        #endregion

        #region constructor

        public VoiceCommand()
        {
            _recognizer = new SpeechRecognizer(new Language("ja"));
            _recognizer.StateChanged += OnRecognizerStateChanged;
            _recognizer.ContinuousRecognitionSession.ResultGenerated += OnRecognizerGeneratedResult;
            _isInitialized = false;
        }

        #endregion

        #region method

        public async Task<bool> Initialize()
        {
            bool isInitializing = true;

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (_isInitialized)
                {
                    await _recognizer.StopRecognitionAsync();
                    _recognizer.Constraints.Clear();
                }

                var grammarFile = await Package.Current.InstalledLocation.GetFileAsync(UriCommandGrammar);
                var grammarConstraints = new SpeechRecognitionGrammarFileConstraint(grammarFile);
                _recognizer.Constraints.Add(grammarConstraints);

                var result = await _recognizer.CompileConstraintsAsync();

                _isInitialized = result.Status == SpeechRecognitionResultStatus.Success;

                if (_isInitialized)
                {
                    await _recognizer.ContinuousRecognitionSession.StartAsync(SpeechContinuousRecognitionMode
                        .PauseOnRecognition);
                }

                isInitializing = false;
            });

            while (isInitializing) await Task.Delay(100);

            return _isInitialized;
        }

        public async Task Pause()
        {
            if (!_isRunning) return;

            await _recognizer.ContinuousRecognitionSession.PauseAsync();
            _isRunning = false;
        }

        public void Resume()
        {
            if (_isRunning) return;

            _recognizer.ContinuousRecognitionSession.Resume();
            _isRunning = true;
        }

        private void OnRecognizerStateChanged(SpeechRecognizer recognizer, SpeechRecognizerStateChangedEventArgs e)
        {
        }

        private void OnRecognizerGeneratedResult(SpeechContinuousRecognitionSession session,
            SpeechContinuousRecognitionResultGeneratedEventArgs e)
        {
            if (!e.Result.SemanticInterpretation.Properties.ContainsKey(SpeechTag)) return;

            var tag = e.Result.SemanticInterpretation.Properties[SpeechTag].LastOrDefault();

            if (!tag.Equals(SpeechTagStart))
            {
                return;
            }

            VoiceCommandReceived?.Invoke(this, new VoiceCommandEventArgs(EVoiceCommand.Start));
        }

        #endregion
    }
}
