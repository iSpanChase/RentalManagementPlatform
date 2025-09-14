(function () {
    const ns = (window.ReportConfig = window.ReportConfig || {});

    ns.register('BookingAmountTrend', {
        title: '訂單金額趨勢',
        defaultType: 'line',
        base: '/ReportForm/ReportForm/',
        endpoint: 'BookingAmountTrend',

        buildFilterUI(container) {
            container.innerHTML = `
                <div class="row filter-row">
                    <div class="col-sm-4">
                        <label class="form-label">縣/市</label>
                        <select class="form-select f-city"></select>
                    </div>
                    <div class="col-sm-4">
                        <label class="form-label">鄉/區</label>
                        <select class="form-select f-district disabled"></select>
                    </div>
                    <div class="col-sm-4">
                    <label class="form-label">訂單狀態</label>
                    <div class="form-check">
                        <input class="form-check-input f-status" type="checkbox" value="Cancelled" id="f-status-cancelled">
                        <label class="form-check-label" for="f-status-cancelled">已取消</label>
                    </div>
                    <div class="form-check">
                        <input class="form-check-input f-status" type="checkbox" value="Pending" id="f-status-pending">
                        <label class="form-check-label" for="f-status-pending">待確認</label>
                    </div>
                    <div class="form-check">
                        <input class="form-check-input f-status" type="checkbox" value="Confirmed" id="f-status-confirmed">
                        <label class="form-check-label" for="f-status-confirmed">已確認</label>
                    </div>
                    <div class="form-check">
                        <input class="form-check-input f-status" type="checkbox" value="Completed" id="f-status-completed">
                        <label class="form-check-label" for="f-status-completed">已完成</label>
                    </div>
                    </div>
                  </div>
            `;

            const citySel = container.querySelector('.f-city');
            const distSel = container.querySelector('.f-district');

            // 載入縣市
            (async () => {
                try {
                    const cities = await ns._fetchGetJSON('/ReportForm/ReportForm/Cities');
                    ns._fillSelect(citySel, cities, { includeAll: true, allText: '(全部)', allValue: '' });
                    distSel.innerHTML = '<option value="">(先選縣/市)</option>';
                    distSel.disabled = true;
                } catch {
                    ns._fillSelect(citySel, [], { includeAll: true });
                    distSel.innerHTML = '<option value="">(載入失敗)</option>';
                    distSel.disabled = true;
                }
            })();

            // 當城市改變時載入區
            citySel.addEventListener('change', async () => {
                const cityId = citySel.value;
                if (!cityId) {
                    distSel.innerHTML = '<option value="">(先選縣/市)</option>';
                    distSel.disabled = true;
                    return;
                }
                try {
                    distSel.disabled = true;
                    distSel.innerHTML = '<option value="">(載入中...)</option>';
                    const districts = await ns._fetchGetJSON(`/ReportForm/ReportForm/Districts?cityId=${encodeURIComponent(cityId)}`);
                    ns._fillSelect(distSel, districts, { includeAll: true, allText: '(全部)', allValue: '' });
                    distSel.disabled = false;
                } catch {
                    distSel.innerHTML = '<option value="">(載入失敗)</option>';
                    distSel.disabled = true;
                }
            });
        },

        serializeFilters(container, fd) {
            let cityId = container.querySelector('.f-city')?.value || '';
            let districtId = container.querySelector('.f-district')?.value || '';
            let statusAll = container.querySelectorAll('.f-status');
            let status = [];
            for (let x of statusAll) {
                if (x.checked)
                    status.push(x.value);
            }
            fd.append('CityId', cityId);
            fd.append('DistrictId', districtId);
            fd.append('Status', status);
        },

        fillFilters(container, form) {
            const get = (k) => form?.[k] ?? '';
            const citySel = container.querySelector('.f-city');
            const distSel = container.querySelector('.f-district');

            const statuses = new Set(String(get('Status') || '').split(',').map(s => s.trim()).filter(Boolean));

            // 等城市載入
            const waitForOptions = (sel) => new Promise(res => {
                const tick = () => (sel && sel.options && sel.options.length) ? res() : setTimeout(tick, 50);
                tick();
            });

            return (async () => {
                await waitForOptions(citySel);
                citySel.value = get('CityId') || '';
                citySel.dispatchEvent(new Event('change')); // 觸發載入區

                // 等區載入完
                await waitForOptions(distSel);
                distSel.value = get('DistrictId') || '';

                // 狀態打勾
                container.querySelectorAll('.f-status').forEach(cb => cb.checked = statuses.has(cb.value));
            })();
        }
    });
})();
