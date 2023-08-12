using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace DiegoG.TelegramBot;

public class MessageQueue
{
    private class TResultCapsule<TResult>
    {
        public readonly SemaphoreSlim Semaphore = new(1, 1);
        public Exception? Exception;
        public TResult? Result;
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

    private readonly Thread SenderThread;

    public void EnqueueAction(BotAction action)
        => BotActionQueue.Enqueue(action);

    public async Task<TResult> EnqueueFunc<TResult>(BotFunc<TResult> func)
    {
        var result = new TResultCapsule<TResult>();
        result.Semaphore.Wait();
        EnqueueAction(async b =>
        {
            try
            {
                result.Result = await func(b);
            }
            catch (Exception e)
            {
                result.Exception = e;
            }
            finally
            {
                result.Semaphore.Release();
            }
        });

        await result.Semaphore.WaitAsync();
        return result.Exception is Exception e ? throw new ApplicationException("An exception was thrown while executing the function", e) : result.Result!;
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

    private const int StandardWait = 500;
    private const int FailureWait = 2000;
    private const int TooManyRequestsWait = 60_000;
    private int Wait___ = StandardWait;

    private int Wait
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

    private readonly TimeSpan OneMinute = TimeSpan.FromMinutes(1);

    private async void Sender()
    {
        List<Task> tasks = new();

        while (QueueStatus is MessageSinkStatus.Active)
        {
            try
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
                        tasks.Add(Task.Run(() => action(BotClient)));
                    }
                    if (tasks.Count > 0)
                        Log.Verbose($"Fired {tasks.Count} new requests, {BotActionQueue.Count} still queued");
                    await Task.WhenAll(tasks);
                    tasks.Clear();
                }
                catch (ApiRequestException e)
                {
                    if (e.HttpStatusCode is System.Net.HttpStatusCode.TooManyRequests || e.ErrorCode == (int)System.Net.HttpStatusCode.TooManyRequests)
                    {
                        Log.Fatal(e, "An error ocurred while executing the queued actions: too many API Requests. Some data may have been lost");
                        Wait = TooManyRequestsWait;
                    }
                    else
                    {
                        Log.Fatal(e, "An error ocurred while executing the queued actions, some data may have been lost");
                        Wait = FailureWait;
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "An error ocurred while executing the queued actions, some data may have been lost");
                    tasks.Clear();
                    Wait = FailureWait;
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "An error ocurred while executing the queued actions, some data may have been lost");
                tasks.Clear();
                Wait = FailureWait;
            }
        }

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
