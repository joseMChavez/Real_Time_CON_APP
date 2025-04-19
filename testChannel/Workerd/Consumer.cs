using testChannel.Services;

namespace testChannel.Workerd
{
    public class Consumer(QueueTask<string> queue, ILogger<Consumer> logger) : BackgroundService
    {
        private readonly QueueTask<string> _queue = queue;
        private readonly ILogger<Consumer> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var message = await _queue.Reader.ReadAsync(stoppingToken);
                // Process the message
                if (!string.IsNullOrEmpty(message))
                {
                    await WebSocketServer.BroadcastMessageAsync(message);
                    _logger.LogInformation($"Processing message: {message}");
                }
                
                // Simulate some work
                await Task.Delay(1000, stoppingToken);
            }
        }
    }   
    
}
