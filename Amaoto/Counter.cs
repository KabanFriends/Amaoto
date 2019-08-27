﻿using System;
using DxLibDLL;

namespace Amaoto
{
    /// <summary>
    /// カウンタークラス。
    /// </summary>
    public class Counter
    {
        /// <summary>
        /// カウンターを初期化します。
        /// </summary>
        /// <param name="begin">開始値。</param>
        /// <param name="end">終了値。</param>
        /// <param name="interval">Tickする間隔(マイクロ秒)。</param>
        /// <param name="isLoop">ループするか否か。</param>
        public Counter(long begin, long end, long interval, bool isLoop)
        {
            NowTime = DX.GetNowHiPerformanceCount();
            Begin = begin;
            End = end;
            Interval = interval;
            Value = begin;
            IsLoop = isLoop;
            State = TimerState.Stopped;
        }

        /// <summary>
        /// Tickします。
        /// </summary>
        /// <returns>何Tickしたか。</returns>
        public long Tick()
        {
            // 何Tickしたかのカウント
            var tickCount = 0;
            var nowTime = DX.GetNowHiPerformanceCount();

            // 停止状態の場合、現在時間をそのままプロパティに代入して、終わる。
            if (State == TimerState.Stopped)
            {
                NowTime = nowTime;
                return 0;
            }

            // 現在時間から以前Tick()したまでの時間の差
            var diffTime = nowTime - NowTime;
            while (diffTime >= Interval)
            {
                // 時間の差が間隔未満になるまで進める
                Value++;
                tickCount++;
                if (Value >= End)
                {
                    if (IsLoop)
                    {
                        // ループ設定かつ現在の値が終了値より大きかったら
                        Value = Begin;
                        Looped?.Invoke();
                    }
                    else
                    {
                        // 非ループ設定かつ現在の値が終了値より大きかったら、終了値を維持してタイマーを停止する。
                        Value = End;
                        Stop();
                    }
                }
                diffTime -= Interval;
            }
            // 余ったdiffTimeを引いて、次Tick()したときにちゃんとなるように
            NowTime = nowTime - diffTime;
            return tickCount;
        }

        /// <summary>
        /// タイマーを開始します。必ずこのメソッドを読んでください。
        /// </summary>
        public void Start()
        {
            if (State == TimerState.Started)
            {
                // すでに開始しているなら、何もしない。
                return;
            }

            // Tick()を呼び出して、NowTimeに現在の時間を代入させてからタイマーを開始する。
            Tick();
            State = TimerState.Started;
        }

        /// <summary>
        /// タイマーを停止します。
        /// </summary>
        public void Stop()
        {
            if (State == TimerState.Stopped)
            {
                // すでに停止しているなら、何もしない。
                return;
            }

            State = TimerState.Stopped;
        }

        /// <summary>
        /// タイマーをリセットします。
        /// </summary>
        public void Reset()
        {
            // 現在時間を入れる。
            NowTime = DX.GetNowHiPerformanceCount();
            // カウンターを最小値に戻す。
            Value = Begin;
        }

        /// <summary>
        /// タイマーのTick間隔を変更します。
        /// </summary>
        /// <param name="interval">Tickする間隔(マイクロ秒)。</param>
        public void ChangeInterval(long interval)
        {
            // 今までのカウンター値を更新する。
            Tick();
            // 間隔を更新する。
            Interval = interval;
            // 間隔更新後、あまりがあるかもしれないのでもう一度カウンター値を更新する。
            Tick();
        }

        /// <summary>
        /// ループした場合、イベントが発生します。
        /// </summary>
        public event Action Looped;
        /// <summary>
        /// 現在のコンピュータの時間(マイクロ秒)。
        /// </summary>
        public long NowTime { get; private set; }
        /// <summary>
        /// 開始値。
        /// </summary>
        public long Begin { get; }
        /// <summary>
        /// 終了値。
        /// </summary>
        public long End { get; }
        /// <summary>
        /// タイマー間隔。
        /// </summary>
        public long Interval { get; private set; }
        /// <summary>
        /// カウンターの現在の値。
        /// </summary>
        public long Value
        {
            get
            {
                return _value;
            }
            set
            {
                // 最小値・最大値を超える場合、丸める。
                if (value < Begin)
                {
                    _value = Begin;
                    return;
                }
                if (End < value)
                {
                    _value = End;
                    return;
                }
                _value = value;
            }
        }
        /// <summary>
        /// ループするかどうか。
        /// </summary>
        public bool IsLoop { get; }
        /// <summary>
        /// 現在の状態。
        /// </summary>
        public TimerState State { get; private set; }
        private long _value { get; set; }
    }

    /// <summary>
    /// タイマーの状態。
    /// </summary>
    public enum TimerState
    {
        /// <summary>
        /// 停止している。
        /// </summary>
        Stopped,
        /// <summary>
        /// 動作している。
        /// </summary>
        Started
    }
}
