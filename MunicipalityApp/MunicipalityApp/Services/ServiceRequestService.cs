// ServiceRequestService.cs (CORRECTED)
using System;
using System.Collections.Generic;
using MunicipalityApp.Models;
using MunicipalityApp.DataStructures;
using MunicipalityApp.Helpers;
using System.Text.Json;
using System.Linq;
using System.IO; // Required for File.ReadAllText/WriteAllText

namespace MunicipalityApp.Services
{
    public class ServiceRequestService
    {
        private const string JsonFileName = "servicerequests.json";
        private readonly BinarySearchTree<Guid, ServiceRequest> _requestTree;
        private readonly MinHeap<ServiceRequest> _priorityQueue;
        private readonly Graph<string> _statusGraph;

        public ServiceRequestService()
        {
            _requestTree = new BinarySearchTree<Guid, ServiceRequest>();
            // Priority heap: smaller number = higher priority
            _priorityQueue = new MinHeap<ServiceRequest>((a, b) =>
                a.PriorityLevel.CompareTo(b.PriorityLevel));

            _statusGraph = new Graph<string>();
            InitializeStatusGraph();
            LoadDataFromPersistence();
        }

        // -------------------------------------------------
        // NEW: Persistence Methods
        // -------------------------------------------------
        private void LoadDataFromPersistence()
        {
            // 1. Ensure the file exists
            AppDataHelper.EnsureJsonFile(JsonFileName);
            var filePath = AppDataHelper.GetFilePath(JsonFileName);
            var requests = new List<ServiceRequest>();
            try
            {
                var jsonString = File.ReadAllText(filePath);
                requests = JsonSerializer.Deserialize<List<ServiceRequest>>(jsonString) ?? new List<ServiceRequest>();
            }
            catch (Exception ex)
            {
                // Log or handle deserialization/file errors (e.g., corrupt file)
                Console.WriteLine($"Error loading service requests: {ex.Message}");
            }
            // 2. Populate data structures
            foreach (var request in requests)
            {
                // Re-populate BST and Min-Heap
                _requestTree.Insert(request.RequestID, request);
                _priorityQueue.Insert(request);
            }
        }

