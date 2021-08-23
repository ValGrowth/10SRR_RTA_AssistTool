using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using static TenSRR_RTA_AssistTool.MyMessageBox;

using DirectShowLib;

namespace TenSRR_RTA_AssistTool {
	class AudioChecker : ISampleGrabberCB {

		private IBaseFilter mAudioCaptureFilter; //ソースフィルタ
		private IBaseFilter mAudioGrabFilter; // Grabber Filterのインタフェース．(音声用)
		private ISampleGrabber mAudioSampleGrabber; //フィルタグラフ内を通る個々のデータ取得用のインタフェース．(音声用)
		private WaveFormatEx mAudioInfoHeader; // 音声のフォーマットを記述する構造体

		private Form1 mForm1;
		private int result;  //エラー判定用フラグ

		private delegate void CaptureDone(double SampleTime, int BufferLength); // キャプチャ終了時の処理を行うデリゲード
		private byte[] mFrameArray; // キャプチャしたフレームデータ用の配列
		private bool mProcessing = false;

		private List<KeyValuePair<double, int>> mSoundHistories = new List<KeyValuePair<double, int>>();

		public AudioChecker(Form1 form1) {
			mForm1 = form1;
		}

		//COMやインタフェースの解放を行う
		public void CloseInterfaces()
		{
			try
			{

				if(mAudioCaptureFilter != null)
				{
					mAudioCaptureFilter.Stop();
					Marshal.ReleaseComObject(mAudioCaptureFilter);
					mAudioCaptureFilter = null;
				}

			}
			catch(Exception e)
			{
				MyMessageBox.Show("エラー：音声インタフェースの終了に失敗しました。Error: An error ocouured when closing audio interface. "+ e.ToString(), MyMessageBoxIcon.Error);
			}
		}

		public bool AddAudioFilters(IGraphBuilder graphBuilder, ICaptureGraphBuilder2 captureGraphBuilder) {

			bool ret = SetDevice();
			if (!ret) {
				return false;
			}

			//サンプルグラバ(audioSampleGrabber)を作成
			mAudioSampleGrabber = GraphFactory.MakeSampleGrabber();

			//フィルタと関連付ける．
			mAudioGrabFilter = (IBaseFilter)mAudioSampleGrabber;

			//キャプチャするビデオデータのフォーマットを設定．
			AMMediaType audioAmMediaType = new AMMediaType();
			audioAmMediaType.majorType = MediaType.Audio;
			audioAmMediaType.subType = MediaSubType.PCM;

			result = mAudioSampleGrabber.SetMediaType(audioAmMediaType);
			if(result < 0) Marshal.ThrowExceptionForHR(result);

			// 音声のcaptureFilter(ソースフィルタ)をgraphBuilder（フィルタグラフマネージャ）に追加．
			result = graphBuilder.AddFilter(mAudioCaptureFilter, "Audio Capture Filter");
			if(result < 0) Marshal.ThrowExceptionForHR(result);

			// audioGrabFilter(変換フィルタ)をgraphBuilder（フィルタグラフマネージャ）に追加．
			result = graphBuilder.AddFilter(mAudioGrabFilter, "Audio Grab Filter");
			if (result < 0) Marshal.ThrowExceptionForHR(result);

			// 音声のキャプチャフィルタをサンプルグラバーフィルタに接続する．
			result = captureGraphBuilder.RenderStream(PinCategory.Capture, MediaType.Audio, mAudioCaptureFilter, null, mAudioGrabFilter);
			if(result < 0) Marshal.ThrowExceptionForHR(result);

			//// 音声のキャプチャフィルタをデフォルトのレンダラフィルタ（ディスプレイ上に出力）に接続する．（プレビュー）
			//result = captureGraphBuilder.RenderStream(PinCategory.Preview, MediaType.Audio, mAudioCaptureFilter, null, null); 
			//if(result < 0) Marshal.ThrowExceptionForHR(result);

			// フレームキャプチャの設定が完了したかを確認する．
			AMMediaType amMediaType = new AMMediaType();
			result = mAudioSampleGrabber.GetConnectedMediaType(amMediaType);
			if(result < 0) Marshal.ThrowExceptionForHR(result);
			if ((amMediaType.formatType != FormatType.WaveEx) || (amMediaType.formatPtr == IntPtr.Zero)) {
				throw new NotSupportedException("エラー：キャプチャできない音声メディアフォーマットです．Error: This audio media format cannnot be caputered.");
			}

			// キャプチャするビデオデータのフォーマットから，audioInfoHeaderを作成する．
			mAudioInfoHeader = (WaveFormatEx) Marshal.PtrToStructure(amMediaType.formatPtr, typeof(WaveFormatEx));
			Marshal.FreeCoTaskMem(amMediaType.formatPtr);
			amMediaType.formatPtr = IntPtr.Zero;

			// フィルタ内を通るサンプルをバッファにコピーしないように指定する．
			result = mAudioSampleGrabber.SetBufferSamples(false);
			// サンプルを一つ（1フレーム）受け取ったらフィルタを停止するように指定する．
			if(result == 0) result = mAudioSampleGrabber.SetOneShot(false);
			// コールバック関数の利用を停止する．
			if(result == 0) result = mAudioSampleGrabber.SetCallback(null, 0); 
			if(result < 0) Marshal.ThrowExceptionForHR(result);

			return true;
		}
		
