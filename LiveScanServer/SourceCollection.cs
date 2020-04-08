﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace KinectServer
{
    public class SourceCollection
    {
        private ConcurrentDictionary<int, Source> sources;

        public event SourceConnectedEventHandler SourceConnected;
        public event SourceDisconnectedEventHandler SourceDisconnected;

        private Thread cleanerThread;
        private bool cleanerToCancel = false;
        public SourceCollection()
        {
            sources = new ConcurrentDictionary<int, Source>();
            CleanerInterval = 5000;
            CleanerThreshold = 5;
        }

        public int CleanerInterval { get; set; } // milliseconds
        public int CleanerThreshold { get; set; } // seconds

        public ICollection<int> SourceIDs
        {
            get
            {
                return sources.Keys;
            }
        }

        public ICollection<Source> Sources
        {
            get
            {
                return sources.Values;
            }
        }

        public ConcurrentDictionary<int, Source> Dictionary
        {
            get
            {
                return sources;
            }
        }

        public void AddFrame(Frame frame)
        {
            if (sources.ContainsKey(frame.SourceID))
            {
                sources[frame.SourceID].Frame = frame;
                sources[frame.SourceID].LastUpdated = DateTime.UtcNow;
            }
            else
            {
                sources[frame.SourceID] = new Source
                {
                    Frame = frame,
                    Settings = new KinectSettings(),
                    LastUpdated = DateTime.UtcNow
                };
                OnSourceConnected(frame.SourceID);
            }
        }

        public Frame GetFrame(int sourceID)
        {
            return sources[sourceID].Frame;
        }

        public bool RemoveSource(int sourceID)
        {
            Source deleted;
            var result = sources.TryRemove(sourceID, out deleted);
            if (result)
                OnSourceDisconnected(sourceID);
            return result;
        }

        public int Count
        {
            get
            {
                return sources.Count;
            }
        }

        public void StartCleaner()
        {
            if (cleanerThread != null)
            {
                if (cleanerThread.IsAlive)
                {
                    Console.WriteLine("Cleaner thread already running");
                    return;
                }
            }

            cleanerThread = new Thread(Clean);
            cleanerThread.IsBackground = true;
            cleanerThread.Name = "StaleSourceFrameCleaner";
            cleanerThread.Start();
        }

        public void StopCleaner()
        {
            if (cleanerThread == null || !cleanerThread.IsAlive)
            {
                Console.WriteLine("Cleaner thread not running");
                return;
            }
            cleanerToCancel = true;
        }

        public bool IsCleaning
        {
            get
            {
                return cleanerThread != null && cleanerThread.IsAlive;
            }
        }

        protected void Clean()
        {
            while (!cleanerToCancel)
            {
                Thread.Sleep(CleanerInterval);

                var now = DateTime.UtcNow;

                foreach(var source in sources)
                {
                    var diff = (now - source.Value.LastUpdated).TotalSeconds;
                    if (diff > CleanerThreshold)
                    {
                        Console.WriteLine($"Stale source found, {source.Key} is {diff} seconds old");
                        RemoveSource(source.Key);
                    }
                }
            }

            cleanerToCancel = false;
        }

        public void Reset()
        {
            StopCleaner();
            sources.Clear();
        }

        protected virtual void OnSourceConnected(int sourceID)
        {
            SourceConnectedEventHandler handler = SourceConnected;
            handler?.Invoke(sourceID);
        }

        protected virtual void OnSourceDisconnected(int sourceID)
        {
            SourceDisconnectedEventHandler handler = SourceDisconnected;
            handler?.Invoke(sourceID);
        }
    }

    public delegate void SourceConnectedEventHandler(int sourceID);
    public delegate void SourceDisconnectedEventHandler(int sourceID);
}
