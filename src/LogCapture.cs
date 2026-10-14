using System;
using System.Collections.Generic;
using UnityEngine;

namespace VaMMCP {
	/// <summary>
	/// Keeps the newest lines VaM and Unity log, each stamped with the local time it arrived. VaM's own
	/// output_log.txt has no timestamps and grows to hundreds of MB, so read_log answers from here instead.
	/// Lines arrive on any thread; everything goes through one lock.
	/// </summary>
	public static class LogCapture {
		public const int Capacity = 5000;

		public class Entry {
			public long Seq;
			public DateTime Time;
			public LogType Type;
			public string Message;
			public string Stack;
			/// <summary>Seq of the scene load this line belongs to (0 before the first one).</summary>
			public long SceneSeq;
		}

		public class SceneLoad {
			public long Seq;
			public DateTime Time;
			public string Path;
		}

		private static readonly object Gate = new object();
		private static readonly LinkedList<Entry> Entries = new LinkedList<Entry>();
		private static long nextSeq = 1;
		private static long dropped;
		private static SceneLoad lastSceneLoad;
		private static bool started;

		public static void Start() {
			if (started) return;
			started = true;
			Application.logMessageReceivedThreaded += OnLog;
		}

		public static void Stop() {
			if (!started) return;
			started = false;
			Application.logMessageReceivedThreaded -= OnLog;
		}

		private static void OnLog(string message, string stackTrace, LogType type) {
			DateTime now = DateTime.Now;
			lock (Gate) {
				Entry e = new Entry {
					Seq = nextSeq++,
					Time = now,
					Type = type,
					Message = message ?? "",
					Stack = stackTrace ?? "",
				};
				// VaM logs "Load <path>" when it starts loading a scene.
				if (type == LogType.Log && e.Message.StartsWith("Load ", StringComparison.Ordinal) &&
				    e.Message.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) {
					lastSceneLoad = new SceneLoad { Seq = e.Seq, Time = now, Path = e.Message.Substring(5) };
				}
				e.SceneSeq = lastSceneLoad != null ? lastSceneLoad.Seq : 0;
				Entries.AddLast(e);
				while (Entries.Count > Capacity) {
					Entries.RemoveFirst();
					dropped++;
				}
			}
		}

		/// <summary>A copy of the entries after <paramref name="afterSeq"/>, plus the state needed to describe them.</summary>
		public static List<Entry> Snapshot(long afterSeq, out SceneLoad scene, out long lastSeq, out long droppedCount) {
			lock (Gate) {
				List<Entry> result = new List<Entry>();
				foreach (Entry e in Entries)
					if (e.Seq > afterSeq) result.Add(e);
				scene = lastSceneLoad;
				lastSeq = nextSeq - 1;
				droppedCount = dropped;
				return result;
			}
		}
	}
}
