using Xunit;
using Infra.Data;

namespace Tests.Infra
{
    public sealed class TextStoreTests : IDisposable
    {
        private readonly string _dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            if (Directory.Exists(_dir))
                Directory.Delete(_dir, true);
        }

        private static TextSnapshot Sample() =>
            new()
            {
                Roots =
                {
                    new TextNode
                    {
                        Id = "e1", Kind = "section", Title = "Root",
                        Children =
                        {
                            new TextNode { Id = "e2", Kind = "heading", Text = "H", Level = 2 },
                            new TextNode { Id = "e3", Kind = "paragraph", Content = "P" },
                            new TextNode { Id = "e4", Kind = "link", DisplayText = "L", Url = "http://x.io" },
                        },
                    },
                },
            };

        [Fact]
        public void SaveLoad_RoundtripsTree()
        {
            JsonTextStore store = new();
            string path = Path.Combine(_dir, "text.json");
            Directory.CreateDirectory(_dir);

            store.Save(Sample(), path);
            TextSnapshot loaded = store.Load(path);

            TextNode root = Assert.Single(loaded.Roots);
            Assert.Equal("section", root.Kind);
            Assert.Equal("Root", root.Title);
            Assert.Equal(3, root.Children.Count);
            Assert.Equal("heading", root.Children[0].Kind);
            Assert.Equal(2, root.Children[0].Level);
            Assert.Equal("http://x.io", root.Children[2].Url);
        }

        [Fact]
        public void Load_MissingFile_ThrowsStoreException()
        {
            JsonTextStore store = new();

            Assert.Throws<StoreException>(() => store.Load(Path.Combine(_dir, "nope.json")));
        }

        [Fact]
        public void Load_WrongTypeFile_ThrowsStoreException()
        {
            JsonTextStore store = new();
            Directory.CreateDirectory(_dir);
            string path = Path.Combine(_dir, "bad.json");
            File.WriteAllText(path, "just a string");

            Assert.Throws<StoreException>(() => store.Load(path));
        }
    }
}
