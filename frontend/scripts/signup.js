// signup.js

import { API_BASE_URL } from './config.js';

document.addEventListener("DOMContentLoaded", function () {
    console.log("DOM fully loaded and parsed");

    // Kullanıcı kayıt fonksiyonu
    async function registerUser() {
        const email = document.getElementById("signupEmail").value;
        const password = document.getElementById("signupPassword").value;
        const userName = document.getElementById("signupUsername").value;

        console.log("Registering user:", { email, password, userName });

        // Şifre uzunluğunu kontrol et
        if (password.length < 8) {
            showPasswordError("The password must be at least 8 characters long.");
            return; // Fonksiyondan çık, kayıt işlemini devam ettirme
        } else {
            clearPasswordError();
        }

        try {
            // API'ye POST isteği gönder
            const response = await fetch(`${API_BASE_URL}/users/register`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    email: email,
                    passwordHash: password,
                    userName: userName,
                }),
            });

            if (response.ok) {
                // Başarılı yanıt
                localStorage.setItem("email", email);
                showAlert("Verification code sent. Please check your email.", "success");
                setTimeout(() => {
                    window.location.href = "verification.html"; // Verification sayfasına yönlendirme
                }, 3000);
            } else {
                // Hata durumunda backend'den dönen mesaj
                const errorData = await response.json();
                showAlert(errorData.title || "An error occurred during registration.", "danger");
            }
        } catch (error) {
            console.error("Error during registration:", error);
            showAlert("An error occurred. Please try again later.", "danger");
        }
    }

    // Hata ve bilgi mesajlarını göstermek için fonksiyon
    function showAlert(message, type) {
        const messageBox = document.getElementById("messageBox");
        messageBox.style.display = "block";
        messageBox.className = `alert alert-${type}`;
        messageBox.textContent = message;

        setTimeout(() => {
            messageBox.style.display = "none";
        }, 3000);
    }

    // Şifre hatasını göstermek için fonksiyon
    function showPasswordError(message) {
        const passwordInput = document.getElementById("signupPassword");
        const feedback = passwordInput.nextElementSibling;

        passwordInput.classList.add("is-invalid");
        if (feedback && feedback.classList.contains("invalid-feedback")) {
            feedback.textContent = message;
        }
    }

    // Şifre hatasını temizlemek için fonksiyon
    function clearPasswordError() {
        const passwordInput = document.getElementById("signupPassword");
        const feedback = passwordInput.nextElementSibling;

        passwordInput.classList.remove("is-invalid");
        if (feedback && feedback.classList.contains("invalid-feedback")) {
            feedback.textContent = "";
        }
    }

    // HTML form submit olayını yakala ve registerUser'ı çağır
    const signupForm = document.getElementById("signupForm");
    if (signupForm) {
        signupForm.addEventListener("submit", (event) => {
            event.preventDefault(); // Sayfanın yeniden yüklenmesini engelle
            console.log("Signup form submitted");
            registerUser(); // Kullanıcı kayıt fonksiyonunu çağır
        });
    } else {
        console.error("signupForm element not found");
    }

    // Terms and Conditions checkbox'ı dinleyerek Sign Up düğmesini kontrol et
    const termsCheckbox = document.getElementById("termsCheckbox");
    if (termsCheckbox) {
        termsCheckbox.addEventListener("change", (event) => {
            const signupButton = document.querySelector("#signupForm button[type='submit']");
            if (signupButton) {
                signupButton.disabled = !event.target.checked; // Checkbox işaretliyse düğmeyi etkinleştir
                console.log(`Checkbox changed: ${event.target.checked}. Sign Up button disabled: ${signupButton.disabled}`);
            } else {
                console.error("Sign Up button not found");
            }
        });
    } else {
        console.error("termsCheckbox element not found");
    }

    // Şifre alanında gerçek zamanlı doğrulama yapmak için event listener ekleyin
    const passwordInput = document.getElementById("signupPassword");
    if (passwordInput) {
        passwordInput.addEventListener("input", (event) => {
            const password = event.target.value;
            if (password.length >= 8) {
                clearPasswordError();
            } else {
                showPasswordError("The password must be at least 8 characters long.");
            }
        });
    } else {
        console.error("signupPassword element not found");
    }
});
