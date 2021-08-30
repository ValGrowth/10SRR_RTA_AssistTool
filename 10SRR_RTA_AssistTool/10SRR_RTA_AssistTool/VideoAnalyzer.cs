using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TenSRR_RTA_AssistTool
{
	class VideoAnalyzer
	{
		private IGTAnalyzer mIGTAnalyzer = new IGTAnalyzer();
		private CourseNumAnalyzer mCourseNumAnalyzer = new CourseNumAnalyzer();
		private FailureAnalyzer mFailureAnalyzer = new FailureAnalyzer();

		private VideoGameState mVideoGameState = new VideoGameState();

		private long mIGTStopTime = 0;
		private List<Tuple<long, double>> mIGTHistory = new List<Tuple<long, double>>();
		private bool mWaitingForLoad = false;

		public VideoGameState GetVideoGameState()
		{
			return mVideoGameState;
		}

		public void UpdateGameState(FastBitmap bitmap)
		{
			//タイマー止まる(not is running)
			//↓
			//メニュー画面が開く
			// →早い→Restart
			// →青が再プレイにある→Failure
			// →それ以外→Clear
			//
			//is running = 前フレームとIGTが異なる
			//
			//過去２秒分のIGTを保持
			//同じIGTで１秒以上経過していたらFailureかClear
			//同じIGTが１秒未満ならRestart
			//Loading時にリセット

			if (mWaitingForLoad)
			{
				if (IsLoading(bitmap))
				{
					// ロード中
					mVideoGameState.mIsLoading = true;
					mVideoGameState.mCourseNo = -1;
					mVideoGameState.mIGT = -1;
					mVideoGameState.mIsFailure = false;
					mVideoGameState.mIsRestart = false;
					mWaitingForLoad = false;
				}
			}
			else
			{
				mVideoGameState.mIsLoading = false;

				long curTime = Timer.Instance.GetUnixTime(DateTime.Now);

				double igt = mIGTAnalyzer.DetectIGT(bitmap);
				if (igt < 0 || igt == 10.0) // IGT検出不可 or ゲーム開始前
				{
					mIGTStopTime = -1;
				}
				else // IGT検出
				{
					if (mIGTHistory.Count > 0 && igt == mIGTHistory[mIGTHistory.Count - 1].Item2) // 前回フレームとIGTが同じ
					{
						if (mIGTStopTime < 0)
						{
							// IGT停止タイミングを記憶
							mIGTStopTime = mIGTHistory[mIGTHistory.Count - 1].Item1;
						}
					}
					else // 前回フレームとIGTが異なる
					{
						mIGTStopTime = -1;
					}
				}

				FailureAnalyzer.SELECTION selection = mFailureAnalyzer.DetectSelection(bitmap);
				if (selection != FailureAnalyzer.SELECTION.NONE)
				{
					int courseNo = mCourseNumAnalyzer.DetectCourseNo(bitmap);
					if (courseNo > 0) // コース番号検出
					{
						if (mIGTStopTime < 0) // IGTが停止していない or ゲーム開始前
						{
							// 何も検出しない
							mVideoGameState.mCourseNo = -1;
							mVideoGameState.mIGT = -1;
							mVideoGameState.mIsFailure = false;
							mVideoGameState.mIsRestart = false;
						}
						else // IGTが停止している
						{
							// Restart,Failure,Clearを検出してIGTをセット
							mVideoGameState.mCourseNo = courseNo;
							mVideoGameState.mIGT = igt;

							mVideoGameState.mIsFailure = false;
							mVideoGameState.mIsRestart = false;

							if (curTime - mIGTStopTime >= 1000) // １秒以上経過している
							{
								if (selection == FailureAnalyzer.SELECTION.RESTART) // カーソルがRESTARTにある
								{
									mVideoGameState.mIsFailure = true;
								}
							}
							else // １秒以上経過していない
							{
								// カーソルがどこにあってもRESTARTとして扱う
								mVideoGameState.mIsRestart = true;
							}
						}

					}

					mWaitingForLoad = true;
				}

				mIGTHistory.Clear();
				mIGTHistory.Add(new Tuple<long, double>(curTime, igt));
			}

		}

	}
}
