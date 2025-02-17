import { API_BASE_URL } from './config.js';

document.addEventListener('DOMContentLoaded', () => {

    // Check login status when page loads
    checkLoggedIn();

    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', handleLogin);
    }
});

async function handleLogin(e) {
    e.preventDefault();
    
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;

    try {
        const response = await fetch(`${API_BASE_URL}/users/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ email, password })
        });

        const data = await response.json();
        
        if (response.ok) {
            localStorage.clear();
            localStorage.setItem('token', data.token);
            localStorage.setItem('email', data.email);
            window.location.href = 'dashboard.html';
        } else {
            showError(data.message || "Invalid email or password.");
        }
    } catch (error) {
        console.error("Error logging in:", error);
        showError("An error occurred. Please try again.");
    }
}


// Check if user is already logged in
function checkLoggedIn() {
    const token = localStorage.getItem('token');
    if (token) {
        localStorage.removeItem('token'); // token varsa siler ve yenisinin verilmesini sağlar.
        window.location.href = 'login.html';
    }
}



  // Hata mesajını göstermek için bir yardımcı fonksiyon
  function showError(message) {
    const errorMessage = document.getElementById("errorMessage");
    errorMessage.style.display = "block"; // Hata mesajını göster
    errorMessage.textContent = message;

    // 3 saniye sonra hata mesajını gizle
    setTimeout(() => {
        errorMessage.style.display = "none";
    }, 3000);
}




