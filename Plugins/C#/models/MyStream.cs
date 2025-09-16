using System.IO;

namespace Plugins.C_.models
{
    public class MyStream : FileStream
    {
        private const byte Key = 64;

        public MyStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync) :
            base(path, mode, access, share, bufferSize, useAsync)
        {
        }

        public MyStream(string path, FileMode mode) : base(path, mode)
        {
        }

        public override int Read(byte[] array, int offset, int count)
        {
            var index = base.Read(array, offset, count);
            for (int i = 0; i < array.Length; i++)
            {
                array[i] ^= Key;
            }

            return index;
        }

        public override void Write(byte[] array, int offset, int count)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] ^= Key;
            }

            base.Write(array, offset, count);
        }
    }
}
