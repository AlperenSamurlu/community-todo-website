// filepath: /c:/Users/asamu/OneDrive/Desktop/sunucu/To-Do (V1)/frontend/scripts/verification.js
import { API_BASE_URL } from './config.js';

document.getElementById("verificationForm").addEventListener("submit", async (event) => {
    event.preventDefault();

    const codeInput = document.getElementById("verificationCode").value; // Kullanıcının girdiği kod
    const email = localStorage.getItem("email"); // Email'i localStorage'dan al
    const messageBox = document.getElementById("messageBox"); // Mesaj kutusu

    if (!email) {
        showMessage("Email not found. Please register again.", "error");
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/users/verify`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({ email, code: codeInput }), // Backend'e email ve kod gönder
        });

        if (response.ok) {
            showMessage("Email verified successfully!", "success");
            setTimeout(() => {
                window.location.href = "login.html"; // Doğrulama başarılıysa yönlendir
            }, 3000);
        } else if (response.status === 400) {
            showMessage("Invalid code. Please try again.", "error");
        } else {
            showMessage("An error occurred. Please try again.", "error");
        }
    } catch (error) {
        console.error("Error verifying code:", error);
        showMessage("An error occurred. Please try again.", "error");
    }
});

// Resend Code işlemi
document.getElementById("resendCode").addEventListener("click", (event) => {
    event.preventDefault();
    showMessage("A new verification code has been sent to your email.", "info");
});

// Mesaj göstermek için fonksiyon
function showMessage(message, type) {
    const messageBox = document.getElementById("messageBox");
    messageBox.style.display = "block";

    // Mesaja göre renk ayarları
    if (type === "success") {
        messageBox.style.color = "green";
    } else if (type === "error") {
        messageBox.style.color = "red";
    } else if (type === "info") {
        messageBox.style.color = "blue";
    }

    messageBox.textContent = message;

    // Mesajı 3 saniye sonra gizle
    setTimeout(() => {
        messageBox.style.display = "none";
    }, 3000);
}