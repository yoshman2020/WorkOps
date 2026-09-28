/**
 * Bootstrap Modal Helper
 */
window.bootstrapModal = {
    /**
     * Show the Bootstrap modal
     * @param {string} id
     */
    show: function (id) {
        const modal = bootstrap.Modal.getOrCreateInstance('#' + id);
        modal.show();
    },

    /**
     * Hide the Bootstrap modal
     * @param {string} id
     */
    hide: function (id) {
        const modal = bootstrap.Modal.getOrCreateInstance('#' + id);
        modal.hide();
    }
};