		private bool SetDevice() {

			DsDevice audioDevice = GraphFactory.GetDevice(FilterCategory.AudioInputDevice, GraphFactory.DEVICE_TYPE.AUDIO);
			if (audioDevice == null) {
				MyMessageBox.Show("エラー：音声デバイスが選択されませんでした。Error: Audio device is not selected.", MyMessageBoxIcon.Warning);
				return false;
			}

			// 音声のキャプチャデバイス(device)とソースフィルタ(captureFilter)を対応付ける．
			mAudioCaptureFilter = GraphFactory.GetCaptureFilter(audioDevice);

			return true;
		}


		public int update() {

			//Console.WriteLine("nAvgBytesPerSec:" + mAudioInfoHeader.nAvgBytesPerSec);
			//Console.WriteLine("nBlockAlign:" + mAudioInfoHeader.nBlockAlign);
			//Console.WriteLine("nChannels:" + mAudioInfoHeader.nChannels);
			//Console.WriteLine("nSamplesPerSec:" + mAudioInfoHeader.nSamplesPerSec);
			//Console.WriteLine("wBitsPerSample:" + mAudioInfoHeader.wBitsPerSample);
			//Console.WriteLine("wFormatTag:" + mAudioInfoHeader.wFormatTag);
			/*
			 *  nAvgBytesPerSecは必須平均データ転送レートを バイト/秒 単位で指定します
				PCM の場合、通常はサンプリングレート * nBlockAlign を指定します

				nBlockAlign は、指定フォーマットの最小単位のデータサイズを指定します
				これをブロックアラインメントと呼び
				PCM の場合、このメンバには nChannels * wBitsPerSample / 8 を指定します

				nAvgBytesPerSec:192000
				nBlockAlign:4
				nChannels:2 ステレオかモノラルか
				nSamplesPerSec:48000 サンプリングレート。１秒間に何個のサンプルをとるか
				wBitsPerSample:16 サンプルあたりのビット数を指定します
				wFormatTag:1

				計算式：
				nBlockAlign = nChannels * wBitsPerSample / 8
				nAvgBytesPerSec = nSamplesPerSec * nBlockAlign
			*/

			try	
			{
				//ビデオデータのサンプリングに利用するコールバック メソッドを指定する．
					//第一引数	 ISampleGrabberCB インターフェイスへのポインタ
					//			 nullを指定するとコールバックを解除
					//第二引数	0->ISampleGrabberCB.SampleCB メソッドを利用 	
					//			1->ISampleGrabberCB.BufferCB メソッドを利用
				result = mAudioSampleGrabber.SetCallback( this, 1 );
			}
			catch(Exception e)
			{
				MyMessageBox.Show("エラー：音声データのサンプリングコールバックの実行に失敗しました。Error: Failed to run sampling callback of audio data." + e.ToString(), MyMessageBoxIcon.Error);
			}
			return result;
		}

		//フレームキャプチャ完了時に呼び出されるコールバック関数
		int ISampleGrabberCB.SampleCB(double SampleTime, IMediaSample pSample)
		{
			return 0;
		}

