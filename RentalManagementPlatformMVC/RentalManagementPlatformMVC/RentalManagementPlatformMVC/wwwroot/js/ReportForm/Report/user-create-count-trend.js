(function () {
    const ns = (window.ReportConfig = window.ReportConfig || {});

    ns.register('UserCreateCountTrend', {
        title: '使用者創建數量趨勢',
        defaultType: 'line',
        base: '/ReportForm/ReportForm/',
        endpoint: 'UserCreateCountTrend',

        buildFilterUI(container) {
            container.innerHTML = `
              <div class="filter-row">
                <label>生理性別：</label>
                <select class="f-gender">
                  <option value="">請選擇</option>
                  <option value="M">男</option>
                  <option value="F">女</option>
                </select>
                <label>年齡區間：</label>
                <input class="f-age-min" type="number" min="0" max="150">
                <label>~</label>
                <input class="f-age-max" type="number" min="0" max="150">
                <label>使用者角色：</label>
                <select class="f-role" multiple></select>
              </div>
            `;

            const roleSel = container.querySelector('.f-role');

            // 載入角色
            (async () => {
                try {
                    const roles = await ns._fetchPostJSON('/ReportForm/ReportForm/Roles');
                    ns._fillSelect(roleSel, roles, { includeAll: false});
                } catch {
                    ns._fillSelect(roleSel, [], { includeAll: true, allText: '角色載入失敗', allValue: '' });
                }
            })();
        },

        serializeFilters(container, fd) {
            const gender = container.querySelector('.f-gender')?.value || '';
            const ageMin = container.querySelector('.f-age-min')?.value || '';
            const ageMax = container.querySelector('.f-age-max')?.value || '';
            const roleId = Array.from(container.querySelector('.f-role')?.selectedOptions || []).map(o => o.value);
            fd.append('Gender', gender);
            fd.append('AgeMin', ageMin);
            fd.append('AgeMax', ageMax);
            fd.append('RoleId', roleId);
        }
    });
})();
