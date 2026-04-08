using System;
using System.Threading;
using H3Engine.Core.Constants;

namespace H3Engine.Common
{
    /// <summary>
    /// Thread-safe progress reporter for map loading operations.
    /// Inspired by VCMI's Load::ProgressAccumulator pattern.
    /// Progress value ranges from 0.0f to 1.0f.
    /// </summary>
    public class LoadProgress
    {
        private int _currentStep;
        private int _totalSteps;
        private string _statusMessage;

        public LoadProgress()
        {
            _currentStep = 0;
            _totalSteps = 1;
            _statusMessage = string.Empty;
        }

        /// <summary>
        /// Set the total number of steps for the current phase.
        /// </summary>
        public void SetupSteps(int totalSteps)
        {
            Interlocked.Exchange(ref _totalSteps, Math.Max(totalSteps, 1));
            Interlocked.Exchange(ref _currentStep, 0);
        }

        /// <summary>
        /// Advance progress by one step.
        /// </summary>
        public void Step()
        {
            Interlocked.Increment(ref _currentStep);
        }

        /// <summary>
        /// Set a status message describing the current loading phase.
        /// </summary>
        public void SetStatus(string message)
        {
            _statusMessage = message;
        }

        /// <summary>
        /// Get current progress as a float from 0.0 to 1.0.
        /// </summary>
        public float Progress
        {
            get
            {
                int current = _currentStep;
                int total = _totalSteps;
                if (total <= 0) return 0f;
                return Math.Min((float)current / total, 1.0f);
            }
        }

        /// <summary>
        /// Get current status message.
        /// </summary>
        public string StatusMessage
        {
            get { return _statusMessage ?? string.Empty; }
        }

        /// <summary>
        /// Mark loading as fully complete.
        /// </summary>
        public void Finish()
        {
            Interlocked.Exchange(ref _currentStep, _totalSteps);
        }
    }
}