        private void SaveData()
        {
            // 1. Get all requests from the BST (our primary source of truth)
            var allRequests = GetAllRequests();
            // 2. Serialize and write to file
            var filePath = AppDataHelper.GetFilePath(JsonFileName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(allRequests, options);
            try
            {
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving service requests: {ex.Message}");
            }
        }

        // -------------------------------------------------
        // 🧩 1. BST — Organizing and Retrieving Requests
        // -------------------------------------------------
        public void AddRequest(ServiceRequest request)
        {
            // Ensure proper initialization before insertion
            request.RequestID = Guid.NewGuid();
            request.DateRequested = DateTime.Now;
            request.Status = "Requested";
            _requestTree.Insert(request.RequestID, request);
            _priorityQueue.Insert(request);
            SaveData();
        }

        public ServiceRequest? GetRequest(Guid requestId)
        {
            return _requestTree.Find(requestId);
        }

        public bool UpdateRequest(ServiceRequest updatedRequest)
        {
            // Use the RequestID from the updated object to find the existing one
            var existing = _requestTree.Find(updatedRequest.RequestID);

            if (existing == null) return false;

            // Handle Min-Heap re-heapify if priority changed
            if (existing.PriorityLevel != updatedRequest.PriorityLevel)
            {
                // Remove the old object from the heap
                // NOTE: This assumes your MinHeap has a working Remove method based on object reference/equality
                _priorityQueue.Remove(existing);
                // Insert the new/updated object back into the heap
                _priorityQueue.Insert(updatedRequest);
            }

            // Update the existing request in the BST (via reference):
            existing.Title = updatedRequest.Title;
            existing.ServiceType = updatedRequest.ServiceType;
            existing.Description = updatedRequest.Description;
            existing.StreetNumber = updatedRequest.StreetNumber;
            existing.StreetAddressLine2 = updatedRequest.StreetAddressLine2;
            existing.Suburb = updatedRequest.Suburb;
            existing.City = updatedRequest.City;
            existing.PostalCode = updatedRequest.PostalCode;
            existing.Latitude = updatedRequest.Latitude;
            existing.Longitude = updatedRequest.Longitude;
            existing.PriorityLevel = updatedRequest.PriorityLevel;
            existing.AssignedDepartment = updatedRequest.AssignedDepartment;
            existing.Attachments = updatedRequest.Attachments;
            existing.CitizenName = updatedRequest.CitizenName;
            existing.CitizenSurname = updatedRequest.CitizenSurname;
            existing.CitizenEmail = updatedRequest.CitizenEmail;
            existing.CitizenCellNumber = updatedRequest.CitizenCellNumber;
            existing.TrackingUsername = updatedRequest.TrackingUsername;

            // CRITICAL STATUS/DATE FIELDS:
            existing.Status = updatedRequest.Status;
            existing.CompletedDate = updatedRequest.CompletedDate;

            SaveData(); // Save the updated state to JSON file

            return true;
        }

        public bool DeleteRequest(Guid requestId)
        {
            var existing = _requestTree.Find(requestId);
            if (existing == null) return false;

            _requestTree.Remove(requestId);
            _priorityQueue.Remove(existing);
            SaveData();
            return true;
        }

        public List<ServiceRequest> GetAllRequests()
        {
            return new List<ServiceRequest>(_requestTree.InOrderTraversal());
        }

        // -------------------------------------------------
        // ⚡ 2. Min-Heap — Managing Request Priorities
        // -------------------------------------------------
        public ServiceRequest? GetNextHighPriority()
        {
            // Note: The explicit count check is redundant if ExtractMin is properly handled,
            // but we keep it for clarity/efficiency.
            if (_priorityQueue.Count == 0) return null;

            ServiceRequest? request = null;

            try
            {
                // The Min-Heap operation is the potential failure point.
                request = _priorityQueue.ExtractMin();
            }
            catch (Exception ex)
            {
                // CRITICAL: If ExtractMin() crashes (e.g., IndexOutOfRangeException on empty heap), 
                // we catch it here and log the error, then return null.
                Console.WriteLine($"ERROR: MinHeap.ExtractMin() crashed. Details: {ex.Message}");
                return null;
            }

            if (request != null)
            {
                // Update status and save data only if extraction was successful
                request.Status = "Assigned";
                request.AssignedDepartment = "Staff-Assigned";

                // SaveData() has its own try/catch, so we just call it.
                SaveData();
            }

            // Return the extracted request (or null if the heap was empty/crashed)
            return request;
        }

        public int GetPendingRequestCount() => _priorityQueue.Count;

        // -------------------------------------------------
        // 🔄 3. Graph + BFS — Status Progression (Finite State Machine)
        // -------------------------------------------------
        private void InitializeStatusGraph()
        {
            // Define all possible states/nodes
            _statusGraph.AddNode("Requested");
            _statusGraph.AddNode("Acknowledged");
            _statusGraph.AddNode("Assigned");
            _statusGraph.AddNode("In Progress");
            _statusGraph.AddNode("On Hold");
            _statusGraph.AddNode("Completed");
            _statusGraph.AddNode("Cancelled");
            _statusGraph.AddNode("Re-opened"); // New state

            // Define the Directed Edges (Valid Transitions)

            // Normal Flow
            _statusGraph.AddDirectedEdge("Requested", "Acknowledged");
            _statusGraph.AddDirectedEdge("Acknowledged", "Assigned");
            _statusGraph.AddDirectedEdge("Assigned", "In Progress");
            _statusGraph.AddDirectedEdge("In Progress", "Completed");

            // Deviations/Cancellations
            _statusGraph.AddDirectedEdge("Requested", "Cancelled");
            _statusGraph.AddDirectedEdge("Acknowledged", "Cancelled");
            _statusGraph.AddDirectedEdge("Assigned", "Cancelled");

            // Reversals/Loops
            _statusGraph.AddDirectedEdge("In Progress", "On Hold");
            _statusGraph.AddDirectedEdge("On Hold", "In Progress");

            // Re-opening logic
            _statusGraph.AddDirectedEdge("Completed", "Re-opened");
            _statusGraph.AddDirectedEdge("Re-opened", "Assigned"); // Re-opened goes back to Assigned for a new cycle
        }

        // 1. METHOD FOR FULL PROGRESSION (USES BFS - KEPT)
        public List<string> GetStatusProgression(string currentStatus)
        {
            var result = new List<string>();
            // Assumes Graph<T> has a GetNode helper
            var startNode = _statusGraph.GetNode(currentStatus);

            if (startNode == null)
                return result;

            _statusGraph.BFS(startNode, node => result.Add(node.Value));
            return result;
        }

        // 2. METHOD FOR IMMEDIATE NEXT STATUSES (USES NEIGHBORS - KEPT)
        public List<string> GetValidNextStatuses(string currentStatus)
        {
            var startNode = _statusGraph.GetNode(currentStatus);

            if (startNode == null)
                return new List<string>();

            // The Neighbors list directly provides the immediate, valid next states in a Directed Graph
            return startNode.Neighbors.Select(n => n.Value).ToList();
        }

        // <--- The duplicate GetStatusProgression method was here and has been REMOVED. --->

        // -------------------------------------------------
        // 🧰 5. Utility
        // -------------------------------------------------
        public void ClearAll()
        {
            _priorityQueue.Clear();
            _requestTree.Clear();
            SaveData();
        }
    }
}