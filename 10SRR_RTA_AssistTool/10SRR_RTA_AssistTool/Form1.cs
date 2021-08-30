using DirectShowLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static TenSRR_RTA_AssistTool.MyMessageBox;

namespace TenSRR_RTA_AssistTool
{
	public partial class Form1 : Form
	{

		VideoChecker mVideoChecker = null;
		AudioChecker mAudioChecker = null;

		List<GameState> mGameStateHistory = new List<GameState>();
		GameState.STATE mCurGamePlayState = GameState.STATE.NONE;

		private IGraphBuilder graphBuilder; //基本的なフィルタグラフマネージャ
		private ICaptureGraphBuilder2 captureGraphBuilder;//ビデオキャプチャ＆編集用のメソッドを備えたキャプチャグラフビルダ

		private IVideoWindow videoWindow; //オーナーウィンドウの位置やサイズなどの設定用のインタフェース．
		private IMediaControl mediaControl;//データのストリーミングの移動、ポーズ、停止などの処理用のインタフェース．
		private IMediaEventEx mediaEvent; //DirectShowイベント処理用のインタフェース

		int result; //エラー判定用フラグ

		public const int WM_GRAPHNOTIFY = 0x00008001;//DirectShowイベントの発生を表すWindows メッセージ.

		public Color FONT_COLOR = Color.White;

		private const bool USE_AUDIO = false;


		public Form1()
		{
			InitializeComponent();
		}

		private async void Form1_Load(object sender, EventArgs e)
		{
			PropertyCache.Instance.Initialize();
			Preferences.Instance.Initialize();
			bool ret = directShowInitialize();
			if (!ret)
			{
				CloseInterfaces();
			}

			ResetRun();

			while (true)
			{
				await update();
			}
		}

		public void ResetRun()
		{
			mGameStateHistory.Clear();
			UpdateDisplay();
		}

		private bool directShowInitialize()
		{
			try
			{
				mVideoChecker = new VideoChecker(this);
				if (USE_AUDIO)
				{
					mAudioChecker = new AudioChecker(this);
				}

				//mVideoChecker.Test();

				// 3. フィルタグラフマネージャを作成し，各種操作を行うためのインタフェースを取得する．

				//graphBuilderを作成．
				graphBuilder = GraphFactory.MakeGraphBuilder();

				//各種インタフェースを取得．
				mediaControl = (IMediaControl)graphBuilder;
				videoWindow = (IVideoWindow)graphBuilder;
				mediaEvent = (IMediaEventEx)graphBuilder;


				// 4. キャプチャグラフビルダと，サンプルグラバフィルタ（個々のビデオデータを取得するフィルタ）を作成する．

				//キャプチャグラフビルダ(captureGraphBuilder)を作成．
				captureGraphBuilder = GraphFactory.MakeCaptureGraphBuilder();

				// 5. 基本となるフィルタグラフマネージャ(graphBuilder)にキャプチャグラフビルダと各フィルタを追加する．

				//captureGraphBuilder（キャプチャグラフビルダ）をgraphBuilder（フィルタグラフマネージャ）に追加．
				result = captureGraphBuilder.SetFiltergraph(graphBuilder);
				if (result < 0) Marshal.ThrowExceptionForHR(result);


				// 1. デバイスを取得し、2. キャプチャデバイスをソースフィルタに対応づける．
				// 6. 各フィルタの接続を行い，入力画像のキャプチャとプレビューの準備を行う．
				bool ret = mVideoChecker.AddVideoFilters(graphBuilder, captureGraphBuilder);
				if (!ret)
				{
					//MyMessageBox.Show("エラー：映像入力デバイスが見つかりませんでした。\nプログラムを終了します。Error: Any video devices are not found.\n Closing this application.", MyMessageBoxIcon.Error);
					return false;
				}

				if (USE_AUDIO)
				{
					ret = mAudioChecker.AddAudioFilters(graphBuilder, captureGraphBuilder);
					if (!ret)
					{
						//MyMessageBox.Show("エラー：音声入力デバイスが見つかりませんでした。\nプログラムを終了します。Error: Any audio devices are not found.\n Closing this application.", MyMessageBoxIcon.Error);
						return false;
					}
				}

				// 7. プレビュー映像（レンダラフィルタの出力）の出力場所を設定する.

				//プレビュー映像を表示するパネルを指定．
				result = videoWindow.put_Owner(this.panel1.Handle);
				if (result < 0) Marshal.ThrowExceptionForHR(result);
				//ビデオ表示領域のスタイルを指定．
				result = videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipChildren);
				if (result < 0) Marshal.ThrowExceptionForHR(result);
				//ビデオパネルのサイズを変更する．
				Rectangle rect = this.panel1.ClientRectangle;
				videoWindow.SetWindowPosition(0, 0, rect.Right, rect.Bottom);

				//レンダラフィルタの出力を可視化する．
				result = videoWindow.put_Visible(OABool.True);
				if (result < 0) Marshal.ThrowExceptionForHR(result);



				// 8. DirectShowイベントを，Windowsメッセージを通して通知するための設定を行う．

				// mediaEvent(DirectShowイベント)をWM_GRAPHNOTIFY(Windowsメッセージ)に対応付ける． 
				result = mediaEvent.SetNotifyWindow(this.Handle, WM_GRAPHNOTIFY, IntPtr.Zero);
				if (result < 0) Marshal.ThrowExceptionForHR(result);



				// 9. プレビューを開始する．

				result = mediaControl.Run();
				if (result < 0) Marshal.ThrowExceptionForHR(result);

			} catch (Exception e)
			{
				MyMessageBox.Show(e.Message, MyMessageBoxIcon.Error);
				return false;
			}
			return true;
		}

