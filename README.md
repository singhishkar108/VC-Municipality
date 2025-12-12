<div align="center">

<h1>🏛️ VC Municipality 🌐</h1>

</div>

---

## 📑Table of Contents

🧭 1. [**Introduction**](#-1-introduction)<br>
💻 2. [**Setting Up the Project Locally**](#-2-setting-up-the-project-locally)<br>
✅ 3. [**Features and Functionality**](#-3-features-and-functionality)<br>
🔐 4. [**User & Admin Navigation**](#-4-user--admin-navigation)<br>
🏗️ 5. [**Architecture**](#️-5-architecture)<br>
♻️ 6. [**Changelog**](#️-6-changelog)<br>
👥 7. [**Author and Contributions**](#-7-author-and-contributions)<br>
⚖️ 8. [**MIT License**](#️-8-mit-license)<br>
❓ 9. [**Frequently Asked Questions (FAQ)**](#-9-frequently-asked-questions-faq)<br>
📚 10. [**References**](#-10-references)<br>

---

## 🧭 1. Introduction:

**VC Municipality** is a modern C#.NET Framework application designed to streamline resident engagement and optimize municipal service delivery in South Africa. Our goal is to provide an efficient, resilient, and performance-optimized platform for residents to access services and track progress, while providing administrators with mission-critical tools for task management and resource allocation.

### Project Scope and Phasing

The project's scope, spanning three parts, focuses on integrating custom-implemented advanced data structures and algorithms to create a highly efficient system capable of scaling to manage a large volume of citizen requests.

- **Part 1 (Foundation):** Established the core application, including secure Role-Based Access Control (RBAC) and fundamental Issue Reporting functionality.
- **Part 2 (Enhancement):** Introduced the Event Management System and Event Recommendation Feature, leveraging structures like Hash Tables, Sets, and Priority Queues for efficient lookups, uniqueness checks, and dynamic prioritization.
- **Part 3 (Optimization & Completion):** This final phase refines the core Service Request Lifecycle Management system to an enterprise-ready standard. It integrates complex, custom structures that guarantee logarithmic time complexity for critical operations and implements graph-based state management and advanced resource optimization algorithms.

### Key Features

- **Role-Based Access Control (RBAC):** Secure authentication system distinguishing between User and Administrator roles to strictly control access and operations.
- **Issue Reporting:** Provides citizens with the ability to submit detailed reports on community issues, including media file attachments, and view their personal submission history.
- **Administrative Oversight:** Administrators have full oversight of all reported issues, with the ability to download attachments and update the progress status.
- **User-Friendly Interface:** A responsive, intuitive web interface designed for accessibility and enhanced with modern UI elements.
- **Offline Data Management:** All application data is stored in JSON files to simplify deployment and maintain transparent data persistence.
- **Event Management System:** A robust system allowing administrators to create, manage, and publish local events and community announcements.
- **Personalized Event Recommendation Feature:** Dynamically suggests events to citizens based on preferences and interactions, significantly enhancing user engagement.
- **Guaranteed Performance:** Achieves consistent logarithmic time complexity for all critical data operations, ensuring responsiveness at scale.
- **Mission-Critical Task Prioritization:** Provides administrators with an instant mechanism to retrieve the most urgent service request.
- **Service Request Integrity:** Enforces a strict, graph-based workflow that prevents invalid status transitions.
- **Field Resource Optimization:** Implements advanced route planning to calculate the most efficient path for field crews.

### Key Technical Implementations

#### Part 1:

- **Array:** Used for fixed-size data collections, such as media file paths or initial user lists.
- **Linked List:** Implemendted for dynamically managing lists of submitted issues where frequent insertions/deletions occur
- **List`<T>`:** Used for initial data storage and manipulation.
- **Authentication and Authorization Logic:** Implemented procedural logic to manage user sessions and enforce role-based permissions based on the RBAC model.

#### Part 2:

- **Sorted Dictionary:** Used for organizing and retrieving data (e.g., events) based on a key (Date/Category) in a sorted manner.
- **Hash Table:** Implemented for fast lookups (insertion and retrieval) of entities like user profiles or event details by a unique ID.
- **Set:** Employed to manage unique items, such as checking for the existence of a user's subscription to an event or ensuring no duplicate events are entered.
- **Priority Queue:** Used to order events or announcements by a non-date-based metric (e.g., urgency or popularity).

#### Part 3:

- **Self-Balancing Binary Search Tree (e.g., AVL Tree):** Organizes all service requests, indexed by `RequestID`.
- **Min-Heap (Priority Queue):** Stores unassigned service requests, ordered by a calculated priority, offering immediate access to the highest-priority item.
- **Graph & Breadth-First Search (BFS):** The Graph models the request status lifecycle (Finite State Machine). BFS is used to determine all valid next statuses, ensuring workflow compliance.
- **Minimum Spanning Tree (MST) Algorithm:** Applied to a sub-graph of related service issues, calculating the optimal, shortest total-distance network for field crew deployment.

---

## 💻 2. Setting Up the Project Locally

### Prerequisites

To successfully compile and run this project, you must have the following installed on your system:

| Component             | Requirement                                                                                                              | Notes                                                                                                                  |
| :-------------------- | :----------------------------------------------------------------------------------------------------------------------- | :--------------------------------------------------------------------------------------------------------------------- |
| **Operating Systems** | Any OS compatible with the **.NET 8.0 Runtime** and SDK (Windows 10/11, macOS 10.15+, or supported Linux distributions). | Required for running and building the application. The project specifically targets .NET 8.0/9.0.                      |
| **.NET SDK**          | **.NET 9.0 or later** is required to build, with compatibility for **.NET 8.0 Runtime** for execution.                   | The project targets .NET 9.0, requiring the corresponding SDK. Ensure the correct runtime is available for deployment. |
| **IDE**               | Compatible version of **Microsoft Visual Studio 2019+** (or Visual Studio Code with C# Dev Kit).                         | Visual Studio 2022 is generally recommended for the best experience.                                                   |
| **Version Control**   | **Git**                                                                                                                  | Must be installed and configured for cloning and managing the repository source code.                                  |
| **System Resources**  | Minimum 4GB RAM, 200MB free disk space.                                                                                  | Recommended minimum specifications for a smooth development experience.                                                |

This project is **intentionally lean**, relying primarily on the **robust, core libraries** provided by the **ASP.NET Core framework** itself, which minimizes external dependencies. The design prioritizes ease of execution and configuration, eliminating the requirement for users to manually input personal API keys or similar credentials.

### Project Configurations

#### `MunicipalityApp.csproj` Snippet

This configuration defines the project as an ASP.NET Core web application targeting the latest framework version.

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

</Project>
```

### Installation and Running the Application

Follow these steps to get the application running on your local machine.

#### 1. Clone the Repository

Open your command-line interface (CLI) or terminal and execute the following commands:

```bash
git clone https://github.com/singhishkar108/VC-Municipality.git
cd VC-Municipality
```

#### 2. Open and Build in Visual Studio (Recommended)

1.  Open **Visual Studio 2022**.
2.  Navigate to **File \> Open \> Project/Solution**.
3.  Browse to the cloned repository and select the **Solution file (.sln)** to load the project.
4.  Visual Studio will automatically perform a package restore (`dotnet restore`).
5.  Select **Build \> Build Solution** (or press `F6`) to compile the project.
6.  Click the **Run** button (or press `F5`) to start the application with debugging, or `Ctrl+F5` to start without debugging.

#### 3. Run via Command Line (Alternative)

If you are using Visual Studio Code or prefer the CLI:

1.  Navigate to the project directory containing the `.csproj` file.
2.  Execute the following commands in sequence:

```bash
# Clean up any previous build files
dotnet clean

# Restore project dependencies
dotnet restore

# Build and run the application
dotnet run
```

The application will launch. You should see a message in the console indicating the application is running, typically on **https://localhost:7151** and **http://localhost:5170**. The browser should open automatically to the default URL.

---

## ✅ 3. Features and Functionality

### 1. User Authentication and Access Control

**Secure Credential Management**: Implements a secure system for user registration and login, requiring a unique username and password for account creation and subsequent authentication.

- **Authentication Enforcement**: A secure login mechanism is utilized to validate user credentials against stored records.
- **Restricted Access**: The **Report Issues**, **Local Events and Announcements**, and **Service Request Status** modules are gated, requiring successful user authentication prior to access.

### 2. Main Menu and System Navigation

- **Report Issues:** Facilitates citizen engagement by allowing residents to submit detailed reports regarding community problems (e.g., infrastructure failures, service outages), including the optional upload of supporting media files.
- **Local Events and Announcements:** Manages and displays official municipal announcements and community events, supported by intelligent recommendation features.
- **Service Request Status:** Provides users with the capability to track the real-time lifecycle and progress of their reported issues, while supplying administrators with a strictly regulated, graph-based workflow for request management.

### 3. Report Issues Functionality

- **Submission Details**: Users are prompted to input critical information, including the issue's location and category.
- **Media Support**: The system supports the optional attachment of supporting images or documents by the user.
- **User Engagement**: An interactive user engagement layer, including encouraging messages and a leaderboard, is implemented to incentivize active and continuous participation.
- **Feedback Mechanism**: Users receive explicit feedback, such as success confirmations or error alerts, regarding the status of their submitted report.
- **Personal Tracking**: Authenticated users can access and view a list of all issues they have personally submitted.
- **Administrative Interface**: Administrators are provided with a comprehensive dashboard to view all reported issues, download any associated media attachments, and execute updates to the progress status of each report.

### 4. Local Events and Announcements

- **Event Dashboard:** Displays a unified page of current events and announcements upon entry, alongside robust filtering options and personalized recommendations.
- **Search and Filter Capabilities:** Users can execute comprehensive searches by title, description, and location. Filtering options include ranges for created date, start date, end date, as well as filters by category and location.
- **Archived Events:** A separate navigation option directs users to archived events, displaying past events within a 30-day window, complete with search and filter functionality.
- **Administrative Management:** Administrators are granted full CRUD (Create, Read, Update, Delete) capabilities for all events and announcements.

#### Intelligent Recommendation System (IRS)

| Component | Functionality |
| :--- | :--- |
| **Search Tracking** | Anonymously tracks user search patterns (category and date queries) to build both user-specific and global preference profiles. |
| **Recommendation Generation** | Calculates a relevance score for each event based on search tracking data, subsequently returning the **top N (default 5)** highest-ranked events as personalized or popularity-based recommendations. |

### 5. Service Request Status Management (Part 3 Focus)

- **Request Creation:** Users can initiate a new service request by submitting full request details, location data, citizen contact information, and optional media files.
- **Citizen Tracking:** Users can view a dedicated list of their submitted service requests, displaying key attributes such as status, priority, and the option to download their submitted media attachments.
- **Administrative Control (CRUD):** Administrators possess the authority to create, update, and manage the full lifecycle of all submitted service requests.

#### Advanced Smart Features (Data Structures)

| Feature                                 | Goal / Benefit                                                                                                                                                                                                             |
| :-------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Guaranteed Workflow Accuracy**        | Defines and enforces a regulated workflow, ensuring status transitions (e.g., Requested $\rightarrow$ Completed) are logical, preventing invalid jumps, and enforcing reliability and accountability.                      |
| **Fastest Resolution of Urgent Issues** | Instantly identifies and retrieves the highest-priority request ($O(1)$ access) via the "Next High Priority" feature, ensuring administrators immediately address the most critical tasks, thus optimizing response times. |
| **Instant Search Results**              | Organizes all service requests efficiently, guaranteeing $O(\log n)$ search performance for lookups by unique identifiers, providing a fast and responsive user experience.                                                |
| **Clear Visibility and Forecasting**    | Maps out all valid future status steps and history, providing administrators with essential data for progress tracking, forecasting timelines, and maintaining transparency.                                               |

---

## 🔐 4. User & Admin Navigation

### User Navigation (Citizen Access)

1. **Registration**

- **Access:** Select the “Register” tab (top-right).
- **Action:** Submit a unique username and secure password. The system enforces validation for uniqueness and password strength.

2. **Login**

- **Access:** Select the “Login” tab (top-right).
- **Action:** Authenticate by entering credentials to gain access to all restricted features.

3. **Report Issues (Part 1 Functionality)**

- **Access:** Navigate to the “Report Issues” module from the Main Menu.
- **Submission:** Provide Location, Category, and Description details. Users have an option to attach media via an `OpenFileDialog`. Submission is completed by clicking “Submit Report”.
- **Tracking:** View all personal submissions by selecting the “My Reported Issues” button.
- **Navigation:** Options include “Reported Issues +” (return to submission page) and “Back to Main Menu.”

4. **Local Events & Announcements (Part 2 Functionality)**

- **Access:** Access this page via the designated card or navigation link from the Main Menu.
- **Search & Filter:** Filter events using criteria like title, category, location, and date ranges (created, start, and end).
- **Recommendations:** The system displays Intelligent Recommendations based on tracked search preferences (Hash Table analysis).
- **Viewing:** View full event details and access Archived Events (past 30 days) via a dedicated page link.

5. **Service Request Status (Part 3 Functionality)**

- **Access:** Navigate to the “Service Request Status” module from the Main Menu.
- **Request Creation:** Initiate a new request by submitting details, location, contact information, and optional media files.
- **Tracking:** View a personal, organized list of submitted requests, which includes current status, assigned priority, and an option to download original media attachments.

### Admin Navigation (Administrative Access)

1. **Login**

- **Access:** Administrators log in using their designated credentials (e.g., Username:, Password:).
- **Result:** Successful authentication grants access to all administrative and management features.

2. **Report Issues Management (Part 1 Functionality)**

- **Access:** Navigate to the “Report Issues” section on the Home Page, which automatically routes them to the management view.
- **Issue Oversight:** View All Reported Issues across the system.
- **Management Actions:**
  - **Update Progress Status:** Change the status of any issue (e.g., Submitted, In Progress, Completed). The status change workflow is enforced by the **Graph** data structure.
  - **Media Access:** Securely access and Download Media Attachments.
  - **Filtering:** Filter and search issues by user, category, progress status, and date range.

3. **Event Management (Part 2 Functionality)**

- **Access:** Access the management features by clicking the “Local Events & Announcements” card on the Home Page while logged in.
- **Functionality:** Administrators have full CRUD (Create, Read, Update, Delete) control over all events and announcements.

4. **Service Request Management (Part 3 Functionality)**

- **Access:** Navigate to the “Service Request Status” module from the Main Menu.
- **Core Functionality (CRUD):** Administrators can Create new requests and perform Update and Delete actions on existing service requests.
- **Prioritization Tool (Min-Heap):** A dedicated feature, “Next High Priority Request,” uses the Min-Heap to instantly retrieve the most urgent pending task, optimizing immediate resource allocation.
- **Request Lookup (BST):** Administrators can use Request ID to perform Instant Search and retrieval of any request, relying on the Binary Search Tree for fast lookup performance.
- **Workflow Visualization (Graph Traversal):** When updating a status, Graph Traversal (BFS) is used to display only the valid next statuses, ensuring guaranteed workflow accuracy and providing clear visibility into the request's status history and possible future states.

---

## 🏗️ 5. Architecture

### Application Structure (ASP.NET Core MVC)

The application code adheres to the **MVC pattern**, which ensures a clear separation of concerns, making the codebase maintainable, testable, and scalable.

- **Model**: This layer manages the application's data and business logic. It includes the Entity Framework Core data context, the entity classes (e.g., Product, Order), and the service classes responsible for interacting with the database and external Azure APIs.
- **View**: The user interface (UI) is rendered using Razor views. This layer is responsible solely for presenting the data to the client and capturing user input.
- **Controller**: Controllers act as the entry point for handling user requests. They receive input, coordinate the necessary actions by calling methods in the Model layer, and determine which View to return to the user.

---

## ♻️ 6. Changelog

This changelog details the architectural and functional updates implemented for Service Request Status (PoE). This focuses on the integration of advanced custom data structures and algorithms to finalize the Service Request Status module.

### Custom Data Structures Implemented

The following custom data structures were implemented in the `_serviceRequestService` layer to guarantee high performance and data integrity:

- **Self-Balancing Binary Search Tree (e.g., AVL Tree):**
  - Purpose: Organizes all service requests, indexed primarily by `RequestID`.
  - Impact: Guarantees consistent $O(\log n)$ time complexity for request lookup, insertion, and deletion, preventing performance degradation under heavy load.
- **Min-Heap (Priority Queue):**
  - Purpose: Stores all pending service requests, ordered by calculated priority level.
  - Impact: Provides administrators with $O(1)$ access to the single most urgent task, enabling instant prioritization and optimization of response times.
- **Graph (Status Model):**
  - Purpose: Models the request status lifecycle as a Finite State Machine, where statuses are nodes and valid transitions are edges.
  - Impact: Enforces strict workflow integrity, preventing administrators from making invalid status jumps (e.g., skipping from "Requested" to "Completed").

### Algorithms Implemented

These algorithms leverage the new structures to enhance operational efficiency:

- **Breadth-First Search (BFS) / Graph Traversal:**
  - Purpose: Traverses the status Graph to determine valid workflow paths.
  - Impact: Used in `GetValidNextStatuses()` to dynamically present administrators with only the logical next steps, ensuring workflow compliance and clear visibility.
- **Minimum Spanning Tree (MST) Algorithm (e.g., Prim's or Kruskal's):**
  - Purpose: Calculates the most cost-effective travel route for field crews when multiple related service issues are clustered geographically.
  - Impact: Optimizes resource deployment and minimizes operational costs by finding the shortest total-distance network connecting required repair points.

### Functional Features

- **Service Request Creation:** Users can now submit new service requests via the Service Request Status module, including details, location, and optional media file attachments.
- **User Request Tracking:** Users can view a dedicated, organized list of their submitted service requests, displaying the current status, priority, and allowing them to download their original media attachments.
- **Admin Request Management:** Administrators gained full **Create, Update, and Delete (CRUD)** capabilities for all service requests.
- **Prioritization Feature:** Implemented the **"Next High Priority Request"** button for administrators, powered by the **Min-Heap**, for instant, critical task assignment.
- **Workflow Integrity:** Implemented system-wide workflow checks using the **Graph** to guarantee **Guaranteed Workflow Accuracy**, ensuring status transitions are always valid and traceable.
- **Search Performance:** Request lookup is now powered by the **Binary Search Tree**, providing **Instant Search Results** ($O(\log n)$) for both user tracking and administrative queries.
- **Visibility & Forecasting:** Enhanced administrative view to display **Clear Visibility** of all valid future status steps and history, enabled by **Graph Traversal**.

### Refinements and Scope Updates

**Local Events and Announcements**

- **Data Structure Separation:** The module has been refactored to implement a distinct logical and structural separation between **Events** and **Announcements**, which were previously aggregated under a single entity.
- **Administrative Interface:** Administrative **CRUD** operations now include a mandatory toggle mechanism to explicitly designate a new entry as either an **Event** or an **Announcement**. This enforces data integrity at the source.
- **User Clarity:** The user-facing interface now clearly displays whether a given entry is classified as an **Event** or an **Announcement**, significantly improving user navigation and information context.
- **User Event/Announcement Filter:** The user-facing interface for 'Local Events & Announcements' has been augmented with a toggle control. This mechanism allows users to isolate content visibility, displaying results filtered exclusively by either Events or Announcements.
- **Clickable Hashtags:** Hashtags displayed on individual cards within the 'Local Events & Announcements' list and on the 'View Event/Announcement Details' screen are now clickable. Selecting a hashtag will redirect the user to a new page, displaying all other events and announcements that share that specific hashtag.

**Service Logic and Tracking Metrics**

| Feature Area               | Original Implementation (Part 2)                                         | Updated Implementation (Part 3)                                                                                      | Key Improvement in Recommendations                                                                                                                        |
| :------------------------- | :----------------------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Tracking Metrics**       | Category popularity (global/user) and Date popularity (global/user).     | Adds **Keyword popularity** (global/user) and **Hashtag popularity** (global/user).                                  | Enables finer-grained interest mapping from user searches, moving beyond broad categories to specific topics and interests.                               |
| **Track Search Signature** | `(Guid? userId, string? category, DateOnly? fromDate, DateOnly? toDate)` | Adds `string? searchQuery` and `string? hashtag`.                                                                    | Allows the system to capture and learn from **free-form search queries** and explicit **hashtag clicks**, providing a richer data source.                 |
| **Keyword/Text Handling**  | None. Search queries were not processed for content.                     | Includes a new **`KeywordHelper` class** to extract meaningful words (ignoring "stop words") from the `searchQuery`. | Provides the ability to match event content (Title/Description) directly to the user's explicit textual interests.                                        |
| **Scoring Factors**        | Category Match (Weight: 10) and Date Match (Weight: 5).                  | Adds **Keyword Match (Weight: 15)**, **Hashtag Match (Weight: 8)**, and a **Recency Boost (Max 100 points)**.        | **Deeper Relevance:** Keywords and Hashtags target event content more precisely, while the Recency Boost ensures recommendations are timely and upcoming. |

**Code Structure and Maintainability**

| Feature Area           | Original Implementation (Part 2)                                                                    | Updated Implementation (Part 3)                                                                                         | Impact on Code Quality                                                                                |
| :--------------------- | :-------------------------------------------------------------------------------------------------- | :---------------------------------------------------------------------------------------------------------------------- | :---------------------------------------------------------------------------------------------------- |
| **Code Structure**     | Uses repetitive `if/else` logic to implement count increments.                                      | Introduces a reusable, generic static helper **`IncrementCount<TKey>`** to clean up the `TrackSearch` method.           | Improved code clarity and maintainability by eliminating redundant logic.                             |
| **Clear Tables Logic** | Manual clearing of tables by enumerating keys and calling `Remove()` within `ClearAllTables`.       | Modularized clearing with a reusable **`ClearTable<TKey, TValue>`** helper, making `ClearAllTables` cleaner.            | Better abstraction and reuse of cleanup logic across different tracking metrics.                      |
| **Persistence (DTO)**  | Tracks only `GlobalCategoryCounts`, `GlobalDateCounts`, `UserCategoryCounts`, and `UserDateCounts`. | Adds persistence fields for `GlobalKeywordCounts`, `GlobalHashtagCounts`, `UserKeywordCounts`, and `UserHashtagCounts`. | Ensures that the new, richer user interest profiles are saved and loaded across application restarts. |

#### Issue Reporting

- **Granular Location Data:** The location data schema now captures detailed geographical information, including:
  - Street Number
  - Street Name
  - Suburb
  - City
  - Postal Code
  - Precise Longitude/Latitude coordinates

This enhancement provides administrators with richer, more actionable location intelligence, improving dispatch and resolution times.

---

## 👥 7. Author and Contributions

### Primary Developer:

- I, **_Ishkar Singh_**, am the sole developer and author of this project:
  Email (for feedback or concerns): **ishkar.singh.108@gmail.com**

### Reporting Issues:

- If you encounter any bugs, glitches, or unexpected behaviour, please open an Issue on the GitHub repository.
- Provide as much detail as possible, including:
  - Steps to reproduce the issue
  - Error messages (if any)
  - Screenshots or logs (if applicable)
  - Expected vs. actual behaviour
- Clear and descriptive reports help improve the project effectively.

### Proposing Enhancements:

- Suggestions for improvements or feature enhancements are encouraged.
- You may open a Discussion or submit an Issue describing the proposed change.
- All ideas will be reviewed and considered for future updates.

---

## ⚖️ 8. MIT License

**Copyright © 2025 Ishkar Singh**<br>

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

---

## ❓ 9. Frequently Asked Questions (FAQ)

### 1. What software and knowledge do I need to run the VC Municipality website locally?

To run the VC Municipality website locally, you'll need:

- **IDE/Editor**: **Visual Studio Code** or **Visual Studio 2019/2022 and later** installed on your system.
- **.NET Framework**: **.NET Core 9.0 or later** (required for the **ASP.NET MVC architecture**).
- **Knowledge**: Basic knowledge of the **C# programming language**, including how to write a console program that requires user input, apply string manipulation, and use automatic properties. Understanding of **ASP.NET MVC structure** is beneficial.

### 2. How do I install and set up the VC Municipality website on my computer?

To install and set up the VC Municipality website:

1.  **Download**: Clone or download the project repository from `https://github.com/VCWVL/prog7312-part-2-singhishkar.git`.
2.  **Open Solution**: Open **Visual Studio Code or Visual Studio** and navigate to **File > Open > Project/Solution**. Browse to the location of the cloned/downloaded project and select the solution file (`.sln`) to open the project.
3.  **Restore**: Restore packages to resolve any dependencies (usually done automatically by the IDE).
4.  **Build**: Build the solution to ensure all dependencies are resolved and the custom data structures compile correctly.
5.  **Run**: Run the application via the IDE or by using the integrated terminal commands below:

```bash
dotnet clean
dotnet restore
dotnet build
dotnet run
```

### 3. What should I do if I encounter errors or need assistance while using the VC Municipality website?

If you encounter errors or need assistance while using the VC Municipality website:

- **Check Console**: Pay attention to any error messages or prompts displayed on the screen or in the application console.
- **Verify Input**: Ensure accurate input values, especially when filtering events or submitting reports, for the best user experience.
- **Consult Documentation**: If you're unsure about a particular step, refer back to the usage instructions provided in the documentation (this README).
- **Contact Support**: If the issue persists, you can reach out via email at `ishkar.singh.108@gmail.com`, or alternatively at `st10395002@vcconnect.edu.za` for further assistance.

### 4. How do I use the new Local Events & Announcements and Recommendation features (Part 2)? 🆕

The event system is designed for both public browsing and administrative management:

#### Public Access & Filtering (Users & Non-Users)

- **Access**: The **Local Events & Announcements** page can be accessed by clicking on the dedicated card or navigation link from the Home Page.
- **Filtering**: Visitors have advanced control over event discovery and can filter events by:
  - **Text Fields**: Title, Category, Hashtags, and Location.
  - **Date Ranges**: Start Date Range, End Date Range, and Created Date Range.
- **Recommendations**: The **Recommended Events** section will be dynamically altered based on the category and date range filters that the visitor applies, providing personalized results.
- **Views**: Visitors may view the event in full detail and can also access **archived events (past events)** using the filtering options.

#### Administration (Admins Only)

- **Access**: Administrators access the event management panel the same way—by navigating to the **Local Events & Announcements** page while logged in.
- **Functionality**: Administrators have full permissions to perform **CRUD (Create, Read, Update, Delete)** operations on all events, maintaining the municipal calendar.

### 5. How does the system ensure my service request is handled quickly and correctly?

The application uses advanced data structures to manage service requests efficiently:

- **Guaranteed Status Flow (Graph)**: The system uses a **Graph** to define all valid steps (e.g., must go from Acknowledged to Assigned). This prevents invalid jumps by administrators, ensuring your request follows a **logical, traceable path**.
- **Instant Priority (Min-Heap)**: When an administrator logs in, the **Min-Heap** instantly identifies the single most urgent request and assigns it, ensuring critical issues are addressed with **O(1) speed**.
- **Fast Tracking (BST)**: The **Binary Search Tree (BST)** organizes millions of requests, allowing the system to instantly find the status of your request by ID with guaranteed **$O(\log n)$ performance**.

### 6. What is the difference between an 'Event' and an 'Announcement' now?

The system now separates these two entities for greater clarity:

- **Event**: Refers to a scheduled activity with a specific start and end time (e.g., a community clean-up day, a public meeting).
- **Announcement**: Refers to timely municipal information without a formal schedule (e.g., a notice of road closure, a general service update, a warning).
- **Admin View**: Administrators explicitly labels an entry as either an **Event** or an **Announcement** during creation or update.

### 7. How does the system decide which is the most urgent issue?

The system uses a **Min-Heap (Priority Queue)** to manage all unassigned requests. Priority is typically determined by a calculated score based on factors like the issue's **severity**, the **number of citizens affected**, or its potential **risk**. The **Min-Heap** ensures that the request with the lowest numerical priority score (meaning the **highest real-world urgency**) is always at the top, ready for **immediate assignment** by an administrator.

### 8. Is the application running an external database? Where is the data stored?

**No**, the application does not rely on an external database (like **SQL Server or MongoDB**). All application data, including user accounts, issue reports, and service requests, is persistently stored **locally in JSON files**. This simplifies deployment and provides transparent data storage.

### 9. How does the system handle service issues that are close together?

For clusters of geographically related service issues, the system utilizes the **Minimum Spanning Tree (MST) algorithm**. This algorithm calculates the **single most efficient route** for a field crew to visit and resolve all clustered issues, **minimizing travel time** and **maximizing operational efficiency**.

---

## 📚 10. References

- **Adam Wilson, n.d. Image: Sunset Cityscape.** [online] _[Unsplash](https://images.unsplash.com/photo-1510783891783-80cda42ebfd2?ixlib=rb-4.1.0&q=85&fm=jpg&crop=entropy&cs=srgb&dl=adam-wilson-1QZYZib7eYs-unsplash.jpg)_ [Accessed 15 October 2025].
- **Artem Kniaz, n.d. Image: Downtown Traffic.** [online] _[Unsplash](https://images.unsplash.com/photo-1606092195730-5d7b9af1efc5?ixlib=rb-4.1.0&q=85&fm=jpg&crop=entropy&cs=srgb&dl=artem-kniaz-DqgMHzeio7g-unsplash.jpg)_ [Accessed 15 October 2025].
- **BVSA, n.d. Have Your Own Budget Shortfall? Here's What To Do.** [online] _[bvsa.co.za](https://bvsa.co.za/have-your-own-budget-shortfall-heres-what-to-do/)_ [Accessed 15 October 2025].
- **Campaign Creators, n.d. Image: Office Desk and Computer.** [online] _[Unsplash](https://images.unsplash.com/photo-1542744173-8e7e53415bb0?ixlib=rb-4.1.0&q=85&fm=jpg&crop=entropy&cs=srgb&dl=campaign-creators-gMsnXqILjp4-unsplash.jpg)_ [Accessed 15 October 2025].
- **DELTABLOC, n.d. Durban Road Closure Image.** [online] _[deltabloc.com](https://deltabloc.com/files/deltabloc/img/projects/db80-series/southafrica-durban-02.jpg)_ [Accessed 15 October 2025].
- **DELTABLOC, n.d. South Africa N2/N3 Durban Project.** [online] _[deltabloc.com](https://deltabloc.com/en/projects/south-africa-n2-n3-durban)_ [Accessed 15 October 2025].
- **Denis Chick, n.d. Image: Modern Building Closeup.** [online] _[Unsplash](https://images.unsplash.com/photo-1535535112387-56ffe8db21ff?ixlib=rb-4.1.0&q=85&fm=jpg&crop=entropy&cs=srgb&dl=denis-chick-mHqIs22M2Kw-unsplash.jpg)_ [Accessed 15 October 2025].
- **Designecologist, n.d. Image: Buildings and Sky.** [online] _[Unsplash](https://images.unsplash.com/photo-1560986752-2e31d9507413?ixlib=rb-4.1.0&q=85&fm=jpg&crop=entropy&cs=srgb&dl=designecologist-5mj5jLhYWpY-unsplash.jpg)_ [Accessed 15 October 2025].
- **eThekwini Municipality, n.d. Official Website.** [online] _[durban.gov.za](https://www.durban.gov.za/)_ [Accessed 15 October 2025].
- **Gemini, n.d. Image Generation Overview (Website Images Resources).** [online] _[google.com](https://gemini.google/overview/image-generation/)_ [Accessed 15 October 2025].
- **Imani, n.d. Image: City Street View.** [online] _[Unsplash](https://images.unsplash.com/photo-1521207418485-99c705420785?ixlib=rb-4.1.0&q=85&fm=jpg&crop=entropy&cs=srgb&dl=imani-vDQ-e3RtaoE-unsplash.jpg)_ [Accessed 15 October 2025].
- **Jeremy Thomas, n.d. Image: City Street and Buildings.** [online] _[Unsplash](https://images.unsplash.com/photo-1472145246862-b24cf25c4a36?ixlib=rb-4.1.0&q=85&fm=jpg&crop=entropy&cs=srgb&dl=jeremy-thomas-jh2KTqHLMjE-unsplash.jpg)_ [Accessed 15 October 2025].
- **Kevin Ku, n.d. Image: Tall City Building.** [online] _[Unsplash](https://images.unsplash.com/photo-1504639725590-34d0984388bd?ixlib=rb-4.1.0&q=85&fm=jpg&crop=entropy&cs=srgb&dl=kevin-ku-w7ZyuGYNpRQ-unsplash.jpg)_ [Accessed 15 October 2025].
- **Maria Lupan, n.d. Image: Lighted Bridge at Night.** [online] _[Unsplash](https://images.unsplash.com/photo-1591486085897-f433f05e7aed?ixlib=rb-4.1.0&q=85&fm=jpg&crop=entropy&cs=srgb&dl=maria-lupan-XeRqsvi9qBc-unsplash.jpg)_ [Accessed 15 October 2025].
- **Matthew Henry, n.d. Image: Street Light and City.** [online] _[Unsplash](https://images.unsplash.com/photo-1473341304170-971dccb5ac1e?ixlib=rb-4.1.0&q=85&fm=jpg&crop=entropy&cs=srgb&dl=matthew-henry-yETqkLnhsUI-unsplash.jpg)_ [Accessed 15 October 2025].
- **Miguel A. Amutio, n.d. Image: Urban Street at Night.** [online] _[Unsplash](https://images.unsplash.com/photo-1613937574892-25f441264a09?ixlib=rb-4.1.0&q=85&fm=jpg&crop=entropy&cs=srgb&dl=miguel-a-amutio-hBfiJshiBvc-unsplash.jpg)_ [Accessed 15 October 2025].
- **Nick Fewings, n.d. Image: Train Station Interior.** [online] _[Unsplash](https://images.unsplash.com/photo-1595278069441-2cf29f8005a4?ixlib=rb-4.1.0&q=85&fm=jpg&crop=entropy&cs=srgb&dl=nick-fewings--2lJGRIY5P0-unsplash.jpg)_ [Accessed 15 October 2025].
- **Quino Al, n.d. Image: City Park and Buildings.** [online] _[Unsplash](https://images.unsplash.com/photo-1475924156734-496f6cac6ec1?ixlib=rb-4.1.0&q=85&fm=jpg&crop=entropy&cs=srgb&dl=quino-al-mBQIfKlvowM-unsplash.jpg)_ [Accessed 15 October 2025].
- **Rock Staar, n.d. Image: City Highway View.** [online] _[Unsplash](https://images.unsplash.com/photo-1638262052640-82e94d64664a?ixlib=rb-4.1.0&q=85&fm=jpg&crop=entropy&cs=srgb&dl=rock-staar-NzIV4vOBA7s-unsplash.jpg)_ [Accessed 15 October 2025].
- **Russn_fckr, n.d. Image: People Walking in City.** [online] _[Unsplash](https://images.unsplash.com/photo-1456086272160-b28b0645b729?ixlib=rb-4.1.0&q=85&fm=jpg&crop=entropy&cs=srgb&dl=russn_fckr-krV5aS4jDjA-unsplash.jpg)_ [Accessed 15 October 2025].
- **Sebastian Morelli Peyton, n.d. Image: City Night Skyline.** [online] _[Unsplash](https://images.unsplash.com/photo-1639600993675-2281b2c939f0?ixlib=rb-4.1.0&q=85&fm=jpg&crop=entropy&cs=srgb&dl=sebastian-morelli-peyton-DVqdk8MTp2I-unsplash.jpg)_ [Accessed 15 October 2025].
- **Tanya Barrow, n.d. Image: City Street View.** [online] _[Unsplash](https://images.unsplash.com/photo-1758794583424-415d44fc095a?ixlib=rb-4.1.0&q=85&fm=jpg&crop=entropy&cs=srgb&dl=tanya-barrow-GIo-iGafRg4-unsplash.jpg)_ [Accessed 15 October 2025].
- **The Climate Reality Project, n.d. Image: City Scape and Traffic.** [online] _[Unsplash](https://images.unsplash.com/photo-1503428593586-e225b39bddfe?ixlib=rb-4.1.0&q=85&fm=jpg&crop=entropy&cs=srgb&dl=the-climate-reality-project-Hb6uWq0i4MI-unsplash.jpg)_ [Accessed 15 October 2025].
- **TripAdvisor, n.d. Durban Botanic Gardens Review.** [online] _[TripAdvisor](https://www.tripadvisor.com/Attraction_Review-g312595-d469379-Reviews-Durban_Botanic_Gardens-Durban_KwaZulu_Natal.html)_ [Accessed 15 October 2025].
- **Wesley Tingey, n.d. Image: Modern City View.** [online] _[Unsplash](https://images.unsplash.com/photo-1676181739859-08330dea8999?ixlib=rb-4.1.0&q=85&fm=jpg&crop=entropy&cs=srgb&dl=wesley-tingey-TdNLjGXVH3s-unsplash.jpg)_ [Accessed 15 October 2025].
