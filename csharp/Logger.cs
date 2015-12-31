//   Copyright 2013 Benjamin Burns
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Burnsba.Toolbox
{
    /// <summary>
    /// Logging helper class. Starts a thread that writes to a log file in the background. Log messages
    /// are received and added to the queue to write to file.
    /// </summary>
    public sealed class Logger : IDisposable
    {
        #region Fields

        /// <summary>
        /// Default thread waiting period, when nothing it happening
        /// </summary>
        private const int _defaultSleepMs = 100;

        /// <summary>
        /// Logging verbose level
        /// </summary>
        private readonly MessageVerbosity _verbosity;

        /// <summary>
        /// Path to log file
        /// </summary>
        private readonly string _logPath;

        /// <summary>
        /// Time format pre-pended to log messages
        /// </summary>
        private readonly string _timeFormat;

        /// <summary>
        /// Queue for messages to be sent to the log file
        /// </summary>
        private Queue<string> _logMessages = new Queue<string>();

        /// <summary>
        /// Lock for the log queue
        /// </summary>
        private Mutex _logLock = new Mutex();

        /// <summary>
        /// variable to nicely shutdown work thread
        /// </summary>
        private bool _timeToShutdown;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="verbosity">Only messages higher than this will be logged</param>
        /// <param name="logPath">Path to log file on disk</param>
        /// <param name="timeFormat">RFC822 time format by default</param>
        public Logger(MessageVerbosity verbosity, string logPath, string timeFormat = "ddd, dd MMM yyyy HH:mm:ss K")
        {
            _verbosity = verbosity;
            _logPath = logPath;
            _timeFormat = timeFormat;

            _timeToShutdown = false;

            // check that log exists, or create it
            if (!File.Exists(_logPath))
            {
                using (File.Create(_logPath)) { }
                // if the file couldn't be created (and _that_ didn't throw an exception), throw an exception
                if (!File.Exists(_logPath))
                    throw new Error.HelixException(Error.HelixExceptionSource.IoError, string.Format("Could not create log file: {0}", _logPath));
            }

            // start the log thread
            Thread t = new Thread(LogThreadWork);
            t.Name = "Logging thread (" + _logPath + ")";
            t.IsBackground = true;
            t.Start();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Path to log file
        /// </summary>
        public string LogPath
        {
            get
            {
                return _logPath;
            }
        }

        /// <summary>
        /// Time format pre-pended to log messages (default is RFC822 format)
        /// </summary>
        public string TimeFormat
        {
            get
            {
                return _timeFormat;
            }
        }

        /// <summary>
        /// Logging verbose level
        /// </summary>
        public MessageVerbosity Verbosity
        {
            get
            {
                return _verbosity;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sends message to the log file. An RFC822 timestamp is pre-pended to the message by default. 
        /// </summary>
        /// <param name="message">Text to send to log file</param>
        /// <param name="mv">Verbosity of message.</param>
        public void Log(string message, MessageVerbosity mv)
        {
            if (_verbosity == MessageVerbosity.Undefined || mv == MessageVerbosity.Undefined)
                return;

            if ((int)mv > (int)_verbosity)
                return;

            _logLock.WaitOne();

            _logMessages.Enqueue(DateTime.Now.ToString(_timeFormat) + "> " + message);

            _logLock.ReleaseMutex();
        }

        /// <summary>
        /// Convenience function to pass exceptions to the log file. Will also write the first InnerException
        /// if it exists
        /// </summary>
        /// <param name="ex"></param>
        public void LogException(Exception ex)
        {
            Log("Exception: " + ex.Message, MessageVerbosity.Verbose);
            if (ex.InnerException != null)
                Log("Inner exception: " + ex.InnerException.Message, MessageVerbosity.Verbose);
        }

        /// <summary>
        /// Background thread worker
        /// </summary>
        private void LogThreadWork()
        {
            while (!_timeToShutdown)
            {
                while (_logMessages.Count > 0)
                {
                    _logLock.WaitOne();

                    string m = _logMessages.Dequeue();

                    _logLock.ReleaseMutex();

                    using (StreamWriter sw = File.AppendText(_logPath))
                    {
                        sw.WriteLine(m);
                    }
                }

                // wait a bit before checking for more messages
                Thread.Sleep(_defaultSleepMs);
            }
        }

        /// <summary>
        /// Shutdown and clean up
        /// </summary>
        public void Dispose()
        {
            // Wait up to five seconds for the queue to finish writing log messages.
            // After five seconds, continue shut down.
            if (_logMessages.Count > 0)
            {
                int max_count = 50;
                while (max_count-- > 0)
                {
                    Thread.Sleep(_defaultSleepMs);

                    if (_logMessages.Count == 0)
                        break;
                }
            }

            // ask nicely for the thread to stop
            _timeToShutdown = true;
            Thread.Sleep(_defaultSleepMs * 2);

            // since the logging thread is a background thread, it will 
            // be forcibly aborted by the garbage collecter if it is still
            // running
        }

        #endregion
    }
}
