using System;
using System.Threading;
using Assessment2.Model;
using Assessment2.Model.Enums;
using Assessment2.Repository;
using Assessment2.Model;
using Assessment2.Model.Enums;
using Assessment2.Repository;

namespace Assessment2.Service
{
    /// <summary>
    /// Business logic service for boiler control operations.
    /// Handles all state transitions, phase execution, and event logging.
    /// </summary>
    internal class BoilerService
    {
        private readonly LogRepository _logRepository;
        private readonly Boiler _boiler;
        private readonly object _stateLock = new();
        private volatile bool _isSequenceRunning = false;

        /// <summary>
        /// Gets the current boiler status.
        /// </summary>
        public BoilerStatus Status => _boiler.Status;

        /// <summary>
        /// Gets the current interlock switch state.
        /// </summary>
        public SwitchState Interlock => _boiler.Interlock;

        /// <summary>
        /// Initializes a new instance of the BoilerService class.
        /// </summary>
        /// <param name="logRepository">The log repository for event logging.</param>
        /// <exception cref="ArgumentNullException">Thrown when logRepository is null.</exception>
        public BoilerService(LogRepository logRepository)
        {
            _logRepository = logRepository ?? throw new ArgumentNullException(nameof(logRepository));
            _boiler = new Boiler();
        }

        /// <summary>
        /// Initializes the boiler system.
        /// Creates log file and logs initial state.
        /// </summary>
        public void Init()
        {
            _logRepository.EnsureLogFileExists();
            LogEvent("Boiler Initialized", $"Status={_boiler.Status}, Interlock={_boiler.Interlock}");
        }

        /// <summary>
        /// Starts the boiler startup sequence.
        /// Executes Pre-Purge phase, Ignition phase, then transitions to Operational state.
        /// </summary>
        public void Start()
        {
            lock (_stateLock)
            {
                // Validate preconditions
                if (_boiler.Status != BoilerStatus.Ready)
                {
                    LogEvent("Start failed", $"Status is {_boiler.Status}, need Ready");
                    return;
                }

                if (_boiler.Interlock != SwitchState.Closed)
                {
                    LogEvent("Start failed", "Interlock must be Closed");
                    return;
                }

                if (_isSequenceRunning)
                {
                    LogEvent("Start failed", "Already running");
                    return;
                }

                _isSequenceRunning = true;
            }

            try
            {
                // Pre-Purge Phase (10 seconds)
                ExecutePhase("Pre-Purge", BoilerStatus.PrePurge, 10);
                if (_boiler.Status == BoilerStatus.Lockout)
                    return;

                LogEvent("Pre-Purge completed", "");

                // Ignition Phase (10 seconds)
                ExecutePhase("Ignition", BoilerStatus.Ignition, 10);
                if (_boiler.Status == BoilerStatus.Lockout)
                    return;

                LogEvent("Ignition phase completed", "");

                // Transition to Operational
                lock (_stateLock)
                {
                    _boiler.Status = BoilerStatus.Operational;
                }

                LogEvent("Boiler now operational", "");
            }
            finally
            {
                lock (_stateLock)
                {
                    _isSequenceRunning = false;
                }
            }
        }

        /// <summary>
        /// Stops the boiler and transitions to Lockout state.
        /// </summary>
        public void Stop()
        {
            lock (_stateLock)
            {
                if (_boiler.Status == BoilerStatus.Lockout)
                {
                    LogEvent("Stop failed", "Already Lockout");
                    return;
                }
                _boiler.Status = BoilerStatus.Lockout;
            }

            LogEvent("Boiler Stopped", "Transitioned to Lockout");
        }

        /// <summary>
        /// Simulates a boiler error (only allowed in Operational state).
        /// Transitions immediately to Lockout state.
        /// </summary>
        public void SimulateError()
        {
            lock (_stateLock)
            {
                if (_boiler.Status != BoilerStatus.Operational)
                {
                    LogEvent("Error simulation failed", "Only in Operational mode");
                    return;
                }
                _boiler.Status = BoilerStatus.Lockout;
            }

            LogEvent("Error", "User simulated error. System in Lockout.");
        }

        /// <summary>
        /// Toggles the Run Interlock Switch between Open and Closed states.
        /// </summary>
        public void ToggleInterlock()
        {
            lock (_stateLock)
            {
                _boiler.Interlock = _boiler.Interlock == SwitchState.Open
                    ? SwitchState.Closed
                    : SwitchState.Open;
            }

            LogEvent("Interlock Switch toggled", $"New state: {_boiler.Interlock}");
        }

        /// <summary>
        /// Attempts to reset the lockout state.
        /// Only succeeds if the Interlock switch is Closed.
        /// </summary>
        public void ResetLockout()
        {
            lock (_stateLock)
            {
                if (_boiler.Interlock != SwitchState.Closed)
                {
                    LogEvent("Reset failed", "Interlock must be Closed");
                    _logRepository.WriteLogEntry("Reset attempted", "Interlock open");
                    return;
                }
                _boiler.Status = BoilerStatus.Ready;
            }

            LogEvent("Boiler Status changed", "Ready");
        }

        /// <summary>
        /// Gets all logged events from the event log file.
        /// </summary>
        /// <returns>Array of log entries.</returns>
        public string[] GetLog()
        {
            return _logRepository.GetAllLogEntries();
        }

        /// <summary>
        /// Executes a phase of the boiler startup sequence.
        /// Displays countdown and allows user to press 'E' to simulate error.
        /// </summary>
        /// <param name="phaseName">Name of the phase (e.g., "Pre-Purge").</param>
        /// <param name="phaseStatus">The boiler status during this phase.</param>
        /// <param name="durationSeconds">Duration of the phase in seconds.</param>
        private void ExecutePhase(string phaseName, BoilerStatus phaseStatus, int durationSeconds)
        {
            lock (_stateLock)
            {
                _boiler.Status = phaseStatus;
            }

            Console.WriteLine($"\n{phaseName} started. Duration: {durationSeconds} seconds.");

            for (int secondsRemaining = durationSeconds; secondsRemaining > 0; secondsRemaining--)
            {
                Console.Write($"\r{phaseName} - {secondsRemaining} second(s) remaining. Press 'E' to simulate error.     ");
                Console.WriteLine("Press 'S' to stop.");
                DateTime phaseEndTime = DateTime.Now.AddSeconds(1);
                while (DateTime.Now < phaseEndTime)
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                        if (keyInfo.Key == ConsoleKey.E)
                        {
                            HandlePhaseError(phaseName);
                            return;
                        }
                        if (keyInfo.Key == ConsoleKey.S)
                        {
                            Stop();
                            return;
                        }
                    }
                    Thread.Sleep(50);
                }
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Handles an error that occurs during a phase.
        /// Transitions to Lockout state and logs the error.
        /// </summary>
        /// <param name="phaseName">Name of the phase where error occurred.</param>
        private void HandlePhaseError(string phaseName)
        {
            lock (_stateLock)
            {
                _boiler.Status = BoilerStatus.Lockout;
            }

            LogEvent("Error", $"Simulated during {phaseName}. System in Lockout.");
        }

        /// <summary>
        /// Logs an event to both console and file.
        /// Prints to console with [EVENT] prefix and writes to log file.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="eventData">Event data/details.</param>
        private void LogEvent(string eventName, string eventData)
        {
            Console.WriteLine($"[EVENT] {eventName}: {eventData}");
            _logRepository.WriteLogEntry(eventName, eventData);
        }
    }
}