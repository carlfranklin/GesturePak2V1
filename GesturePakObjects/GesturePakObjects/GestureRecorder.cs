using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Speech.Recognition;
        
namespace GesturePak
{
    
    public class GestureRecorder
    {
        private Gesture Gesture = null;
        public List<Frame> Frames;
        private SpeechRecognitionEngine recognizer;
        private Grammar StartGrammar;

        public event EventHandler RecordingStarted;
        public event EventHandler RecordingStopped;

        public GestureRecorder()
        {
            Frames = new List<Frame>();
        }

        private bool _isrecording = false;
        public bool IsRecording
        {
            get { return _isrecording; }
        }

        public void StartSpeechRecognition()
        {
            recognizer = new SpeechRecognitionEngine();
            recognizer.SpeechRecognized += recognizer_SpeechRecognized;
            string[] phrases = { "Start Recording", "OK Stop" };
            StartGrammar = CreateGrammar(phrases);
            recognizer.LoadGrammar(StartGrammar);
            recognizer.SetInputToDefaultAudioDevice();
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }

        public void StopSpeechRecognition()
        {
            recognizer.RecognizeAsyncStop();
        }

        void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (IsPartOfSentence(e.Result)) return;

            if (e.Result.Text == "Start Recording")
                StartRecording();
            else
            {
                if (e.Result.Text == "OK Stop")
                {
                    StopRecording();
                }
            }
        }

        private bool IsPartOfSentence(RecognitionResult result)
        {
            foreach (var word in result.Words)
            {
                if (word.Text == "...")
                    return true;
            }
            return false;
        }

        public void StartRecording()
        {
            Frames = new List<Frame>();
            _isrecording = true;
            if (RecordingStarted != null)
                RecordingStarted(this, new EventArgs());
        }

        public Frame RecordFrame(Body Body)
        {
            if (!IsRecording)
                throw new Exception("You must call StartRecording first.");

            var Frame = new Frame(Body);
            Frames.Add(Frame);
            return Frame;
        }

        public Frame RecordFrame(Frame Frame)
        {
            if (!IsRecording)
                throw new Exception("You must call StartRecording first.");
            Frames.Add(Frame);
            return Frame;
        }

        public void StopRecording()
        {
            _isrecording = false;
            if (RecordingStopped != null)
                RecordingStopped(this, new EventArgs());
        }

        

        public Gesture GetRecordedGesture()
        {
            if (Frames.Count == 0)
                throw new Exception("Record some frames first.");

            Gesture = new Gesture();
            Gesture.Name = "New Gesture";

            foreach (Frame Frame in Frames)
            {
                Gesture.Frames.Add(Frame);    
            }
            Gesture.Renumber();

            return Gesture;
        }

        private Grammar CreateGrammar(string[] phrases)
        {
            Grammar g;

            var choices = new Choices(phrases);
            var beforeBuilder = new GrammarBuilder();
            beforeBuilder.AppendWildcard();
            var beforeKey = new SemanticResultKey("beforeKey", beforeBuilder);
            var afterBuilder = new GrammarBuilder();
            afterBuilder.AppendWildcard();
            var afterKey = new SemanticResultKey("afterKey", afterBuilder);
            var builder = new GrammarBuilder();
            builder.Culture = recognizer.RecognizerInfo.Culture;
            builder.Append(beforeBuilder);
            builder.Append(choices);
            builder.Append(afterBuilder);
            g = new Grammar(builder);

            return g;
        }
    }
}
