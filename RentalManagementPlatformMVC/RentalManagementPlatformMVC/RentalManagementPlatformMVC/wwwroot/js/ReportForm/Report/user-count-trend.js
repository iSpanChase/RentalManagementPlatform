(function () {
    const ns = (window.ReportConfig = window.ReportConfig || {});

    ns.register('UserCountTrend', {
        title: '使用者數量趨勢',
        defaultType: 'line',
        base: '/ReportForm/ReportForm/',
        endpoint: 'UserCountTrend',

        buildFilterUI(container) {
            container.innerHTML = `
                <div class="row filter-row">
                    <div class="col-sm-3">
                        <label class="form-label">性別</label>
                        <select class="form-select f-gender">
                            <option value="">請選擇</option>
                            <option value="M">男</option>
                            <option value="F">女</option>
                        </select>
                    </div>
                    <div class="col-sm-4">
                        <label class="form-label">年齡區間</label>
                            <div class="d-flex align-items-center gap-3">
                                <input class="form-control f-age-min" type="number" min="0" max="150">
                                <span>~</span>
                                <input class="form-control f-age-max" type="number" min="0" max="150">
                            </div>
                        </div>
                    <div class="col-sm-5 role-filled">
                        <label class="form-label">使用者角色</label>
                    </div>
                  </div>
            `;

            const roleFilled = container.querySelector('.role-filled');

            // 載入角色
            (async () => {
                try {
                    const roles = await ns._fetchPostJSON('/ReportForm/ReportForm/Roles');
                    ns._fillCheckbox(roleFilled, roles,"f-role");
                } catch {
                    ns._fillCheckbox(roleFilled, [],"f-role");
                }
            })();
        },

        serializeFilters(container, fd) {
            let gender = container.querySelector('.f-gender')?.value || '';
            let ageMin = container.querySelector('.f-age-min')?.value || '';
            let ageMax = container.querySelector('.f-age-max')?.value || '';
            let roleAll = container.querySelectorAll('.f-role');
            let role = [];
            for (let x of roleAll) {
                if (x.checked)
                    role.push(x.value);
            }
            fd.append('Gender', gender);
            fd.append('AgeMin', ageMin);
            fd.append('AgeMax', ageMax);
            fd.append('RoleId', role);
        },

        fillFilters(container, form) {
            const get = (k) => form?.[k] ?? '';
            container.querySelector('.f-gender').value = get('Gender') || '';

            const ageMinEl = container.querySelector('.f-age-min');
            const ageMaxEl = container.querySelector('.f-age-max');
            if (ageMinEl) ageMinEl.value = get('AgeMin') || '';
            if (ageMaxEl) ageMaxEl.value = get('AgeMax') || '';

            // RoleId 可能是 "1,2,3" 或陣列字串
            const pick = new Set(String(get('RoleId') || '').split(',').map(s => s.trim()).filter(Boolean));

            // 等待 checkbox 渲染完成（因為它是 async 載入）
            return new Promise((resolve) => {
                const tryFill = () => {
                    const boxes = container.querySelectorAll('.f-role');
                    if (!boxes.length) {
                        setTimeout(tryFill, 50);
                        return;
                    }
                    boxes.forEach(b => { b.checked = pick.has(b.value); });
                    resolve();
                };
                tryFill();
            });
        }
    });
})();
