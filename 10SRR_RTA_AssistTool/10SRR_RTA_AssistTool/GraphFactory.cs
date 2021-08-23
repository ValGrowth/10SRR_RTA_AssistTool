using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using static TenSRR_RTA_AssistTool.MyMessageBox;

using DirectShowLib;

namespace TenSRR_RTA_AssistTool {
	public static class GraphFactory {

		public enum DEVICE_TYPE {
			VIDEO,
			AUDIO,

			NONE,
		};

		public static IGraphBuilder MakeGraphBuilder() {

			// graphBuilderを作成．
			Type comType = Type.GetTypeFromCLSID(typeof(FilterGraph).GUID);
			if (comType == null)
				throw new NotImplementedException("DirectShowのFiterGraphオブジェクトが作成できません．");
			object comObject = Activator.CreateInstance(comType);
			return (IGraphBuilder)comObject;
		}

		public static ICaptureGraphBuilder2 MakeCaptureGraphBuilder() {

			//キャプチャグラフビルダ(captureGraphBuilder)を作成．
			Type comType = Type.GetTypeFromCLSID(typeof(CaptureGraphBuilder2).GUID);
			object comObject = Activator.CreateInstance(comType);
			return (ICaptureGraphBuilder2) comObject;
		}

		public static ISampleGrabber MakeSampleGrabber() {

			//サンプルグラバ(videoSampleGrabber)を作成
			Type comType = Type.GetTypeFromCLSID(typeof(SampleGrabber).GUID);
			object comObject = Activator.CreateInstance(comType);
			return (ISampleGrabber) comObject;
		}

		public static DsDevice GetDevice(Guid FilterCategory, DEVICE_TYPE deviceType) {
			string[] deviceNames = GetDeviceNameList(FilterCategory);
			if (deviceNames == null) {
				return null;
			}
			foreach (string deviceName in deviceNames) {
				Console.WriteLine(deviceName);
			}

			// まずオプションの設定値を取得する
			string optionDeviceName = "";
			foreach (string deviceName in deviceNames) {
				string defaultDeviceName = GetDefaultDeviceName(deviceType);
				if (deviceName == defaultDeviceName) {
					optionDeviceName = deviceName;
					break;
				}
			}
			if (!string.IsNullOrEmpty(optionDeviceName)) {
				DsDevice device = GetDeviceByName(FilterCategory, optionDeviceName);
				if (device != null) {
					return device;
				}
			}

			// オプションの設定値が現在の選択肢に無ければ機器選択ウィンドウを表示する
			ChooseDeviceWindow chooseDeviceWindow = new ChooseDeviceWindow(deviceNames, deviceType);
			chooseDeviceWindow.ShowDialog();
			string choosedDeviceName = chooseDeviceWindow.mDeviceName;

			return GetDeviceByName(FilterCategory, choosedDeviceName);
		}

		private static DsDevice GetDeviceByName(Guid FilterCategory, string deviceName) {
			DsDevice device = null;
			DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory);
			foreach (DsDevice d in devices) {
				if (d.Name == deviceName) {
					device = d as DsDevice;
					break;
				}
			}
			return device;
		}

		private static string GetDefaultDeviceName(DEVICE_TYPE deviceType) {
			if (deviceType == DEVICE_TYPE.VIDEO) {
				return Preferences.Instance.mVideoDeviceName;
			}
			if (deviceType == DEVICE_TYPE.AUDIO) {
				return Preferences.Instance.mAudioDeviceName;
			}
			return null;
		}

		public static string[] GetDeviceNameList(Guid FilterCategory) {
			DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory);

			//PCに接続されているキャプチャデバイス（USBカメラなど)のリストを取得．
			if(devices == null || devices.Length == 0) {
				MyMessageBox.Show("エラー：キャプチャ可能なデバイスが見つかりません。Error: Device that can be captured could not be found.", MyMessageBoxIcon.Error);
				return null;
			}
			string[] deviceNames = new string[devices.Length];
			for (int i = 0; i < devices.Length; ++i) {
				deviceNames[i] = devices[i].Name;
			}
			return deviceNames;
		}

		public static IBaseFilter GetCaptureFilter(DsDevice device) {
			object captureObject = null;

			// キャプチャデバイス(device)とソースフィルタ(captureFilter)を対応付ける．
			Guid GuidBF = typeof(IBaseFilter).GUID;
			device.Mon.BindToObject(null, null, ref GuidBF, out captureObject);
			return (IBaseFilter)captureObject;
		}


	}
}