		private void Form1_FormClosed_1(object sender, FormClosedEventArgs e)
		{
			//// 閉じるときの処理
			CloseInterfaces();
		}

		//COMやインタフェースの解放を行う
		public void CloseInterfaces()
		{
			try
			{
				if (mediaControl != null)
				{
					mediaControl.Stop();
					Thread.Sleep(100);
					mediaControl = null;
				}

				if (mediaEvent != null)
				{
					mediaEvent.SetNotifyWindow(IntPtr.Zero, WM_GRAPHNOTIFY, IntPtr.Zero);
					mediaEvent = null;
				}

				if (videoWindow != null)
				{
					videoWindow.put_Visible(OABool.False);
					videoWindow.put_Owner(IntPtr.Zero);
					videoWindow = null;
				}

				if (captureGraphBuilder != null)
				{
					Marshal.ReleaseComObject(captureGraphBuilder);
					captureGraphBuilder = null;
				}

				if (graphBuilder != null)
				{
					Marshal.ReleaseComObject(graphBuilder);
					graphBuilder = null;
				}

				if (mVideoChecker != null)
				{
					mVideoChecker.CloseInterfaces();
					mVideoChecker = null;
				}
				if (mAudioChecker != null)
				{
					mAudioChecker.CloseInterfaces();
					mAudioChecker = null;
				}
			}
			catch (Exception e)
			{
				MyMessageBox.Show("エラー：インタフェースの終了に失敗しました。Error: An error ocouured when closing video interface. " + e.ToString(), MyMessageBoxIcon.Error);
			}
		}

