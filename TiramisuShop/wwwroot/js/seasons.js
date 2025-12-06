document.addEventListener("DOMContentLoaded", function () {
    const container = document.getElementById('season-container');
    const seasonType = container.getAttribute('data-season');

    if (seasonType === "None" || !seasonType) return;

    // Cấu hình cho từng mùa
    const configs = {
        'Spring': { className: 'sakura', icon: '', count: 30, speed: 5 }, // Hoa đào
        'Summer': { className: 'sun-particle', icon: '', count: 20, speed: 7 }, // Nắng/Đom đóm
        'Autumn': { className: 'leaf', icon: '', count: 25, speed: 6 }, // Lá vàng
        'Winter': { className: 'snowflake', icon: '❄', count: 50, speed: 4 } // Tuyết
    };

    const config = configs[seasonType];
    if (!config) return;

    let w = window.innerWidth;

    // Hàm tạo 1 phần tử
    function createFlake() {
        const flake = document.createElement('div');
        flake.classList.add('season-flake');
        flake.classList.add(config.className);

        if (config.icon) flake.innerHTML = config.icon;

        // Vị trí ngẫu nhiên
        flake.style.left = Math.random() * 100 + 'vw';
        flake.style.opacity = Math.random();
        flake.style.fontSize = (Math.random() * 10 + 10) + 'px'; // Kích thước ngẫu nhiên

        // Thời gian rơi ngẫu nhiên
        const duration = Math.random() * 5 + config.speed;
        flake.style.animationDuration = duration + 's';
        flake.style.animationDelay = Math.random() * 5 + 's';

        container.appendChild(flake);

        // Xóa khi rơi xong để tránh nặng máy
        setTimeout(() => {
            flake.remove();
            createFlake(); // Tạo lại cái mới để duy trì số lượng
        }, duration * 1000);
    }

    // Khởi tạo số lượng phần tử ban đầu
    for (let i = 0; i < config.count; i++) {
        setTimeout(createFlake, Math.random() * 3000);
    }
});