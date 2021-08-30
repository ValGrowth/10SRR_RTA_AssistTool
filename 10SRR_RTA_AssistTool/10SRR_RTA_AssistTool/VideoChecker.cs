using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

using static TenSRR_RTA_AssistTool.MyMessageBox;

using DirectShowLib;


namespace TenSRR_RTA_AssistTool {
	class VideoChecker : ISampleGrabberCB {

		private IBaseFilter mVideoCaptureFilter; //ソースフィルタ
		private IBaseFilter mVideoGrabFilter; //Grabber Filterのインタフェース．(映像用)
		private ISampleGrabber mVideoSampleGrabber; //フィルタグラフ内を通る個々のデータ取得用のインタフェース．(映像用)
		private VideoInfoHeader mVideoInfoHeader; // ビデオイメージのフォーマットを記述する構造体

		private Form1 mForm1;

		private delegate void CaptureDone(double SampleTime);//静止画キャプチャ終了時の処理を行うデリゲード
        private byte[] mFrameArray;//キャプチャしたフレームデータ用の配列
        private bool mProcessing = false;	// キャプチャしたデータの処理中フラグ

		private VideoAnalyzer mVideoAnalyzer = new VideoAnalyzer();
		private VideoGameState mVideoGameState = new VideoGameState();

		public VideoChecker(Form1 form1) {
			mForm1 = form1;
		}

		//COMやインタフェースの解放を行う
		public void CloseInterfaces()
		{
			try
			{

				if(mVideoGrabFilter != null) mVideoGrabFilter = null;

				if(mVideoSampleGrabber != null)
				{
					Marshal.ReleaseComObject(mVideoSampleGrabber);
					mVideoSampleGrabber = null;
				}

				if(mVideoCaptureFilter != null)
				{
					mVideoCaptureFilter.Stop();
					Marshal.ReleaseComObject(mVideoCaptureFilter);
					mVideoCaptureFilter = null;
				}


			}
			catch(Exception e)
			{
				MyMessageBox.Show("エラー：映像インタフェースの終了に失敗しました。Error: An error ocouured when closing video interface. " + e.ToString(), MyMessageBoxIcon.Error);
			}
		}

		public bool AddVideoFilters(IGraphBuilder graphBuilder, ICaptureGraphBuilder2 captureGraphBuilder) {

			bool ret = SetDevice();
			if (!ret) {
				return false;
			}

			//サンプルグラバ(videoSampleGrabber)を作成
			mVideoSampleGrabber = GraphFactory.MakeSampleGrabber();

			//フィルタと関連付ける．
			mVideoGrabFilter = (IBaseFilter) mVideoSampleGrabber;

			//キャプチャするビデオデータのフォーマットを設定．
			AMMediaType amMediaType = new AMMediaType();
			amMediaType.majorType = MediaType.Video;
			amMediaType.subType = MediaSubType.RGB24;
			amMediaType.formatType = FormatType.VideoInfo; 

			int result;
			result = mVideoSampleGrabber.SetMediaType(amMediaType);
			if(result < 0) Marshal.ThrowExceptionForHR(result);

			// 映像のcaptureFilter(ソースフィルタ)をgraphBuilder（フィルタグラフマネージャ）に追加．
			result = graphBuilder.AddFilter(mVideoCaptureFilter, "Video Capture Device");
			if(result < 0) Marshal.ThrowExceptionForHR(result);

			//videoGrabFilter(変換フィルタ)をgraphBuilder（フィルタグラフマネージャ）に追加．
			result = graphBuilder.AddFilter(mVideoGrabFilter, "Video Grab Filter");
			//result = graphBuilder.AddFilter(videoGrabFilter, "Frame Grab Filter");
			if(result < 0) Marshal.ThrowExceptionForHR(result);

			// 映像のキャプチャフィルタをサンプルグラバーフィルタに接続する．
			result = captureGraphBuilder.RenderStream(PinCategory.Capture, MediaType.Video, mVideoCaptureFilter, null, mVideoGrabFilter);
			if(result < 0) Marshal.ThrowExceptionForHR(result);

			// 映像のキャプチャフィルタをデフォルトのレンダラフィルタ（ディスプレイ上に出力）に接続する．（プレビュー）
			result = captureGraphBuilder.RenderStream(PinCategory.Preview, MediaType.Video, mVideoCaptureFilter, null, null); 
			if(result < 0) Marshal.ThrowExceptionForHR(result);
			
			// フレームキャプチャの設定が完了したかを確認する．
			amMediaType = new AMMediaType();
			result = mVideoSampleGrabber.GetConnectedMediaType(amMediaType);
			if(result < 0) Marshal.ThrowExceptionForHR(result);
			if ((amMediaType.formatType != FormatType.VideoInfo) || (amMediaType.formatPtr == IntPtr.Zero)) {
				throw new NotSupportedException("エラー：キャプチャできない映像メディアフォーマットです．Error: This video media format cannnot be caputered.");
			}

			// キャプチャするビデオデータのフォーマットから，videoInfoHeaderを作成する．
			mVideoInfoHeader = (VideoInfoHeader) Marshal.PtrToStructure(amMediaType.formatPtr, typeof(VideoInfoHeader));
			Marshal.FreeCoTaskMem(amMediaType.formatPtr);
			amMediaType.formatPtr = IntPtr.Zero;

			// フィルタ内を通るサンプルをバッファにコピーしないように指定する．
			result = mVideoSampleGrabber.SetBufferSamples(false);
			// サンプルを一つ（1フレーム）受け取ったらフィルタを停止するように指定する．
			if(result == 0) result = mVideoSampleGrabber.SetOneShot(false);
			// コールバック関数の利用を停止する．
			if(result == 0) result = mVideoSampleGrabber.SetCallback(null, 0); 
			if(result < 0) Marshal.ThrowExceptionForHR(result);

			return true;
		}

