using Newtonsoft.Json.Linq;

namespace CompareDataTool.Infrastructure.DataSources.FlatFile
{
    public class FlatFileManager
    {
        public string[] Lines { get; }

        public string[] Columns { get; }

        public string FilePath { get; }

        public char Separator { get; }

        public FlatFileManager(string filePath, char separator, bool containsHeader)
        {
            FilePath = filePath;
            Separator = separator;
            Lines = File.ReadAllLines(FilePath);
            if (containsHeader)
            {
                Columns = Lines.Take(1).First().Split(Separator);
                Lines = Lines.Take(Lines.Length).Skip(1).ToArray();
            }
            else
            {
                Columns = Array.Empty<string>();
            }
        }

        public string[] GetAllLines()
        {
            return Lines;
        }

        public IEnumerable<T> GetData<T>(int pageNumber, int pageSize) where T : class
        {
            List<T> list = new List<T>();

            var properties = typeof(T).GetProperties();
            var currentLines = Lines.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            foreach (var line in currentLines)
            {
                var item = new JObject();
                var lineData = line.Split(Separator);
                foreach (var property in properties)
                {
                    if (Columns.Any(x => x.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var index = Array.IndexOf(Columns, property.Name);
                        item.Add(property.Name, lineData[index]);
                    }
                }

                list.Add(item.ToObject<T>());
            }

            return list;
        }
    }
}
