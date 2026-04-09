document.addEventListener('click', (event) => {
    const button = event.target.closest('[data-toggle-password]');
    if (!button) {
        const dismissButton = event.target.closest('[data-dismiss-flash]');
        if (dismissButton) {
            dismissButton.closest('.flash-message')?.remove();
        }

        return;
    }

    const selector = button.getAttribute('data-toggle-password');
    const input = selector ? document.querySelector(selector) : null;
    if (!input) {
        return;
    }

    const isPassword = input.getAttribute('type') === 'password';
    input.setAttribute('type', isPassword ? 'text' : 'password');
    button.textContent = isPassword ? 'Сховати' : 'Показати';
});

window.addEventListener('load', () => {
    document.querySelector('[data-loader]')?.classList.add('is-hidden');
});

document.addEventListener('submit', (event) => {
    const form = event.target;
    if (!(form instanceof HTMLFormElement)) {
        return;
    }

    const submitButton = form.querySelector('button[type="submit"]');
    if (!(submitButton instanceof HTMLButtonElement)) {
        return;
    }

    submitButton.disabled = true;
    submitButton.dataset.originalText = submitButton.textContent ?? '';
    submitButton.textContent = 'Завантаження...';

    window.setTimeout(() => {
        submitButton.disabled = false;
        submitButton.textContent = submitButton.dataset.originalText ?? submitButton.textContent;
    }, 4000);
});
