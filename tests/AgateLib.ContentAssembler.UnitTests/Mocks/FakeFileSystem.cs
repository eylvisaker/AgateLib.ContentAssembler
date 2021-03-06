﻿using AgateLib.ContentAssembler.Shims;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AgateLib.ContentAssembler.Mocks
{
    public class FakeFileSystem : IFileSystem
    {
        private Dictionary<string, string> files = new Dictionary<string, string>();
        private Dictionary<string, string> fileCopies = new Dictionary<string, string>();

        public FakeFileSystem(string pathSeparator = "/")
        {
            if (pathSeparator.Length != 1)
                throw new ArgumentException("Path separator must have length 1.");

            File = new FakeFile(this);
            Path = new FakePath(this);
            Directory = new FakeDirectory(this);
            PathSeparator = pathSeparator;
        }

        public Dictionary<string, string> FileContents => files;

        public void AddFile(string path, string contents)
        {
            files[path] = contents;
        }

        public void RemoveFile(string path)
        {
            files.Remove(path);
        }

        private void RecordFileCopy(string sourceFileName, string destFileName)
        {
            fileCopies[sourceFileName] = destFileName;
        }

        public IReadOnlyDictionary<string, string> FileCopies => fileCopies;

        public string PathRoot { get; set; }

        public IFile File { get; }

        public IDirectory Directory { get; }

        public IPath Path { get; }

        public string PathSeparator { get; }

        private class ObservableStream : Stream
        {
            private readonly Stream underlyingStream;

            public ObservableStream(Stream underlyingStream)
            {
                this.underlyingStream = underlyingStream;
            }

            public event Action Disposed;

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                Disposed?.Invoke();
            }

            public override bool CanRead => underlyingStream.CanRead;

            public override bool CanSeek => underlyingStream.CanSeek;

            public override bool CanWrite => underlyingStream.CanWrite;

            public override long Length => underlyingStream.Length;

            public override long Position
            {
                get => underlyingStream.Position;
                set => underlyingStream.Position = value;
            }

            public override void Flush()
                => underlyingStream.Flush();

            public override int Read(byte[] buffer, int offset, int count)
                => underlyingStream.Read(buffer, offset, count);

            public override long Seek(long offset, SeekOrigin origin)
                => underlyingStream.Seek(offset, origin);

            public override void SetLength(long value)
                => underlyingStream.SetLength(value);

            public override void Write(byte[] buffer, int offset, int count)
                => underlyingStream.Write(buffer, offset, count);
        }

        private class FakeFile : IFile
        {
            private FakeFileSystem fakeFileSystem;

            public FakeFile(FakeFileSystem fakeFileSystem)
            {
                this.fakeFileSystem = fakeFileSystem;
            }

            public string PathRoot { get; set; }

            public void Copy(string sourceFileName, string destFileName)
            {
                fakeFileSystem.RecordFileCopy(sourceFileName, destFileName);
                fakeFileSystem.files[destFileName] = fakeFileSystem.files[sourceFileName];
            }

            public void Delete(string path)
            {
                fakeFileSystem.files.Remove(path);
            }

            public bool Exists(string path) => fakeFileSystem.files.ContainsKey(path);

            public DateTime GetLastWriteTimeUtc(string path)
            {
                if (Exists(path))
                    return DateTime.Now;

                throw new FileNotFoundException(path);
            }

            public Stream Open(string path, FileMode mode, FileAccess access)
            {
                MemoryStream buffer;

                switch (mode)
                {
                    case FileMode.Create:
                    case FileMode.CreateNew:
                    case FileMode.Truncate:
                        buffer = new MemoryStream();
                        break;

                    default:
                        throw new NotSupportedException();
                }

                var result = new ObservableStream(buffer);

                result.Disposed += () =>
                {
                    fakeFileSystem.files[path] = Encoding.UTF8.GetString(buffer.GetBuffer());
                };

                return result;
            }

            public string ReadAllText(string fileName)
            {
                if (fakeFileSystem.files.TryGetValue(fileName, out string contents))
                {
                    return contents;
                }

                throw new FileNotFoundException();
            }

            public void WriteAllText(string path, string contents) => fakeFileSystem.files[path] = contents;
        }

        private class FakeDirectory : IDirectory
        {
            private FakeFileSystem fakeFileSystem;

            public FakeDirectory(FakeFileSystem fakeFileSystem)
            {
                this.fakeFileSystem = fakeFileSystem;
            }

            public void CreateDirectory(string outputPath)
            {
            }

            public IEnumerable<string> EnumerateDirectories(string path)
            {
                return fakeFileSystem.files.Keys
                    .Where(x => x.StartsWith(path))
                    .Where(x => x.Length > path.Length)
                    .Where(x => x.Substring(path.Length + 1).Contains(fakeFileSystem.PathSeparator))
                    .Select(x =>
                    {
                        int slash = x.IndexOf(fakeFileSystem.PathSeparator, path.Length + 1);
                        if (slash >= 0)
                        {
                            return x.Substring(0, slash);
                        }
                        return null;
                    })
                    .Where(x => x != null)
                    .Distinct();
            }

            public IEnumerable<string> EnumerateFiles(string path)
            {
                if (!path.EndsWith(fakeFileSystem.PathSeparator))
                    path += fakeFileSystem.PathSeparator;

                return fakeFileSystem.files.Keys
                    .Where(x => x.StartsWith(path))
                    .Where(x => x.Length > path.Length)
                    .Where(x => !x.Substring(path.Length + 1).Contains(fakeFileSystem.PathSeparator));
            }
        }

        private class FakePath : IPath
        {
            private FakeFileSystem fakeFileSystem;

            public FakePath(FakeFileSystem fakeFileSystem)
            {
                this.fakeFileSystem = fakeFileSystem;
            }

            public string Combine(string path1, string path2)
            {
                return $"{path1}{fakeFileSystem.PathSeparator}{path2}";
            }

            public string Combine(string path1, string path2, string path3)
            {
                return $"{path1}{fakeFileSystem.PathSeparator}{path2}{fakeFileSystem.PathSeparator}{path3}";
            }

            public string GetDirectoryName(string path)
            {
                return System.IO.Path.GetDirectoryName(path).Replace('\\', fakeFileSystem.PathSeparator[0]);
            }

            public string GetFileName(string path)
            {
                return System.IO.Path.GetFileName(path);
            }

            public string GetFileNameWithoutExtension(string path)
                => System.IO.Path.GetFileNameWithoutExtension(path);

            public string GetFullPath(string path)
                => System.IO.Path.GetFullPath(path);

            public string GetExtension(string path)
                => System.IO.Path.GetExtension(path);
        }
    }
}
