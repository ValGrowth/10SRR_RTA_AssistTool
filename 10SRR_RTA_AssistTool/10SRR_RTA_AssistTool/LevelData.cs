using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenSRR_RTA_AssistTool
{
    class LevelData
    {
        public string mLevelNo; // コースごとに一意の文字列
        public int mSerialIdx; // 連続で同じコースをプレイするときのインデックス
        public string mLevelCode; // チャート内で一意の文字列（LevelNoとSerialIdxを組み合わせたもの）
        public string mJpTitle;
        public string mEnTitle;
        public int mReward; // 報酬コイン枚数
        public int mInLevelCoin; // コース内コイン枚数
        public int mTotalCoin; // Reward + InLevelCoin
        public int mCumulativeCoin; // 累計コイン枚数
        public int mNeededCoin; // 城建設で消費するコイン
        public Bitmap mImage;
        public FastBitmap mTitleImage; // タイトルが表示される画面の画像
        public string mCastleList; // 城建設リスト（名称とコマンド）
        public string mLevelSelectCommand; // コース選択コマンド
        public Tuple<int, int> mAllowedLoss; // 許されたコインロス
        public Tuple<int, int> mFinalAllowedLoss; // 最終コースで許されたコインロス
        public int mAdditionalCoin; // ピーチ城パートで取得するコイン
        public string mRemark; // 備考
        public LevelData mNextLevel;

        public LevelData()
        {
            mLevelNo = "";
            mSerialIdx = 0;
            mLevelCode = "";
            mJpTitle = "";
            mEnTitle = "";
            mReward = 0;
            mInLevelCoin = 0;
            mTotalCoin = 0;
            mCumulativeCoin = 0;
            mNeededCoin = 0;
            mImage = null;
            mTitleImage = null;
            mCastleList = "";
            mLevelSelectCommand = "";
            mAllowedLoss = new Tuple<int, int>(0, 0);
            mFinalAllowedLoss = new Tuple<int, int>(0, 0);
            mAdditionalCoin = 0;
            mRemark = "";
            mNextLevel = null;
        }

        public void LoadAsMinimum(List<string> list)
		{
            mLevelNo = list[0];
            mJpTitle = list[1];
            mEnTitle = list[2];
            string imagePath = "./Images/Levels/" + list[3];
            try
			{
                mImage = new Bitmap(imagePath);
                mTitleImage = new FastBitmap(mImage);
            } catch (Exception e)
			{
                Console.WriteLine("画像ファイルが見つかりませんでした。[" + list[3] + "]");
                mImage = null;
                mTitleImage = null;
            }

            mSerialIdx = -1;
            mLevelCode = "";
            mReward = -1;
            mInLevelCoin = -1;
            mTotalCoin = -1;
            mCumulativeCoin = -1;
            mNeededCoin = -1;
            mCastleList = "";
            mLevelSelectCommand = "";
            mAllowedLoss = new Tuple<int, int>(-1, -1);
            mFinalAllowedLoss = new Tuple<int, int>(-1, -1);
            mAdditionalCoin = -1;
            mRemark = "";
            mNextLevel = null;
        }

        public LevelData(List<string> list)
        {
            mLevelNo = list[0];
            if (!int.TryParse(list[1], out mSerialIdx))
            {
                mSerialIdx = -1;
            }
            mLevelCode = GetLevelCode(mLevelNo, mSerialIdx);
            mJpTitle = list[2];
            mEnTitle = list[3];
            if (!int.TryParse(list[4], out mReward))
            {
                mReward = 0;
            }
            if (!int.TryParse(list[5], out mInLevelCoin))
            {
                mInLevelCoin = 0;
            }
            mTotalCoin = mReward + mInLevelCoin;
            if (!int.TryParse(list[6], out mCumulativeCoin))
            {
                mCumulativeCoin = 0;
            }
            if (!int.TryParse(list[7], out mNeededCoin))
            {
                mNeededCoin = 0;
            }
            mCastleList = list[8];
            mLevelSelectCommand = list[9];
            string imagePath = "./Images/Levels/" + list[10];
            try
            {
                mImage = new Bitmap(imagePath);
                mTitleImage = new FastBitmap(mImage);
            }
            catch (Exception e)
            {
                Console.WriteLine("画像ファイルが見つかりませんでした。[" + list[3] + "]");
                mImage = null;
                mTitleImage = null;
            }
            if (!int.TryParse(list[11], out mAdditionalCoin))
            {
                mAdditionalCoin = 0;
            }
            mRemark = list[12];
            mNextLevel = null;
        }

        public static string GetLevelCode(string levelNo, int serialIdx)
        {
            if (levelNo == "" || serialIdx <= 0)
            {
                return "";
            }
            if (serialIdx == 1)
            {
                return levelNo;
            }
            else if (serialIdx >= 2)
            {
                return levelNo + "-" + serialIdx;
            }
            return "";
        }

    }
}
