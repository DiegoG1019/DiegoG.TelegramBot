using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading;
using DiegoG.TelegramBot.Types;
using DiegoG.Utilities;
using System.Collections.Concurrent;
using Telegram.Bot;
using Serilog;
using Telegram.Bot.Exceptions;
using Serilog.Events;

namespace DiegoG.TelegramBot
{
    public class MessageQueue
    {
        class TResultCapsule<TResult>
        {
            public bool IsReady { get; set; } = false;
            public TResult Result { get; set; }
        }

        /// <summary>
        /// The limit of requests per minute that can be issued
        /// </summary>
        public int ApiSaturationLimit { get; set; }

        public enum MessageSinkStatus
        {
            Inactive,
            Active,
            Stopping,
            Stopped,
            ForceStopping,
            ForceStopped
        }

        public delegate Task BotAction(TelegramBotClient bot);
        public delegate Task<TResult> BotFunc<TResult>(TelegramBotClient bot);

        public MessageSinkStatus QueueStatus { get; private set; } = MessageSinkStatus.Inactive;

        private ConcurrentQueue<BotAction> BotActionQueue { get; set; }

        private Thread SenderThread;

        public void EnqueueAction(BotAction action)
            => BotActionQueue.Enqueue(action);

        public async Task<TResult> EnqueueFunc<TResult>(BotFunc<TResult> func)
        {
            var result = new TResultCapsule<TResult>();
            EnqueueAction(async b =>
            {
                result.Result = await func(b);
                result.IsReady = true;
            });

            while (!result.IsReady)
                await Task.Delay(200);
            return result.Result;
        }

        public void Stop()
            => QueueStatus = MessageSinkStatus.Stopping;

        public void ForceStop()
            => QueueStatus = MessageSinkStatus.ForceStopping;

        public TelegramBotClient BotClient { get; private set; }
        public MessageQueue(TelegramBotClient client, int apiSaturationLimit)
        {
            BotClient = client;
            BotActionQueue = new();
            SenderThread = new(Sender);

            for (int i = 0; i < 21; i++)
                Requests.Enqueue(DateTime.Now);
            while (!Requests.IsEmpty)
                Requests.TryDequeue(out _);

            SenderThread.Start();
            QueueStatus = MessageSinkStatus.Active;

            ApiSaturationLimit = apiSaturationLimit;
        }

        const int StandardWait = 500;
        const int FailureWait = 60_000;
        int Wait___ = StandardWait;
        int Wait
        {
            get
            {
                var t = Wait___;
                Wait___ = StandardWait;
                return t;
            }
            set => Wait___ = value;
        }

        private readonly ConcurrentQueue<DateTime> Requests = new();

        private readonly TimeSpan TM_ = TimeSpan.FromMinutes(1);
        private ref readonly TimeSpan OneMinute => ref TM_;

        private async void Sender()
        {
            AsyncTaskManager tasks = new();

            while(QueueStatus is MessageSinkStatus.Active)
            {
                if (CheckForceStopping())
                    return;

                Thread.Sleep(Wait);

                var start = Requests.Count;

                while (!Requests.IsEmpty)
                {
                    if (CheckForceStopping())
                        return;

                    if (Requests.TryPeek(out var x) && DateTime.Now - x >= OneMinute)
                    {
                        Requests.TryDequeue(out _);
                        continue;
                    }

                    break;
                }

                var now = Requests.Count;
                if (start != now)
                    Log.Verbose($"{start - now} requests cooled down, {now} still hot");

                try
                {
                    while (Requests.Count < ApiSaturationLimit && BotActionQueue.TryDequeue(out var action))
                    {
                        if (CheckForceStopping())
                            return;

                        await Task.Delay(100);
                        Requests.Enqueue(DateTime.Now);
                        tasks.Run(() => action(BotClient));
                    }
                    if(tasks.Count > 0)
                        Log.Verbose($"Fired {tasks.Count} new requests, {BotActionQueue.Count} still queued");
                    await tasks;
                }
                catch(ApiRequestException e)
                {
                    Log.Fatal($"An while executing the queued actions, too many API Requests, some data may have been lost: {e}");
                    tasks.Clear();
                    Wait = FailureWait;
                }
                catch(Exception e)
                {
                    Log.Error($"An error ocurred while executing the queued actions, some data may have been lost: {e}");
                    tasks.Clear();
                }
            }

            if (QueueStatus is not MessageSinkStatus.ForceStopped)
                QueueStatus = MessageSinkStatus.Stopped;

            bool CheckForceStopping()
            {
                if (QueueStatus is MessageSinkStatus.ForceStopping)
                {
                    Log.Information("BotActionQueue was forcefully stopped");
                    QueueStatus = MessageSinkStatus.ForceStopped;
                    return true;
                }
                return false;
            }
        }
    }
}
