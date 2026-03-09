using Newtonsoft.Json.Linq;

namespace CompareDataTool.Infrastructure.FlatFile
{
    public class FlatFileManager
    {
        public string[] Lines { get; }

        public string[] Columns { get; }

        public string FilePath { get; }

        public char Separator { get; }

        public FlatFileManager(string filePath, char separator, bool containsHeader)
        {
            this.FilePath = filePath;
            this.Separator = separator;
            this.Lines = File.ReadAllLines(FilePath);
            if (containsHeader)
            {
                this.Columns = this.Lines.Take(1).First().Split(this.Separator);
                this.Lines = this.Lines.Take(this.Lines.Length).Skip(1).ToArray();
            }
            else
            {
                this.Columns = Array.Empty<string>();
            }
        }

        public string[] GetAllLines()
        {
            return this.Lines;
        }

        public IEnumerable<T> GetData<T>(int pageNumber, int pageSize) where T : class
        {
            List<T> list = new List<T>();

            var properties = typeof(T).GetProperties();
            var currentLines = this.Lines.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            foreach (var line in currentLines)
            {
                var item = new JObject();
                var lineData = line.Split(this.Separator);
                foreach (var property in properties)
                {
                    if (this.Columns.Any(x => x.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var index = Array.IndexOf(this.Columns, property.Name);
                        item.Add(property.Name, lineData[index]);
                    }
                }

                list.Add(item.ToObject<T>());
            }

            return list;
        }
    }
}
