//window.blazorLocalStorage = {
//    setItem: function (key, value) {
//        localStorage.setItem(key, value);
//    },
//    getItem: function (key) {
//        return localStorage.getItem(key);
//    }
//};

// Debounce function
function debounce(func, wait) {
    let timeout;

    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };

        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Scroll event handler with debounce
window.detectScrollBottom = (dotNetHelper) => {
    const onScroll = () => {
        if (window.innerHeight + window.pageYOffset >= document.documentElement.scrollHeight - 10) {
            console.log('Attempting to load more posts');
            dotNetHelper.invokeMethodAsync('LoadMorePosts');
        }
    };

    // Apply debounce to the scroll event handler
    const debouncedOnScroll = debounce(onScroll, 100);

    // Remove the event listener if it was previously attached to prevent duplicates
    window.removeEventListener('scroll', debouncedOnScroll);

    // Add the new event listener
    window.addEventListener('scroll', debouncedOnScroll);
};

function scrollToBottom(id) {
    var element = document.getElementById(id);
    element.scrollTop = element.scrollHeight;
}