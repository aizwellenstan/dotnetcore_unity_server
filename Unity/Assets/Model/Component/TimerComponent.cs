using System.Collections.Generic;
using System.Threading;

namespace ETModel
{
	public struct Timer
	{
		public long Id { get; set; }
		public long Time { get; set; }
		public ETTaskCompletionSource tcs;
	}

    [ObjectSystem]
    public class TimerComponentAwakeSystem : AwakeSystem<TimerComponent>
    {
        public override void Awake(TimerComponent self)
        {
            self.Awake();
        }
    }

    [ObjectSystem]
	public class TimerComponentUpdateSystem : UpdateSystem<TimerComponent>
	{
		public override void Update(TimerComponent self)
		{
			self.Update();
		}
	}

	public class TimerComponent : Component
	{
		private readonly Dictionary<long, Timer> timers = new Dictionary<long, Timer>();

		/// <summary>
		/// key: time, value: timer id
		/// </summary>
		private readonly MultiMap<long, long> timeId = new MultiMap<long, long>();

		private readonly Queue<long> timeOutTime = new Queue<long>();
		
		private readonly Queue<long> timeOutTimerIds = new Queue<long>();

		// 记录最小时间，不用每次都去MultiMap取第一个值
		private long minTime;

        public float deltaTime { private set; get; }

        public float time { private set; get; }

        private long initialTicks { set; get; }

        private long lastTicks { set; get; }

        public void Awake()
        {
            initialTicks = System.DateTime.Now.Ticks;
            lastTicks = System.DateTime.Now.Ticks;
            EvaluateDeltaTime();
        }

        public void Update()
		{
            EvaluateDeltaTime();

            if (this.timeId.Count == 0)
			{
				return;
			}

			long timeNow = TimeHelper.ClientNowMilliSeconds();

			if (timeNow < this.minTime)
			{
				return;
			}
			
			foreach (KeyValuePair<long, List<long>> kv in this.timeId.GetDictionary())
			{
				long k = kv.Key;
				if (k > timeNow)
				{
					minTime = k;
					break;
				}
				this.timeOutTime.Enqueue(k);
			}

			while(this.timeOutTime.Count > 0)
			{
				long time = this.timeOutTime.Dequeue();
				foreach(long timerId in this.timeId[time])
				{
					this.timeOutTimerIds.Enqueue(timerId);	
				}
				this.timeId.Remove(time);
			}

			while(this.timeOutTimerIds.Count > 0)
			{
				long timerId = this.timeOutTimerIds.Dequeue();
				Timer timer;
				if (!this.timers.TryGetValue(timerId, out timer))
				{
					continue;
				}
				this.timers.Remove(timerId);
				timer.tcs.SetResult();
			}
		}

        private void EvaluateDeltaTime()
        {
            var ticks = System.DateTime.Now.Ticks;
            deltaTime = (ticks - lastTicks) / 10000000.0f;
            time = (ticks - initialTicks) / 10000000.0f;
            lastTicks = ticks;
        }

		private void Remove(long id)
		{
			this.timers.Remove(id);
		}

		public ETTask WaitTillAsync(long tillTime, CancellationToken cancellationToken)
		{
			ETTaskCompletionSource tcs = new ETTaskCompletionSource();
			Timer timer = new Timer { Id = IdGenerater.GenerateId(), Time = tillTime, tcs = tcs };
			this.timers[timer.Id] = timer;
			this.timeId.Add(timer.Time, timer.Id);
			if (timer.Time < this.minTime)
			{
				this.minTime = timer.Time;
			}
			cancellationToken.Register(() => { this.Remove(timer.Id); });
			return tcs.Task;
		}

		public ETTask WaitTillAsync(long tillTime)
		{
			ETTaskCompletionSource tcs = new ETTaskCompletionSource();
			Timer timer = new Timer { Id = IdGenerater.GenerateId(), Time = tillTime, tcs = tcs };
			this.timers[timer.Id] = timer;
			this.timeId.Add(timer.Time, timer.Id);
			if (timer.Time < this.minTime)
			{
				this.minTime = timer.Time;
			}
			return tcs.Task;
		}

		public ETTask WaitAsync(long time, CancellationToken cancellationToken)
		{
			ETTaskCompletionSource tcs = new ETTaskCompletionSource();
			Timer timer = new Timer { Id = IdGenerater.GenerateId(), Time = TimeHelper.ClientNowMilliSeconds() + time, tcs = tcs };
			this.timers[timer.Id] = timer;
			this.timeId.Add(timer.Time, timer.Id);
			if (timer.Time < this.minTime)
			{
				this.minTime = timer.Time;
			}
			cancellationToken.Register(() => { this.Remove(timer.Id); });
			return tcs.Task;
		}

        /// <summary>
        /// 等待指定(秒)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public ETTask WaitForSecondAsync(float time)
        {
            return WaitForMilliSecondAsync((long)System.Math.Round(time * 1000));
        }

        /// <summary>
        /// 等待指定(毫秒)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
		public ETTask WaitForMilliSecondAsync(long time)
		{
			ETTaskCompletionSource tcs = new ETTaskCompletionSource();
			Timer timer = new Timer { Id = IdGenerater.GenerateId(), Time = TimeHelper.ClientNowMilliSeconds() + time, tcs = tcs };
			this.timers[timer.Id] = timer;
			this.timeId.Add(timer.Time, timer.Id);
			if (timer.Time < this.minTime)
			{
				this.minTime = timer.Time;
			}
			return tcs.Task;
		}
	}
}