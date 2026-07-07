function showUpcomingToast(featureName) {
    // Remove existing toast if present
    const existing = document.getElementById('upcoming-toast');
    if (existing) {
        existing.remove();
    }

    // Create toast container
    const toast = document.createElement('div');
    toast.id = 'upcoming-toast';
    toast.className = 'custom-toast';
    toast.innerHTML = `
        <span class="custom-toast-icon"><i class="fas fa-info"></i></span>
        <span>The <strong>${featureName}</strong> feature is upcoming. Coming soon!</span>
    `;

    document.body.appendChild(toast);

    // Trigger animation
    setTimeout(() => {
        toast.classList.add('show');
    }, 50);

    // Auto dismiss after 3 seconds
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => {
            toast.remove();
        }, 300);
    }, 3000);
}

function togglePasswordVisibility(fieldId, toggleIcon) {
    const passwordField = document.getElementById(fieldId);
    if (!passwordField) return;

    if (passwordField.type === "password") {
        passwordField.type = "text";
        toggleIcon.classList.remove("fa-eye");
        toggleIcon.classList.add("fa-eye-slash");
    } else {
        passwordField.type = "password";
        toggleIcon.classList.remove("fa-eye-slash");
        toggleIcon.classList.add("fa-eye");
    }
}

function toggleMobileMenu() {
    const navbarCollapse = document.getElementById('navbarCollapse');
    if (navbarCollapse) {
        navbarCollapse.classList.toggle('show');
    }
}

function toggleUserDropdown(event) {
    event.stopPropagation(); // Prevent bubbling up to document click listener
    const menu = document.getElementById('userDropdownMenu');
    if (menu) {
        menu.classList.toggle('show');
    }
}

// Automatically close the dropdown menu if clicked outside
document.addEventListener('click', function(event) {
    const menu = document.getElementById('userDropdownMenu');
    const toggleBtn = document.querySelector('.dropdown-toggle');
    if (menu && menu.classList.contains('show')) {
        if (!menu.contains(event.target) && (!toggleBtn || !toggleBtn.contains(event.target))) {
            menu.classList.remove('show');
        }
    }
});

let currentConfirmCallback = null;

function showConfirm(title, message, confirmCallback) {
    const titleEl = document.getElementById('globalConfirmTitle');
    const msgEl = document.getElementById('globalConfirmMessage');
    const modalEl = document.getElementById('globalConfirmModal');
    
    if (titleEl) titleEl.innerText = title;
    if (msgEl) msgEl.innerText = message;
    
    currentConfirmCallback = confirmCallback;
    
    if (modalEl) {
        $(modalEl).css('display', 'flex').hide().fadeIn(200);
    }
}

$(document).ready(function() {
    // Global confirm modal Close listeners
    $('#globalConfirmCancelBtn, .global-close-modal').on('click', function() {
        $('#globalConfirmModal').fadeOut(200);
        currentConfirmCallback = null;
    });

    // Global confirm modal Yes (submit) action listener
    $('#globalConfirmYesBtn').on('click', function() {
        if (typeof currentConfirmCallback === 'function') {
            currentConfirmCallback();
        }
        $('#globalConfirmModal').fadeOut(200);
        currentConfirmCallback = null;
    });
});