		private Task update()
		{

			// サンプルグラバーのコールバックメソッド を有効にし，サンプリングを開始する．
			if (mVideoChecker != null)
			{
				mVideoChecker.update();
			}
			if (mAudioChecker != null)
			{
				mAudioChecker.update();
			}

			if (mVideoChecker != null && (!USE_AUDIO || mAudioChecker != null))
			{
				if (!CheckBox_Pause.Checked) // ポーズ中でない
				{
					VideoGameState videoGameState = mVideoChecker.GetVideoGameState();

					// コースのプレイ開始を検出した
					if (mCurGamePlayState == GameState.STATE.NONE || mCurGamePlayState == GameState.STATE.PLAYED)
					{
						if (videoGameState.mIsLoading)
						{
							mCurGamePlayState = GameState.STATE.PLAYING;
						}
					}
					else if (mCurGamePlayState == GameState.STATE.PLAYING)
					{
						if (videoGameState.mCourseNo > 0)
						{
							GameState targetState = null;
							foreach (GameState state in mGameStateHistory)
							{
								if (state.GetCourseNo() == videoGameState.mCourseNo)
								{
									targetState = state;
									break;
								}
							}
							if (targetState == null)
							{
								targetState = new GameState();
								mGameStateHistory.Add(targetState);
							}
							targetState.UpdateState(videoGameState.mCourseNo, videoGameState.mIGT, videoGameState.isDeath());
							mCurGamePlayState = GameState.STATE.PLAYED;
							if (!videoGameState.isDeath() && videoGameState.mCourseNo % 10 == 0)
							{
								SendKeyToSplit();
							}
						}
						UpdateDisplay();
					}
				}
			}

			return Task.Run(() => Thread.Sleep(100));
		}

