using System.Diagnostics;
using UnityEngine;

namespace QC.Utilities.EventBusSystem
{
    /// <summary>
    /// Debugging information that tracks where and when an event was raised.
    /// Captures stack trace, frame count, and timestamp for event tracing.
    /// Only captures data in UNITY_EDITOR or DEVELOPMENT_BUILD to avoid performance overhead in production.
    /// </summary>
    public readonly struct EventTrace
    {
        /// <summary>
        /// The Unity frame number when the event was raised.
        /// </summary>
        public readonly int Frame;

        /// <summary>
        /// The precise time (in seconds since startup) when the event was raised.
        /// </summary>
        public readonly double Time;

        /// <summary>
        /// The source location where the event was raised (method, file, and line number).
        /// </summary>
        public readonly string Source;

        /// <summary>
        /// A default empty trace for use when tracing is disabled or unavailable.
        /// </summary>
        public static readonly EventTrace Empty = new(0, 0, null);

        /// <summary>
        /// Private constructor for creating trace instances.
        /// </summary>
        /// <param name="frame">The frame number</param>
        /// <param name="time">The timestamp</param>
        /// <param name="source">The source location string</param>
        private EventTrace(int frame, double time, string source)
        {
            Frame = frame;
            Time = time;
            Source = source;
        }

        /// <summary>
        /// Checks if this trace contains valid debugging information.
        /// </summary>
        public bool IsValid => Source != null;

        /// <summary>
        /// Returns a formatted string representation of the trace information.
        /// </summary>
        /// <returns>A human-readable trace string showing frame, time, and source location</returns>
        public override string ToString()
        {
            if (!IsValid) return "[no trace]";
            return $"[Frame {Frame}, Time: {Time}, Source: {Source}]";
        }
        
        /// <summary>
        /// Captures the current call stack, frame, and time information for debugging.
        /// Only performs capturing in editor or development builds; returns Empty in production builds.
        /// </summary>
        /// <returns>An EventTrace containing the caller's location and timing information</returns>
        public static EventTrace Capture()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Walk up the stack by 2 frames to skip Capture() and the event bus method
            StackTrace stackTrace = new StackTrace(2, true);
            StackFrame frame = stackTrace.GetFrame(0);

            string source;
            if (frame != null)
            {
                // Extract method name, file name, and line number
                string method = frame.GetMethod()?.DeclaringType?.Name + "." + frame.GetMethod()?.Name;
                string file = System.IO.Path.GetFileName(frame.GetFileName() ?? "Unknown");
                int line = frame.GetFileLineNumber();
                source = $"{method} ({file}:{line})";
            }
            else
            {
                source = "Unknown";
            }

            // Capture current frame and timestamp from Unity
            return new EventTrace(UnityEngine.Time.frameCount, UnityEngine.Time.timeAsDouble, source);
#else
            // In production builds, return an empty trace to avoid performance overhead
            return Empty;
#endif
        }
    }
}