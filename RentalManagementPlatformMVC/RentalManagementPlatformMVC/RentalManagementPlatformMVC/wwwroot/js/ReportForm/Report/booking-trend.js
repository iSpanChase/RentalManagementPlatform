(function () {
    const ns = (window.ReportConfig = window.ReportConfig || {});

    /** 共同邏輯（城市/區域/狀態）報表的工廠 */
    function registerReportFilter({
        id,
        title,
        endpoint,
        base = '/ReportForm/ReportForm/',
        defaultType = 'bar',
        timePolicy,
    }) {
        ns.register(id, {
            title,
            defaultType,
            base,
            endpoint,

            timePolicy,
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
                const cityId = container.querySelector('.f-city')?.value || '';
                const districtId = container.querySelector('.f-district')?.value || '';
                fd.append('CityId', cityId);
                fd.append('DistrictId', districtId);
            },

            fillFilters(container, form) {
                const get = (k) => form?.[k] ?? '';
                const citySel = container.querySelector('.f-city');
                const distSel = container.querySelector('.f-district');

                const waitForOptions = (sel) => new Promise(res => {
                    const tick = () => (sel && sel.options && sel.options.length) ? res() : setTimeout(tick, 50);
                    tick();
                });

                return (async () => {
                    await waitForOptions(citySel);
                    citySel.value = get('CityId') || '';
                    citySel.dispatchEvent(new Event('change')); // 觸發載入區

                    await waitForOptions(distSel);
                    distSel.value = get('DistrictId') || '';
                })();
            }
        });
    }

    registerReportFilter({
        id: 'BookingAverageAmountTrend',
        title: '訂單平均金額趨勢',
        endpoint: 'BookingAverageAmountTrend',
    });

    registerReportFilter({
        id: 'BookingTotalAmountTrend',
        title: '訂單總金額趨勢',
        endpoint: 'BookingTotalAmountTrend',
    });

    registerReportFilter({
        id: 'BookingCountTrend',
        title: '訂單數量趨勢',
        endpoint: 'BookingCountTrend',
    });
})();