		private void UpdateDisplay()
		{
			CheckBox_PreviewVideo.Checked = PropertyCache.Instance.mPreviewVideo == 1;
			CheckBox_PlayAudio.Checked = PropertyCache.Instance.mPlayAudio == 1;

			Dictionary<string, Label> labels = new Dictionary<string, Label>();
			labels.Add(Label_Idx1.Name, Label_Idx1);
			labels.Add(Label_LevelCode1.Name, Label_LevelCode1);
			labels.Add(Label_LevelTitle1.Name, Label_LevelTitle1);
			labels.Add(Label_ChartCoin1.Name, Label_ChartCoin1);
			labels.Add(Label_CurCoin1.Name, Label_CurCoin1);
			labels.Add(Label_CurDiff1.Name, Label_CurDiff1);
			labels.Add(Label_TotalCoin1.Name, Label_TotalCoin1);
			labels.Add(Label_TotalDiff1.Name, Label_TotalDiff1);
			labels.Add(Label_NeededDiff1.Name, Label_NeededDiff1);
			labels.Add(Label_NeededIdx1.Name, Label_NeededIdx1);
			labels.Add(Label_FinalDiff1.Name, Label_FinalDiff1);

			labels.Add(Label_Idx2.Name, Label_Idx2);
			labels.Add(Label_LevelCode2.Name, Label_LevelCode2);
			labels.Add(Label_LevelTitle2.Name, Label_LevelTitle2);
			labels.Add(Label_ChartCoin2.Name, Label_ChartCoin2);
			labels.Add(Label_CurCoin2.Name, Label_CurCoin2);
			labels.Add(Label_CurDiff2.Name, Label_CurDiff2);
			labels.Add(Label_TotalCoin2.Name, Label_TotalCoin2);
			labels.Add(Label_TotalDiff2.Name, Label_TotalDiff2);
			labels.Add(Label_NeededDiff2.Name, Label_NeededDiff2);
			labels.Add(Label_NeededIdx2.Name, Label_NeededIdx2);
			labels.Add(Label_FinalDiff2.Name, Label_FinalDiff2);

			const int DISP_NUM = 2;
			for (int i = 0; i < DISP_NUM; ++i)
			{
				// 初期化
				labels["Label_Idx" + (i + 1)].Text = "-";
				labels["Label_LevelCode" + (i + 1)].Text = "-";
				labels["Label_LevelTitle" + (i + 1)].Text = "-";
				labels["Label_ChartCoin" + (i + 1)].Text = "-";
				labels["Label_CurCoin" + (i + 1)].Text = "-";
				labels["Label_CurDiff" + (i + 1)].Text = "-";
				labels["Label_TotalCoin" + (i + 1)].Text = "-";
				labels["Label_TotalDiff" + (i + 1)].Text = "-";
				labels["Label_NeededDiff" + (i + 1)].Text = "-";
				labels["Label_NeededIdx" + (i + 1)].Text = "-";
				labels["Label_FinalDiff" + (i + 1)].Text = "-";

				labels["Label_CurDiff" + (i + 1)].ForeColor = FONT_COLOR;
				labels["Label_TotalDiff" + (i + 1)].ForeColor = FONT_COLOR;
				labels["Label_NeededDiff" + (i + 1)].ForeColor = FONT_COLOR;
				labels["Label_FinalDiff" + (i + 1)].ForeColor = FONT_COLOR;

				// コースデータ取得
				bool chartFound = false; // チャートにあった
				GameState state = null;
				LevelData levelData = null;
				int index = mGameStateHistory.Count - DISP_NUM + i;
				if (index >= 0)
				{
					state = mGameStateHistory[index];
					levelData = state.GetLevelData();
					if (levelData != null)
					{
						chartFound = true;
					}
					else
					{
						// チャートに無かったのでコースタイトルのみを検索
						levelData = state.GetLevelDataMinimum();
					}
				}

				// コースデータが見つかった場合、画面更新
				if (levelData != null)
				{
					// コース番号、コース名を表示
					string levelCode = levelData.mLevelCode;
					string levelName = levelData.mJpTitle; // コース名
					labels["Label_Idx" + (i + 1)].Text = state.GetAllSerialIdx().ToString();
					labels["Label_LevelCode" + (i + 1)].Text = levelCode;
					labels["Label_LevelTitle" + (i + 1)].Text = levelName;

					if (chartFound)
					{
						// チャートの目標枚数も表示
						int chartCoin = levelData.mInLevelCoin;
						labels["Label_ChartCoin" + (i + 1)].Text = chartCoin.ToString();
					}
					if (state.GetState() == GameState.STATE.PLAYED)
					{
						// コインの取得枚数を表示
						int curCoin = state.GetCurCoinNum(); // 実際に獲得したコイン
						int gotCumulativeCoin = state.GetCumulativeCoinNum(); // 実際に獲得したコイン（累計）

						labels["Label_CurCoin" + (i + 1)].Text = curCoin.ToString();
						labels["Label_TotalCoin" + (i + 1)].Text = gotCumulativeCoin.ToString();

						if (chartFound)
						{
							// コインの差分も表示
							int curCoinDiff = state.GetCurCoinDiff(); // チャートとの差
							int cumulativeCoinDiff = state.GetCumulativeCoinDiff(); // チャートとの差（累計）
							int neededDiff = cumulativeCoinDiff + state.GetLevelData().mAllowedLoss.Item2;
							int neededIdx = state.GetLevelData().mAllowedLoss.Item1;
							int finalDiff = cumulativeCoinDiff + state.GetLevelData().mFinalAllowedLoss.Item2;

							string curCoinSign = (curCoinDiff >= 0) ? "+" : "";
							string cumulativeCoinSign = (cumulativeCoinDiff >= 0) ? "+" : "";
							string neededCoinSign = (neededDiff >= 0) ? "+" : "";
							string finalCoinSign = (finalDiff >= 0) ? "+" : "";
							labels["Label_CurDiff" + (i + 1)].Text = curCoinSign + curCoinDiff.ToString();
							labels["Label_TotalDiff" + (i + 1)].Text = cumulativeCoinSign + cumulativeCoinDiff.ToString();
							labels["Label_NeededDiff" + (i + 1)].Text = neededCoinSign + neededDiff.ToString();
							labels["Label_NeededIdx" + (i + 1)].Text = "[" + neededIdx.ToString() + "]";
							labels["Label_FinalDiff" + (i + 1)].Text = finalCoinSign + finalDiff.ToString();

							//Color greenColor = Color.FromArgb(0, 175, 52);
							//Color redColor = Color.FromArgb(193, 41, 10);

							//Color greenColor = Color.FromArgb(0, 210, 62);
							//Color redColor = Color.FromArgb(231, 49, 12);

							Color greenColor = Color.FromArgb(0, 231, 68);
							Color redColor = Color.FromArgb(254, 53, 13);

							if (curCoinDiff >= 0)
							{
								labels["Label_CurDiff" + (i + 1)].ForeColor = greenColor;
							}
							else
							{
								labels["Label_CurDiff" + (i + 1)].ForeColor = redColor;
							}
							if (cumulativeCoinDiff >= 0)
							{
								labels["Label_TotalDiff" + (i + 1)].ForeColor = greenColor;
							}
							else
							{
								labels["Label_TotalDiff" + (i + 1)].ForeColor = redColor;
							}
							if (neededDiff >= 0)
							{
								labels["Label_NeededDiff" + (i + 1)].ForeColor = greenColor;
							}
							else
							{
								labels["Label_NeededDiff" + (i + 1)].ForeColor = redColor;
							}
							if (finalDiff >= 0)
							{
								labels["Label_FinalDiff" + (i + 1)].ForeColor = greenColor;
							}
							else
							{
								labels["Label_FinalDiff" + (i + 1)].ForeColor = redColor;
							}
						}
					}
				}
			}

			// 補助コマンド表示
			{
				LevelData levelData = null;
				if (mGameStateHistory.Count > 0)
				{
					GameState state = mGameStateHistory[mGameStateHistory.Count - 1];
					levelData = state.GetLevelData();
				}
				if (levelData != null)
				{
					Label_CastleList.Text = levelData.mCastleList;
					Label_LevelSelectCommand.Text = levelData.mLevelSelectCommand;
					if (levelData.mNextLevel != null)
					{
						Label_NextLevel.Text = "[" + levelData.mNextLevel.mLevelCode + "]（" + levelData.mNextLevel.mJpTitle + "）";
						Label_Remark.Text = levelData.mNextLevel.mRemark;
					}
					else
					{
						Label_NextLevel.Text = "";
						Label_Remark.Text = "";
					}
				}
				else
				{
					Label_CastleList.Text = "";
					Label_LevelSelectCommand.Text = "";
					Label_Remark.Text = "";
					Label_NextLevel.Text = "";
				}
			}
		}

