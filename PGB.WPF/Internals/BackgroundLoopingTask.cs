namespace PGB.WPF.Internals
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal class BackgroundLoopingTask
    {
        private readonly AutoResetEvent _are;
        private volatile bool _running;
        private Task _task;

        public BackgroundLoopingTask(Action action)
        {
            Action = action;
            _are = new AutoResetEvent(false);
        }

        public TaskCreationOptions CreationOptions { get; set; } = TaskCreationOptions.LongRunning;

        public int MillisecondsDelay { get; set; } = 10;

        public Action Action { get; set; }

        public bool Running => _running;

        public Task Start()
        {
            return Task.Run(() =>
            {
                if (_running)
                {
                    return;
                }

                _running = true;
                _task = Task.Factory.StartNew(() =>
                {
                    while (!_are.WaitOne(MillisecondsDelay))
                    {
                        Action();
                    }

                    _running = false;
                }, CreationOptions);
            });
        }

        public Task Stop()
        {
            return Task.Run(() =>
            {
                if (!_running)
                {
                    return;
                }

                _are.Set();
                _task.Wait();
            });
        }
    }
}