﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcTools.Utils.Helpers {
    public class TaskCache {
        private Dictionary<long, Task> _running = new Dictionary<long, Task>();

        public Task Get(Func<Task> fn, params object[] arguments) {
            lock (_running) {
                var checksum = arguments.Aggregate<object, long>(0, (current, t) => (current * 397) ^ t.GetHashCode());
                if (_running.TryGetValue(checksum, out var running) && !running.IsCanceled && !running.IsCompleted && !running.IsFaulted) {
                    return running;
                }

                var task = fn();
                if (!task.IsCanceled && !task.IsCompleted && !task.IsFaulted) {
                    _running[checksum] = task;
                    task.ContinueWith(v => {
                        lock (_running) {
                            _running.Remove(checksum);
                        }
                    });
                }

                return task;
            }
        }

        public Task<T> Get<T>(Func<Task<T>> fn, params object[] arguments) {
            lock (_running) {
                var checksum = arguments.Aggregate<object, long>(typeof(T).Name.GetHashCode(), (current, t) => (current * 397) ^ t.GetHashCode());
                if (_running.TryGetValue(checksum, out var running) && !running.IsCanceled && !running.IsCompleted && !running.IsFaulted) {
                    return (Task<T>)running;
                }

                var task = fn();
                if (!task.IsCanceled && !task.IsCompleted && !task.IsFaulted) {
                    _running[checksum] = task;
                    task.ContinueWith(v => {
                        lock (_running) {
                            _running.Remove(checksum);
                        }
                    });
                }

                return task;
            }
        }
    }
}