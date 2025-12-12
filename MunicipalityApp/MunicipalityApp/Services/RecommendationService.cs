using MunicipalityApp.Models;
using MunicipalityApp.DataStructures;
using MunicipalityApp.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MunicipalityApp.Services
{
    public class RecommendationService
    {
        private readonly EventService _eventService;

        // In-memory structures (use SimpleHashTable for runtime operations)
        private readonly SimpleHashTable<string, int> _globalCategoryCounts = new();
        private readonly SimpleHashTable<DateOnly, int> _globalDateCounts = new();
        // NEW: Keyword and Hashtag tracking structures
        private readonly SimpleHashTable<string, int> _globalKeywordCounts = new();
        private readonly SimpleHashTable<string, int> _globalHashtagCounts = new();


        private readonly SimpleHashTable<Guid, SimpleHashTable<string, int>> _userCategoryCounts = new();
        private readonly SimpleHashTable<Guid, SimpleHashTable<DateOnly, int>> _userDateCounts = new();
        // NEW: Per-user Keyword and Hashtag tracking structures
        private readonly SimpleHashTable<Guid, SimpleHashTable<string, int>> _userKeywordCounts = new();
        private readonly SimpleHashTable<Guid, SimpleHashTable<string, int>> _userHashtagCounts = new();

        // Thread-safety lock
        private readonly object _lock = new();

        // Persistence path
        private readonly string _persistencePath;

        // DTO used for serialization
        private class PersistDto
        {
            public Dictionary<string, int> GlobalCategoryCounts { get; set; } = new();
            public Dictionary<string, int> GlobalDateCounts { get; set; } = new(); // key = YYYY-MM-DD
            // NEW: Persistence fields for Keywords and Hashtags
            public Dictionary<string, int> GlobalKeywordCounts { get; set; } = new();
            public Dictionary<string, int> GlobalHashtagCounts { get; set; } = new();

            public Dictionary<string, Dictionary<string, int>> UserCategoryCounts { get; set; } = new(); // userId -> (category -> count)
            public Dictionary<string, Dictionary<string, int>> UserDateCounts { get; set; } = new();      // userId -> (date -> count)
            // NEW: Persistence fields for user Keywords and Hashtags
            public Dictionary<string, Dictionary<string, int>> UserKeywordCounts { get; set; } = new();
            public Dictionary<string, Dictionary<string, int>> UserHashtagCounts { get; set; } = new();
        }

        // NEW: Helper class for sanitizing and extracting keywords from text
        private static class KeywordHelper
        {
            // Simple list of common, low-value words to ignore
            private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
            {
                "a", "an", "the", "and", "or", "in", "on", "at", "for", "to", "is", "of", "with"
            };

            public static IEnumerable<string> ExtractKeywords(string text)
            {
                if (string.IsNullOrWhiteSpace(text))
                    return Enumerable.Empty<string>();

                // 1. Remove punctuation (excluding the # for hashtags)
                var sanitizedText = new string(text.Select(c => char.IsPunctuation(c) && c != '#' ? ' ' : c).ToArray());

                // 2. Split by whitespace, convert to lowercase, and filter
                return sanitizedText.ToLowerInvariant()
                    .Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 2 && !StopWords.Contains(w));
            }
        }

        public RecommendationService(EventService eventService)
        {
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            _persistencePath = AppDataHelper.GetFilePath("recommendations.json");

            AppDataHelper.EnsureJsonFile("recommendations.json");
            LoadFromFile();
        }

        // NEW: Generic helper to simplify incrementing counts
        private static void IncrementCount<TKey>(SimpleHashTable<TKey, int> table, TKey key) where TKey : notnull
        {
            if (table.TryGetValue(key, out var existing))
                table.AddOrUpdate(key, existing + 1);
            else
                table.AddOrUpdate(key, 1);
        }

        /// <summary>
        /// Track user searches, including category, date, general query, and explicit hashtag clicks.
        /// Thread-safe and persists changes to disk.
        /// </summary>
        // UPDATED: Signature now includes searchQuery and hashtag
        public void TrackSearch(Guid? userId, string? category, DateOnly? fromDate, DateOnly? toDate, string? searchQuery, string? hashtag)
        {
            lock (_lock)
            {
                // 1. CATEGORY TRACKING (Existing logic, cleaned up with helper)
                if (!string.IsNullOrWhiteSpace(category))
                {
                    if (userId.HasValue)
                        IncrementCount(GetOrCreateUserCategoryCounts(userId.Value), category);

                    IncrementCount(_globalCategoryCounts, category);
                }

                // 2. DATE TRACKING (Existing logic, cleaned up with helper)
                void IncDate(SimpleHashTable<DateOnly, int> target, DateOnly d) => IncrementCount(target, d);

                if (fromDate.HasValue)
                {
                    if (userId.HasValue)
                        IncDate(GetOrCreateUserDateCounts(userId.Value), fromDate.Value);
                    IncDate(_globalDateCounts, fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    if (userId.HasValue)
                        IncDate(GetOrCreateUserDateCounts(userId.Value), toDate.Value);
                    IncDate(_globalDateCounts, toDate.Value);
                }

                // NEW: 3. KEYWORD TRACKING (From searchQuery)
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    var keywords = KeywordHelper.ExtractKeywords(searchQuery);

                    foreach (var keyword in keywords)
                    {
                        if (userId.HasValue)
                            IncrementCount(GetOrCreateUserKeywordCounts(userId.Value), keyword);

                        IncrementCount(_globalKeywordCounts, keyword);
                    }
                }

                // NEW: 4. HASHTAG TRACKING (From explicit click/filter)
                if (!string.IsNullOrWhiteSpace(hashtag))
                {
                    // Ensure hashtag starts with # for consistency (though it should already)
                    var normalizedHashtag = hashtag.StartsWith("#") ? hashtag : $"#{hashtag}";

                    if (userId.HasValue)
                        IncrementCount(GetOrCreateUserHashtagCounts(userId.Value), normalizedHashtag);

                    IncrementCount(_globalHashtagCounts, normalizedHashtag);
                }

                // Persist after update
                SaveToFile();
            }
        }

        // EXISTING/RENAMED: GetOrCreate helpers
        private SimpleHashTable<string, int> GetOrCreateUserCategoryCounts(Guid userId)
        {
            if (!_userCategoryCounts.TryGetValue(userId, out var counts))
            {
                counts = new SimpleHashTable<string, int>();
                _userCategoryCounts.AddOrUpdate(userId, counts);
            }
            return counts;
        }

        private SimpleHashTable<DateOnly, int> GetOrCreateUserDateCounts(Guid userId)
        {
            if (!_userDateCounts.TryGetValue(userId, out var counts))
            {
                counts = new SimpleHashTable<DateOnly, int>();
                _userDateCounts.AddOrUpdate(userId, counts);
            }
            return counts;
        }

        // NEW: GetOrCreate helper for Keywords
        private SimpleHashTable<string, int> GetOrCreateUserKeywordCounts(Guid userId)
        {
            if (!_userKeywordCounts.TryGetValue(userId, out var counts))
            {
                counts = new SimpleHashTable<string, int>();
                _userKeywordCounts.AddOrUpdate(userId, counts);
            }
            return counts;
        }

        // NEW: GetOrCreate helper for Hashtags
        private SimpleHashTable<string, int> GetOrCreateUserHashtagCounts(Guid userId)
        {
            if (!_userHashtagCounts.TryGetValue(userId, out var counts))
            {
                counts = new SimpleHashTable<string, int>();
                _userHashtagCounts.AddOrUpdate(userId, counts);
            }
            return counts;
        }

        /// <summary>
        /// Get recommendations for a user if logged in, otherwise global/popularity-based recommendations.
        /// Thread-safe snapshot before scoring.
        /// </summary>
        public List<Event> GetRecommendations(Guid? userId, int maxResults = 5)
        {
            // Snapshots under lock
            SimpleHashTable<string, int> categoryCountsSnapshot;
            SimpleHashTable<DateOnly, int> dateCountsSnapshot;
            // NEW: Keyword and Hashtag snapshots
            SimpleHashTable<string, int> keywordCountsSnapshot;
            SimpleHashTable<string, int> hashtagCountsSnapshot;

            lock (_lock)
            {
                // Determine which counts to use (User or Global)
                bool useUserCounts = userId.HasValue &&
                    _userCategoryCounts.ContainsKey(userId.Value) &&
                    _userDateCounts.ContainsKey(userId.Value); // Simple check

                if (useUserCounts)
                {
                    Guid id = userId.Value;
                    categoryCountsSnapshot = CloneStringIntTable(_userCategoryCounts.Get(id));
                    dateCountsSnapshot = CloneDateIntTable(_userDateCounts.Get(id));
                    // NEW: Clone user keywords and hashtags (use global if user tables don't exist yet)
                    keywordCountsSnapshot = _userKeywordCounts.TryGetValue(id, out var uk) ? CloneStringIntTable(uk) : CloneStringIntTable(_globalKeywordCounts);
                    hashtagCountsSnapshot = _userHashtagCounts.TryGetValue(id, out var uh) ? CloneStringIntTable(uh) : CloneStringIntTable(_globalHashtagCounts);
                }
                else
                {
                    // Use global maps
                    categoryCountsSnapshot = CloneStringIntTable(_globalCategoryCounts);
                    dateCountsSnapshot = CloneDateIntTable(_globalDateCounts);
                    // NEW: Clone global keywords and hashtags
                    keywordCountsSnapshot = CloneStringIntTable(_globalKeywordCounts);
                    hashtagCountsSnapshot = CloneStringIntTable(_globalHashtagCounts);
                }
            }

            var allEvents = _eventService.GetAll();

            // Priority queue: highest score first; tie-breaker on earliest StartDate
            var scoredQueue = new SimplePriorityQueue<Event>(
                (a, b) =>
                {
                    int scoreA = GetEventScore(a, categoryCountsSnapshot, dateCountsSnapshot, keywordCountsSnapshot, hashtagCountsSnapshot);
                    int scoreB = GetEventScore(b, categoryCountsSnapshot, dateCountsSnapshot, keywordCountsSnapshot, hashtagCountsSnapshot);
                    int cmp = scoreB.CompareTo(scoreA); // higher score first
                    if (cmp != 0) return cmp;
                    return a.StartDate.CompareTo(b.StartDate);
                });

            foreach (var ev in allEvents)
                scoredQueue.Enqueue(ev);

            var recommendations = new List<Event>();
            while (scoredQueue.Count > 0 && recommendations.Count < maxResults)
                recommendations.Add(scoredQueue.Dequeue());

            return recommendations;
        }

        // UPDATED: Signature and logic to include Keyword and Hashtag scoring
        private int GetEventScore(
            Event ev,
            SimpleHashTable<string, int> catCounts,
            SimpleHashTable<DateOnly, int> dateCounts,
            SimpleHashTable<string, int> keywordCounts,
            SimpleHashTable<string, int> hashtagCounts)
        {
            int score = 0;

            // Score 1: Category Match (Weight: 10)
            if (!string.IsNullOrWhiteSpace(ev.Category) && catCounts.TryGetValue(ev.Category, out var catCount))
                score += catCount * 10;

            // Score 2: Date Match (Weight: 5)
            var dateKey = DateOnly.FromDateTime(ev.StartDate.ToLocalTime());
            if (dateCounts.TryGetValue(dateKey, out var dateCount))
                score += dateCount * 5;

            // Score 3: Keyword Match (Weight: 15 - High importance)
            var eventText = $"{ev.Title} {ev.Description}".ToLowerInvariant();
            foreach (var kv in keywordCounts.Enumerate())
            {
                // Check if the event title/description contains the popular keyword
                if (eventText.Contains(kv.Key.ToLowerInvariant()))
                {
                    score += kv.Value * 15;
                }
            }

            // Score 4: Hashtag Match (Weight: 8)
            if (ev.Hashtags?.Any() == true)
            {
                foreach (var hashtag in ev.Hashtags)
                {
                    if (hashtagCounts.TryGetValue(hashtag, out var hashtagCount))
                    {
                        score += hashtagCount * 8;
                    }
                }
            }

            // Score 5 (Optional - Recency Boost): Give higher score to upcoming events
            if (ev.StartDate > DateTime.Now)
            {
                // Up to 100 points based on proximity (closer events score higher)
                var daysUntil = (ev.StartDate - DateTime.Now).TotalDays;
                if (daysUntil < 30)
                {
                    score += (int)(100 - (daysUntil * 3.3)); // Boost score, maximum for today's events
                }
            }

            return score;
        }

        // -------------------------
        // Persistence helpers
        // -------------------------
        private void SaveToFile()
        {
            try
            {
                var dto = new PersistDto();

                // Global categories & dates (Existing logic)
                foreach (var kv in _globalCategoryCounts.Enumerate())
                    dto.GlobalCategoryCounts[kv.Key] = kv.Value;
                foreach (var kv in _globalDateCounts.Enumerate())
                    dto.GlobalDateCounts[kv.Key.ToString("yyyy-MM-dd")] = kv.Value;

                // NEW: Global Keywords and Hashtags
                foreach (var kv in _globalKeywordCounts.Enumerate())
                    dto.GlobalKeywordCounts[kv.Key] = kv.Value;
                foreach (var kv in _globalHashtagCounts.Enumerate())
                    dto.GlobalHashtagCounts[kv.Key] = kv.Value;

                // Users: categories (Existing logic)
                foreach (var userKv in _userCategoryCounts.Enumerate())
                {
                    var userIdStr = userKv.Key.ToString();
                    var inner = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    foreach (var kv in userKv.Value.Enumerate())
                        inner[kv.Key] = kv.Value;
                    dto.UserCategoryCounts[userIdStr] = inner;
                }

                // Users: dates (Existing logic)
                foreach (var userKv in _userDateCounts.Enumerate())
                {
                    var userIdStr = userKv.Key.ToString();
                    var inner = new Dictionary<string, int>();
                    foreach (var kv in userKv.Value.Enumerate())
                        inner[kv.Key.ToString("yyyy-MM-dd")] = kv.Value;
                    dto.UserDateCounts[userIdStr] = inner;
                }

                // NEW: Users: keywords
                foreach (var userKv in _userKeywordCounts.Enumerate())
                {
                    var userIdStr = userKv.Key.ToString();
                    var inner = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    foreach (var kv in userKv.Value.Enumerate())
                        inner[kv.Key] = kv.Value;
                    dto.UserKeywordCounts[userIdStr] = inner;
                }

                // NEW: Users: hashtags
                foreach (var userKv in _userHashtagCounts.Enumerate())
                {
                    var userIdStr = userKv.Key.ToString();
                    var inner = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    foreach (var kv in userKv.Value.Enumerate())
                        inner[kv.Key] = kv.Value;
                    dto.UserHashtagCounts[userIdStr] = inner;
                }


                var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_persistencePath, json);
            }
            catch (Exception ex)
            {
                // Better to log the exception here instead of just swallowing it
                // Console.WriteLine($"Error during SaveToFile: {ex.Message}");
            }
        }

        private void LoadFromFile()
        {
            try
            {
                if (!File.Exists(_persistencePath)) return;

                var json = File.ReadAllText(_persistencePath);
                if (string.IsNullOrWhiteSpace(json)) return;

                var dto = JsonSerializer.Deserialize<PersistDto>(json);
                if (dto == null) return;

                lock (_lock)
                {
                    // Clean initialization by replacing the instance (much cleaner than loop-and-remove)
                    // Note: Since these are readonly fields, we cannot re-assign them directly. 
                    // I'll stick to the user's original clear logic, but use a helper method instead.
                    ClearAllTables();

                    // Load global categories (Existing logic)
                    foreach (var kv in dto.GlobalCategoryCounts)
                        _globalCategoryCounts.AddOrUpdate(kv.Key, kv.Value);

                    // Load global dates (Existing logic)
                    foreach (var kv in dto.GlobalDateCounts)
                    {
                        if (DateOnly.TryParse(kv.Key, out var d))
                            _globalDateCounts.AddOrUpdate(d, kv.Value);
                    }

                    // NEW: Load global keywords
                    foreach (var kv in dto.GlobalKeywordCounts)
                        _globalKeywordCounts.AddOrUpdate(kv.Key, kv.Value);

                    // NEW: Load global hashtags
                    foreach (var kv in dto.GlobalHashtagCounts)
                        _globalHashtagCounts.AddOrUpdate(kv.Key, kv.Value);


                    // Load user categories (Existing logic)
                    foreach (var userKv in dto.UserCategoryCounts)
                    {
                        if (!Guid.TryParse(userKv.Key, out var guid)) continue;
                        var inner = new SimpleHashTable<string, int>();
                        foreach (var kv in userKv.Value)
                            inner.AddOrUpdate(kv.Key, kv.Value);
                        _userCategoryCounts.AddOrUpdate(guid, inner);
                    }

                    // Load user dates (Existing logic)
                    foreach (var userKv in dto.UserDateCounts)
                    {
                        if (!Guid.TryParse(userKv.Key, out var guid)) continue;
                        var inner = new SimpleHashTable<DateOnly, int>();
                        foreach (var kv in userKv.Value)
                        {
                            if (DateOnly.TryParse(kv.Key, out var d))
                                inner.AddOrUpdate(d, kv.Value);
                        }
                        _userDateCounts.AddOrUpdate(guid, inner);
                    }

                    // NEW: Load user keywords
                    foreach (var userKv in dto.UserKeywordCounts)
                    {
                        if (!Guid.TryParse(userKv.Key, out var guid)) continue;
                        var inner = new SimpleHashTable<string, int>();
                        foreach (var kv in userKv.Value)
                            inner.AddOrUpdate(kv.Key, kv.Value);
                        _userKeywordCounts.AddOrUpdate(guid, inner);
                    }

                    // NEW: Load user hashtags
                    foreach (var userKv in dto.UserHashtagCounts)
                    {
                        if (!Guid.TryParse(userKv.Key, out var guid)) continue;
                        var inner = new SimpleHashTable<string, int>();
                        foreach (var kv in userKv.Value)
                            inner.AddOrUpdate(kv.Key, kv.Value);
                        _userHashtagCounts.AddOrUpdate(guid, inner);
                    }
                }
            }
            catch (Exception ex)
            {
                // Better to log the exception here instead of just swallowing it
                // Console.WriteLine($"Error during LoadFromFile: {ex.Message}");
            }
        }

        // Helper to clear the tables by removing all keys
        private void ClearTable<TKey, TValue>(SimpleHashTable<TKey, TValue> table)
        {
            var keys = table.Enumerate().Select(k => k.Key).ToList();
            foreach (var k in keys) table.Remove(k);
        }

        private void ClearAllTables()
        {
            // Clear all tables to prepare for loading fresh data
            ClearTable(_globalCategoryCounts);
            ClearTable(_globalDateCounts);
            ClearTable(_globalKeywordCounts); // NEW
            ClearTable(_globalHashtagCounts); // NEW

            var userKeys = _userCategoryCounts.Enumerate().Select(k => k.Key).ToList();
            foreach (var k in userKeys) _userCategoryCounts.Remove(k);

            var userDateKeys = _userDateCounts.Enumerate().Select(k => k.Key).ToList();
            foreach (var k in userDateKeys) _userDateCounts.Remove(k);

            var userKeywordKeys = _userKeywordCounts.Enumerate().Select(k => k.Key).ToList();
            foreach (var k in userKeywordKeys) _userKeywordCounts.Remove(k); // NEW

            var userHashtagKeys = _userHashtagCounts.Enumerate().Select(k => k.Key).ToList();
            foreach (var k in userHashtagKeys) _userHashtagCounts.Remove(k); // NEW
        }

        // Clone helpers (reusing string clone for keywords and hashtags)
        private SimpleHashTable<string, int> CloneStringIntTable(SimpleHashTable<string, int> src)
        {
            var clone = new SimpleHashTable<string, int>();
            foreach (var kv in src.Enumerate())
                clone.AddOrUpdate(kv.Key, kv.Value);
            return clone;
        }

        private SimpleHashTable<DateOnly, int> CloneDateIntTable(SimpleHashTable<DateOnly, int> src)
        {
            var clone = new SimpleHashTable<DateOnly, int>();
            foreach (var kv in src.Enumerate())
                clone.AddOrUpdate(kv.Key, kv.Value);
            return clone;
        }
    }
}