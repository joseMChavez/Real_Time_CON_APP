using System.Threading.Channels;

namespace testChannel.Services
{
    public class QueueTask<T>
    {
        private readonly Channel<T> _channel;
        public ChannelReader<T> Reader => _channel.Reader;
        public QueueTask(int estimateSize=1000)
        {
            // Create a bounded channel with a specified capacity   

            BoundedChannelOptions channelOptions = new(estimateSize)
            {
                FullMode = BoundedChannelFullMode.Wait
            };  
            _channel = Channel.CreateBounded<T>(channelOptions);
        }
        public async Task EnqueueAsync(T item)
        =>await _channel.Writer.WriteAsync(item);
          
    }
}
