(function () {
    const ns = (window.ReportConfig = window.ReportConfig || {});

    ns.register('BookingRevenueTrend', {
        title: '訂單營收趨勢',
        defaultType: 'line',
        base: '/ReportForm/ReportForm/',
        endpoint: 'BookingRevenueTrend',

        buildFilterUI(container) {
            container.innerHTML = `
              <div class="filter-row">
                <label>縣/市：</label>
                <select class="f-city"></select>
                <label>鄉/區：</label>
                <select class="f-district" disabled></select>
                <label>訂單狀態：</label>
                <select class="f-status" multiple>
                  <option value="Cancelled">已取消</option>
                  <option value="Pending">待確認</option>
                  <option value="Confirmed">已確認</option>
                  <option value="Completed">已完成</option>
                </select>
              </div>
            `;

            const citySel = container.querySelector('.f-city');
            const distSel = container.querySelector('.f-district');

            // 載入縣市
            (async () => {
                try {
                    const cities = await ns._fetchJSON('/ReportForm/ReportForm/Cities');
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
                    const districts = await ns._fetchJSON(`/ReportForm/ReportForm/Districts?cityId=${encodeURIComponent(cityId)}`);
                    ns._fillSelect(distSel, districts, { includeAll: true, allText: '(全部)', allValue: '' });
                    distSel.disabled = false;
                } catch {
                    distSel.innerHTML = '<option value="">(載入失敗)</option>';
                    distSel.disabled = true;
                }
            });
        },

        serializeFilters(container, fd) {
            const cityId = container.querySelector('.f-city')?.value || '';
            const districtId = container.querySelector('.f-district')?.value || '';
            const status = Array.from(container.querySelector('.f-status')?.selectedOptions || []).map(o => o.value);
            fd.append('CityId', cityId);
            fd.append('DistrictId', districtId);
            fd.append('Status', status);
        }
    });
})();
