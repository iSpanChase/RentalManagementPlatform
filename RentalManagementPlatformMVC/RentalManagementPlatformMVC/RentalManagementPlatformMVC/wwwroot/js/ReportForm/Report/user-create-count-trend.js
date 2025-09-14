(function () {
    const ns = (window.ReportConfig = window.ReportConfig || {});

    ns.register('UserCreateCountTrend', {
        title: '使用者創建數量趨勢',
        defaultType: 'line',
        base: '/ReportForm/ReportForm/',
        endpoint: 'UserCreateCountTrend',

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
                    ns._fillCheckbox(roleFilled, roles, "f-role");
                } catch {
                    ns._fillCheckbox(roleFilled, [], "f-role");
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
        }
    });
})();
