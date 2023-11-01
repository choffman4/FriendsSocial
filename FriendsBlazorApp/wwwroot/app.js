window.blazorLocalStorage = {
    setItem: function (key, value) {
        localStorage.setItem(key, value);
    },
    getItem: function (key) {
        return localStorage.getItem(key);
    }
};

window.toggleModal = function (modalId) {
    const modal = new bootstrap.Modal(document.getElementById(modalId));
    modal.toggle();
}

window.forceCloseModal = function (modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'none';
        document.body.classList.remove('modal-open');
    }
}

window.removeModalBackdrop = function () {
    const backdrop = document.querySelector('.modal-backdrop');
    if (backdrop) {
        backdrop.remove();
    }
}
