using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace Litium.SampleApps.Erp.WebHooks
{
    internal class WebHookStorage : JsonFileStorageBase<WebHookData>
    {
        public WebHookStorage() : base("WebHookEvents")
        { }

        protected override string GetFileName(WebHookData item)
        {
            return $"{item.Action}-{item.Metadata.Attempt}-{item.Metadata.EventSystemId}.{ItemExtension}";
        }
    }

    internal abstract class JsonFileStorageBase<T> where T : class 
    {
        protected const string ItemExtension = "json";
        protected const string SearchPattern = "*." + ItemExtension;

        protected readonly string _folderFullPath;
        protected DirectoryInfo _directoryInfo;

        public JsonFileStorageBase(string folderName)
        {
            _folderFullPath = GetStorageLocation(folderName);
        }

        protected abstract string GetFileName(T item);

        public void Add(T item)
        {
            var filePath = GetFileFullPath(Folder, item);
            using (var file = File.CreateText(filePath))
            {
                var serializer = new JsonSerializer()
                {
                    Formatting = Formatting.Indented,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()

                };
                serializer.Serialize(file, item);
            }
        }

        public T FirstOrDefault()
        {
            if (Folder == null)
            {
                return null;
            }

            T item;

            var file = Folder.EnumerateFiles(SearchPattern, SearchOption.TopDirectoryOnly).OrderBy(x => x.CreationTime).FirstOrDefault();
            if (file == null)
                return null;

            using (var sr = file.OpenText())
            {
                var json = sr.ReadToEnd();
                item = JsonConvert.DeserializeObject<T>(json);
            }

            return item;
        }

        public IEnumerable<T> GetAll()
        {
            if (Folder == null)
            {
                yield break;
            }


            var files = Folder.EnumerateFiles(SearchPattern, SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                T item = null;

                using (var sr = file.OpenText())
                {
                    var json = sr.ReadToEnd();
                    yield return item = JsonConvert.DeserializeObject<T>(json);
                }
            }
        }

        public void Delete(T item)
        {
            var filePath = GetFileFullPath(Folder, item);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private string GetStorageLocation(string folderName)
        {
            var baseDirectory = new DirectoryInfo(HostingEnvironment.MapPath("~/"));

            return Path.Combine(baseDirectory.Parent.FullName, folderName);
        }

        protected DirectoryInfo Folder
        {
            get
            {
                if (_directoryInfo != null)
                {
                    return _directoryInfo;
                }

                _directoryInfo = Directory.CreateDirectory(_folderFullPath);
                return _directoryInfo;
            }
        }

        protected string GetFileFullPath(DirectoryInfo directoryInfo, T item)
        {
            var fileName = GetFileName(item);

            return Path.Combine(directoryInfo.FullName, fileName);
        }
    }
}
