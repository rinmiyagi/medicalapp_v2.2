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
