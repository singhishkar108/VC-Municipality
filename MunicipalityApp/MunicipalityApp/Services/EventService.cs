using MunicipalityApp.Models;
using MunicipalityApp.DataStructures;
using MunicipalityApp.Helpers;
using System.Text.Json;

namespace MunicipalityApp.Services
{
    public class EventService
    {
        // ADDED: Constants for the event types
        public static class EventTypes
        {
            public const string Event = "Event";
            public const string Announcement = "Announcement";
        }

        private readonly string _jsonPath;
        private readonly object _lock = new();

        public EventService()
        {
            _jsonPath = AppDataHelper.GetFilePath("events.json");
            AppDataHelper.EnsureJsonFile("events.json");
        }

        private SimpleSortedDictionary<DateTime, List<Event>> LoadEvents()
        {
            lock (_lock)
            {
                var json = File.ReadAllText(_jsonPath);
                var list = JsonSerializer.Deserialize<List<Event>>(json) ?? new List<Event>();

                var sorted = new SimpleSortedDictionary<DateTime, List<Event>>();
                foreach (var e in list)
                {
                    if (!sorted.TryGetValue(e.StartDate, out var events))
                    {
                        events = new List<Event>();
                        sorted.AddOrUpdate(e.StartDate, events);
                    }
                    events.Add(e);
                }

                return sorted;
            }
        }

        private void SaveEvents(SimpleSortedDictionary<DateTime, List<Event>> events)
        {
            lock (_lock)
            {
                var list = new List<Event>();
                foreach (var kv in events.Enumerate())
                    list.AddRange(kv.Value);

                var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_jsonPath, json);
            }
        }

        public List<Event> GetAll()
        {
            var sorted = LoadEvents();
            var result = new List<Event>();
            foreach (var kv in sorted.Enumerate())
                result.AddRange(kv.Value);
            return result;
        }

        public Event? GetById(Guid id)
        {
            var sorted = LoadEvents();
            foreach (var kv in sorted.Enumerate())
            {
                var ev = kv.Value.FirstOrDefault(e => e.Id == id);
                if (ev != null) return ev;
            }
            return null;
        }

        public void Create(Event ev)
        {
            var sorted = LoadEvents();

            if (ev.Id == Guid.Empty) ev.Id = Guid.NewGuid();
            if (ev.Timestamp == default) ev.Timestamp = DateTime.UtcNow;

            // **FIX IMPLEMENTED HERE:** Ensure 'Type' is set before saving. 
            // Default to "Event" if the caller (Controller) hasn't explicitly set it.
            if (string.IsNullOrWhiteSpace(ev.Type))
            {
                ev.Type = EventTypes.Event;
            }

            if (!sorted.TryGetValue(ev.StartDate, out var events))
            {
                events = new List<Event>();
                sorted.AddOrUpdate(ev.StartDate, events);
            }
            events.Add(ev);

            SaveEvents(sorted);
        }

        public bool Update(Event updated)
        {
            var sorted = LoadEvents();
            foreach (var kv in sorted.Enumerate())
            {
                var idx = kv.Value.FindIndex(e => e.Id == updated.Id);
                if (idx >= 0)
                {
                    // If StartDate changed, move to new date key
                    if (kv.Key != updated.StartDate)
                    {
                        kv.Value.RemoveAt(idx);
                        if (!sorted.TryGetValue(updated.StartDate, out var newDateList))
                        {
                            newDateList = new List<Event>();
                            sorted.AddOrUpdate(updated.StartDate, newDateList);
                        }
                        newDateList.Add(updated);
                    }
                    else
                    {
                        kv.Value[idx] = updated;
                    }
                    SaveEvents(sorted);
                    return true;
                }
            }
            return false;
        }

        public bool Delete(Guid id)
        {
            var sorted = LoadEvents();
            bool removed = false;

            var keys = sorted.Enumerate().Select(kv => kv.Key).ToList();
            foreach (var key in keys)
            {
                if (sorted.TryGetValue(key, out var events))
                {
                    int before = events.Count;
                    events.RemoveAll(e => e.Id == id);
                    if (events.Count == 0)
                        sorted.Remove(key);
                    removed |= (events.Count != before);
                }
            }

            if (removed) SaveEvents(sorted);
            return removed;
        }

        /// <summary>
        /// Search events by category/date range. NOTE: this method does NOT call RecommendationService.TrackSearch.
        /// Controllers should call RecommendationService.TrackSearch(...) themselves so we avoid DI cycles.
        /// </summary>
        public List<Event> Search(string? category, DateOnly? from, DateOnly? to)
        {
            var all = GetAll();
            var result = new List<Event>();

            foreach (var ev in all)
            {
                // Filter 1: Only return results for type "Event".
                if (!ev.Type.Equals(EventTypes.Event, StringComparison.OrdinalIgnoreCase)) continue;

                bool matches = true;

                if (!string.IsNullOrWhiteSpace(category))
                    matches &= string.Equals(ev.Category?.Trim(), category.Trim(), StringComparison.OrdinalIgnoreCase);

                var localDate = DateOnly.FromDateTime(ev.StartDate.ToLocalTime());
                if (from.HasValue) matches &= localDate.CompareTo(from.Value) >= 0;
                if (to.HasValue) matches &= localDate.CompareTo(to.Value) <= 0;

                if (matches) result.Add(ev);
            }

            return result;
        }

        public List<string> GetUniqueCategories()
        {
            var all = GetAll();
            var set = new SimpleHashSet<string>();
            foreach (var ev in all)
            {
                if (!string.IsNullOrWhiteSpace(ev.Category))
                    set.Add(ev.Category.Trim());
            }
            var list = set.ToList();
            list.Sort(StringComparer.OrdinalIgnoreCase);
            return list;
        }

        public List<DateOnly> GetUniqueEventDates()
        {
            var all = GetAll();
            var set = new SimpleHashSet<DateOnly>();
            foreach (var ev in all)
            {
                // Filter 2: Only collect dates from Type "Event".
                if (ev.Type.Equals(EventTypes.Event, StringComparison.OrdinalIgnoreCase))
                    set.Add(DateOnly.FromDateTime(ev.StartDate.ToLocalTime()));
            }

            var list = set.ToList();
            list.Sort();
            return list;
        }

        public List<string> GetUniqueLocations()
        {
            var all = GetAll();
            var set = new SimpleHashSet<string>();
            foreach (var ev in all)
            {
                if (!string.IsNullOrWhiteSpace(ev.Location))
                    set.Add(ev.Location.Trim());
            }

            var list = set.ToList();
            list.Sort(StringComparer.OrdinalIgnoreCase);
            return list;
        }
    }
}