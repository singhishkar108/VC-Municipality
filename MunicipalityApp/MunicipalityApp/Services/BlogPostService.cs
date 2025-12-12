using MunicipalityApp.Models;
using MunicipalityApp.Helpers;
using System.Text.Json;

namespace MunicipalityApp.Services
{
    public class BlogPostService
    {
        private readonly string _jsonPath;
        private readonly object _lock = new();

        public BlogPostService()
        {
            _jsonPath = AppDataHelper.GetFilePath("blogposts.json");
            AppDataHelper.EnsureJsonFile("blogposts.json");
        }

        private List<BlogPost> Load()
        {
            lock (_lock)
            {
                var json = File.ReadAllText(_jsonPath);
                return JsonSerializer.Deserialize<List<BlogPost>>(json) ?? new List<BlogPost>();
            }
        }

        private void Save(List<BlogPost> posts)
        {
            lock (_lock)
            {
                var json = JsonSerializer.Serialize(posts, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_jsonPath, json);
            }
        }

        public List<BlogPost> GetAll() =>
            Load().OrderByDescending(p => p.Timestamp).ToList();

        public BlogPost? GetById(Guid id) =>
            Load().FirstOrDefault(p => p.Id == id);

        public void Create(BlogPost post)
        {
            var list = Load();
            list.Add(post);
            Save(list);
        }

        public bool Update(BlogPost updated)
        {
            var list = Load();
            var idx = list.FindIndex(p => p.Id == updated.Id);
            if (idx == -1) return false;

            list[idx] = updated;
            Save(list);
            return true;
        }

        public bool Delete(Guid id)
        {
            var list = Load();
            var removed = list.RemoveAll(p => p.Id == id);
            if (removed > 0)
            {
                Save(list);
                return true;
            }
            return false;
        }

        public List<BlogPost> SearchByDate(DateOnly? fromDate, DateOnly? toDate)
        {
            var all = GetAll();
            if (fromDate is null && toDate is null) return all;

            return all.Where(p =>
            {
                var d = DateOnly.FromDateTime(p.Timestamp.ToUniversalTime());
                bool afterFrom = fromDate == null || d >= fromDate.Value;
                bool beforeTo = toDate == null || d <= toDate.Value;
                return afterFrom && beforeTo;
            }).ToList();
        }

        public void LikePost(Guid id, string username)
        {
            var list = Load();
            var post = list.FirstOrDefault(p => p.Id == id);
            if (post == null) return;

            if (!post.LikedBy.Contains(username, StringComparer.OrdinalIgnoreCase))
            {
                post.LikedBy.Add(username);
                post.Likes = post.LikedBy.Count;
                Save(list);
            }
        }

        public void UnlikePost(Guid id, string username)
        {
            var list = Load();
            var post = list.FirstOrDefault(p => p.Id == id);
            if (post == null) return;

            if (post.LikedBy.RemoveWhere(u =>
                string.Equals(u, username, StringComparison.OrdinalIgnoreCase)) > 0)
            {
                post.Likes = post.LikedBy.Count;
                Save(list);
            }
        }
    }
}
