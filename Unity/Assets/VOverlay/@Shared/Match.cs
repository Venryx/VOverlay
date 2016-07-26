using UnityEngine;
using VDFN;
using VTree.BiomeDefenseN.MapsN;
using VTree.BiomeDefenseN.ObjectsN;
using VTree.VOverlayN.MapsN;
using VTree_Structures;

namespace VTree.VOverlayN.SharedN{
	public class Match : Node {
		Match s;

		public bool started;
		public bool paused;
		public bool finished;
		public bool _InProgress { get { return started && !finished; } }
		public bool _Running { get { return started && !paused && !finished; } }

		[P(false)] public GameObject obj;

		public Map map;

		[P(false)] public bool coreMapInitDone;
		[NoAutoProfile] public void NotifyPostCoreMapInit() { coreMapInitDone = true; }

		// data-frame stuff
		// ==========

		// data-frame-related view-frame-tick handler
		void PreViewFrameTick_EM2() {
			if (!_Running)
				return;

			// for now, data-frame is same as view-frame
			Profiler_LastDataFrame.PreDataFrameTick(); // first call profiler, so it can set up profiling for this upcoming frame
			s.a(a=>a.dataFrame).set = dataFrame + 1;
			Profiler_LastDataFrame.PostDataFrameTick(); // notify end of data-frame portion; rest gets put into "after-data-frame view-frame processing"
		}

		public int dataFrame;
		public int dataFramesPerSecond = 30;
		public double SecondsPerDataFrame { get { return 1d / dataFramesPerSecond; } }
		public class DataFrameFor_Class {
			public DataFrameFor_Class(Match match) { this.match = match; }
			public Match match;
			public int XSecondsFromNow(double seconds) { return XSecondsFromFrameY(seconds, match.dataFrame); }
			public int XSecondsFromFrameY(double seconds, int frameY) { return frameY + (int)(seconds * match.dataFramesPerSecond); }
		}
		public DataFrameFor_Class DataFrameFor { get { return new DataFrameFor_Class(s); } }
	}
}