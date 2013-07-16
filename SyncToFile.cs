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
using System.ComponentModel;
using System.IO;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Burnsba.Toolbox
{
    public interface IWatchable
    {
        event EventHandler Changed;
    }
    public interface ISerializable
    {
        XElement ToXElement();
    }
    public interface ISerializeWatchable : ISerializable, IWatchable
    {
    }
	
    /// <summary>
    /// This class is used to watch another object of type ISerializeWatchable. When that object fires
    /// the Changed event, this class will queue a task to write to disk. Disk writes are delayed by at least 
    /// _minTimeToSave between write attempts. All contents of the file are overwritten on the write.
    /// </summary>
    public class SyncToFile
    {
        #region Fields

        #region Constants
        // default time to wait between save requests
        private const int _minDayDefault = 0;
        private const int _minHourDefault = 0;
        private const int _minMinDefault = 1;
        private const int _minSecDefault = 0;
        private const int _minMsDefault = 0;
        #endregion

        /// <summary>
        /// Item to watch; when the change event is fired, a disk write is queued
        /// </summary>
        private ISerializeWatchable _itemToWatch;

        /// <summary>
        /// Flag to know whether to update the journal
        /// </summary>
        private bool _isDirty = false;

        /// <summary>
        /// Flag if item changed while it was being written to disk
        /// </summary>
        private bool _queuedDirty = false;

        /// <summary>
        /// Last time the journal was written to disk
        /// </summary>
        private DateTime _lastSaveTime = DateTime.MinValue;

        /// <summary>
        /// Minimum time to wait before writing journal to disk again
        /// </summary>
        private readonly TimeSpan _minTimeToSave;

        /// <summary>
        /// Threading object for lock
        /// </summary>
        private object _lockDirty = new object();
        private object _lockSaving = new object();

        /// <summary>
        /// Make sure two threads aren't trying to save at the same time
        /// </summary>
        private bool _currentlySaving = false;

        /// <summary>
        /// Suspend or resume the ability to write to disk
        /// </summary>
        private bool _acceptRequests = true;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates SyncToFile object
        /// </summary>
        /// <param name="itemToWatch">Object with Changed event</param>
        /// <param name="filename">Path of file to write to</param>
        /// <param name="minTimeToSave">Minimum amount of time to wait between writing to disk after Changed event</param>
        public SyncToFile(ISerializeWatchable itemToWatch, string filename, TimeSpan minTimeToSave)
        {
            _itemToWatch = itemToWatch;
            Filename = filename;
            ConstructorHelper();

            _minTimeToSave = minTimeToSave;
        }

        /// <summary>
        /// Creates SyncToFile object
        /// </summary>
        /// <param name="itemToWatch">Object with Changed event</param>
        /// <param name="filename">Path of file to write to</param>
        /// <param name="minHour">Minimum number of hours to wait between writing to disk after Changed event</param>
        /// <param name="minMinute">Minimum number of minutes to wait between writing to disk after Changed event</param>
        /// <param name="minSeconds">Minimum number of seconds to wait between writing to disk after Changed event</param>
        public SyncToFile(ISerializeWatchable itemToWatch, string filename,
                int minHour = _minHourDefault, int minMinute = _minMinDefault, int minSeconds = _minSecDefault)
        {
            _itemToWatch = itemToWatch;
            Filename = filename;
            ConstructorHelper();

            _minTimeToSave = new TimeSpan(minHour, minMinute, minSeconds);
        }

        /// <summary>
        /// Creates SyncToFile object
        /// </summary>
        /// <param name="itemToWatch">Object with Changed event</param>
        /// <param name="filename">Path of file to write to</param>
        /// <param name="minDay">Minimum number of days to wait between writing to disk after Changed event</param>
        /// <param name="minHour">Minimum number of hours to wait between writing to disk after Changed event</param>
        /// <param name="minMinute">Minimum number of minutes to wait between writing to disk after Changed event</param>
        /// <param name="minSeconds">Minimum number of seconds to wait between writing to disk after Changed event</param>
        /// <param name="minMs">Minimum number of milliseconds to wait between writing to disk after Changed event</param>
        public SyncToFile(ISerializeWatchable itemToWatch, string filename,
                int minDay = _minDayDefault, int minHour = _minHourDefault, int minMinute = _minMinDefault, int minSeconds = _minSecDefault, int minMs = _minMsDefault)
        {
            _itemToWatch = itemToWatch;
            Filename = filename;
            ConstructorHelper();

            _minTimeToSave = new TimeSpan(minDay, minHour, minMinute, minSeconds, minMs);
        }

        /// <summary>
        /// Constructor helper for common tasks. Hooks event handler
        /// </summary>
        private void ConstructorHelper()
        {
            if (_itemToWatch == null)
                throw new ArgumentNullException("itemToWatch");

            _itemToWatch.Changed += SyncToFile_ItemChanged;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Path to file that is used to serialize object
        /// </summary>
        public string Filename
        {
            get;
            private set;
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Reads a file assuming it is XML
        /// </summary>
        /// <param name="filename">Path to file</param>
        /// <returns>XElement containing the contents of the file</returns>
        public static XElement Load(string filename)
        {
            if (filename == "")
                throw new ArgumentException("Can not load object: Filename not set");
            if (!File.Exists(filename))
                throw new ArgumentException("File does not exist: " + filename);

            TextReader reader = null;
            string text;
            try
            {
                reader = new StreamReader(filename);
                text = reader.ReadToEnd();
            }
            catch //(Exception ex)
            {
                //throw ex;
                text = "empty";
            }
            finally
            {
                reader.Close();
            }

            XElement loadFrom = null;
            try
            {
                loadFrom = XElement.Parse(text);
            }
            catch 
            {
                // don't abort on this exception
                loadFrom = new XElement("empty");
            }

            return loadFrom;
        }

        /// <summary>
        /// Forces the saving of the item using XML. Preferred method is to trigger Changed event
        /// </summary>
        public void ForceSave()
        {
            Save();
        }

        /// <summary>
        /// All incoming change events will be ignored
        /// </summary>
        public void Suspend()
        {
            _acceptRequests = false;
        }

        /// <summary>
        /// All incoming change events will queue a disk write
        /// </summary>
        public void Resume()
        {
            _acceptRequests = true;
        }

        /// <summary>
        /// Clears the flag to reset any pending disk writes
        /// </summary>
        public void ClearFlag()
        {
            lock (_lockDirty)
            {
                _isDirty = false;
                _queuedDirty = false;
            }
        }

        /// <summary>
        /// Convenience function to clear the dirty flag and accept new change events
        /// to write the object to disk
        /// </summary>
        public void ClearFlageAndResume()
        {
            ClearFlag();
            Resume();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Saves the state of the item to Filename using XML
        /// </summary>
        private void Save()
        {
            lock (_lockSaving)
            {
                if (_currentlySaving == true)
                    return;
                _currentlySaving = true;
            }

            TextWriter writer = null;
            try
            {
                writer = new StreamWriter(Filename);
                string toWrite = _itemToWatch.ToXElement().ToString();
                writer.Write(toWrite);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                writer.Close();
                lock (_lockSaving)
                {
                    _currentlySaving = false;
                }
            }
        }

        /// <summary>
        /// Checks that enough time has elapsed then saves the object
        /// </summary>
        private void CheckAndSave()
        {
            // but don't update too frequently
            TimeSpan difference = DateTime.Now.Subtract(_lastSaveTime);
            if (difference < _minTimeToSave)
            {
                Thread.Sleep(_minTimeToSave.Subtract(difference));
            }

            // check if the queue was cleared while waiting
            lock (_lockDirty)
            {
                if (_isDirty == false && _queuedDirty == false)
                    return;
            }

            // save first ...
            this.Save();
            // then update items ...
            _lastSaveTime = DateTime.Now;
            lock (_lockDirty)
            {
                _isDirty = false;

                // check if the item changed while or after it was being written to disk
                if (_queuedDirty)
                {
                    _isDirty = true;
                    System.Threading.Tasks.Task.Factory.StartNew(() => { CheckAndSave(); }, TaskCreationOptions.LongRunning);
                    _queuedDirty = false;
                }
            }

        }

        /// <summary>
        /// Event handler. Queues the item to be saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SyncToFile_ItemChanged(object sender, EventArgs e)
        {
            if (_acceptRequests == false)
                return;

            lock (_lockDirty)
            {
                if (_isDirty == false)
                {
                    // schedule a new task to run, and put it on its own thread
                    _isDirty = true;
                    System.Threading.Tasks.Task.Factory.StartNew(() => { CheckAndSave(); }, TaskCreationOptions.LongRunning);                    
                }
                else
                {
                    // change occured while writing or waiting to write to disk
                    _queuedDirty = true;
                }
            }
        }

        #endregion

        #endregion
    }
}