		//フレームキャプチャ完了時に呼び出されるコールバック関数
		int ISampleGrabberCB.BufferCB(double SampleTime, IntPtr pBuffer, int BufferLength)
		{
			// コールバックの処理中なら終了
			if (mProcessing) {
				return 0;
			}

			mProcessing = true;

			mFrameArray = new byte[mAudioInfoHeader.nAvgBytesPerSec / 100];

			// メモリ内のサンプリングされたデータ(pBuffer)を配列(frameArray)にコピーする．
			if ((pBuffer != IntPtr.Zero) && (BufferLength <= mFrameArray.Length)) {
				Marshal.Copy(pBuffer, mFrameArray, 0, BufferLength);
			}

			//Console.WriteLine("SampleTime:" + SampleTime);
			//Console.WriteLine("BufferLength:" + BufferLength);

			try {
				// CaptureDoneデリゲードを呼び出す．
				mForm1.BeginInvoke(new CaptureDone(this.OnCaptureDone), new object[] { SampleTime, BufferLength });
			} catch (Exception e) {
				MyMessageBox.Show("エラー：音声データのサンプリング完了時の処理に失敗しました。Error: Failed to process when complete sampling of audio data." + e.ToString(), MyMessageBoxIcon.Error);
			}

			return 0;
		}

		//キャプチャしたデータの配列をメモリ空間内に固定し，ビットマップに変換する． 
		void OnCaptureDone(double SampleTime, int BufferLength)
		{
			try {
				if(mAudioSampleGrabber == null) {
					return;
				}

				//コールバック関数の利用を停止する
				mAudioSampleGrabber.SetCallback(null, 0);

				// 絶対値を合計する
				int sum = 0;
				int count = 0;
				for (int index = 0; index < BufferLength; index += mAudioInfoHeader.nBlockAlign) {
					short sample = getAbsAmplitude(index, mFrameArray);
					sum += sample;

					//Console.Write(sample + " ");
					++count;
				}
				int ave = sum / count;
				mSoundHistories.Add(new KeyValuePair<double, int>(SampleTime, ave));

				mFrameArray = null;
				mProcessing = false;

				return;
			} catch (Exception e) {
				MyMessageBox.Show("エラー：音声のフレームのキャプチャ（Grab）に失敗しました。Error: Failed to capture the frame of audio.(Grab) " + e.ToString(), MyMessageBoxIcon.Error);
			}
		}

		private short getAbsAmplitude(int index, byte[] data) {
			if (mAudioInfoHeader.nChannels == 2) {
				// ステレオ　右と左の平均値を取る
				if (mAudioInfoHeader.wBitsPerSample == 16) {
					// 16bit
					return (short)((
						Math.Abs((short)((data[index + 1] << 8) | data[index + 0])) +
						Math.Abs((short)((data[index + 3] << 8) | data[index + 2]))
						) / 2);
				} else {
					// 8bit
					return Math.Abs((short)((data[index + 1] + data[index + 0]) / 2));
				}
			} else {
				// モノラル
				if (mAudioInfoHeader.wBitsPerSample == 16) {
					// 16bit
					return (short)Math.Abs((data[index + 1] << 8) | data[index + 0]);
				} else {
					// 8bit
					return Math.Abs(data[index]);
				}
			}
		}
		
		// 蓄積した過去のデータのうち、古いものを捨てる
		public void OptimizeHistory() {
			double maxSampleTime = -1.0;
			for (int i = mSoundHistories.Count - 1; i >= 0; --i) {
				KeyValuePair<double, int> data = mSoundHistories[i];
				if (maxSampleTime < 0) {
					maxSampleTime = data.Key;
				}
				if (data.Key < maxSampleTime - DeathSetting.Instance.PastTimeRange) {
					mSoundHistories.RemoveAt(i);
				}
			}
		}

		private bool isDead(int amplitude) {
			// 音声がしきい値より小さいときは死んでる
			return amplitude <= DeathSetting.Instance.DeadAmplitudeThreshold;
		}

		public bool isDead() {

			if (mSoundHistories.Count == 0) {
				return false;
			}

			// 過去の数件分の全てのデータにおいて死んでいるか
			foreach (KeyValuePair<double, int> sample in mSoundHistories) {
				if (!isDead(sample.Value)) {
					return false;
				}
			}

			return true;
		}

	}
}
