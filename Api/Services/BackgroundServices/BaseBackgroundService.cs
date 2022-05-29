using System;
using System.Threading;
using System.Threading.Tasks;
using Api.Exceptions;
using Microsoft.Extensions.Hosting;

namespace Api.Services.BackgroundServices
{
    public abstract class BaseBackgroundService : BackgroundService
    {
        /// <summary>
        /// Interval between <see cref="DoWork"/> calls (in milliseconds)
        /// </summary>
        protected abstract int ExecutionInterval { get; }

        private TaskStatus DoWorkStatus { get; set; } = TaskStatus.Created;
    
        protected abstract Task DoWork(CancellationToken cancellationToken);
        
        public async Task RunManually(CancellationToken cancellationToken)
        {
            if (this.DoWorkStatus is TaskStatus.Running)
            {
                throw new TaskRunningException("DoWork already in progress");
            }

            try
            {
                this.DoWorkStatus = TaskStatus.Running;
                await DoWork(cancellationToken);
                this.DoWorkStatus = TaskStatus.RanToCompletion;
            }
            catch (OperationCanceledException)
            {
                this.DoWorkStatus = TaskStatus.Canceled;
                    
                // catch the cancellation exception
                // to stop execution
                return;
            }
            catch (Exception e)
            {
                this.DoWorkStatus = TaskStatus.Faulted;
                    
                Console.WriteLine(e.ToString());
            }
            
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // keep looping until we get a cancellation request
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (this.DoWorkStatus is not TaskStatus.Running)
                    {
                        // Add repeating code here
                        this.DoWorkStatus = TaskStatus.Running;
                        await DoWork(cancellationToken);
                        this.DoWorkStatus = TaskStatus.RanToCompletion;
                    }

                    // add a delay to not run in a tight loop
                    await Task.Delay(this.ExecutionInterval, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    this.DoWorkStatus = TaskStatus.Canceled;
                    
                    // catch the cancellation exception
                    // to stop execution
                    return;
                }
                catch (Exception e)
                {
                    this.DoWorkStatus = TaskStatus.Faulted;
                    
                    Console.WriteLine(e.ToString());
                    // add a delay to not run in a tight loop
                    await Task.Delay(this.ExecutionInterval, cancellationToken);
                }
            }
        }
    }
}