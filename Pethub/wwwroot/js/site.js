// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

/**
 * Toggles the visibility of a password input field.
 * @param {string} inputId - The id of the password input element.
 * @param {string} btnId - The id of the button element that triggers the toggle.
 */
function togglePassword(inputId, btnId) {
    const input = document.getElementById(inputId);
    const btn = document.getElementById(btnId);

    if (!input || !btn) return;

    const isHidden = input.type === 'password';
    input.type = isHidden ? 'text' : 'password';

    // Update button text only (no emojis)
    btn.textContent = isHidden ? 'Hide' : 'Show';
    btn.setAttribute('aria-label', isHidden ? 'Hide password' : 'Show password');
}
