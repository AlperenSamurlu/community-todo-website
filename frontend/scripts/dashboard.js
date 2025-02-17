import { API_BASE_URL } from './config.js';

// dashboard.html ye giriş yapmadan girmeyi engeller
async function checkAuth() {
    const token = localStorage.getItem("token");
    
    if (!token) {
        window.location.href = "login.html";
        return false;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/users/verify`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            localStorage.clear();
            window.location.href = "login.html";
            return false;
        }

        return true;
    } catch (error) {
        console.error("Auth check failed:", error);
        localStorage.clear();
        window.location.href = "login.html";
        return false;
    }
}

document.addEventListener("DOMContentLoaded", async () => {

    if (!await checkAuth()) {
        return;
    }

    let communitiesList = document.querySelector("#communities ul.list-group"); // Topluluk listesi
    let createCommunityForm = document.querySelector("#createCommunityModal form");
    let joinCommunityForm = document.getElementById("joinCommunityForm");
    let searchCommunityInput = document.getElementById("searchCommunity");
    let searchResults = document.getElementById("searchResults");
    let communityPasswordInput = document.getElementById("communityPassword");
    let selectedCommunityId = null; // Seçilen topluluğun ID'si
    let taskForm = document.querySelector("form.mb-4");
    let taskList = document.getElementById("taskList");
    let taskTypeSelect = document.getElementById("taskType");
    setTimeout(initializePage, 100);
  
    fetchUserCommunitiesCurrentCommunitiesSection();
    fetchUserTasks();
    fetchUserCommunities(); // Toplulukları al ve dropdown'ı doldur

    // Bildirim
    async function fetchNotifications() {
        try {
          const response = await fetch(`${API_BASE_URL}/notifications/`, {
            headers: {
                "Authorization": `Bearer ${localStorage.getItem("token")}`,
                "Content-Type": "application/json"
                }
            });
          if (!response.ok) {
            throw new Error("Failed to fetch notifications.");
          }
      
          const data = await response.json();
          const notifications = data.$values || []; // API'den dönen $values listesini al
          const notificationsList = document.getElementById("notificationsList");
      
          notificationsList.innerHTML = "";
      
          if (notifications.length === 0) {
            const noNotificationsItem = document.createElement("li");
            noNotificationsItem.className = "list-group-item bg-dark text-white";
            noNotificationsItem.textContent = "No notifications available.";
            notificationsList.appendChild(noNotificationsItem);
            return;
          }
      
          notifications.forEach((notification) => {
            const listItem = document.createElement("li");
            listItem.className = "list-group-item bg-dark text-white";
            listItem.textContent = notification.message;
            notificationsList.appendChild(listItem);
          });
        } catch (error) {
          console.error("Error fetching notifications:", error);
        }
    }
      
    document.getElementById("notifications-tab").addEventListener("click", fetchNotifications);
      
    async function initializePage() {
        try {
            const response = await fetch(`${API_BASE_URL}/users/getUserName`, {
                headers: {
                    "Authorization": `Bearer ${localStorage.getItem("token")}`
                }
            });

            
            if (!response.ok) {
                console.error("Failed to get username. Status:", response.status);
                document.getElementById("dashboardTitle").textContent = "Dashboard";
            } else {
                const data = await response.json();
                document.getElementById("dashboardTitle").textContent = `Welcome ${data.userName}`;
            }
        } catch (error) {
            console.error("Failed to get username:", error);
            document.getElementById("dashboardTitle").textContent = "Dashboard";
        }
    
        // Yükleme tamamlandıktan sonra içerik göster
        document.getElementById("loadingScreen").style.display = "none";
        document.getElementById("mainContent").style.display = "block";
    }
    
    // communitiesListForCurrentCommunitiesSection
    async function fetchUserCommunitiesCurrentCommunitiesSection() {
        try {
    
            const response = await fetch(`${API_BASE_URL}/communities/user/communities`, {
                headers: {
                    "Authorization": `Bearer ${localStorage.getItem("token")}`
                }
            });
    
            if (!response.ok) {
                console.error("Failed to fetch communities. Status:", response.status);
                return;
            }
    
            const communitiesResponse = await response.json();
            console.log("Fetched Communities:", communitiesResponse);
    
            const communities = communitiesResponse.$values || [];
            const communitiesListElementForCurrentCommunities = document.getElementById("communitiesListForCurrentCommunitiesSection");
            if (!communitiesListElementForCurrentCommunities) {
                console.error("HTML Element not found: communitiesListForCurrentCommunitiesSection");
                return;
            }
    
            communitiesListElementForCurrentCommunities.innerHTML = "";
    
            if (communities.length === 0) {
                communitiesListElementForCurrentCommunities.innerHTML = "<li class='text-white'>No communities found.</li>";
                return;
            }
    
            communities.forEach((community) => {
                const listItem = document.createElement("li");
                listItem.className = "list-group-item bg-dark text-white";
    
                // **GÜNCELLENEN SATIR:** Her li elemanına data-community-id ekleniyor
                listItem.setAttribute("data-community-id", community.communityId);
    
                listItem.innerHTML = `
                    <strong>${community.communityName} - ID: ${community.communityId}</strong>
                    <br>
                    Role: ${community.role || "N/A"}
                    <br>
                    <div class="mt-2">
                        <button class="btn btn-danger btn-sm leave-community-button" data-community-id="${community.communityId}">Leave</button>
                        ${
                            community.role === "Admin"
                            ? `<button class="btn btn-primary btn-sm manage-community-button" data-community-id="${community.communityId}">Manage</button>`
                            : ""
                        }
                    </div>
                `;
    
                communitiesListElementForCurrentCommunities.appendChild(listItem);
            });
        } catch (error) {
            console.error("Error fetching communities for Current Communities:", error);
        }
    }
        
    // Kullanıcının katıldığı toplulukları getir ve dropdown'a ekle task type ı doldurur
    async function fetchUserCommunities() {
         taskTypeSelect.innerHTML = "";
    
         const defaultOption = document.createElement("option");
         defaultOption.value = "individual";
         defaultOption.textContent = "Individual";
         taskTypeSelect.appendChild(defaultOption);
        try {

            const response = await fetch(`${API_BASE_URL}/communities/user/communities`, {
                headers: {
                    "Authorization": `Bearer ${localStorage.getItem("token")}`
                }
            });

            if (!response.ok) {
                throw new Error("Topluluklar getirilemedi.");
            }
    
            const communities = await response.json();
    
            communities.$values.forEach(community => {
                const option = document.createElement("option");
                option.value = community.communityId;
                option.textContent = community.communityName;
                taskTypeSelect.appendChild(option);
            });
        } catch (error) {
            console.error("Error listing communities:", error);
        }
    }
    
    // 2. Yeni Topluluk Oluşturma
    createCommunityForm.addEventListener("submit", async (e) => {
        e.preventDefault();
    
        const communityName = document.getElementById("communityName").value;
        const communityDescription = document.getElementById("communityDescription").value;
        const communityPassword = document.getElementById("communityPassword").value;
    
        try {
            const response = await fetch(`${API_BASE_URL}/communities`, {
                method: "POST",
                headers: {
                     "Content-Type": "application/json" ,
                     "Authorization": `Bearer ${localStorage.getItem("token")}`
                     },
                body: JSON.stringify({
                    communityName,
                    description: communityDescription,
                    password: communityPassword,
                }),
            });
    
            if (response.ok) {
                alert("Community created successfully!");
                createCommunityForm.reset();
                fetchUserCommunitiesCurrentCommunitiesSection();
                fetchUserCommunities();
            } else {
                alert("Failed to create community.");
            }
        } catch (error) {
            console.error("Error creating community:", error);
        }
    });
    
    // 3. Topluluk Arama
    searchCommunityInput.addEventListener("input", async () => {
        const query = searchCommunityInput.value;
    
        if (query.trim() === "") {
            searchResults.innerHTML = "";
            return;
        }
    
        try {
            const response = await fetch(`${API_BASE_URL}/communities/search?query=${query}` , {
                headers: {
                    "Authorization": `Bearer ${localStorage.getItem("token")}`
                  }
            });
            const data = await response.json();
        
            const communities = data["$values"];
        
            searchResults.innerHTML = "";
        
            communities.forEach((community) => {
                const listItem = document.createElement("li");
                listItem.className =
                    "list-group-item bg-dark text-white d-flex justify-content-between align-items-center";
                listItem.innerHTML = `
                    <span>
                        <strong>${community.communityName}</strong> - ID: ${community.communityId}
                        <br>
                        <small>${community.description}</small>
                    </span>
                    <button class="btn btn-primary btn-sm join-btn" data-community-id="${community.communityId}">Join</button>
                `;
                searchResults.appendChild(listItem);
            });
        } catch (error) {
            console.error("Error fetching communities:", error);
        }
    });
    
    
    // join butonuna ayrı event listener eklemek yerine container (searchResults) üzerinden dinleme yapıyoruz.
    document.getElementById("searchResults").addEventListener("click", (e) => {
        if (e.target && e.target.classList.contains("join-btn")) {
            selectedCommunityId = e.target.dataset.communityId;
            console.log("Selected Community ID:", selectedCommunityId);
    
            // Şifre alanını sıfırla
            const pswInput = document.getElementById("psw");
            pswInput.value = "";
    
            // Mevcut modal varsa yeniden kullan, yoksa oluştur
            const passwordModalElement = document.getElementById("passwordModal");
            let passwordModal = bootstrap.Modal.getInstance(passwordModalElement);
            if (!passwordModal) {
                passwordModal = new bootstrap.Modal(passwordModalElement);
            }
            passwordModal.show();
        }
    });

    
    // Topluluğa Katılma Formu Submit Olayı
    document.getElementById("joinCommunityForm").addEventListener("submit", async (e) => {
        e.preventDefault();
    
        const pswInput = document.getElementById("psw");
        const communityPassword = pswInput.value.trim();
        const communityId = selectedCommunityId;
       
        console.log("Community Password Value:", communityPassword);
    
        if (!communityPassword) {
            console.log("Community ID:", communityId);
            console.log("Entered Password:", communityPassword);
            alert("Password cannot be empty!");
            return;
        }
    
        if (!communityId  || !communityPassword) {
            alert("Community ID or Password is missing!");
            console.log("Community ID:", communityId);
            console.log("Entered Password:", communityPassword);
            return;
        }
    
        try {
            const response = await fetch(`${API_BASE_URL}/communities/${communityId}/join`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${localStorage.getItem("token")}`
                },
                body: JSON.stringify({
                    password: communityPassword
                }),
            });
    
            if (response.ok) {
                alert("Successfully joined the community!");
                pswInput.value = "";
                bootstrap.Modal.getInstance(document.getElementById("passwordModal")).hide();
                fetchUserTasks();
                fetchUserCommunitiesCurrentCommunitiesSection();
                fetchUserCommunities();
            } else {
                const errorData = await response.json();
                alert(`Failed to join the community. ${errorData.message}`);
            }
        } catch (error) {
            console.error("Error joining community:", error);
            alert("An error occurred while trying to join the community.");
        }
    });
    
    // Topluluk üyelerini yükle
    async function fetchCommunityMembers(communityId) {
        console.log("Fetching members for Community ID:", communityId);
    
        try {
            const response = await fetch(`${API_BASE_URL}/usercommunities/${communityId}/members`, {
                headers: {
                    "Authorization": `Bearer ${localStorage.getItem("token")}`,
                    "Content-Type": "application/json"
                }
            });
            if (!response.ok) {
                throw new Error("Failed to fetch community members.");
            }
    
            const data = await response.json();
            console.log("Fetched Members:", data);
    
            const members = data.$values || [];
            const membersList = document.getElementById("communityMembersList");
            membersList.innerHTML = "";
    
            if (members.length === 0) {
                membersList.innerHTML = `<li class="list-group-item text-muted text-white">No members found.</li>`;
                return;
            }
    
            members.forEach((member) => {
                const listItem = document.createElement("li");
                listItem.className = "list-group-item d-flex justify-content-between align-items-center";
                listItem.innerHTML = `
                    <span>${member.userName} (${member.role})</span>
                <div>
                    ${member.role !== "Admin" ? `
                        <button class="btn btn-sm btn-danger remove-member" data-user-id="${member.userId}" data-community-id="${communityId}">Remove</button>
                    ` : ""}
                    ${member.role !== "Admin" ? `
                        <button class="btn btn-sm btn-success promote-member" data-user-id="${member.userId}" data-community-id="${communityId}">Promote</button>
                    ` : ""}
                </div>
            `;
                membersList.appendChild(listItem);
            });
        } catch (error) {
            console.error("Error fetching community members:", error);
        }
    }
    
    // Üyeyi kaldırma ve terfi ettirme işlemleri
    document.getElementById("communityMembersList").addEventListener("click", async (e) => {
        const communityId = e.target.getAttribute("data-community-id");
        const userId = e.target.getAttribute("data-user-id");
    
        if (!communityId || !userId) {
            console.error("Community ID or User ID is missing.");
            return;
        }
    
        if (e.target.classList.contains("remove-member")) {
            console.log("Removing User ID:", userId, "from Community ID:", communityId);
    
            try {
                const response = await fetch(`${API_BASE_URL}/usercommunities/${communityId}/${userId}`, {
                    method: "DELETE",
                    headers: {
                        "Authorization": `Bearer ${localStorage.getItem("token")}`
                    }
                });
    
                if (response.ok) {
                    e.target.closest("li").remove();
                    //fetchUserCommunities();
                    //fetchUserTasks();
                    //fetchUserCommunitiesCurrentCommunitiesSection();
                    showMessage("Member removed successfully.", "success");
                } else {
                    showMessage("Failed to remove member.", "danger");
                }
            } catch (error) {
                console.error("Error removing member:", error);
            }
        } else if (e.target.classList.contains("promote-member")) {
            console.log("Promoting User ID:", userId, "in Community ID:", communityId);
    
            try {
                const response = await fetch(`${API_BASE_URL}/usercommunities/${communityId}/promote/${userId}`, {
                    method: "PUT",
                    headers: {
                        "Authorization": `Bearer ${localStorage.getItem("token")}`
                    }
                    
                });
    
                if (response.ok) {
                    showMessage("Member promoted successfully.", "success");
                    //e.target.closest("li").querySelector("span").textContent = `${userId} (Admin)`;
                    //e.target.remove();
                    await fetchCommunityMembers(communityId);
                } else {
                    showMessage("Failed to promote member.", "danger");
                }
            } catch (error) {
                console.error("Error promoting member:", error);
            }
        }
    });
    
    // TO DO ALANI
    
    // Görev Ekleme Formu Submit Olayı
    taskForm.addEventListener("submit", async (e) => {
        e.preventDefault();
    
        const taskTitle = document.getElementById("taskTitle").value.trim();
        const taskDescription = document.getElementById("taskDescription").value.trim();
        const startDate = document.getElementById("startDate").value;
        const endDate = document.getElementById("endDate").value;
        const taskType = document.getElementById("taskType").value;
    
        if (!taskTitle || !startDate || !endDate) {
            alert("Please fill in all required fields!");
            return;
        }
        if (new Date(startDate) > new Date(endDate)) {
            alert("The start date must be before the end date.");
            return;
        }
        
        const isIndividual = taskType === "individual";
        const communityId = isIndividual ? null : taskType;
    
        const taskData = {
            taskTitle,
            taskDescription,
            startDate,
            endDate,
            isIndividual,
            communityId,
            
        };
    
        try {
            const response = await fetch(`${API_BASE_URL}/tasks`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${localStorage.getItem("token")}`
                },
                body: JSON.stringify(taskData),
            });
    
            if (response.ok) {
                const newTask = await response.json();
                addTaskToUI(newTask);
                taskForm.reset();
            } else {
                const errorData = await response.json().catch(() => null);
                const errorMessage = errorData?.message || "An unexpected error occurred.";
                alert(`Görev eklenemedi: ${errorMessage}`);
            }
        } catch (error) {
            console.error("Error adding a task:", error);
            alert("An error occurred while adding a task.");
        }
    });
    
    async function fetchUserTasks() {
        try {
            const response = await fetch(`${API_BASE_URL}/tasks/user/tasks` , {
                headers: {
                    "Authorization": `Bearer ${localStorage.getItem("token")}`
                }
            });
            if (!response.ok) {
                const errorMessage = await response.text();
                console.error("Error Message:", errorMessage);
                throw new Error("Tasks could not be delivered.");
            }
    
            const tasksResponse = await response.json();
            const tasks = tasksResponse.values?.$values || [];
            console.log("Tasks:", tasks);
    
            const taskList = document.getElementById("taskList");
            const noMissionsMessage = document.getElementById("noMissionsMessage");
            taskList.innerHTML = "";
    
            if (tasks.length === 0) {
                // Eğer görev yoksa "No missions yet." mesajını göster
                noMissionsMessage.style.display = "block";
                return;
            }
            noMissionsMessage.style.display = "none";
            if (Array.isArray(tasks)) {
                tasks.forEach(addTaskToUI);
            } else {
                console.error("Unexpected response format:", tasksResponse);
            }
        } catch (error) {
            console.error("Task listing error:", error);
        }
    }
    
    // Görev silme
    document.getElementById("taskList").addEventListener("click", async (e) => {
        if (e.target.classList.contains("delete-task")) {
            const taskId = e.target.closest("li").getAttribute("data-task-id");
            console.log("Delete Task ID:", taskId);
    
            if (!taskId ) {
                console.error("Task ID is null or undefined.");
                return;
            }
    
            try {
                const response = await fetch(`${API_BASE_URL}/tasks/${taskId}`, {
                    method: "DELETE",
                    headers: {
                        "Content-Type": "application/json",
                        "Authorization": `Bearer ${localStorage.getItem("token")}`,
                    },
                });
    
                if (response.ok) {
                    showMessage("Task deleted successfully.", "success");
                    e.target.closest("li").remove();
                } else {
                    showMessage("Failed to delete the task.", "danger");
                    console.error("Delete Error:", await response.json());
                }
            } catch (error) {
                console.error("Error deleting a task:", error);
            }
        }
    });
    
    // Edit task modalı
    document.querySelector("#taskList").addEventListener("click", async (e) => {
        if (e.target.classList.contains("edit-task")) {
            const taskId = e.target.closest("li").getAttribute("data-task-id");
    
            console.log("Edit Task ID:", taskId);
    
            try {
                const response = await fetch(`${API_BASE_URL}/tasks/${taskId}`, { 
                    method: "GET" ,
                    headers: {
                        "Authorization": `Bearer ${localStorage.getItem("token")}`
                    }
                
                });
                if (!response.ok) {
                    console.error("Failed to fetch task details:", response.status);
                    return;
                }
    
                const task = await response.json();
                console.log("Fetched Task:", task);
    
                document.getElementById("editTaskId").value = task.taskId;
                document.getElementById("editTaskTitle").value = task.taskTitle;
                document.getElementById("editTaskDescription").value = task.taskDescription;
                document.getElementById("editStartDate").value = task.startDate.split("T")[0];
                document.getElementById("editEndDate").value = task.endDate.split("T")[0];
                document.getElementById("editIsCompleted").checked = task.isCompleted;
    
                const editModal = new bootstrap.Modal(document.getElementById("editTaskModal"));
                editModal.show();
            } catch (error) {
                console.error("Error fetching task details:", error);
            }
        }
    });
    
    document.getElementById("updateTaskForm").addEventListener("submit", async (event) => {
        event.preventDefault();
    
        const taskId = document.getElementById("editTaskId").value;
        const updatedTask = {
            taskTitle: document.getElementById("editTaskTitle").value,
            taskDescription: document.getElementById("editTaskDescription").value,
            startDate: document.getElementById("editStartDate").value,
            endDate: document.getElementById("editEndDate").value,
            isCompleted: document.getElementById("editIsCompleted").checked,
        };
    
        console.log("Updating task:", updatedTask);
    
        try {
            const response = await fetch(`${API_BASE_URL}/tasks/${taskId}`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${localStorage.getItem("token")}`
                },
                body: JSON.stringify(updatedTask),
            });
    
            if (response.ok) {
                showMessage("Task status updated.", "success");
                fetchUserTasks();
            } else {
                showMessage("Failed to update task status.", "danger");
                console.error(await response.json());
            }
        } catch (error) {
            console.error("Error updating the task:", error);
        }
    });
    
    // görev durumu güncelleme checkbox
    document.getElementById("taskList").addEventListener("change", async (e) => {
        if (e.target.type === "checkbox") {
            const taskId = e.target.closest("li").getAttribute("data-task-id");
            const isCompleted = e.target.checked;
    
            console.log("Updating Completion Status for Task ID:", taskId);
            console.log("New Status:", isCompleted);
    
            try {
                const response = await fetch(`${API_BASE_URL}/tasks/${taskId}/complete`, {
                    method: "PUT",
                    headers: {
                        "Content-Type": "application/json",
                        "Authorization": `Bearer ${localStorage.getItem("token")}`
                    },
                    body: JSON.stringify({ isCompleted }),
                });
    
                if (response.ok) {
                    showMessage("Task status updated.", "success");
                } else {
                    showMessage("Failed to update task status.", "danger");
                    console.error("Completion Update Error:", await response.json());
                }
            } catch (error) {
                console.error("Error updating task status:", error);
            }
        }
    });
    
    // Görev Düzenleme Modali Açma
    document.querySelector(".list-group").addEventListener("click", (e) => {
        if (e.target.classList.contains("edit-task")) {
            const taskId = e.target.closest("li").getAttribute("data-task-id");
            console.log("Edit Task ID:", taskId);
    
            const editModal = new bootstrap.Modal(document.getElementById("editTaskModal"));
            editModal.show();
        }
    });
    
    //Görev Listesine Yeni Bir Görev Ekleme İşlemi
    function addTaskToUI(task) {
        const taskList = document.getElementById("taskList");
        const noMissionsMessage = document.getElementById("noMissionsMessage");

        if (noMissionsMessage) {
            noMissionsMessage.style.display = "none";
        }

        
        const taskItem = document.createElement("li");
        taskItem.className = "list-group-item bg-dark text-white";
        taskItem.setAttribute("data-task-id", task.taskId);
        taskItem.innerHTML = `
            <strong>${task.taskTitle}</strong> - ${task.taskDescription || "Açıklama yok"}
            <br>
            <small>${task.startDate.split("T")[0]} - ${task.endDate.split("T")[0]}</small>
            <br>
            <small>${task.isIndividual ? "Individual" : `Community ID: ${task.communityId}`}</small>
            <div class="mt-2">
                <label for="isCompleted-${task.taskId}" class="form-check-label">Is it complete?</label>
                <input type="checkbox" id="isCompleted-${task.taskId}" class="form-check-input ms-2" ${task.isCompleted ? "checked" : ""}>
                <button class="btn btn-warning btn-sm ms-2 edit-task">Edit</button>
                <button class="btn btn-danger btn-sm ms-2 delete-task">Delete</button>
            </div>
        `;
    
        taskList.appendChild(taskItem);
    }
    
    document.getElementById("communitiesListForCurrentCommunitiesSection").addEventListener("click", async (e) => {
        // Topluluk yönetimi
        if (e.target.classList.contains("manage-community-button")) {
            const listItem = e.target.closest(".list-group-item");
            const communityId = listItem ? listItem.getAttribute("data-community-id") : null;
    
            console.log("Manage Community ID:", communityId);
    
            if (!communityId) {
                console.error("Community ID is null or undefined.");
                return;
            }
    
            await fetchCommunityMembers(communityId);
            const manageModal = new bootstrap.Modal(document.getElementById("manageCommunityModal"));
            manageModal.show();
        }
    
        // Topluluktan çıkma
        if (e.target.classList.contains("leave-community-button")) {
            const communityId = e.target.getAttribute("data-community-id");
    
            if (!communityId) {
                alert("Community knowledge is missing.");
                return;
            }
    
            if (confirm("Are you sure you want to leave this community?")) {
                try {
                    const response = await fetch(`${API_BASE_URL}/usercommunities/${communityId}`, {
                        method: "DELETE",
                        headers: {
                            "Content-Type": "application/json",
                            "Authorization": `Bearer ${localStorage.getItem("token")}`
                        },
                    });
    
                    if (response.ok) {
                        alert("Successful exit from the community.");
                        e.target.closest("li").remove();
                        fetchUserCommunities();
                        fetchUserTasks();
                    } else {
                        console.error("Hata:", await response.text());
                        alert("There was no exit from the community.");
                    }
                } catch (error) {
                    console.error("Error logging out of the community:", error);
                    alert("An error has occurred. Please try again.");
                }
            }
        }
    });
    
    // SETTING //
    async function updatePassword() {
        const currentPassword = document.getElementById("currentPassword").value;
        const newPassword = document.getElementById("newPassword").value;
    
        const response = await fetch(`${API_BASE_URL}/users/change-password`, {
            method: 'PUT',
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${localStorage.getItem("token")}`
            },
            body: JSON.stringify({
                currentPassword: currentPassword,
                newPassword: newPassword
            })
        });
        
        if (!response.ok) {
            const errorData = await response.text();
            showMessage(errorData, "danger");
        } else {
            showMessage("Password updated successfully.", "success");
        }
    }
    
    document.getElementById("passwordForm").addEventListener("submit", (event) => {
        event.preventDefault();
        updatePassword();
    });
    // E-mail bilgilendirmesi
    document.getElementById("enableEmailNotifications").addEventListener("click", async () => {
        
    
        try {
            const response = await fetch(`${API_BASE_URL}/users/notifications/enable`, {
                    method: "PUT",
                    headers: {
                    "Authorization": `Bearer ${localStorage.getItem("token")}`,
                    "Content-Type": "application/json"
                }
            });
    
            if (response.ok) {
                showMessage("E-mail notifications have been enabled.", "success");
            } else {
                showMessage("Failed to enable e-mail notifications.", "danger");
            }
        } catch (error) {
            console.error("Error enabling e-mail notifications:", error);
        }
    });
    
    document.getElementById("disableEmailNotifications").addEventListener("click", async () => {
       
    
        try {
            const response = await fetch(`${API_BASE_URL}/users/notifications/disable`, {
                    method: "PUT",
                    headers: {
                    "Authorization": `Bearer ${localStorage.getItem("token")}` ,
                    "Content-Type": "application/json"
                }
            });
    
            if (response.ok) {
                showMessage("E-mail notifications have been disabled.", "success");
            } else {
                showMessage("Failed to disable e-mail notifications.", "danger");
            }
        } catch (error) {
            console.error("Error disabling e-mail notifications:", error);
        }
    });
    
    
    // YARDIMCI FONKSİYONLAR
    function showMessage(message, type = "info", duration = 3000) {
        const alertClass = {
            success: "alert-success",
            danger: "alert-danger",
            warning: "alert-warning",
            info: "alert-info",
        }[type] || "alert-info";
    
        let messageBox = document.getElementById("globalMessageBox");
        if (!messageBox) {
            messageBox = document.createElement("div");
            messageBox.id = "globalMessageBox";
            messageBox.style.position = "fixed";
            messageBox.style.top = "10px";
            messageBox.style.right = "10px";
            messageBox.style.zIndex = "9999";
            messageBox.style.minWidth = "300px";
            messageBox.style.maxWidth = "400px";
            document.body.appendChild(messageBox);
        }
    
        const alertBox = document.createElement("div");
        alertBox.className = `alert ${alertClass}`;
        alertBox.role = "alert";
        alertBox.textContent = message;
        messageBox.appendChild(alertBox);
    
        setTimeout(() => {
            alertBox.remove();
            if (!messageBox.hasChildNodes()) {
                messageBox.remove();
            }
        }, duration);
    }
    document.getElementById("logoutButton").addEventListener("click", () => {
        localStorage.clear();
        localStorage.removeItem("token");
        window.location.href = "login.html";
    });
});