		private bool SetDevice() {
			// 1. デバイスを取得
			DsDevice videoDevice = GraphFactory.GetDevice(FilterCategory.VideoInputDevice, GraphFactory.DEVICE_TYPE.VIDEO);

			if (videoDevice == null) {
				MyMessageBox.Show("エラー：映像デバイスが選択されませんでした。Error: Video device is not selected.", MyMessageBoxIcon.Warning);
				return false;
			}

            // 2. キャプチャデバイスをソースフィルタに対応づける．

			// 映像のキャプチャデバイス(device)とソースフィルタ(captureFilter)を対応付ける．
			mVideoCaptureFilter = GraphFactory.GetCaptureFilter(videoDevice);

			return true;
		}

		public int update() {

			if (mVideoSampleGrabber == null)
			{
				return -1;
			}

			int result = -1;
			try
			{
				//ビデオデータのサンプリングに利用するコールバック メソッドを指定する．
					//第一引数	 ISampleGrabberCB インターフェイスへのポインタ
					//			 nullを指定するとコールバックを解除
					//第二引数	0->ISampleGrabberCB.SampleCB メソッドを利用 	
					//			1->ISampleGrabberCB.BufferCB メソッドを利用
				result = mVideoSampleGrabber.SetCallback( this, 1 );
			}
			catch(Exception e)
			{
				MyMessageBox.Show("エラー：映像データのサンプリングコールバックの実行に失敗しました。Error: Failed to run sampling callback of video data." + e.ToString(), MyMessageBoxIcon.Error);
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

            // 1. キャプチャするフレームデータのサイズを設定する．
            int size = mVideoInfoHeader.BmiHeader.ImageSize; 
            mFrameArray = new byte [size + 64000]; 

			//メモリ内のサンプリングされたデータ(pBuffer)を配列(frameArray)にコピーする．
			if((pBuffer != IntPtr.Zero) && (BufferLength > 1000) && (BufferLength <= mFrameArray.Length)) {
				Marshal.Copy(pBuffer, mFrameArray, 0, BufferLength);
            }

			try
			{	
				//CaptureDoneデリゲードを呼び出す．
				mForm1.BeginInvoke(new CaptureDone(this.OnCaptureDone), new object[] { SampleTime });
			}
			catch(Exception e)
			{
				MyMessageBox.Show("エラー：映像データのサンプリング完了時の処理に失敗しました。Error: Failed to process when complete sampling of video data." + e.ToString(), MyMessageBoxIcon.Error);
			}
			return 0;
		}

		//キャプチャしたデータの配列をメモリ空間内に固定し，ビットマップに変換する． 
		void OnCaptureDone(double SampleTime)
		{
			try
			{
				int result;
				if(mVideoSampleGrabber == null) {
					mFrameArray = null;
					mProcessing = false;
					return;
				}

				//コールバック関数の利用を停止する
				result = mVideoSampleGrabber.SetCallback(null, 0);

				//フレームデータのサイズを取得
				int width = mVideoInfoHeader.BmiHeader.Width;
				int height = mVideoInfoHeader.BmiHeader.Height;

				//widthが4の倍数でない場合＆widthとheightの値が適正でない場合は終了．
				if( ((width & 0x03) != 0) || (width < 32) || (width > 4096) || (height < 32) || (height> 4096) ) {
					mFrameArray = null;
					mProcessing = false;
					return;
				}

				//stride(1ライン分のデータサイズ(byte)=width* 3(RGB))を設定．
				int stride = width * 3;

				//配列frameArrayのアドレスを，メモリ空間内で固定する．
				GCHandle gcHandle = GCHandle.Alloc(mFrameArray, GCHandleType.Pinned);
				
				int addr = (int) gcHandle.AddrOfPinnedObject();
				addr += (height - 1) * stride;
				
				//frameArrayを格納したメモリアドレスから，ビットマップデータを作成．
				Bitmap bitmap = new Bitmap(width, height, -stride, PixelFormat.Format24bppRgb, (IntPtr) addr);
				gcHandle.Free();

				// 画像を高速に扱えるオブジェクトに格納
				FastBitmap bitmapPlus = new FastBitmap(bitmap);

				// ゲームの状態を分析して更新
				bitmapPlus.BeginAccess();
				UpdateGameState(bitmapPlus);
				bitmapPlus.EndAccess();

/*
				Image pre = null;

				//画面上にキャプチャした画像を表示する．
			
				pre = this.pictureBox1.Image;
				this.pictureBox1.Image =  bitmap;
				

				if(pre != null) pre.Dispose();
*/
				mFrameArray = null;
				mProcessing = false;
				
				return;
			}
			catch(Exception e)
			{
				MyMessageBox.Show("エラー：映像のフレームのキャプチャ（Grab）に失敗しました。Error: Failed to capture the frame of video.(Grab) " + e.ToString(), MyMessageBoxIcon.Error);
			}
		}

		private void UpdateGameState(FastBitmap bitmap)
        {
			mVideoGameState = mVideoAnalyzer.Detect(bitmap);
		}

		public VideoGameState GetVideoGameState()
        {
			return mVideoGameState;
		}

	}
}
