using MunicipalityApp.Models;
using System;
using System.Collections.Generic;

namespace MunicipalityApp.DataStructures
{
    // Node of doubly linked list
    public class IssueNode
    {
        public Issue Data { get; set; }
        public IssueNode? Next { get; set; }
        public IssueNode? Prev { get; set; }

        public IssueNode(Issue issue)
        {
            Data = issue;
            Next = null;
            Prev = null;
        }
    }

    // Doubly linked list for issues
    public class IssueDoublyLinkedList
    {
        private IssueNode? head;
        private IssueNode? tail;

        public void AddIssue(Issue issue)
        {
            var node = new IssueNode(issue);

            if (head == null)
            {
                head = tail = node;
                return;
            }

            tail!.Next = node;
            node.Prev = tail;
            tail = node;
        }

        public List<Issue> GetAllIssues()
        {
            var issues = new List<Issue>();
            var current = head;
            while (current != null)
            {
                issues.Add(current.Data);
                current = current.Next;
            }
            return issues;
        }

        // Update progress directly on the linked list node
        public bool UpdateProgress(Guid issueId, string newProgress)
        {
            var current = head;
            while (current != null)
            {
                if (current.Data.Id == issueId)
                {
                    current.Data.Progress = newProgress;
                    return true;
                }
                current = current.Next;
            }
            return false; // Not found
        }
    }
}
