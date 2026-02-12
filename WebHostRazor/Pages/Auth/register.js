document.addEventListener('DOMContentLoaded', function () {
    console.log('Register.js loaded successfully');

    // Get success state from hidden field
    var isSuccessField = document.getElementById('isRegistrationSuccess');
    var isSuccess = isSuccessField ? isSuccessField.value === 'true' : false;

    console.log('isSuccessField:', isSuccessField);
    console.log('isSuccess value:', isSuccess);

    // Show popup if registration successful
    if (isSuccess) {
        console.log('Registration successful, showing popup...');
        showSuccessPopup();
    } else {
        console.log('Registration not successful or value not set');
    }

    var emailTimeout;
    var emailInput = document.getElementById('emailInput');
    var emailCheck = document.getElementById('emailCheck'); // Fix: Missing closing quote
    var passwordInput = document.getElementById('passwordInput');
    var passwordStrength = document.getElementById('passwordStrength');
    var registerForm = document.getElementById('registerForm');
    var submitBtn = document.getElementById('submitBtn');

    // Email availability check
    if (emailInput && emailCheck) {
        emailInput.addEventListener('input', function () {
            clearTimeout(emailTimeout);
            var email = this.value.trim();

            if (email.length > 0 && isValidEmail(email)) {
                emailTimeout = setTimeout(function () {
                    checkEmail(email);
                }, 500);
            } else {
                emailCheck.style.display = 'none';
            }
        });
    }

    // Password strength indicator
    if (passwordInput && passwordStrength) {
        passwordInput.addEventListener('input', function () {
            var password = this.value;
            var strength = getPasswordStrength(password);

            if (password.length === 0) {
                passwordStrength.style.display = 'none';
                return;
            }

            passwordStrength.style.display = 'block';
            passwordStrength.className = 'password-strength';

            if (strength.score <= 2) {
                passwordStrength.classList.add('strength-weak');
                passwordStrength.textContent = 'Weak: ' + strength.feedback;
            } else if (strength.score <= 3) {
                passwordStrength.classList.add('strength-medium');
                passwordStrength.textContent = 'Medium: ' + strength.feedback;
            } else {
                passwordStrength.classList.add('strength-strong');
                passwordStrength.textContent = 'Strong password!';
            }
        });
    }

    // Form submission
    if (registerForm && submitBtn) {
        registerForm.addEventListener('submit', function () {
            submitBtn.textContent = 'Creating Account...';
            submitBtn.disabled = true;
            this.classList.add('loading');
        });
    }
});

// Success popup functions
function showSuccessPopup() {
    console.log('showSuccessPopup called');
    var popup = document.getElementById('successPopup');
    console.log('popup element:', popup);

    if (popup) {
        popup.classList.add('show');
        startCountdown();
        console.log('Popup should be visible now');
    } else {
        console.error('Popup element not found');
    }
}

function closePopup() {
    console.log('closePopup called');
    var popup = document.getElementById('successPopup');
    if (popup) {
        popup.classList.remove('show');
    }
    if (window.countdownInterval) {
        clearInterval(window.countdownInterval);
    }
}

function redirectToLogin() {
    console.log('Redirecting to login...');
    window.location.href = '/Auth/Login';
}

function startCountdown() {
    console.log('Starting countdown...');
    var countdown = 5;
    var countdownElement = document.getElementById('countdownNumber');
    console.log('countdownElement:', countdownElement);

    if (countdownElement) {
        window.countdownInterval = setInterval(function () {
            countdown--;
            countdownElement.textContent = countdown;
            console.log('Countdown:', countdown);

            if (countdown <= 0) {
                clearInterval(window.countdownInterval);
                redirectToLogin();
            }
        }, 1000);
    } else {
        console.error('Countdown element not found');
    }
}

// Email and password functions
function checkEmail(email) {
    fetch('/Auth/Register?handler=CheckEmail&email=' + encodeURIComponent(email))
        .then(function (response) {
            if (!response.ok) throw new Error('Network error');
            return response.json();
        })
        .then(function (data) {
            var checkDiv = document.getElementById('emailCheck');
            if (checkDiv) {
                checkDiv.classList.remove('email-taken', 'email-available');
                if (data.available) {
                    checkDiv.classList.add('email-available');
                    checkDiv.textContent = '✓ Email available';
                } else {
                    checkDiv.classList.add('email-taken');
                    checkDiv.textContent = '✗ Email already exists';
                }
                checkDiv.style.display = 'block';
            }
        })
        .catch(function (error) {
            console.error('Email check error:', error);
        });
}

function isValidEmail(email) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

function getPasswordStrength(password) {
    var score = 0;
    var feedback = '';

    if (password.length >= 8) score++;
    if (/[a-z]/.test(password)) score++;
    if (/[A-Z]/.test(password)) score++;
    if (/\d/.test(password)) score++;
    if (/[@$!%*?&]/.test(password)) score++;

    if (password.length < 8) {
        feedback = 'At least 8 characters needed';
    } else if (score <= 2) {
        feedback = 'Add uppercase, numbers, symbols';
    } else if (score <= 3) {
        feedback = 'Add more character types';
    } else {
        feedback = 'Excellent!';
    }

    return { score: score, feedback: feedback };
}