		private void closeToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void resetConfigToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SettingForm settingForm = new SettingForm();
			settingForm.ShowDialog(this);
		}

		private void CheckBox_PreviewVideo_CheckedChanged(object sender, EventArgs e)
		{
			PropertyCache.Instance.mPreviewVideo = CheckBox_PreviewVideo.Checked ? 1 : 0;
			PropertyCache.Instance.SaveToFile();

			//レンダラフィルタの出力の可視化設定を変更．
			if (videoWindow != null)
			{
				if (PropertyCache.Instance.mPreviewVideo == 1)
				{
					result = videoWindow.put_Visible(OABool.True);
				}
				else
				{
					result = videoWindow.put_Visible(OABool.False);
				}
				if (result < 0) Marshal.ThrowExceptionForHR(result);
			}
		}

		private void CheckBox_PlayAudio_CheckedChanged(object sender, EventArgs e)
		{
			PropertyCache.Instance.mPlayAudio = CheckBox_PlayAudio.Checked ? 1 : 0;
			PropertyCache.Instance.SaveToFile();
		}

		private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PreferencesForm form = new PreferencesForm();
			DialogResult result = form.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// リロード
				CloseInterfaces();
				directShowInitialize();
			}
		}

		private void Button_Reset_Click(object sender, EventArgs e)
		{
			// CSV出力の確認
			if (mGameStateHistory.Count > 0)
			{
				DialogResult result = MessageBox.Show("Reset run?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (result == DialogResult.Yes)
				{
					ResetRun();
				}
			}
		}

		private void OutputCSV()
		{
			string fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + "10SRR_RTA.csv";
			string[] header = GameState.CSV_HEADER;
			int COLUMN_NUM = header.Length;

			// ファイルを開く
			using (StreamWriter file = new StreamWriter(fileName, false, Encoding.GetEncoding("Shift_JIS")))
			{
				string headerStr = "";
				for (int i = 0; i < COLUMN_NUM; ++i)
				{
					if (i > 0)
					{
						headerStr += ",";
					}
					headerStr += "\"" + header[i] + "\"";
				}
				file.WriteLine(headerStr);

				int count = 0;
				foreach (GameState g in mGameStateHistory)
				{
					string str = g.MakeCSVLine();
					file.WriteLine(str);
					++count;
				}
				Console.WriteLine("CSV Saved. (" + count + " Lines.)");
				Label_CSVMessage.Text = "Saved. " + fileName;
			}
		}

		//private void LoadCSV()
		//{
		//	// ファイル選択ウィンドウを開く
		//
		//	OpenFileDialog ofd = new OpenFileDialog();
		//
		//	// 設定する
		//	ofd.FileName = "*.csv"; // はじめに「ファイル名」で表示される文字列を指定する
		//	ofd.InitialDirectory = "";
		//	ofd.Filter = "CSVファイル(*.csv)|*.csv|すべてのファイル(*.*)|*.*";
		//	ofd.FilterIndex = 1; // [ファイルの種類]ではじめに選択されるものを指定する（1番目の「CSVファイル」が選択されているようにする）
		//	ofd.Title = "開くファイルを選択してください"; // タイトルを設定する
		//	ofd.RestoreDirectory = true; // ダイアログボックスを閉じる前に現在のディレクトリを記憶するようにする
		//
		//	//ダイアログを表示する
		//	if (ofd.ShowDialog() == DialogResult.OK)
		//	{
		//		//OKボタンがクリックされたとき
		//		Console.WriteLine(ofd.FileName);
		//
		//		List<List<string>> csvData = CsvReader.ReadCsv(ofd.FileName, true, true);
		//
		//		ResetRun();
		//
		//		foreach (List<string> line in csvData)
		//		{
		//			GameState state = new GameState();
		//			state.SetFromCSVLine(line);
		//			mGameStateHistory.Add(state);
		//		}
		//
		//		UpdateDisplay();
		//
		//		Label_CSVMessage.Text = "Loaded. " + ofd.FileName;
		//	}
		//}

		private void Button_CSVOutput_Click(object sender, EventArgs e)
		{
			OutputCSV();
		}

		private void Button_CSVLoad_Click(object sender, EventArgs e)
		{
			Console.WriteLine("Loading CSV is not implemented.");
			//LoadCSV();
		}

		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		private void SendKeyToSplit()
		{
			if (!CheckBox_SplitSending.Checked)
			{
				Console.WriteLine("Doesn't send EnterKey. (pausing)");
				return;
			}

			Process liveSplitProcess = null;

			//すべてのプロセスを列挙する
			foreach (Process p in Process.GetProcesses())
			{
				//指定された文字列がメインウィンドウのタイトルに含まれているか調べる
				if (p.MainWindowTitle.IndexOf("LiveSplit") >= 0)
				{
					//含まれていたら、コレクションに追加
					liveSplitProcess = p;
					break;
				}
			}

			if (liveSplitProcess == null)
			{
				Console.WriteLine("LiveSplit is not found.");
				return;
			}

			// 選択しているプロセスをアクティブ
			SetForegroundWindow(liveSplitProcess.MainWindowHandle);
			// キーストロークを送信
			SendKeys.Send("{Enter}");

			Console.WriteLine("EnterKey was successfully send to LiveSplit.");
		}

		private void Button_Undo_Click(object sender, EventArgs e)
		{
			// 履歴を１つ削除する
			if (mGameStateHistory.Count == 0)
			{
				return;
			}
			mGameStateHistory.RemoveAt(mGameStateHistory.Count - 1);
			UpdateDisplay();
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			// CSV出力の確認
			if (mGameStateHistory.Count > 0)
			{
				DialogResult result = MessageBox.Show("終了する前にCSV出力しますか？", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (result == DialogResult.Yes)
				{
					OutputCSV();
				}
			}
		}
	}
}
