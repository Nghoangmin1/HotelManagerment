document.addEventListener('DOMContentLoaded', function () {
    // === 1. ROUTING & PANEL SWITCHING ===
    const navItems = document.querySelectorAll('.sidebar-nav .nav-item');
    const contentPanels = document.querySelectorAll('.content-panel');
    const dashboardCards = document.querySelectorAll('.dashboard-card');

    function switchPanel(targetId) {
        // Hide all panels
        contentPanels.forEach(panel => {
            panel.classList.remove('active');
        });

        // Show target panel
        const targetPanel = document.getElementById(targetId + '-panel');
        if (targetPanel) {
            targetPanel.classList.add('active');
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }

        // Update sidebar nav active state
        navItems.forEach(item => {
            item.classList.remove('active');
            if (item.getAttribute('data-target') === targetId) {
                item.classList.add('active');
            }
        });

        // Update URL hash without jumping
        if (window.location.hash !== '#/' + targetId) {
            history.pushState(null, null, '#/' + targetId);
        }

        // Close mobile sidebar if open
        const sidebar = document.getElementById('sidebar');
        if (sidebar) {
            sidebar.classList.remove('open');
        }
    }

    // Nav list clicks
    navItems.forEach(item => {
        item.addEventListener('click', function (e) {
            const target = this.getAttribute('data-target');
            if (target) {
                e.preventDefault();
                switchPanel(target);
            }
        });
    });

    // Dashboard card clicks
    dashboardCards.forEach(card => {
        card.addEventListener('click', function (e) {
            const route = this.getAttribute('data-route');
            if (route) {
                e.preventDefault();
                switchPanel(route);
            }
        });
    });

    // Hash change handler
    function handleHashChange() {
        const hash = window.location.hash || '#/home';
        const route = hash.replace('#/', '');
        const validRoutes = ['home', 'colors', 'typography', 'layout', 'navigation', 'elements', 'mockup'];
        if (validRoutes.includes(route)) {
            switchPanel(route);
        } else {
            switchPanel('home');
        }
    }

    window.addEventListener('hashchange', handleHashChange);
    // Initial routing
    handleHashChange();


    // === 2. MOBILE SIDEBAR TOGGLES ===
    const openBtn = document.getElementById('sidebar-open-btn');
    const closeBtn = document.getElementById('sidebar-close-btn');
    const sidebar = document.getElementById('sidebar');
    const mainContent = document.querySelector('.main-content');

    if (openBtn && sidebar) {
        openBtn.addEventListener('click', function () {
            sidebar.classList.add('open');
        });
    }

    if (closeBtn && sidebar) {
        closeBtn.addEventListener('click', function () {
            sidebar.classList.remove('open');
        });
    }

    // Close when clicking main content (on mobile screen sizes)
    if (mainContent && sidebar) {
        mainContent.addEventListener('click', function () {
            if (window.innerWidth <= 992 && sidebar.classList.contains('open')) {
                sidebar.classList.remove('open');
            }
        });
    }


    // === 3. COLOR COPY-TO-CLIPBOARD ===
    const colorCards = document.querySelectorAll('.color-card');
    const toast = document.getElementById('copy-toast');

    colorCards.forEach(card => {
        card.addEventListener('click', function () {
            const hexColor = this.getAttribute('data-color');
            if (hexColor) {
                // Copy to clipboard
                navigator.clipboard.writeText(hexColor).then(() => {
                    // Update toast message & show
                    if (toast) {
                        toast.querySelector('span').textContent = `Đã sao chép: ${hexColor}`;
                        toast.classList.add('show');
                        setTimeout(() => {
                            toast.classList.remove('show');
                        }, 2000);
                    }
                }).catch(err => {
                    console.error('Không thể sao chép: ', err);
                });
            }
        });
    });


    // === 4. CONTROL SHOWCASE DEMOS ===
    // Loading Button simulation
    const loadingBtn = document.getElementById('btn-loading-demo');
    if (loadingBtn) {
        loadingBtn.addEventListener('click', function () {
            const originalHtml = this.innerHTML;
            this.disabled = true;
            this.innerHTML = '<span class="spinner"></span><span>Đang xử lý...</span>';
            
            setTimeout(() => {
                this.disabled = false;
                this.innerHTML = originalHtml;
            }, 2000);
        });
    }

    // Form validation demo
    const valForm = document.getElementById('validation-form-demo');
    if (valForm) {
        valForm.addEventListener('submit', function (e) {
            e.preventDefault();
            const inputField = document.getElementById('validation-input-demo');
            const errorMsg = document.getElementById('validation-error-demo');

            if (inputField.value.trim() === '') {
                inputField.classList.add('error');
                if (errorMsg) errorMsg.style.display = 'flex';
            } else {
                inputField.classList.remove('error');
                if (errorMsg) errorMsg.style.display = 'none';
                
                // Show success feedback
                alert('Dữ liệu hợp lệ! Đã gửi.');
                inputField.value = '';
            }
        });
    }


    // === 5. HOTEL MOCKUP DASHBOARD SIMULATION ===
    // Mock Room Data
    let rooms = [
        { id: '101', type: 'Phòng Đơn Standard', status: 'available', price: '350.000đ', guest: '', phone: '', checkin: '' },
        { id: '102', type: 'Phòng Đơn Standard', status: 'occupied', price: '350.000đ', guest: 'Nguyễn Văn A', phone: '0901234567', checkin: '30/05/2026 14:00' },
        { id: '103', type: 'Phòng Đôi Standard', status: 'available', price: '500.000đ', guest: '', phone: '', checkin: '' },
        { id: '104', type: 'Phòng Đôi Standard', status: 'dirty', price: '500.000đ', guest: '', phone: '', checkin: '' },
        { id: '201', type: 'Phòng Đôi Deluxe', status: 'occupied', price: '800.000đ', guest: 'Trần Thị B', phone: '0988776655', checkin: '29/05/2026 12:30' },
        { id: '202', type: 'Phòng Đôi Deluxe', status: 'available', price: '800.000đ', guest: '', phone: '', checkin: '' },
        { id: '203', type: 'Phòng Suite VIP', status: 'reserved', price: '1.500.000đ', guest: 'Lê Văn C', phone: '0912345678', checkin: '31/05/2026 18:00 (Dự kiến)' },
        { id: '204', type: 'Phòng Suite VIP', status: 'available', price: '1.500.000đ', guest: '', phone: '', checkin: '' }
    ];

    let selectedRoomId = null;

    // Cache DOM elements
    const roomGrid = document.getElementById('hotel-room-grid');
    const filterButtons = document.querySelectorAll('.filter-btn');
    const detailContent = document.getElementById('hotel-detail-content');

    // Modal elements
    const bookingModal = document.getElementById('booking-modal');
    const modalCloseBtn = document.getElementById('modal-close-btn');
    const modalCancelBtn = document.getElementById('modal-cancel-btn');
    const bookingForm = document.getElementById('booking-form');
    const modalRoomNumber = document.getElementById('modal-room-number');

    // Update Status Statistics Cards
    function updateStats() {
        const total = rooms.length;
        const available = rooms.filter(r => r.status === 'available').length;
        const occupied = rooms.filter(r => r.status === 'occupied').length;
        const dirty = rooms.filter(r => r.status === 'dirty').length;
        const reserved = rooms.filter(r => r.status === 'reserved').length;

        const statAvail = document.getElementById('stat-available');
        const statOccupy = document.getElementById('stat-occupied');
        const statDirty = document.getElementById('stat-dirty');
        
        if (statAvail) statAvail.textContent = `${available}/${total}`;
        if (statOccupy) statOccupy.textContent = `${occupied}`;
        if (statDirty) statDirty.textContent = `${dirty}`;
    }

    // Render Room Cards Grid
    function renderRooms(filterStatus = 'all') {
        if (!roomGrid) return;
        roomGrid.innerHTML = '';

        rooms.forEach(room => {
            if (filterStatus !== 'all' && room.status !== filterStatus) {
                return;
            }

            const card = document.createElement('div');
            card.className = `room-card ${room.status}`;
            if (selectedRoomId === room.id) {
                card.style.borderColor = 'var(--primary-color)';
                card.style.boxShadow = 'var(--shadow-md)';
            }
            card.setAttribute('data-room-id', room.id);

            let statusText = 'Còn trống';
            if (room.status === 'occupied') statusText = 'Đang ở';
            else if (room.status === 'dirty') statusText = 'Đang dọn';
            else if (room.status === 'reserved') statusText = 'Đã đặt';

            card.innerHTML = `
                <div class="room-number">P.${room.id}</div>
                <div class="room-type">${room.type}</div>
                <div class="room-status-badge">
                    <span class="room-status-dot"></span>${statusText}
                </div>
            `;

            card.addEventListener('click', function () {
                selectRoom(room.id);
            });

            roomGrid.appendChild(card);
        });

        updateStats();
    }

    // Display Selected Room Details
    function selectRoom(roomId) {
        selectedRoomId = roomId;
        const room = rooms.find(r => r.id === roomId);
        if (!room || !detailContent) return;

        // Rerender grid to update active card border
        const activeFilter = document.querySelector('.filter-btn.active').getAttribute('data-filter');
        renderRooms(activeFilter);

        let statusText = 'Còn trống';
        let actionButtonsHtml = '';

        if (room.status === 'available') {
            statusText = '<span style="color:var(--color-success)">Còn trống (Available)</span>';
            actionButtonsHtml = `
                <button class="btn btn-primary w-full mt-16" id="btn-mock-book">
                    <i class="fa-solid fa-key"></i> Nhận phòng ngay (Check-in)
                </button>
            `;
        } else if (room.status === 'occupied') {
            statusText = '<span style="color:var(--color-info)">Đang có khách (Occupied)</span>';
            actionButtonsHtml = `
                <button class="btn btn-danger w-full mt-16" id="btn-mock-checkout">
                    <i class="fa-solid fa-right-from-bracket"></i> Trả phòng & Thanh toán
                </button>
            `;
        } else if (room.status === 'dirty') {
            statusText = '<span style="color:var(--color-warning)">Đang dọn dẹp (Dirty)</span>';
            actionButtonsHtml = `
                <button class="btn btn-primary w-full mt-16" id="btn-mock-clean" style="background-color: var(--color-warning); border-color: var(--color-warning);">
                    <i class="fa-solid fa-broom"></i> Hoàn tất dọn dẹp (Cleaned)
                </button>
            `;
        } else if (room.status === 'reserved') {
            statusText = '<span style="color:var(--secondary-color)">Đã đặt trước (Reserved)</span>';
            actionButtonsHtml = `
                <button class="btn btn-primary w-full mt-16" id="btn-mock-checkin-reserved">
                    <i class="fa-solid fa-key"></i> Xác nhận nhận phòng
                </button>
                <button class="btn btn-ghost w-full mt-8" id="btn-mock-cancel-reservation" style="color: var(--color-danger);">
                    Hủy đặt phòng
                </button>
            `;
        }

        detailContent.innerHTML = `
            <div class="hotel-detail-card animate-fade">
                <div class="detail-title">
                    <span>Phòng P.${room.id}</span>
                    <span style="font-size:0.8rem; font-weight:normal; font-family:var(--font-mono)">${room.price}/đêm</span>
                </div>
                <div class="detail-row">
                    <span class="detail-label">Loại phòng:</span>
                    <span class="detail-value">${room.type}</span>
                </div>
                <div class="detail-row">
                    <span class="detail-label">Trạng thái:</span>
                    <span class="detail-value">${statusText}</span>
                </div>
                ${room.guest ? `
                    <div style="border-top: 1px dashed var(--border-light); margin: 12px 0; padding-top: 12px;">
                        <div class="detail-row">
                            <span class="detail-label">Khách hàng:</span>
                            <span class="detail-value">${room.guest}</span>
                        </div>
                        <div class="detail-row">
                            <span class="detail-label">Số điện thoại:</span>
                            <span class="detail-value" style="font-family:var(--font-mono)">${room.phone}</span>
                        </div>
                        <div class="detail-row">
                            <span class="detail-label">Thời gian vào:</span>
                            <span class="detail-value" style="font-size:0.8rem;">${room.checkin}</span>
                        </div>
                    </div>
                ` : ''}
                <div style="margin-top:20px;">
                    ${actionButtonsHtml}
                </div>
            </div>
        `;

        // Attach action handlers
        const bookBtn = document.getElementById('btn-mock-book');
        const checkinReservedBtn = document.getElementById('btn-mock-checkin-reserved');
        const checkoutBtn = document.getElementById('btn-mock-checkout');
        const cleanBtn = document.getElementById('btn-mock-clean');
        const cancelReserveBtn = document.getElementById('btn-mock-cancel-reservation');

        if (bookBtn) {
            bookBtn.addEventListener('click', openBookingModal);
        }

        if (checkinReservedBtn) {
            checkinReservedBtn.addEventListener('click', function () {
                openBookingModal();
                // Pre-fill with reserved details
                const guestNameInput = document.getElementById('booking-guest-name');
                const guestPhoneInput = document.getElementById('booking-guest-phone');
                if (guestNameInput) guestNameInput.value = room.guest;
                if (guestPhoneInput) guestPhoneInput.value = room.phone;
            });
        }

        if (checkoutBtn) {
            checkoutBtn.addEventListener('click', function () {
                if (confirm(`Xác nhận trả phòng ${room.id} và xuất hóa đơn dịch vụ?`)) {
                    room.status = 'dirty';
                    room.guest = '';
                    room.phone = '';
                    room.checkin = '';
                    selectRoom(roomId);
                }
            });
        }

        if (cleanBtn) {
            cleanBtn.addEventListener('click', function () {
                room.status = 'available';
                selectRoom(roomId);
            });
        }

        if (cancelReserveBtn) {
            cancelReserveBtn.addEventListener('click', function () {
                if (confirm('Hủy bỏ thông tin đặt trước của phòng này?')) {
                    room.status = 'available';
                    room.guest = '';
                    room.phone = '';
                    room.checkin = '';
                    selectRoom(roomId);
                }
            });
        }
    }

    // Modal Control Functions
    function openBookingModal() {
        if (!bookingModal || !modalRoomNumber) return;
        modalRoomNumber.textContent = selectedRoomId;
        
        // Reset form
        if (bookingForm) bookingForm.reset();

        bookingModal.classList.add('show');
    }

    function closeBookingModal() {
        if (bookingModal) {
            bookingModal.classList.remove('show');
        }
    }

    if (modalCloseBtn) modalCloseBtn.addEventListener('click', closeBookingModal);
    if (modalCancelBtn) modalCancelBtn.addEventListener('click', closeBookingModal);

    // Form Submit Check-in
    if (bookingForm) {
        bookingForm.addEventListener('submit', function (e) {
            e.preventDefault();
            const guestName = document.getElementById('booking-guest-name').value;
            const guestPhone = document.getElementById('booking-guest-phone').value;

            if (!guestName || !guestPhone) {
                alert('Vui lòng nhập đầy đủ thông tin khách hàng!');
                return;
            }

            const room = rooms.find(r => r.id === selectedRoomId);
            if (room) {
                const now = new Date();
                const timeStr = `${String(now.getDate()).padStart(2, '0')}/${String(now.getMonth() + 1).padStart(2, '0')}/${now.getFullYear()} ${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}`;

                room.status = 'occupied';
                room.guest = guestName;
                room.phone = guestPhone;
                room.checkin = timeStr;

                closeBookingModal();
                selectRoom(selectedRoomId);
            }
        });
    }

    // Room filters binding
    filterButtons.forEach(btn => {
        btn.addEventListener('click', function () {
            filterButtons.forEach(b => b.classList.remove('active'));
            this.classList.add('active');

            const filterValue = this.getAttribute('data-filter');
            renderRooms(filterValue);
        });
    });

    // Initialize Mockup rendering
    renderRooms('all');
});
