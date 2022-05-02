using System.Net;

namespace Pwa.Utility
{
    public class ProgressiveStreamContent : StreamContent
    {
        // Define the variables which is the stream that represents the file
        private readonly Stream _fileStream;

        private const int defaultBufferSize = 4096;

        public event Action<long, double> OnProgress;

        private int _bufferSize;

        public ProgressiveStreamContent(Stream stream, Action<long, double> progress) : this(stream,
            defaultBufferSize, progress)
        {
        }

        public ProgressiveStreamContent(Stream stream, int bufferSize, Action<long, double> onProgress) : base(stream)
        {
            _fileStream = stream;
            _bufferSize = bufferSize;
            OnProgress += onProgress;
        }

        //Override the SerialzeToStreamAsync method which provides us with the stream that we can write our chunks into it
        protected async override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            // Variable that holds the amount of uploaded bytes
            var totalLength = _fileStream.Length;

            // Define an array of bytes with the the length of the maximum amount of bytes to be pushed per time
            _bufferSize = (int)Math.Min(_bufferSize, totalLength);
            var buffer = new byte[_bufferSize];

            long uploaded = 0;

            // Create an while loop that we will break it internally when all bytes uploaded to the server
            while (true)
            {
                using (_fileStream)
                {
                    // In this part of code here in every loop we read a chunk of bytes and write them to the stream of the HttpContent
                    var length = await _fileStream.ReadAsync(buffer, 0, _bufferSize);

                    // Check if the amount of bytes read recently, if there is no bytes read break the loop
                    if (length <= 0) break;

                    // Add the amount of read bytes to uploaded variable
                    uploaded += length;

                    // Calculate the percntage of the uploaded bytes out of the total remaining
                    var percentage = Convert.ToDouble(uploaded * 100 / _fileStream.Length);

                    // Write the bytes to the HttpContent stream

                    await stream.WriteAsync(buffer.AsMemory(0, length));

                    // Fire the event of OnProgress to notify the client about progress so far
                    OnProgress?.Invoke(uploaded, percentage);

                    stream.Flush();
                    // Add this delay over here just to simulate to notice the progress, because locally it's going to be so fast that you can barely notice it
                    await Task.Delay(250);
                }
            }
            stream.Flush();
        }
    }
}
